using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Suppliers;

public class SaveSupplierRequest
{
    [Required(ErrorMessage = "Tên nhà cung cấp không được để trống")]
    [MaxLength(200, ErrorMessage = "Tên nhà cung cấp tối đa 200 ký tự")]
    public string SupplierName { get; set; } = null!;

    [MaxLength(200, ErrorMessage = "Tên người đại diện tối đa 200 ký tự")]
    public string? ContactName { get; set; }

    [MaxLength(200, ErrorMessage = "Số điện thoại tối đa 200 ký tự")]
    public string? Phone { get; set; }

    [MaxLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
    public string? Address { get; set; }

    public bool? IsActive { get; set; }

    public List<int>? CategoryIds { get; set; }
}
