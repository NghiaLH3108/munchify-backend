using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Promotions;

public class ApplyPromotionRequest
{
    [Required]
    public string PromotionCode { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Subtotal { get; set; }
}
