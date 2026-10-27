using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Auth;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Users;

namespace Munchify.BLL.Service.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
