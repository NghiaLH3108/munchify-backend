using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Orders;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// POST /api/orders – Tạo đơn hàng mới (Checkout).
    /// Lấy staffId từ JWT claims (sub / nameidentifier).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<OrderResponse>.ErrorResult("Dữ liệu không hợp lệ."));

        // Lấy staffId từ JWT – fallback về 1 khi chưa có auth
        var staffIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("sub");
        int staffId = int.TryParse(staffIdClaim, out var id) ? id : 1;

        var response = await _orderService.CreateOrderAsync(request, staffId, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// GET /api/orders – Lịch sử đơn hàng (phân trang).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderSummaryDto>>>> GetOrders(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var response = await _orderService.GetOrdersAsync(page, pageSize, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
