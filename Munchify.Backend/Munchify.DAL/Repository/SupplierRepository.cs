using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(MunchifyDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Supplier>> GetAllWithCategoriesAsync()
    {
        return await _context.Suppliers
            .Include(s => s.Categories)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Supplier?> GetByIdWithCategoriesAsync(int supplierId)
    {
        return await _context.Suppliers
            .Include(s => s.Categories)
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
    }

    public async Task<bool> HasAssociatedImportsAsync(int supplierId)
    {
        return await _context.StockImports.AnyAsync(i => i.SupplierId == supplierId);
    }
}
