using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Auth;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Users;

namespace Munchify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/login - Đăng nhập tài khoản, trả về JWT Token và thông tin User
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<LoginResponse>.ErrorResult("Dữ liệu đầu vào không hợp lệ"));
        }

        var result = await _authService.LoginAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// GET /api/auth/me - Lấy thông tin tài khoản hiện tại từ JWT Token
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe(CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResult("Phiên đăng nhập không hợp lệ hoặc đã hết hạn"));
        }

        var result = await _authService.GetCurrentUserAsync(userId, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// POST /api/auth/forgot-password - Gửi mã OTP khôi phục mật khẩu qua email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<string>>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Email không hợp lệ"));
        }

        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// POST /api/auth/reset-password - Đặt lại mật khẩu với mã OTP
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<string>.ErrorResult("Dữ liệu đặt lại mật khẩu không hợp lệ"));
        }

        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        if (!result.Success)
        {
            if (result.StatusCode == 404)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result);
    }
}
