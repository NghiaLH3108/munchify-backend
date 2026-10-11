using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Combos;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    IQueryable<Product> GetProductsQuery();
    Task<IEnumerable<Product>> GetProductsForStockAsync(int? supplierId);
    Task<Product?> GetProductByIdWithDetailsAsync(int id);
    Task<IEnumerable<Product>> GetCombosWithComponentsAsync(CancellationToken cancellationToken = default);

    // ── Admin combo management ──
    Task<(IEnumerable<Product> Items, int TotalCount)> GetCombosPagedAsync(ComboFilterParams filter, CancellationToken cancellationToken = default);
    Task<Product?> GetComboByIdAsync(int id, bool track = false, CancellationToken cancellationToken = default);
    Task<bool> ProductCodeExistsAsync(string productCode, int? excludeProductId = null, CancellationToken cancellationToken = default);
    Task<List<Product>> GetProductsByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    Task<bool> HasHistoryAsync(int productId, CancellationToken cancellationToken = default);
    void RemoveComboItems(IEnumerable<ProductCombo> items);
    Task AddComboItemsAsync(IEnumerable<ProductCombo> items, CancellationToken cancellationToken = default);
}
