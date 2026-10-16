using System;
using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Promotions;

public class CreatePromotionRequest
{
    public string? PromotionCode { get; set; }
    public string PromotionName { get; set; } = null!;
    public string? Description { get; set; }
    public string DiscountType { get; set; } = null!;
    public decimal DiscountValue { get; set; }
    public decimal? MinimumOrderAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    
    public List<int> ProductIds { get; set; } = new();
}
