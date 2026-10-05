using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository
{
    public class StockImportRepository : GenericRepository<StockImport>, IStockImportRepository
    {
        public StockImportRepository(MunchifyDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<StockImport>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.StockImportDetails)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.Unit)
                .ToListAsync();
        }

        public async Task<StockImport?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(i => i.Supplier)
                .Include(i => i.User)
                .Include(i => i.StockImportDetails)
                    .ThenInclude(d => d.Product)
                        .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(i => i.ImportId == id);
        }

        public async Task<Dictionary<int, int>> GetPendingImportQuantitiesAsync()
        {
            var pendingQuantities = await _context.StockImportDetails
                .Where(d => d.Import.Status == "DRAFT" || d.Import.Status == "PENDING")
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(d => d.Quantity) })
                .ToDictionaryAsync(x => x.ProductId, x => x.TotalQty);

            return pendingQuantities;
        }

        public async Task<decimal?> GetLatestImportPriceAsync(int productId)
        {
            return await _context.StockImportDetails
                .Where(d => d.ProductId == productId)
                .OrderByDescending(d => d.ImportId)
                .Select(d => (decimal?)d.ImportPrice)
                .FirstOrDefaultAsync();
        }
    }
}
