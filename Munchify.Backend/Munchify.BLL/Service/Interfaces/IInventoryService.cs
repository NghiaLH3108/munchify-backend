using System;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;

namespace Munchify.BLL.Service.Interfaces;

public interface IInventoryService
{
    Task<ApiResponse<PagedResult<StockMovementDto>>> GetStockMovementsAsync(
        int? productId = null, 
        string? movementType = null, 
        DateTime? from = null, 
        DateTime? to = null, 
        int page = 1, 
        int pageSize = 20);

    Task<ApiResponse<StockMovementDto>> AdjustStockAsync(AdjustStockRequest request, int userId = 1);

    Task<ApiResponse<System.Collections.Generic.IEnumerable<ForecastItemDto>>> GetForecastingAsync(int daysHistory = 30);
    Task<ApiResponse<bool>> GenerateDraftImportsFromForecastAsync(System.Collections.Generic.List<ForecastItemDto> items, int userId = 1);
}
