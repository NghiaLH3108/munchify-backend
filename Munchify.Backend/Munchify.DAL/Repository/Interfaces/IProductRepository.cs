using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    IQueryable<Product> GetProductsQuery();
    Task<IEnumerable<Product>> GetProductsForStockAsync(int? supplierId);
    Task<Product?> GetProductByIdWithDetailsAsync(int id);
}
