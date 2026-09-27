using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;

namespace Munchify.BLL.Service.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PagedResult<ProductDto>>> GetProductsForStaffAsync(ProductFilterParams filter);
    Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id);
}
