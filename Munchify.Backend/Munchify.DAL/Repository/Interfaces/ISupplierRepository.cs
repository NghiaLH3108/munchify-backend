using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface ISupplierRepository : IGenericRepository<Supplier>
{
    Task<IEnumerable<Supplier>> GetAllWithCategoriesAsync();
    Task<Supplier?> GetByIdWithCategoriesAsync(int supplierId);
    Task<bool> HasAssociatedImportsAsync(int supplierId);
}
