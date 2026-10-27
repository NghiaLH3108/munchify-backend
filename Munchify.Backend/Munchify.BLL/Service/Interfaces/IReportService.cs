using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Reports;

namespace Munchify.BLL.Service.Interfaces;

public interface IReportService
{
    /// <summary>F-07.1 + payment method report (Cash / QR / Transfer).</summary>
    Task<ApiResponse<RevenueSummaryDto>> GetRevenueSummaryAsync(
        ReportFilterParams filter, CancellationToken cancellationToken = default);

    /// <summary>F-07.2: top-selling products and combos.</summary>
    Task<ApiResponse<List<TopSellingProductDto>>> GetTopSellingAsync(
        ReportFilterParams filter, int limit = 10, CancellationToken cancellationToken = default);

    /// <summary>Excel (.xlsx) workbook: Revenue, Payment methods, Top selling.</summary>
    Task<ApiResponse<byte[]>> ExportToExcelAsync(
        ReportFilterParams filter, int limit = 10, CancellationToken cancellationToken = default);
}
