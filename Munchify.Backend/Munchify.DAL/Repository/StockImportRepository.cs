using System.Collections.Generic;
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
    }
}
