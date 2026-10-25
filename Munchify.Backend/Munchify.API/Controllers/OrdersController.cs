using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service;
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
    /// Helper lấy userId từ JWT claims (NameIdentifier / sub), fallback an toàn về 1 nếu chưa auth hoặc claim không hợp lệ.
    /// </summary>
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                 ?? User.FindFirst("sub");

        if (claim != null && int.TryParse(claim.Value, out var userId) && userId > 0)
        {
            return userId;
        }

        return 1;
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

        int staffId = GetCurrentUserId();

        try
        {
            var response = await _orderService.CreateOrderAsync(request, staffId, cancellationToken);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ApiResponse<OrderResponse>.ErrorResult(ex.Message));
        }
        catch (NotFoundException ex)
        {
            return NotFound(ApiResponse<OrderResponse>.NotFoundResult(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderResponse>.ErrorResult($"Lỗi xử lý đơn hàng: {ex.Message}"));
        }
    }

    /// <summary>
    /// GET /api/orders – Danh sách đơn hàng có tìm kiếm và bộ lọc nâng cao (phân trang).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderSummaryDto>>>> GetOrders(
        [FromQuery] OrderFilterRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await _orderService.GetOrdersAsync(request, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// GET /api/orders/{id} – Chi tiết đơn hàng và các mặt hàng snapshot.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetOrderDetail(
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        var response = await _orderService.GetOrderDetailAsync(id, cancellationToken);

        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// GET /api/orders/vietqr – Sinh thông tin và hình ảnh mã VietQR chuẩn Napas 247 cho thanh toán tại quầy POS.
    /// </summary>
    [HttpGet("vietqr")]
    public ActionResult<ApiResponse<VietQrInfoDto>> GetVietQr(
        [FromQuery] decimal amount,
        [FromQuery] string? orderCode = null)
    {
        var result = _orderService.GenerateVietQr(amount, orderCode);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }
}
