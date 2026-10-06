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

    public IQueryable<Product> GetProductsQuery()
    {
        return _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Suppliers)
            .Include(p => p.Unit)
            .AsQueryable();
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
}
