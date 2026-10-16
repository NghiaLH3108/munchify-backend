using System;

namespace Munchify.DAL.DTOs.Orders;

public class PaymentDto
{
    public int PaymentId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TransactionCode { get; set; }
    public DateTime? PaidAt { get; set; }
}
