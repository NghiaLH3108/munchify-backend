namespace Munchify.DAL.DTOs.Orders;

public class VietQrInfoDto
{
    public string BankId { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string AccountNo { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string QrImageUrl { get; set; } = string.Empty;
    public string QrQuickLink { get; set; } = string.Empty;
}
