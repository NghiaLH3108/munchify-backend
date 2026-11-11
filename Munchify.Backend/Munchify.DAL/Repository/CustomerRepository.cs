using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Customers;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class CustomerRepository : ICustomerRepository
{
    private readonly MunchifyDbContext _context;

    public CustomerRepository(MunchifyDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Phone == phone, cancellationToken);
    }

    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == id, cancellationToken);
    }

    public async Task<bool> ExistsPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        return await _context.Customers.AnyAsync(c => c.Phone == phone, cancellationToken);
    }

    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;
        
        await _context.Customers.AddAsync(customer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        return customer;
    }

    public async Task<(System.Collections.Generic.IEnumerable<Customer> Customers, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, string? searchKeyword, CancellationToken cancellationToken = default)
    {
        var query = _context.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchKeyword))
        {
            query = query.Where(c => c.CustomerName.Contains(searchKeyword) || c.Phone.Contains(searchKeyword));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (customers, totalCount);
    }

    public async Task<(System.Collections.Generic.IEnumerable<CustomerPointHistoryDto> History, int TotalCount)> GetPointHistoryAsync(int customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => o.CustomerId == customerId && (o.PointsEarned > 0 || o.PointsRedeemed > 0));

        var totalCount = await query.CountAsync(cancellationToken);

        var history = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new Munchify.DAL.DTOs.Customers.CustomerPointHistoryDto
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                PointsEarned = o.PointsEarned,
                PointsRedeemed = o.PointsRedeemed,
                TotalAmount = o.TotalAmount,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return (history, totalCount);
    }
}
