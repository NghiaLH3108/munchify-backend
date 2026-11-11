using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Suppliers;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;

    public SupplierService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IEnumerable<SupplierDto>>> GetSuppliersAsync(string? search = null, bool? isActive = null)
    {
        var suppliers = await _unitOfWork.Suppliers.GetAllWithCategoriesAsync();
        var query = suppliers.AsQueryable();

        // Xử lý Business Logic lọc tìm kiếm
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim();
            query = query.Where(s => s.SupplierName.Contains(searchLower, StringComparison.OrdinalIgnoreCase)
                                  || (s.ContactName != null && s.ContactName.Contains(searchLower, StringComparison.OrdinalIgnoreCase))
                                  || (s.Phone != null && s.Phone.Contains(searchLower, StringComparison.OrdinalIgnoreCase)));
        }

        // Xử lý Business Logic lọc theo trạng thái
        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var dtos = query.OrderByDescending(s => s.CreatedAt).Select(MapToDto).ToList();
        return ApiResponse<IEnumerable<SupplierDto>>.SuccessResult(dtos, "Lấy danh sách nhà cung cấp thành công.");
    }

    public async Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdWithCategoriesAsync(id);
        if (supplier == null)
        {
            return ApiResponse<SupplierDto>.ErrorResult("Không tìm thấy nhà cung cấp.");
        }

        return ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier), "Lấy thông tin nhà cung cấp thành công.");
    }

    public async Task<ApiResponse<SupplierDto>> CreateSupplierAsync(SaveSupplierRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SupplierName))
        {
            return ApiResponse<SupplierDto>.ErrorResult("Tên nhà cung cấp không được để trống.");
        }

        var supplier = new Supplier
        {
            SupplierName = request.SupplierName.Trim(),
            ContactName = request.ContactName?.Trim(),
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow
        };

        // Gán danh mục liên kết nếu có
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var matchedCategories = categories.Where(c => request.CategoryIds.Contains(c.CategoryId)).ToList();
            foreach (var cat in matchedCategories)
            {
                supplier.Categories.Add(cat);
            }
        }

        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.CompleteAsync();

        return ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier), "Thêm nhà cung cấp mới thành công.");
    }

    public async Task<ApiResponse<SupplierDto>> UpdateSupplierAsync(int id, SaveSupplierRequest request)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdWithCategoriesAsync(id);
        if (supplier == null)
        {
            return ApiResponse<SupplierDto>.ErrorResult("Không tìm thấy nhà cung cấp để cập nhật.");
        }

        if (string.IsNullOrWhiteSpace(request.SupplierName))
        {
            return ApiResponse<SupplierDto>.ErrorResult("Tên nhà cung cấp không được để trống.");
        }

        supplier.SupplierName = request.SupplierName.Trim();
        supplier.ContactName = request.ContactName?.Trim();
        supplier.Phone = request.Phone?.Trim();
        supplier.Address = request.Address?.Trim();
        
        if (request.IsActive.HasValue)
        {
            supplier.IsActive = request.IsActive.Value;
        }

        // Cập nhật danh mục liên kết nếu được cung cấp
        if (request.CategoryIds != null)
        {
            supplier.Categories.Clear();
            var categories = await _unitOfWork.Categories.GetAllAsync();
            var matchedCategories = categories.Where(c => request.CategoryIds.Contains(c.CategoryId)).ToList();
            foreach (var cat in matchedCategories)
            {
                supplier.Categories.Add(cat);
            }
        }

        await _unitOfWork.CompleteAsync();

        return ApiResponse<SupplierDto>.SuccessResult(MapToDto(supplier), "Cập nhật nhà cung cấp thành công.");
    }

    public async Task<ApiResponse<bool>> DeleteSupplierAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
        {
            return ApiResponse<bool>.ErrorResult("Không tìm thấy nhà cung cấp để xóa.");
        }

        // Thực hiện xóa mềm (Soft Delete) theo yêu cầu: Đặt IsActive = false
        supplier.IsActive = false;
        await _unitOfWork.CompleteAsync();

        return ApiResponse<bool>.SuccessResult(true, "Đã ngừng hợp tác và vô hiệu hóa nhà cung cấp (Xóa mềm thành công).");
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            ContactName = supplier.ContactName,
            Phone = supplier.Phone,
            Address = supplier.Address,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            CategoryIds = supplier.Categories.Select(c => c.CategoryId).ToList(),
            CategoryNames = supplier.Categories.Select(c => c.CategoryName).ToList()
        };
    }
}
