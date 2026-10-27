namespace Munchify.DAL.DTOs.Reports;

/// <summary>F-07.1: revenue summary for a day / month / year.</summary>
public class RevenueSummaryDto
{
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AvgOrderValue { get; set; }
    public List<PaymentMethodBreakdownDto> ByPaymentMethod { get; set; } = new();

    /// <summary>Chart buckets: hours (day), days (month) or months (year).</summary>
    public List<RevenueDataPointDto> DataPoints { get; set; } = new();
}

/// <summary>Payment report row (CASH | QR_CODE | BANK_TRANSFER).</summary>
public class PaymentMethodBreakdownDto
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int Count { get; set; }
}

public class RevenueDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}
