using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Reports;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReportsController : ControllerBase
{
    private const string XlsxContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    /// <summary>GET /api/reports/revenue?period=day&amp;date=2026-10-14 — revenue + payment method report.</summary>
    [HttpGet("revenue")]
    public async Task<ActionResult<ApiResponse<RevenueSummaryDto>>> GetRevenue(
        [FromQuery] ReportFilterParams filter, CancellationToken cancellationToken)
    {
        var response = await _reportService.GetRevenueSummaryAsync(filter, cancellationToken);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>GET /api/reports/top-selling?period=month&amp;year=2026&amp;month=10&amp;limit=10</summary>
    [HttpGet("top-selling")]
    public async Task<ActionResult<ApiResponse<List<TopSellingProductDto>>>> GetTopSelling(
        [FromQuery] ReportFilterParams filter, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var response = await _reportService.GetTopSellingAsync(filter, limit, cancellationToken);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>GET /api/reports/export?period=month&amp;year=2026&amp;month=10 — download .xlsx</summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportExcel(
        [FromQuery] ReportFilterParams filter, [FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var response = await _reportService.ExportToExcelAsync(filter, limit, cancellationToken);
        if (!response.Success || response.Data == null)
            return BadRequest(response);

        var fileName = $"BaoCao_{filter.Period}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(response.Data, XlsxContentType, fileName);
    }
}
