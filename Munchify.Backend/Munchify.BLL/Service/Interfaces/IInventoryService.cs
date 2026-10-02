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
}
