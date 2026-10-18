using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Orders;

namespace Munchify.BLL.Service.Interfaces;

public interface IOrderService
{
    /// <summary>
    /// Creates a new order atomically – validates stock, applies promo/points,
    /// deducts inventory, updates customer loyalty, and persists payment.
    /// </summary>
    Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        int staffId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetOrdersAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tạo thông tin và link ảnh VietQR chuẩn Napas 247 cho thanh toán tại quầy POS.
    /// </summary>
    ApiResponse<VietQrInfoDto> GenerateVietQr(decimal amount, string? orderCode = null);
}
