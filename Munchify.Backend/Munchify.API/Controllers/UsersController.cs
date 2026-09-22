using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Users;

namespace Munchify.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// GET /api/users - Lấy danh sách nhân viên (hỗ trợ phân trang và lọc role/status)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> GetStaffList(
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetStaffListAsync(role, isActive, page, pageSize, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/users/{id} - Lấy chi tiết tài khoản nhân viên theo ID
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetStaffById(int id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetStaffByIdAsync(id, cancellationToken);
        if (!result.Success)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// POST /api/users - Tạo tài khoản nhân viên mới (Mật khẩu được mã hóa BCrypt)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateStaff([FromBody] CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResult("Dữ liệu không hợp lệ"));
        }

        var result = await _userService.CreateStaffAsync(request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetStaffById), new { id = result.Data?.UserId }, result);
    }

    /// <summary>
    /// PUT /api/users/{id} - Cập nhật thông tin nhân viên
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateStaff(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResult("Dữ liệu không hợp lệ"));
        }

        var result = await _userService.UpdateStaffAsync(id, request, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// PATCH /api/users/{id}/status - Khóa / Mở khóa tài khoản nhân viên (Đổi cờ IsActive)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleStaffStatus(int id, CancellationToken cancellationToken = default)
    {
        var result = await _userService.ToggleStaffStatusAsync(id, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
