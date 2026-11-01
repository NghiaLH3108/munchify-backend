using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Combos;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;

namespace Munchify.BLL.Service.Interfaces;

public interface IComboService
{
    Task<ApiResponse<PagedResult<ProductDetailDto>>> GetCombosAsync(ComboFilterParams filter, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> GetComboByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> CreateComboAsync(SaveComboRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<ProductDetailDto>> UpdateComboAsync(int id, SaveComboRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteComboAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ToggleComboStatusAsync(int id, CancellationToken cancellationToken = default);
}
