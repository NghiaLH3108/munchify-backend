using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;

namespace Munchify.API.Controllers;

[ApiController]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("api/stock-movements")]
    // [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<StockMovementDto>>>> GetStockMovements(
        [FromQuery] int? productId = null,
        [FromQuery] string? movementType = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _inventoryService.GetStockMovementsAsync(productId, movementType, from, to, page, pageSize);
        return Ok(result);
    }

    [HttpPost("api/stock-movements")]
    public async Task<ActionResult<ApiResponse<StockMovementDto>>> AdjustStock([FromBody] AdjustStockRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<StockMovementDto>.ErrorResult("Dữ liệu không hợp lệ"));
        }

        // Mock userId for now since authentication is not configured yet.
        int userId = 1; 
        
        var result = await _inventoryService.AdjustStockAsync(request, userId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
