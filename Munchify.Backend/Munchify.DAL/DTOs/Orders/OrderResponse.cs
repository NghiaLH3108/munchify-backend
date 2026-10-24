using System;
using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Orders;

/// <summary>
/// Full order details returned from POST /api/orders and GET /api/orders/{id}.
/// </summary>
public class OrderResponse
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? StaffName { get; set; }
    public decimal? CashGiven { get; set; }
    public decimal? ChangeAmount { get; set; }
    public PaymentDto Payment { get; set; } = new();
    public List<OrderDetailDto> OrderDetails { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}
