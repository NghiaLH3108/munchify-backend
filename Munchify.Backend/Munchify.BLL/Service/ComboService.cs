using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Combos;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class ComboService : IComboService
{
    private readonly IUnitOfWork _unitOfWork;

    // CategoryId=5 (Goi Combo Tiet Kiem), UnitId=2 (Goi) - co dinh, admin khong can chon
    private const int ComboDefaultCategoryId = 5;
    private const int ComboDefaultUnitId = 2;

    public ComboService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResult<ProductDetailDto>>> GetCombosAsync(
        ComboFilterParams filter, CancellationToken cancellationToken = default)
    {
        if (filter.Page < 1) filter.Page = 1;
        if (filter.PageSize < 1) filter.PageSize = 10;
        if (filter.PageSize > 100) filter.PageSize = 100;

        var (items, total) = await _unitOfWork.Products.GetCombosPagedAsync(filter, cancellationToken);

        var result = new PagedResult<ProductDetailDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = total,
            PageNumber = filter.Page,
            PageSize = filter.PageSize
        };
        return ApiResponse<PagedResult<ProductDetailDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<ProductDetailDto>> GetComboByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var combo = await _unitOfWork.Products.GetComboByIdAsync(id, false, cancellationToken);
        if (combo == null)
            return ApiResponse<ProductDetailDto>.NotFoundResult("Khong tim thay combo.");

        return ApiResponse<ProductDetailDto>.SuccessResult(MapToDto(combo));
    }

    public async Task<ApiResponse<ProductDetailDto>> CreateComboAsync(SaveComboRequest request, CancellationToken cancellationToken = default)
    {
        var error = await ValidateAsync(request, null, cancellationToken);
        if (error != null)
            return ApiResponse<ProductDetailDto>.ErrorResult(error);

        var code = string.IsNullOrWhiteSpace(request.ProductCode)
            ? await GenerateComboCodeAsync(cancellationToken)
            : request.ProductCode.Trim();

        var now = DateTime.Now;
        var combo = new Product
        {
            ProductCode = code,
            ProductName = request.ProductName.Trim(),
            Description = request.Description?.Trim(),
            Price = request.Price,
            CategoryId = ComboDefaultCategoryId,
            UnitId = ComboDefaultUnitId,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            IsCombo = true,
            IsActive = request.IsActive,
            StockQuantity = 0,
            MinStockLevel = 0,
            CreatedAt = now,
            UpdatedAt = now,
            ProductComboParentProducts = request.Items.Select(i => new ProductCombo
            {
                ComponentProductId = i.ComponentProductId,
                Quantity = i.Quantity
            }).ToList()
        };

        await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            await _unitOfWork.Products.AddAsync(combo);
            await _unitOfWork.CompleteAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        var created = await _unitOfWork.Products.GetComboByIdAsync(combo.ProductId, false, cancellationToken);
        return ApiResponse<ProductDetailDto>.SuccessResult(MapToDto(created!), "Tao combo thanh cong.");
    }

    public async Task<ApiResponse<ProductDetailDto>> UpdateComboAsync(int id, SaveComboRequest request, CancellationToken cancellationToken = default)
    {
        var combo = await _unitOfWork.Products.GetComboByIdAsync(id, true, cancellationToken);
        if (combo == null)
            return ApiResponse<ProductDetailDto>.NotFoundResult("Khong tim thay combo.");

        var error = await ValidateAsync(request, id, cancellationToken);
        if (error != null)
            return ApiResponse<ProductDetailDto>.ErrorResult(error);

        await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!string.IsNullOrWhiteSpace(request.ProductCode))
                combo.ProductCode = request.ProductCode.Trim();
            combo.ProductName = request.ProductName.Trim();
            combo.Description = request.Description?.Trim();
            combo.Price = request.Price;
            // CategoryId va UnitId giu nguyen co dinh - khong cap nhat tu request
            combo.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            combo.IsActive = request.IsActive;
            combo.UpdatedAt = DateTime.Now;

            _unitOfWork.Products.RemoveComboItems(combo.ProductComboParentProducts.ToList());
            await _unitOfWork.Products.AddComboItemsAsync(
                request.Items.Select(i => new ProductCombo
                {
                    ParentProductId = id,
                    ComponentProductId = i.ComponentProductId,
                    Quantity = i.Quantity
                }), cancellationToken);

            await _unitOfWork.CompleteAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        var updated = await _unitOfWork.Products.GetComboByIdAsync(id, false, cancellationToken);
        return ApiResponse<ProductDetailDto>.SuccessResult(MapToDto(updated!), "Cap nhat combo thanh cong.");
    }

    public async Task<ApiResponse<bool>> DeleteComboAsync(int id, CancellationToken cancellationToken = default)
    {
        var combo = await _unitOfWork.Products.GetComboByIdAsync(id, true, cancellationToken);
        if (combo == null)
            return ApiResponse<bool>.NotFoundResult("Khong tim thay combo.");

        var hasHistory = await _unitOfWork.Products.HasHistoryAsync(id, cancellationToken);

        await using var tx = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (hasHistory)
            {
                combo.IsActive = false;
                combo.UpdatedAt = DateTime.Now;
            }
            else
            {
                combo.Promotions.Clear();
                _unitOfWork.Products.RemoveComboItems(combo.ProductComboParentProducts.ToList());
                _unitOfWork.Products.Remove(combo);
            }

            await _unitOfWork.CompleteAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch
        {
            await tx.RollbackAsync(cancellationToken);
            throw;
        }

        return ApiResponse<bool>.SuccessResult(
            true,
            hasHistory
                ? "Combo da co lich su giao dich nen duoc chuyen sang trang thai ngung ban."
                : "Xoa combo thanh cong.");
    }

    public async Task<ApiResponse<bool>> ToggleComboStatusAsync(int id, CancellationToken cancellationToken = default)
    {
        var combo = await _unitOfWork.Products.GetComboByIdAsync(id, true, cancellationToken);
        if (combo == null)
            return ApiResponse<bool>.NotFoundResult("Không tìm thấy combo.");

        if (!combo.IsActive)
        {
            var inactive = combo.ProductComboParentProducts
                .FirstOrDefault(c => c.ComponentProduct != null && !c.ComponentProduct.IsActive);
            if (inactive != null)
            {
                return ApiResponse<bool>.ErrorResult(
                    $"Không thể bật combo vì món thành phần '{inactive.ComponentProduct.ProductName}' đang ngừng bán.");
            }
        }

        combo.IsActive = !combo.IsActive;
        combo.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CompleteAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(
            combo.IsActive,
            combo.IsActive ? "Đã bật trạng thái combo thành công." : "Đã tạm dừng combo thành công.");
    }

    // helpers

    private async Task<string?> ValidateAsync(SaveComboRequest request, int? excludeId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ProductName))
            return "Ten combo khong duoc de trong.";
        if (request.Price <= 0)
            return "Gia ban combo phai lon hon 0.";
        if (request.Items == null || request.Items.Count == 0)
            return "Combo phai co it nhat mot san pham thanh phan.";
        if (request.Items.Any(i => i.Quantity <= 0))
            return "So luong san pham thanh phan phai lon hon 0.";
        if (request.Items.GroupBy(i => i.ComponentProductId).Any(g => g.Count() > 1))
            return "San pham thanh phan bi trung lap.";
        if (excludeId.HasValue && request.Items.Any(i => i.ComponentProductId == excludeId.Value))
            return "Combo khong the chua chinh no.";

        if (!string.IsNullOrWhiteSpace(request.ProductCode) &&
            await _unitOfWork.Products.ProductCodeExistsAsync(request.ProductCode.Trim(), excludeId, ct))
            return "Ma san pham da ton tai.";

        var ids = request.Items.Select(i => i.ComponentProductId).ToList();
        var components = await _unitOfWork.Products.GetProductsByIdsAsync(ids, ct);
        if (components.Count != ids.Count)
            return "Co san pham thanh phan khong ton tai.";
        if (components.Any(c => c.IsCombo))
            return "San pham thanh phan phai la san pham don, khong the la combo.";
        if (components.Any(c => !c.IsActive))
            return "San pham thanh phan phai dang hoat dong.";

        return null;
    }

    private async Task<string> GenerateComboCodeAsync(CancellationToken ct)
    {
        string code;
        do
        {
            code = $"CB-{Random.Shared.Next(100000, 999999)}";
        } while (await _unitOfWork.Products.ProductCodeExistsAsync(code, null, ct));
        return code;
    }

    private static ProductDetailDto MapToDto(Product p) => new()
    {
        ProductId = p.ProductId,
        ProductCode = p.ProductCode,
        ProductName = p.ProductName,
        Price = p.Price,
        StockQuantity = p.StockQuantity,
        CategoryId = p.CategoryId,
        CategoryName = p.Category?.CategoryName ?? string.Empty,
        UnitId = p.UnitId,
        UnitName = p.Unit?.UnitName ?? string.Empty,
        ImageUrl = p.ImageUrl,
        IsCombo = p.IsCombo,
        IsActive = p.IsActive,
        Description = p.Description,
        MinStockLevel = p.MinStockLevel,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        ComboItems = p.ProductComboParentProducts.Select(pc => new ComboItemDto
        {
            ComponentProductId = pc.ComponentProductId,
            ComponentProductName = pc.ComponentProduct?.ProductName ?? string.Empty,
            Quantity = pc.Quantity,
            UnitPrice = pc.ComponentProduct?.Price ?? 0,
            UnitName = pc.ComponentProduct?.Unit?.UnitName ?? string.Empty,
            ComponentStockQuantity = pc.ComponentProduct?.StockQuantity ?? 0
        }).ToList(),
        // BasePrice = tong gia goc cac thanh phan (SUM component.Price x quantity)
        // Frontend dung de tinh % giam gia
        BasePrice = p.ProductComboParentProducts
            .Sum(pc => (pc.ComponentProduct?.Price ?? 0) * pc.Quantity)
    };
}