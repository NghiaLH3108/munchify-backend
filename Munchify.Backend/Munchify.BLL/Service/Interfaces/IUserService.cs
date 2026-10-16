using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Users;

namespace Munchify.BLL.Service.Interfaces;

public interface IUserService
{
    Task<ApiResponse<PagedResult<UserDto>>> GetStaffListAsync(string? role, bool? isActive, string? searchKeyword, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetStaffByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> CreateStaffAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateStaffAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> ToggleStaffStatusAsync(int id, CancellationToken cancellationToken = default);
}
