using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(MunchifyDbContext context) : base(context)
    {
    }

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(ProductFilterParams filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Suppliers)
            .Include(p => p.Unit)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.Trim().ToLower();
            query = query.Where(p => p.ProductName.ToLower().Contains(searchTerm) || 
                                     p.ProductCode.ToLower().Contains(searchTerm));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
        }

        if (filter.UnitId.HasValue)
        {
            query = query.Where(p => p.UnitId == filter.UnitId.Value);
        }

        if (filter.IsCombo.HasValue)
        {
            query = query.Where(p => p.IsCombo == filter.IsCombo.Value);
        }

        if (filter.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filter.IsActive.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.ProductName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return (items, totalCount);
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
        var query = _context.Products.Include(p => p.Unit).AsQueryable();

        if (supplierId.HasValue)
        {
            query = query.Where(p => p.Category.Suppliers.Any(s => s.SupplierId == supplierId.Value));
        }

        return await query.ToListAsync();
    }
}
