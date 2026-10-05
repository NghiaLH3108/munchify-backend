using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;

namespace Munchify.BLL.Service.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsForStaffAsync(ProductFilterParams filter);
    Task<ApiResponse<IEnumerable<ProductForStockDto>>> GetProductsForStockAsync(int? supplierId);
    Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id);
    Task<ApiResponse<IEnumerable<ProductDetailDto>>> GetCombosForStaffAsync(CancellationToken cancellationToken = default);
}
