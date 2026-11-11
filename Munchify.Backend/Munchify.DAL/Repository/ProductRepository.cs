using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Combos;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(MunchifyDbContext context) : base(context)
    {
    }

    public IQueryable<Product> GetProductsQuery()
    {
        return _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Suppliers)
            .Include(p => p.Unit)
            .AsQueryable();
    }

    public async Task<List<Product>> GetActiveSingleProductsWithSuppliersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => !p.IsCombo && p.IsActive)
            .Include(p => p.Category)
                .ThenInclude(c => c.Suppliers)
            .Include(p => p.Unit)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetProductByIdWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.ProductComboParentProducts)
                .ThenInclude(pc => pc.ComponentProduct)
                    .ThenInclude(cp => cp.Unit)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    public async Task<IEnumerable<Product>> GetProductsForStockAsync(int? supplierId)
    {
        var query = _context.Products
            .Include(p => p.Unit)
            .Where(p => !p.IsCombo && p.IsActive)
            .AsQueryable();

        if (supplierId.HasValue)
        {
            var filtered = await query
                .Where(p => p.Category.Suppliers.Any(s => s.SupplierId == supplierId.Value))
                .ToListAsync();
            if (filtered.Count > 0)
            {
                return filtered;
            }
        }

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetCombosWithComponentsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.ProductComboParentProducts)
                .ThenInclude(pc => pc.ComponentProduct)
                    .ThenInclude(cp => cp.Unit)
            .Where(p => p.IsCombo && p.IsActive)
            .OrderBy(p => p.ProductName)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetCombosPagedAsync(
        ComboFilterParams filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.ProductComboParentProducts)
                .ThenInclude(pc => pc.ComponentProduct)
                    .ThenInclude(cp => cp.Unit)
            .Where(p => p.IsCombo);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(term) ||
                                     p.ProductCode.ToLower().Contains(term));
        }

        if (filter.IsActive.HasValue)
            query = query.Where(p => p.IsActive == filter.IsActive.Value);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .ThenBy(p => p.ProductName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Product?> GetComboByIdAsync(int id, bool track = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Include(p => p.ProductComboParentProducts)
                .ThenInclude(pc => pc.ComponentProduct)
                    .ThenInclude(cp => cp.Unit);

        if (!track) query = query.AsNoTracking();
        else query = query.Include(p => p.Promotions);

        return await query.FirstOrDefaultAsync(p => p.ProductId == id && p.IsCombo, cancellationToken);
    }

    public async Task<bool> ProductCodeExistsAsync(string productCode, int? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(
            p => p.ProductCode == productCode && (!excludeProductId.HasValue || p.ProductId != excludeProductId.Value),
            cancellationToken);
    }

    public async Task<List<Product>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default)
    {
        var idList = ids.ToList();
        return await _context.Products
            .AsNoTracking()
            .Where(p => idList.Contains(p.ProductId))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasHistoryAsync(int productId, CancellationToken cancellationToken = default)
    {
        return await _context.OrderDetails.AnyAsync(od => od.ProductId == productId, cancellationToken)
            || await _context.StockMovements.AnyAsync(sm => sm.ProductId == productId, cancellationToken)
            || await _context.StockImportDetails.AnyAsync(sd => sd.ProductId == productId, cancellationToken);
    }

    public void RemoveComboItems(IEnumerable<ProductCombo> items)
    {
        _context.ProductCombos.RemoveRange(items);
    }

    public async Task AddComboItemsAsync(IEnumerable<ProductCombo> items, CancellationToken cancellationToken = default)
    {
        await _context.ProductCombos.AddRangeAsync(items, cancellationToken);
    }
}
