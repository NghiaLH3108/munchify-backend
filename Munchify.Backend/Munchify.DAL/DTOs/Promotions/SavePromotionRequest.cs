using System;
using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Promotions;

public class SavePromotionRequest
{
    [Required]
    [StringLength(50)]
    public string PromotionCode { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string PromotionName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public string DiscountType { get; set; } = "FIXED_AMOUNT"; // PERCENTAGE | FIXED_AMOUNT

    [Range(0, double.MaxValue)]
    public decimal DiscountValue { get; set; }

    public decimal? MinimumOrderAmount { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; } = true;
}
