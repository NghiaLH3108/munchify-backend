using Munchify.DAL.DTOs.Reports;

namespace Munchify.DAL.Repository.Interfaces;

/// <summary>
/// Read-only aggregation queries for reporting (F-07.x).
/// Range is half-open: [from, to). Only COMPLETED orders / PAID payments are counted.
/// </summary>
public interface IReportRepository
{
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        DateTime from, DateTime to, string period, CancellationToken ct = default);

    Task<List<TopSellingProductDto>> GetTopSellingAsync(
        DateTime from, DateTime to, int limit, CancellationToken ct = default);
}
