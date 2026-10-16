using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Promotions;

namespace Munchify.BLL.Service.Interfaces;

public interface IPromotionService
{
    Task<ApiResponse<PagedResult<PromotionDto>>> GetAllPromotionsAsync(PromotionFilterParams filterParams);
    Task<ApiResponse<List<PromotionDto>>> GetAllPromotionsListAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PromotionDetailDto>> GetPromotionByIdAsync(int id);
    Task<ApiResponse<PromotionDetailDto>> CreatePromotionAsync(CreatePromotionRequest request);
    Task<ApiResponse<PromotionDetailDto>> UpdatePromotionAsync(int id, UpdatePromotionRequest request);
    Task<ApiResponse<bool>> TogglePromotionStatusAsync(int id);
    Task<ApiResponse<ApplyPromotionResponse>> ApplyPromotionAsync(
        ApplyPromotionRequest request,
        CancellationToken cancellationToken = default);
}
