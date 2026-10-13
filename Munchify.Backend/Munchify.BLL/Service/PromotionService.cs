using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Promotions;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class PromotionService : IPromotionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PromotionService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<PromotionDto>>> GetAllPromotionsAsync(PromotionFilterParams filterParams)
    {
        var allPromotions = await _unitOfWork.Promotions.GetAllAsync();
        var query = allPromotions.AsQueryable();

        if (!string.IsNullOrEmpty(filterParams.SearchTerm))
        {
            var searchTerm = filterParams.SearchTerm.ToLower();
            query = query.Where(p => 
                p.PromotionName.ToLower().Contains(searchTerm) || 
                p.PromotionCode.ToLower().Contains(searchTerm));
        }

        if (filterParams.IsActive.HasValue)
        {
            query = query.Where(p => p.IsActive == filterParams.IsActive.Value);
        }

        int totalItems = query.Count();
        var pagedList = query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .ToList();

        var dtoList = _mapper.Map<List<PromotionDto>>(pagedList);

        var result = new PagedResult<PromotionDto>
        {
            Items = dtoList,
            TotalCount = totalItems,
            PageNumber = filterParams.PageNumber,
            PageSize = filterParams.PageSize
        };

        return ApiResponse<PagedResult<PromotionDto>>.SuccessResult(result, "Promotions retrieved successfully");
    }

    public async Task<ApiResponse<PromotionDetailDto>> GetPromotionByIdAsync(int id)
    {
        var promotion = await _unitOfWork.Promotions.GetPromotionWithDetailsAsync(id);
        if (promotion == null)
            return ApiResponse<PromotionDetailDto>.ErrorResult("Promotion not found");

        var detailDto = _mapper.Map<PromotionDetailDto>(promotion);
        return ApiResponse<PromotionDetailDto>.SuccessResult(detailDto);
    }

    public async Task<ApiResponse<PromotionDetailDto>> CreatePromotionAsync(CreatePromotionRequest request)
    {
        if (request.StartDate.Date < DateTime.Now.Date)
            return ApiResponse<PromotionDetailDto>.ErrorResult("Ngày bắt đầu không được ở trong quá khứ");

        if (request.EndDate <= request.StartDate)
            return ApiResponse<PromotionDetailDto>.ErrorResult("Ngày kết thúc phải sau ngày bắt đầu");

        var valError = ValidatePromotionRules(request.DiscountType, request.DiscountValue, request.MinimumOrderAmount);
        if (valError != null)
            return ApiResponse<PromotionDetailDto>.ErrorResult(valError);

        string code;
        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
        {
            code = request.PromotionCode.Trim().ToUpper();
            bool exists = await _unitOfWork.Promotions.ExistsCodeAsync(code);
            if (exists)
            {
                return ApiResponse<PromotionDetailDto>.ErrorResult($"Mã khuyến mãi '{code}' đã tồn tại trong hệ thống");
            }
        }
        else
        {
            do
            {
                code = GenerateRandomCode(8);
            } while (await _unitOfWork.Promotions.ExistsCodeAsync(code));
        }

        var promotion = new Promotion
        {
            PromotionCode = code,
            PromotionName = request.PromotionName.Trim(),
            Description = request.Description?.Trim(),
            DiscountType = request.DiscountType,
            DiscountValue = request.DiscountValue,
            MinimumOrderAmount = request.MinimumOrderAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        if (request.ProductIds != null && request.ProductIds.Any())
        {
            foreach (var productId in request.ProductIds.Distinct())
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product != null)
                {
                    promotion.Products.Add(product);
                }
            }
        }

        await _unitOfWork.Promotions.AddAsync(promotion);
        await _unitOfWork.CompleteAsync();

        var createdDetail = _mapper.Map<PromotionDetailDto>(promotion);
        return ApiResponse<PromotionDetailDto>.SuccessResult(createdDetail, "Tạo chương trình khuyến mãi thành công");
    }

    public async Task<ApiResponse<PromotionDetailDto>> UpdatePromotionAsync(int id, UpdatePromotionRequest request)
    {
        var promotion = await _unitOfWork.Promotions.GetPromotionWithDetailsAsync(id);
        if (promotion == null)
            return ApiResponse<PromotionDetailDto>.ErrorResult("Không tìm thấy chương trình khuyến mãi");

        if (request.EndDate <= request.StartDate)
            return ApiResponse<PromotionDetailDto>.ErrorResult("Ngày kết thúc phải sau ngày bắt đầu");

        var valError = ValidatePromotionRules(request.DiscountType, request.DiscountValue, request.MinimumOrderAmount);
        if (valError != null)
            return ApiResponse<PromotionDetailDto>.ErrorResult(valError);

        promotion.PromotionName = request.PromotionName.Trim();
        promotion.Description = request.Description?.Trim();
        promotion.DiscountType = request.DiscountType;
        promotion.DiscountValue = request.DiscountValue;
        promotion.MinimumOrderAmount = request.MinimumOrderAmount;
        promotion.StartDate = request.StartDate;
        promotion.EndDate = request.EndDate;
        promotion.IsActive = request.IsActive;
        promotion.UpdatedAt = DateTime.Now;

        promotion.Products.Clear();
        if (request.ProductIds != null && request.ProductIds.Any())
        {
            foreach (var productId in request.ProductIds.Distinct())
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product != null)
                {
                    promotion.Products.Add(product);
                }
            }
        }

        _unitOfWork.Promotions.Update(promotion);
        await _unitOfWork.CompleteAsync();

        var updatedDetail = _mapper.Map<PromotionDetailDto>(promotion);
        return ApiResponse<PromotionDetailDto>.SuccessResult(updatedDetail, "Cập nhật chương trình khuyến mãi thành công");
    }

    private static string? ValidatePromotionRules(string discountType, decimal discountValue, decimal? minOrder)
    {
        if (discountType != "PERCENTAGE" && discountType != "FIXED_AMOUNT")
        {
            return "Loại giảm giá không hợp lệ. Chỉ chấp nhận PERCENTAGE hoặc FIXED_AMOUNT";
        }

        if (discountType == "PERCENTAGE")
        {
            if (discountValue <= 0 || discountValue > 100)
            {
                return "Giá trị giảm giá theo phần trăm phải nằm trong khoảng từ 1% đến 100%";
            }
        }
        else // FIXED_AMOUNT
        {
            if (discountValue <= 0)
            {
                return "Số tiền giảm giá phải lớn hơn 0";
            }
        }

        if (minOrder.HasValue && minOrder.Value < 0)
        {
            return "Giá trị đơn hàng tối thiểu không được là số âm";
        }

        return null;
    }

    public async Task<ApiResponse<bool>> TogglePromotionStatusAsync(int id)
    {
        var promotion = await _unitOfWork.Promotions.GetByIdAsync(id);
        if (promotion == null)
            return ApiResponse<bool>.ErrorResult("Promotion not found");

        promotion.IsActive = !promotion.IsActive;
        promotion.UpdatedAt = DateTime.Now;

        _unitOfWork.Promotions.Update(promotion);
        await _unitOfWork.CompleteAsync();

        return ApiResponse<bool>.SuccessResult(promotion.IsActive, $"Promotion status changed to {(promotion.IsActive ? "Active" : "Paused")}");
    }

    private string GenerateRandomCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
