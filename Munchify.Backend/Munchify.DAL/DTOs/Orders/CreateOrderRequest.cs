using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Orders;

public class CreateOrderRequest
{
    public int? CustomerId { get; set; }

    public string? PromotionCode { get; set; }

    [Required]
    public string PaymentMethod { get; set; } = "CASH"; // CASH | QR_CODE | BANK_TRANSFER

    [Range(0, int.MaxValue)]
    public int PointsToRedeem { get; set; } = 0;

    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();
}
