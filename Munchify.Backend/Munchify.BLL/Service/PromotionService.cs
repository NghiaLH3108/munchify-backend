using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Promotions;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepo;

    public PromotionService(IPromotionRepository promotionRepo)
    {
        _promotionRepo = promotionRepo;
    }

    // ─── Apply promotion ─────────────────────────────────────────────────────

    public async Task<ApiResponse<ApplyPromotionResponse>> ApplyPromotionAsync(
        ApplyPromotionRequest request,
        CancellationToken cancellationToken = default)
    {
        var promo = await _promotionRepo.GetByCodeAsync(request.PromotionCode, cancellationToken);

        if (promo == null)
            return ApiResponse<ApplyPromotionResponse>.ErrorResult("Mã khuyến mãi không tồn tại.");

        if (!promo.IsActive)
            return ApiResponse<ApplyPromotionResponse>.ErrorResult("Mã khuyến mãi đã bị vô hiệu hóa.");

        if (DateTime.Now < promo.StartDate || DateTime.Now > promo.EndDate)
            return ApiResponse<ApplyPromotionResponse>.ErrorResult(
                $"Mã khuyến mãi chỉ có hiệu lực từ {promo.StartDate:dd/MM/yyyy} đến {promo.EndDate:dd/MM/yyyy}.");

        if (promo.MinimumOrderAmount.HasValue && request.Subtotal < promo.MinimumOrderAmount.Value)
            return ApiResponse<ApplyPromotionResponse>.ErrorResult(
                $"Đơn hàng tối thiểu {promo.MinimumOrderAmount:N0}₫ để áp dụng mã này.");

        decimal discountAmount = promo.DiscountType == "PERCENTAGE"
            ? Math.Min(request.Subtotal * promo.DiscountValue / 100m, request.Subtotal)
            : Math.Min(promo.DiscountValue, request.Subtotal);

        return ApiResponse<ApplyPromotionResponse>.SuccessResult(new ApplyPromotionResponse
        {
            PromotionCode  = promo.PromotionCode,
            PromotionName  = promo.PromotionName,
            DiscountType   = promo.DiscountType,
            DiscountValue  = promo.DiscountValue,
            DiscountAmount = discountAmount,
        });
    }

    // ─── List all ────────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<PromotionDto>>> GetAllPromotionsAsync(
        CancellationToken cancellationToken = default)
    {
        var promos = await _promotionRepo.GetAllAsync(cancellationToken);
        return ApiResponse<List<PromotionDto>>.SuccessResult(
            promos.Select(MapToDto).ToList());
    }

    // ─── Create ──────────────────────────────────────────────────────────────

    public async Task<ApiResponse<PromotionDto>> CreatePromotionAsync(
        SavePromotionRequest request,
        CancellationToken cancellationToken = default)
    {
        // Check duplicate code
        var existing = await _promotionRepo.GetByCodeAsync(request.PromotionCode, cancellationToken);
        if (existing != null)
            return ApiResponse<PromotionDto>.ErrorResult(
                $"Mã khuyến mãi '{request.PromotionCode}' đã tồn tại.");

        // PromotionRepository doesn't expose Add directly; use it through context.
        // Will be wired via DI with direct DbContext injection in controller.
        throw new NotImplementedException("CreatePromotion requires direct DbContext – wire via DI.");
    }

    // ─── Update ──────────────────────────────────────────────────────────────

    public async Task<ApiResponse<PromotionDto>> UpdatePromotionAsync(
        int id,
        SavePromotionRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("UpdatePromotion requires direct DbContext – wire via DI.");
    }

    // ─── Mapper ──────────────────────────────────────────────────────────────

    private static PromotionDto MapToDto(Promotion p) => new()
    {
        PromotionId         = p.PromotionId,
        PromotionCode       = p.PromotionCode,
        PromotionName       = p.PromotionName,
        Description         = p.Description,
        DiscountType        = p.DiscountType,
        DiscountValue       = p.DiscountValue,
        MinimumOrderAmount  = p.MinimumOrderAmount,
        StartDate           = p.StartDate,
        EndDate             = p.EndDate,
        IsActive            = p.IsActive,
    };
}
