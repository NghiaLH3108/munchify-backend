using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class StockMovementRepository : GenericRepository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(MunchifyDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<StockMovement>> GetPagedAsync(
        int? productId = null, 
        string? movementType = null, 
        DateTime? from = null, 
        DateTime? to = null, 
        int page = 1, 
        int pageSize = 20)
    {
        var query = _dbSet
            .Include(sm => sm.Product)
            .Include(sm => sm.User)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(sm => sm.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(movementType))
        {
            query = query.Where(sm => sm.MovementType == movementType);
        }

        if (from.HasValue)
        {
            query = query.Where(sm => sm.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(sm => sm.CreatedAt <= to.Value);
        }

        int totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(sm => sm.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<StockMovement>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };
    }
}
