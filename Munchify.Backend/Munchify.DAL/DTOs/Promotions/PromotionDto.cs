using System;

namespace Munchify.DAL.DTOs.Promotions;

public class PromotionDto
{
    public int PromotionId { get; set; }
    public string PromotionCode { get; set; } = null!;
    public string PromotionName { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
