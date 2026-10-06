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

    public async Task<System.Collections.Generic.Dictionary<int, double>> GetSalesDataForForecastingAsync(int daysHistory)
    {
        var fromDate = DateTime.Now.AddDays(-daysHistory);
        
        var salesData = await _dbSet
            .Where(sm => sm.CreatedAt >= fromDate && 
                        (sm.MovementType == "SALE" || sm.MovementType == "COMBO_SALE"))
            .GroupBy(sm => sm.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalSold = (double)g.Sum(sm => Math.Abs(sm.Quantity)) // Quantity is typically negative for sales
            })
            .ToListAsync();

        return salesData.ToDictionary(x => x.ProductId, x => x.TotalSold);
    }
}
