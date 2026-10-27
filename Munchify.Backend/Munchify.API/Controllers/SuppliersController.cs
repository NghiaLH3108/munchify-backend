using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Suppliers;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// View Supplier List - Lấy danh sách nhà cung cấp (hỗ trợ tìm kiếm & lọc theo trạng thái)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSuppliers([FromQuery] string? search, [FromQuery] bool? isActive)
    {
        var response = await _supplierService.GetSuppliersAsync(search, isActive);
        return Ok(response);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của 1 nhà cung cấp theo Id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSupplierById(int id)
    {
        var response = await _supplierService.GetSupplierByIdAsync(id);
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    /// <summary>
    /// Add Supplier - Thêm nhà cung cấp mới
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateSupplier([FromBody] SaveSupplierRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<SupplierDto>.ErrorResult("Dữ liệu đầu vào không hợp lệ."));
        }

        var response = await _supplierService.CreateSupplierAsync(request);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return CreatedAtAction(nameof(GetSupplierById), new { id = response.Data?.SupplierId }, response);
    }

    /// <summary>
    /// Update Supplier - Cập nhật thông tin nhà cung cấp
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] SaveSupplierRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<SupplierDto>.ErrorResult("Dữ liệu đầu vào không hợp lệ."));
        }

        var response = await _supplierService.UpdateSupplierAsync(id, request);
        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Delete Supplier - Xóa mềm nhà cung cấp (IsActive = false)
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var response = await _supplierService.DeleteSupplierAsync(id);
        if (!response.Success)
        {
            return NotFound(response);
        }

        return Ok(response);
    }
}
