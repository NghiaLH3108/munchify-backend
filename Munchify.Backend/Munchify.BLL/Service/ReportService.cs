using System.Globalization;
using ClosedXML.Excel;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Reports;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class ReportService : IReportService
{
    private const int MaxLimit = 50;

    private readonly IReportRepository _reportRepository;

    public ReportService(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ApiResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(
        ReportFilterParams filter, CancellationToken cancellationToken = default)
    {
        if (!TryResolveRange(filter, out var from, out var to, out var period, out var error))
            return ApiResponse<RevenueSummaryDto>.ErrorResult(error);

        var data = await _reportRepository.GetRevenueSummaryAsync(from, to, period, cancellationToken);
        return ApiResponse<RevenueSummaryDto>.SuccessResult(data, "Lấy báo cáo doanh thu thành công");
    }

    public async Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingAsync(
        ReportFilterParams filter, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (!TryResolveRange(filter, out var from, out var to, out _, out var error))
            return ApiResponse<List<TopSellingProductDto>>.ErrorResult(error);

        var data = await _reportRepository.GetTopSellingAsync(from, to, NormalizeLimit(limit), cancellationToken);
        return ApiResponse<List<TopSellingProductDto>>.SuccessResult(data, "Lấy top sản phẩm bán chạy thành công");
    }

    public async Task<ApiResponse<byte[]>> ExportToExcelAsync(
        ReportFilterParams filter, int limit = 10, CancellationToken cancellationToken = default)
    {
        if (!TryResolveRange(filter, out var from, out var to, out var period, out var error))
            return ApiResponse<byte[]>.ErrorResult(error);

        var revenue = await _reportRepository.GetRevenueSummaryAsync(from, to, period, cancellationToken);
        var top = await _reportRepository.GetTopSellingAsync(from, to, NormalizeLimit(limit), cancellationToken);

        var title = BuildTitle(period, from);
        using var workbook = new XLWorkbook();
        BuildRevenueSheet(workbook, title, revenue);
        BuildPaymentSheet(workbook, title, revenue);
        BuildTopSellingSheet(workbook, title, top);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return ApiResponse<byte[]>.SuccessResult(stream.ToArray(), "Xuất báo cáo thành công");
    }

    // ── Validation / range resolution ────────────────────────────────────────

    /// <summary>Resolves the filter into a half-open range [from, to).</summary>
    private static bool TryResolveRange(
        ReportFilterParams filter, out DateTime from, out DateTime to, out string period, out string error)
    {
        from = to = default;
        error = string.Empty;
        period = (filter.Period ?? string.Empty).Trim().ToLowerInvariant();
        var now = DateTime.Now;

        switch (period)
        {
            case "day":
                from = (filter.Date ?? now).Date;
                to = from.AddDays(1);
                return true;

            case "month":
            {
                var year = filter.Year ?? now.Year;
                var month = filter.Month ?? now.Month;
                if (year < 2000 || year > 2100 || month < 1 || month > 12)
                {
                    error = "Năm hoặc tháng không hợp lệ.";
                    return false;
                }
                from = new DateTime(year, month, 1);
                to = from.AddMonths(1);
                return true;
            }

            case "year":
            {
                var year = filter.Year ?? now.Year;
                if (year < 2000 || year > 2100)
                {
                    error = "Năm không hợp lệ.";
                    return false;
                }
                from = new DateTime(year, 1, 1);
                to = from.AddYears(1);
                return true;
            }

            default:
                error = "Period phải là 'day', 'month' hoặc 'year'.";
                return false;
        }
    }

    private static int NormalizeLimit(int limit) => limit < 1 ? 10 : Math.Min(limit, MaxLimit);

    private static string BuildTitle(string period, DateTime from) => period switch
    {
        "day" => $"Ngày {from:dd/MM/yyyy}",
        "month" => $"Tháng {from:MM/yyyy}",
        _ => $"Năm {from:yyyy}"
    };

    // ── Excel sheets ─────────────────────────────────────────────────────────

    private static void BuildRevenueSheet(XLWorkbook wb, string title, RevenueSummaryDto r)
    {
        var ws = wb.Worksheets.Add("Doanh thu");
        ws.Cell(1, 1).Value = $"BÁO CÁO DOANH THU - {title}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Tổng doanh thu";
        ws.Cell(3, 2).Value = r.TotalRevenue;
        ws.Cell(4, 1).Value = "Số đơn hoàn tất";
        ws.Cell(4, 2).Value = r.OrderCount;
        ws.Cell(5, 1).Value = "Giá trị đơn trung bình";
        ws.Cell(5, 2).Value = r.AvgOrderValue;
        ws.Range(3, 1, 5, 1).Style.Font.Bold = true;
        ws.Cell(3, 2).Style.NumberFormat.Format = "#,##0";
        ws.Cell(5, 2).Style.NumberFormat.Format = "#,##0";

        var header = 7;
        ws.Cell(header, 1).Value = "Mốc thời gian";
        ws.Cell(header, 2).Value = "Doanh thu (VNĐ)";
        ws.Cell(header, 3).Value = "Số đơn";
        StyleHeader(ws.Range(header, 1, header, 3));

        var row = header + 1;
        foreach (var p in r.DataPoints)
        {
            ws.Cell(row, 1).Value = p.Label;
            ws.Cell(row, 2).Value = p.Revenue;
            ws.Cell(row, 3).Value = p.OrderCount;
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            row++;
        }
        ws.Columns().AdjustToContents();
    }

    private static void BuildPaymentSheet(XLWorkbook wb, string title, RevenueSummaryDto r)
    {
        var ws = wb.Worksheets.Add("Phương thức thanh toán");
        ws.Cell(1, 1).Value = $"PHƯƠNG THỨC THANH TOÁN - {title}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "Phương thức";
        ws.Cell(3, 2).Value = "Số giao dịch";
        ws.Cell(3, 3).Value = "Số tiền (VNĐ)";
        ws.Cell(3, 4).Value = "Tỷ trọng";
        StyleHeader(ws.Range(3, 1, 3, 4));

        var total = r.ByPaymentMethod.Sum(x => x.Amount);
        var row = 4;
        foreach (var m in r.ByPaymentMethod)
        {
            ws.Cell(row, 1).Value = PaymentLabel(m.PaymentMethod);
            ws.Cell(row, 2).Value = m.Count;
            ws.Cell(row, 3).Value = m.Amount;
            ws.Cell(row, 4).Value = total == 0 ? 0 : (double)(m.Amount / total);
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0";
            ws.Cell(row, 4).Style.NumberFormat.Format = "0.0%";
            row++;
        }
        ws.Columns().AdjustToContents();
    }

    private static void BuildTopSellingSheet(XLWorkbook wb, string title, List<TopSellingProductDto> items)
    {
        var ws = wb.Worksheets.Add("Top bán chạy");
        ws.Cell(1, 1).Value = $"TOP SẢN PHẨM / COMBO BÁN CHẠY - {title}";
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(3, 1).Value = "#";
        ws.Cell(3, 2).Value = "Tên";
        ws.Cell(3, 3).Value = "Loại";
        ws.Cell(3, 4).Value = "Số lượng";
        ws.Cell(3, 5).Value = "Doanh thu (VNĐ)";
        StyleHeader(ws.Range(3, 1, 3, 5));

        var row = 4;
        var rank = 1;
        foreach (var i in items)
        {
            ws.Cell(row, 1).Value = rank++;
            ws.Cell(row, 2).Value = i.ProductName;
            ws.Cell(row, 3).Value = i.IsCombo ? "Combo" : "Sản phẩm";
            ws.Cell(row, 4).Value = i.TotalQuantity;
            ws.Cell(row, 5).Value = i.TotalRevenue;
            ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
            row++;
        }
        ws.Columns().AdjustToContents();
    }

    private static void StyleHeader(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFE8D6");
        range.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
    }

    private static string PaymentLabel(string method) => method switch
    {
        "CASH" => "Tiền mặt",
        "QR_CODE" => "VietQR",
        "BANK_TRANSFER" => "Chuyển khoản",
        _ => method
    };
}
