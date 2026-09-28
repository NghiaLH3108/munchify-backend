using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<(IEnumerable<Product> Items, int TotalCount)> GetProductsAsync(ProductFilterParams filter);
    Task<IEnumerable<Product>> GetProductsForStockAsync(int? supplierId);
    Task<Product?> GetProductByIdWithDetailsAsync(int id);
}
