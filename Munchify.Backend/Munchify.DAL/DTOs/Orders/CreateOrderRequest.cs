using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Orders;

public class CreateOrderRequest
{
    public int? CustomerId { get; set; }

    public string? PromotionCode { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "CASH"; // CASH | QR_CODE

    /// <summary>
    /// Số tiền khách đưa (Bắt buộc và phải >= TotalAmount khi PaymentMethod == CASH).
    /// </summary>
    public decimal? CashGiven { get; set; }

    [Range(0, int.MaxValue)]
    public int PointsToRedeem { get; set; } = 0;

    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();
}
