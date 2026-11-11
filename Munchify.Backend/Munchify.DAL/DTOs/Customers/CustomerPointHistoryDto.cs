using System;

namespace Munchify.DAL.DTOs.Customers;

public class CustomerPointHistoryDto
{
    public int OrderId { get; set; }
    public string OrderCode { get; set; } = null!;
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
}
