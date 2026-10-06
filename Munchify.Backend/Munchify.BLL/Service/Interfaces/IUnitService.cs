using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Units;

namespace Munchify.BLL.Service.Interfaces;

public interface IUnitService
{
    Task<ApiResponse<PagedResult<UnitDto>>> GetUnitsAsync(string searchTerm = "", bool? isActive = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<UnitDto>> GetUnitByIdAsync(int id);
    Task<ApiResponse<UnitDto>> CreateUnitAsync(SaveUnitRequest request);
    Task<ApiResponse<UnitDto>> UpdateUnitAsync(int id, SaveUnitRequest request);
    Task<ApiResponse<bool>> DeleteUnitAsync(int id);
}
