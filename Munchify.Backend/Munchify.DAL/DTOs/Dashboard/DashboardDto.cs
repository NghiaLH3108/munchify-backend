using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Dashboard;

public class DashboardTrendPointDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public bool IsPeak { get; set; }
}

public class DashboardPaymentMethodDto
{
    public string MethodKey { get; set; } = string.Empty;
    public string MethodName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SharePercent { get; set; }
}

public class DashboardTopProductDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public bool IsCombo { get; set; }
    public decimal UnitPrice { get; set; }
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueSharePercent { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public bool IsOutOfStock { get; set; }
}

/// <summary>
/// Comprehensive KPI & Analytics data for the Admin Dashboard.
/// </summary>
public class DashboardSummaryDto
{
    // ── Doanh thu hôm nay ──────────────────────────────────────────────
    public decimal TodayRevenue { get; set; }
    public decimal YesterdayRevenue { get; set; }

    /// <summary>Change percent vs yesterday. Null when yesterday had zero revenue.</summary>
    public decimal? RevenueChangePercent { get; set; }

    // ── Tổng đơn hàng hôm nay ──────────────────────────────────────────
    public int TodayOrderCount { get; set; }
    public int YesterdayOrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }

    // ── Cảnh báo tồn kho ───────────────────────────────────────────────
    public int LowStockProductCount { get; set; }
    public int TotalMenuProductsCount { get; set; }

    // ── Khách hàng thân thiết ──────────────────────────────────────────
    public int TotalCustomerCount { get; set; }
    public int NewCustomersTodayCount { get; set; }
    public int LoyaltyCustomerCount { get; set; }
    public decimal LoyaltyCustomerPercent { get; set; }

    // ── Xu hướng doanh thu & Giờ cao điểm ──────────────────────────────
    public string PeakHourLabel { get; set; } = string.Empty;
    public decimal PeakHourRevenue { get; set; }
    public int PeakHourOrderCount { get; set; }
    public List<DashboardTrendPointDto> HourlyTrend { get; set; } = new();
    public List<DashboardTrendPointDto> WeeklyTrend { get; set; } = new();
    public List<DashboardTrendPointDto> MonthlyTrend { get; set; } = new();

    // ── Cơ cấu phương thức thanh toán ──────────────────────────────────
    public int PaymentTotalOrderCount { get; set; }
    public List<DashboardPaymentMethodDto> PaymentMethods { get; set; } = new();

    // ── Top sản phẩm & combo bán chạy ──────────────────────────────────
    public List<DashboardTopProductDto> TopProducts { get; set; } = new();
}
