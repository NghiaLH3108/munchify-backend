using System;

namespace Munchify.DAL.DTOs.Orders;

/// <summary>
/// Compact DTO for the order history list (GET /api/orders).
/// </summary>
public class OrderSummaryDto
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? StaffName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ItemCount { get; set; }
}
