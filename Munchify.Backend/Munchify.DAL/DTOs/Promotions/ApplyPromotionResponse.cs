namespace Munchify.DAL.DTOs.Promotions;

public class ApplyPromotionResponse
{
    public string PromotionCode { get; set; } = string.Empty;
    public string PromotionName { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;   // PERCENTAGE | FIXED_AMOUNT
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }  // Actual discount in VND
}
