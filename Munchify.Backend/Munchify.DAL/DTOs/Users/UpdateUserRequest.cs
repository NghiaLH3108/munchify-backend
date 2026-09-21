using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Users;

public class UpdateUserRequest
{
    [Required(ErrorMessage = "Họ và tên không được để trống")]
    [StringLength(100, ErrorMessage = "Họ và tên tối đa 100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Role không được để trống")]
    public string Role { get; set; } = "STAFF";
}
