using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Munchify.API.Hubs;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;

namespace Munchify.API.Controllers;

[ApiController]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IHubContext<InventoryHub> _hubContext;
    private readonly IEmailService _emailService;
    private readonly IProductService _productService;

    public InventoryController(
        IInventoryService inventoryService, 
        IHubContext<InventoryHub> hubContext,
        IEmailService emailService,
        IProductService productService)
    {
        _inventoryService = inventoryService;
        _hubContext = hubContext;
        _emailService = emailService;
        _productService = productService;
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

        // Broadcast Real-time SignalR & Send Low Stock Alert Email
        if (result.Data != null && result.Data.NewQuantity <= 10 && request.QuantityChange < 0)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveLowStockWarning", 
                $"Cảnh báo: Sản phẩm có ID {request.ProductId} sắp cạn kho (chỉ còn {result.Data.NewQuantity}). AI đề xuất nhập thêm!");

            // Gửi email tự động cho Admin
            var prodRes = await _productService.GetProductDetailAsync(request.ProductId);
            if (prodRes.Success && prodRes.Data != null)
            {
                var prod = prodRes.Data;
                await _emailService.SendLowStockAlertAsync(
                    productName: prod.ProductName,
                    productCode: prod.ProductCode,
                    currentStock: result.Data.NewQuantity,
                    minStock: prod.MinStockLevel,
                    unitName: prod.UnitName
                );
            }
        }

        return Ok(result);
    }

    [HttpGet("api/inventory/forecasting")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ForecastItemDto>>>> GetForecasting([FromQuery] int daysHistory = 30)
    {
        var result = await _inventoryService.GetForecastingAsync(daysHistory);
        return Ok(result);
    }

    [HttpPost("api/stock-imports/generate-from-forecast")]
    public async Task<ActionResult<ApiResponse<bool>>> GenerateDraftImportsFromForecast([FromBody] List<ForecastItemDto> items)
    {
        if (items == null || items.Count == 0)
        {
            return BadRequest(ApiResponse<bool>.ErrorResult("Dữ liệu không hợp lệ"));
        }

        int userId = 1; // Mock user id
        var result = await _inventoryService.GenerateDraftImportsFromForecastAsync(items, userId);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
