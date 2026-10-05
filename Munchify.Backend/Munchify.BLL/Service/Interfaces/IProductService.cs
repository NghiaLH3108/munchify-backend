using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;

namespace Munchify.BLL.Service.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductFilterParams filter);
    Task<ApiResponse<IEnumerable<ProductForStockDto>>> GetProductsForStockAsync(int? supplierId);
    Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id);
    Task<ApiResponse<ProductDetailDto>> CreateProductAsync(ProductRequest request);
    Task<ApiResponse<ProductDetailDto>> UpdateProductAsync(int id, ProductRequest request);
    Task<ApiResponse<bool>> ToggleProductStatusAsync(int id);
}
