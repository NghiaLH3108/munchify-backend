using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Reports;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class ReportRepository : IReportRepository
{
    private const string OrderCompleted = "COMPLETED";
    private const string PaymentPaid = "PAID";

    private readonly MunchifyDbContext _context;

    public ReportRepository(MunchifyDbContext context)
    {
        _context = context;
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(
        DateTime from, DateTime to, string period, CancellationToken ct = default)
    {
        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.OrderStatus == OrderCompleted
                        && o.CompletedAt != null
                        && o.CompletedAt >= from && o.CompletedAt < to)
            .Select(o => new { CompletedAt = o.CompletedAt!.Value, o.TotalAmount })
            .ToListAsync(ct);

        var totalRevenue = orders.Sum(o => o.TotalAmount);
        var orderCount = orders.Count;

        var byPaymentMethod = await _context.Payments
            .AsNoTracking()
            .Where(p => p.PaymentStatus == PaymentPaid
                        && p.Order.OrderStatus == OrderCompleted
                        && p.Order.CompletedAt != null
                        && p.Order.CompletedAt >= from && p.Order.CompletedAt < to)
            .GroupBy(p => p.PaymentMethod)
            .Select(g => new PaymentMethodBreakdownDto
            {
                PaymentMethod = g.Key,
                Amount = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(ct);

        return new RevenueSummaryDto
        {
            TotalRevenue = totalRevenue,
            OrderCount = orderCount,
            AvgOrderValue = orderCount == 0 ? 0 : Math.Round(totalRevenue / orderCount, 0),
            ByPaymentMethod = byPaymentMethod,
            DataPoints = BuildDataPoints(period, from, orders.Select(o => (o.CompletedAt, o.TotalAmount)))
        };
    }

    public async Task<List<TopSellingProductDto>> GetTopSellingAsync(
        DateTime from, DateTime to, int limit, CancellationToken ct = default)
    {
        return await _context.OrderDetails
            .AsNoTracking()
            .Where(d => d.Order.OrderStatus == OrderCompleted
                        && d.Order.CompletedAt != null
                        && d.Order.CompletedAt >= from && d.Order.CompletedAt < to)
            .GroupBy(d => new { d.ProductId, d.Product.ProductName, d.Product.IsCombo })
            .Select(g => new TopSellingProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.ProductName,
                IsCombo = g.Key.IsCombo,
                TotalQuantity = g.Sum(d => d.Quantity),
                TotalRevenue = g.Sum(d => d.TotalAmount)
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ThenByDescending(x => x.TotalRevenue)
            .Take(limit)
            .ToListAsync(ct);
    }

    /// <summary>
    /// day → 24 hourly buckets, month → one bucket per day, year → 12 monthly buckets.
    /// Empty buckets are kept so the chart axis is continuous.
    /// </summary>
    private static List<RevenueDataPointDto> BuildDataPoints(
        string period, DateTime from, IEnumerable<(DateTime At, decimal Amount)> orders)
    {
        var list = orders.ToList();

        return period switch
        {
            "day" => Enumerable.Range(0, 24)
                .Select(h => Bucket($"{h:00}:00", list.Where(o => o.At.Hour == h)))
                .ToList(),

            "month" => Enumerable.Range(1, DateTime.DaysInMonth(from.Year, from.Month))
                .Select(d => Bucket($"{d:00}/{from.Month:00}", list.Where(o => o.At.Day == d)))
                .ToList(),

            _ => Enumerable.Range(1, 12)
                .Select(m => Bucket($"Tháng {m}", list.Where(o => o.At.Month == m)))
                .ToList()
        };
    }

    private static RevenueDataPointDto Bucket(string label, IEnumerable<(DateTime At, decimal Amount)> items)
    {
        var arr = items.ToList();
        return new RevenueDataPointDto
        {
            Label = label,
            Revenue = arr.Sum(i => i.Amount),
            OrderCount = arr.Count
        };
    }
}
