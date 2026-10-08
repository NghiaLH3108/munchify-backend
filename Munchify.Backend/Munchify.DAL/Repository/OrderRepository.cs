using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
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
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .FirstAsync(o => o.OrderId == order.OrderId, cancellationToken);
    }

    public async Task<PagedResult<Order>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .OrderByDescending(o => o.CreatedAt)
            .AsQueryable();

        int totalCount = await query.CountAsync(cancellationToken);

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
            .Include(o => o.OrderDetails)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.OrderId == orderId, cancellationToken);
    }
}
