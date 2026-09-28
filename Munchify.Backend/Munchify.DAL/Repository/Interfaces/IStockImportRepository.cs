using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces
{
    public interface IStockImportRepository : IGenericRepository<StockImport>
    {
        Task<IEnumerable<StockImport>> GetAllWithDetailsAsync();
        Task<StockImport?> GetByIdWithDetailsAsync(int id);
    }
}
