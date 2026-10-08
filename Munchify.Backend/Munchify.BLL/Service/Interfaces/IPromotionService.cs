using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Promotions;

namespace Munchify.BLL.Service.Interfaces;

public interface IPromotionService
{
    Task<ApiResponse<ApplyPromotionResponse>> ApplyPromotionAsync(
        ApplyPromotionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<List<PromotionDto>>> GetAllPromotionsAsync(
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PromotionDto>> CreatePromotionAsync(
        SavePromotionRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PromotionDto>> UpdatePromotionAsync(
        int id,
        SavePromotionRequest request,
        CancellationToken cancellationToken = default);
}
