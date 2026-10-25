using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Orders;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly MunchifyDbContext _context;

    public OrderRepository(MunchifyDbContext context)
    {
        _context = context;
    }

    public async Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Reload with full navigation
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Staff)
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .FirstAsync(o => o.OrderId == order.OrderId, cancellationToken);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Staff)
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .AsQueryable();

        // Search query (OrderCode, Customer Name, Customer Phone)
        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var search = filter.SearchQuery.Trim();
            query = query.Where(o =>
                o.OrderCode.Contains(search) ||
                (o.Customer != null && o.Customer.CustomerName.Contains(search)) ||
                (o.Customer != null && o.Customer.Phone.Contains(search)));
        }

        // StaffId (STAFF chỉ thấy đơn của chính mình)
        if (filter.StaffId.HasValue)
        {
            query = query.Where(o => o.StaffId == filter.StaffId.Value);
        }

        // OrderStatus
        if (!string.IsNullOrWhiteSpace(filter.OrderStatus) && filter.OrderStatus != "ALL")
        {
            query = query.Where(o => o.OrderStatus == filter.OrderStatus);
        }

        // PaymentMethod
        if (!string.IsNullOrWhiteSpace(filter.PaymentMethod) && filter.PaymentMethod != "ALL")
        {
            query = query.Where(o => o.Payments.Any(p => p.PaymentMethod == filter.PaymentMethod));
        }

        // FromDate
        if (filter.FromDate.HasValue)
        {
            var from = filter.FromDate.Value.Date;
            query = query.Where(o => o.CreatedAt >= from);
        }

        // ToDate
        if (filter.ToDate.HasValue)
        {
            var to = filter.ToDate.Value.Date.AddDays(1);
            query = query.Where(o => o.CreatedAt < to);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        int totalCount = await query.CountAsync(cancellationToken);

        int page = filter.Page < 1 ? 1 : filter.Page;
        int pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize,
        };
    }

    public async Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default)
    {
        return await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Staff)
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }
}
