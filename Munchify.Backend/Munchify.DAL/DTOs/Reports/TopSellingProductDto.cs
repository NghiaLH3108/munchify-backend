namespace Munchify.DAL.DTOs.Reports;

/// <summary>F-07.2: best-selling product or combo.</summary>
public class TopSellingProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public bool IsCombo { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
