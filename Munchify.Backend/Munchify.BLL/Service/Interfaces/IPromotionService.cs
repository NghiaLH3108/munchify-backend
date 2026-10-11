using System.Threading.Tasks;
using Munchify.DAL.DTOs.Promotions;
using Munchify.DAL.DTOs.Common; // Assuming PaginatedList or similar is here, or I can just use a common response object

namespace Munchify.BLL.Service.Interfaces;

public interface IPromotionService
{
    // Keeping it generic or simple for now depending on ApiResponse
    Task<ApiResponse<PagedResult<PromotionDto>>> GetAllPromotionsAsync(PromotionFilterParams filterParams);
    Task<ApiResponse<PromotionDetailDto>> GetPromotionByIdAsync(int id);
    Task<ApiResponse<PromotionDetailDto>> CreatePromotionAsync(CreatePromotionRequest request);
    Task<ApiResponse<PromotionDetailDto>> UpdatePromotionAsync(int id, UpdatePromotionRequest request);
    Task<ApiResponse<bool>> TogglePromotionStatusAsync(int id);
}
