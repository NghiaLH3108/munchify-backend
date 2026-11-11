using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCrypt.Net;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Users;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ApiResponse<PagedResult<UserDto>>> GetStaffListAsync(string? role, bool? isActive, string? searchKeyword, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var users = await _userRepository.GetAllStaffAsync(role, isActive, searchKeyword, page, pageSize, cancellationToken);
        var totalCount = await _userRepository.GetTotalStaffCountAsync(role, isActive, searchKeyword, cancellationToken);

        var dtos = users.Select(MapToDto);

        var pagedResult = new PagedResult<UserDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<UserDto>>.SuccessResult(pagedResult, "Lấy danh sách nhân viên thành công");
    }

    public async Task<ApiResponse<UserDto>> GetStaffByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResult($"Không tìm thấy tài khoản nhân viên có ID {id}");
        }

        return ApiResponse<UserDto>.SuccessResult(MapToDto(user));
    }

    public async Task<ApiResponse<UserDto>> CreateStaffAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Check username uniqueness
        if (await _userRepository.ExistsUsernameAsync(request.Username, cancellationToken))
        {
            return ApiResponse<UserDto>.ErrorResult($"Tên đăng nhập '{request.Username}' đã tồn tại");
        }

        // Check phone uniqueness
        if (!string.IsNullOrWhiteSpace(request.Phone) && await _userRepository.ExistsPhoneAsync(request.Phone, null, cancellationToken))
        {
            return ApiResponse<UserDto>.ErrorResult($"Số điện thoại '{request.Phone}' đã được sử dụng");
        }

        // Check email uniqueness
        if (!string.IsNullOrWhiteSpace(request.Email) && await _userRepository.ExistsEmailAsync(request.Email, null, cancellationToken))
        {
            return ApiResponse<UserDto>.ErrorResult($"Email '{request.Email}' đã được sử dụng");
        }

        // Validate Role
        var role = request.Role?.ToUpperInvariant() ?? "STAFF";
        if (role != "ADMIN" && role != "STAFF")
        {
            return ApiResponse<UserDto>.ErrorResult("Role chỉ chấp nhận 'ADMIN' hoặc 'STAFF'");
        }

        // Hash password with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var newUser = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = passwordHash,
            FullName = request.FullName.Trim(),
            Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(newUser, cancellationToken);
        return ApiResponse<UserDto>.SuccessResult(MapToDto(createdUser), "Tạo tài khoản nhân viên thành công");
    }

    public async Task<ApiResponse<UserDto>> UpdateStaffAsync(int id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return ApiResponse<UserDto>.ErrorResult($"Không tìm thấy tài khoản nhân viên có ID {id}");
        }

        // Check phone uniqueness
        if (!string.IsNullOrWhiteSpace(request.Phone) && await _userRepository.ExistsPhoneAsync(request.Phone, id, cancellationToken))
        {
            return ApiResponse<UserDto>.ErrorResult($"Số điện thoại '{request.Phone}' đã được sử dụng bởi tài khoản khác");
        }

        // Check email uniqueness
        if (!string.IsNullOrWhiteSpace(request.Email) && await _userRepository.ExistsEmailAsync(request.Email, id, cancellationToken))
        {
            return ApiResponse<UserDto>.ErrorResult($"Email '{request.Email}' đã được sử dụng bởi tài khoản khác");
        }

        var role = request.Role?.ToUpperInvariant() ?? "STAFF";
        if (role != "ADMIN" && role != "STAFF")
        {
            return ApiResponse<UserDto>.ErrorResult("Role chỉ chấp nhận 'ADMIN' hoặc 'STAFF'");
        }

        user.FullName = request.FullName.Trim();
        user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        user.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        user.Role = role;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return ApiResponse<UserDto>.SuccessResult(MapToDto(user), "Cập nhật thông tin nhân viên thành công");
    }

    public async Task<ApiResponse<bool>> ToggleStaffStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
        {
            return ApiResponse<bool>.ErrorResult($"Không tìm thấy tài khoản nhân viên có ID {id}");
        }

        // Không cho phép khóa ADMIN nếu đây là ADMIN đang hoạt động duy nhất
        if (user.Role == "ADMIN" && user.IsActive)
        {
            var activeAdmins = await _userRepository.GetAllStaffAsync("ADMIN", true, null, 1, 10, cancellationToken);
            if (activeAdmins.Count() <= 1)
            {
                return ApiResponse<bool>.ErrorResult("Không thể khóa tài khoản Quản trị viên (ADMIN) duy nhất đang hoạt động trong hệ thống");
            }
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        var statusMessage = user.IsActive ? "Kích hoạt tài khoản nhân viên thành công" : "Khóa tài khoản nhân viên thành công";
        return ApiResponse<bool>.SuccessResult(user.IsActive, statusMessage);
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            FullName = user.FullName,
            Phone = user.Phone,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
