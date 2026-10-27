using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Dashboard;

namespace Munchify.BLL.Service;

public class DashboardService : IDashboardService
{
    private readonly MunchifyDbContext _db;

    public DashboardService(MunchifyDbContext db)
    {
        _db = db;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);
            var tomorrow = today.AddDays(1);
            var weekStart = today.AddDays(-6);
            var sixMonthsStart = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

            // ── Orders completed today / yesterday ──────────────────────────
            var todayOrders = await _db.Orders
                .Include(o => o.Payments)
                .Where(o => o.CreatedAt >= today && o.CreatedAt < tomorrow
                            && o.OrderStatus.ToUpper() == "COMPLETED")
                .ToListAsync(cancellationToken);

            var yesterdayOrders = await _db.Orders
                .Where(o => o.CreatedAt >= yesterday && o.CreatedAt < today
                            && o.OrderStatus.ToUpper() == "COMPLETED")
                .ToListAsync(cancellationToken);

            decimal todayRevenue = todayOrders.Sum(o => o.TotalAmount);
            decimal yesterdayRevenue = yesterdayOrders.Sum(o => o.TotalAmount);

            decimal? revenueChangePercent = null;
            if (yesterdayRevenue > 0)
            {
                revenueChangePercent = Math.Round(
                    ((todayRevenue - yesterdayRevenue) / yesterdayRevenue) * 100, 1);
            }

            int todayOrderCount = todayOrders.Count;
            int yesterdayOrderCount = yesterdayOrders.Count;

            decimal avgOrderValue = todayOrderCount > 0
                ? Math.Round(todayRevenue / todayOrderCount, 0)
                : 0;

            // ── Low stock products & total menu count ──────────────────────
            int lowStockCount = await _db.Products
                .Where(p => p.IsActive && !p.IsCombo
                            && p.StockQuantity <= p.MinStockLevel)
                .CountAsync(cancellationToken);

            int totalMenuProductsCount = await _db.Products
                .Where(p => p.IsActive)
                .CountAsync(cancellationToken);

            // ── Customers ──────────────────────────────────────────────────
            int totalCustomers = await _db.Customers.CountAsync(cancellationToken);
            int newCustomersToday = await _db.Customers
                .Where(c => c.CreatedAt >= today && c.CreatedAt < tomorrow)
                .CountAsync(cancellationToken);
            int loyaltyCustomers = await _db.Customers
                .Where(c => c.LoyaltyPoints > 0)
                .CountAsync(cancellationToken);
            decimal loyaltyPercent = totalCustomers > 0
                ? Math.Round((decimal)loyaltyCustomers * 100m / totalCustomers, 1)
                : 0m;

            // ── Hourly Trend (today, 7 buckets) ────────────────────────────
            var hourBuckets = new (int StartHour, int EndHour, string Label)[]
            {
                (0, 10, "08:00"),
                (10, 12, "11:00"),
                (12, 14, "13:00"),
                (14, 16, "15:00"),
                (16, 18, "17:00"),
                (18, 20, "19:00"),
                (20, 24, "21:00"),
            };

            var hourlyTrend = hourBuckets.Select(b =>
            {
                var bucketOrders = todayOrders
                    .Where(o => o.CreatedAt.Hour >= b.StartHour && o.CreatedAt.Hour < b.EndHour)
                    .ToList();
                return new DashboardTrendPointDto
                {
                    Label = b.Label,
                    Revenue = bucketOrders.Sum(o => o.TotalAmount),
                    OrderCount = bucketOrders.Count,
                    IsPeak = false,
                };
            }).ToList();

            var maxHourlyRev = hourlyTrend.Max(x => x.Revenue);
            string peakHourLabel = "Chưa phát sinh đơn";
            decimal peakHourRevenue = 0;
            int peakHourOrderCount = 0;
            if (maxHourlyRev > 0)
            {
                var peakIdx = hourlyTrend.FindIndex(x => x.Revenue == maxHourlyRev);
                hourlyTrend[peakIdx].IsPeak = true;
                var bucket = hourBuckets[peakIdx];
                peakHourLabel = $"{bucket.StartHour:00}:00 - {bucket.EndHour:00}:00";
                peakHourRevenue = hourlyTrend[peakIdx].Revenue;
                peakHourOrderCount = hourlyTrend[peakIdx].OrderCount;
            }

            // ── Weekly Trend (last 7 days) ─────────────────────────────────
            var weekOrders = await _db.Orders
                .Where(o => o.CreatedAt >= weekStart && o.CreatedAt < tomorrow
                            && o.OrderStatus.ToUpper() == "COMPLETED")
                .Select(o => new { o.CreatedAt, o.TotalAmount })
                .ToListAsync(cancellationToken);

            var weeklyTrend = Enumerable.Range(0, 7).Select(offset =>
            {
                var d = weekStart.AddDays(offset);
                var nextD = d.AddDays(1);
                var dayOrders = weekOrders.Where(o => o.CreatedAt >= d && o.CreatedAt < nextD).ToList();
                return new DashboardTrendPointDto
                {
                    Label = d.ToString("dd/MM"),
                    Revenue = dayOrders.Sum(o => o.TotalAmount),
                    OrderCount = dayOrders.Count,
                    IsPeak = false,
                };
            }).ToList();

            var maxWeeklyRev = weeklyTrend.Max(x => x.Revenue);
            if (maxWeeklyRev > 0)
            {
                var idx = weeklyTrend.FindIndex(x => x.Revenue == maxWeeklyRev);
                weeklyTrend[idx].IsPeak = true;
            }

            // ── Monthly Trend (last 6 months) ──────────────────────────────
            var sixMonthsOrders = await _db.Orders
                .Where(o => o.CreatedAt >= sixMonthsStart && o.CreatedAt < tomorrow
                            && o.OrderStatus.ToUpper() == "COMPLETED")
                .Select(o => new { o.CreatedAt, o.TotalAmount })
                .ToListAsync(cancellationToken);

            var monthlyTrend = Enumerable.Range(0, 6).Select(offset =>
            {
                var mStart = sixMonthsStart.AddMonths(offset);
                var mEnd = mStart.AddMonths(1);
                var mOrders = sixMonthsOrders.Where(o => o.CreatedAt >= mStart && o.CreatedAt < mEnd).ToList();
                return new DashboardTrendPointDto
                {
                    Label = $"T{mStart.Month}",
                    Revenue = mOrders.Sum(o => o.TotalAmount),
                    OrderCount = mOrders.Count,
                    IsPeak = false,
                };
            }).ToList();

            var maxMonthlyRev = monthlyTrend.Max(x => x.Revenue);
            if (maxMonthlyRev > 0)
            {
                var idx = monthlyTrend.FindIndex(x => x.Revenue == maxMonthlyRev);
                monthlyTrend[idx].IsPeak = true;
            }

            // ── Payment Methods Breakdown ──────────────────────────────────
            var ordersForPayments = todayOrders;
            var paymentMethods = BuildPaymentMethodBreakdown(ordersForPayments);

            // ── Top Products & Combos (today, fallback to recent if today is empty) ─
            var todayOrderIds = todayOrders.Select(o => o.OrderId).ToList();
            var orderDetailsQuery = _db.OrderDetails
                .Include(od => od.Product)
                    .ThenInclude(p => p.Category)
                .Include(od => od.Product)
                    .ThenInclude(p => p.Unit)
                .Include(od => od.Product)
                    .ThenInclude(p => p.ProductComboParentProducts)
                        .ThenInclude(pc => pc.ComponentProduct)
                .Where(od => od.Order.OrderStatus.ToUpper() == "COMPLETED");

            var relevantOrderDetails = todayOrderIds.Count > 0
                ? await orderDetailsQuery
                    .Where(od => todayOrderIds.Contains(od.OrderId))
                    .ToListAsync(cancellationToken)
                : await orderDetailsQuery
                    .Where(od => od.Order.CreatedAt >= sixMonthsStart)
                    .ToListAsync(cancellationToken);

            decimal totalDetailsRevenue = relevantOrderDetails.Sum(od => od.TotalAmount);
            var topProducts = relevantOrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g =>
                {
                    var first = g.First();
                    var p = first.Product;
                    decimal rev = g.Sum(x => x.TotalAmount);
                    int qty = g.Sum(x => x.Quantity);
                    bool isCombo = p?.IsCombo ?? false;
                    int stockQty = p?.StockQuantity ?? 0;
                    int minStock = p?.MinStockLevel ?? 0;
                    bool isOutOfStock = false;

                    if (p != null && isCombo && p.ProductComboParentProducts.Any())
                    {
                        isOutOfStock = p.ProductComboParentProducts.Any(
                            pc => (pc.ComponentProduct?.StockQuantity ?? 0) < pc.Quantity);
                        stockQty = p.ProductComboParentProducts.Min(
                            pc => pc.Quantity > 0
                                ? (pc.ComponentProduct?.StockQuantity ?? 0) / pc.Quantity
                                : 0);
                    }
                    else if (p != null)
                    {
                        isOutOfStock = p.StockQuantity <= p.MinStockLevel;
                    }

                    return new DashboardTopProductDto
                    {
                        ProductId = g.Key,
                        ProductCode = p?.ProductCode ?? $"SP{g.Key}",
                        ProductName = p?.ProductName ?? first.ProductName,
                        Description = p?.Description,
                        CategoryName = p?.Category?.CategoryName ?? "Chưa phân loại",
                        UnitName = p?.Unit?.UnitName ?? first.UnitName,
                        IsCombo = isCombo,
                        UnitPrice = p?.Price ?? first.UnitPrice,
                        QuantitySold = qty,
                        TotalRevenue = rev,
                        RevenueSharePercent = totalDetailsRevenue > 0
                            ? Math.Round(rev * 100m / totalDetailsRevenue, 1)
                            : 0m,
                        StockQuantity = stockQty,
                        MinStockLevel = minStock,
                        IsOutOfStock = isOutOfStock,
                    };
                })
                .OrderByDescending(x => x.TotalRevenue)
                .ThenByDescending(x => x.QuantitySold)
                .Take(15)
                .ToList();

            var dto = new DashboardSummaryDto
            {
                TodayRevenue = todayRevenue,
                YesterdayRevenue = yesterdayRevenue,
                RevenueChangePercent = revenueChangePercent,
                TodayOrderCount = todayOrderCount,
                YesterdayOrderCount = yesterdayOrderCount,
                AverageOrderValue = avgOrderValue,
                LowStockProductCount = lowStockCount,
                TotalMenuProductsCount = totalMenuProductsCount,
                TotalCustomerCount = totalCustomers,
                NewCustomersTodayCount = newCustomersToday,
                LoyaltyCustomerCount = loyaltyCustomers,
                LoyaltyCustomerPercent = loyaltyPercent,
                PeakHourLabel = peakHourLabel,
                PeakHourRevenue = peakHourRevenue,
                PeakHourOrderCount = peakHourOrderCount,
                HourlyTrend = hourlyTrend,
                WeeklyTrend = weeklyTrend,
                MonthlyTrend = monthlyTrend,
                PaymentTotalOrderCount = todayOrderCount,
                PaymentMethods = paymentMethods,
                TopProducts = topProducts,
            };

            return ApiResponse<DashboardSummaryDto>.SuccessResult(dto);
        }
        catch (Exception ex)
        {
            return ApiResponse<DashboardSummaryDto>.ErrorResult(
                $"Lỗi khi tải dữ liệu Dashboard: {ex.Message}");
        }
    }

    private static List<DashboardPaymentMethodDto> BuildPaymentMethodBreakdown(
        List<Munchify.DAL.Entities.Order> orders)
    {
        int qrCount = 0;
        decimal qrAmount = 0;
        int cashCount = 0;
        decimal cashAmount = 0;
        int transferCount = 0;
        decimal transferAmount = 0;

        foreach (var order in orders)
        {
            var payment = order.Payments.FirstOrDefault();
            var method = (payment?.PaymentMethod ?? "CASH").ToUpperInvariant();
            var amount = payment?.Amount ?? order.TotalAmount;

            if (method.Contains("QR") || method.Contains("MOMO"))
            {
                qrCount++;
                qrAmount += amount;
            }
            else if (method.Contains("TRANSFER") || method.Contains("BANK"))
            {
                transferCount++;
                transferAmount += amount;
            }
            else
            {
                cashCount++;
                cashAmount += amount;
            }
        }

        decimal totalAmt = qrAmount + cashAmount + transferAmount;
        int totalCnt = qrCount + cashCount + transferCount;

        decimal CalcShare(decimal amt, int cnt)
        {
            if (totalAmt > 0) return Math.Round(amt * 100m / totalAmt, 0);
            if (totalCnt > 0) return Math.Round((decimal)cnt * 100m / totalCnt, 0);
            return 0m;
        }

        return new List<DashboardPaymentMethodDto>
        {
            new()
            {
                MethodKey = "QR_CODE",
                MethodName = "VietQR / MoMo",
                OrderCount = qrCount,
                TotalAmount = qrAmount,
                SharePercent = CalcShare(qrAmount, qrCount),
            },
            new()
            {
                MethodKey = "CASH",
                MethodName = "Tiền mặt (Cash)",
                OrderCount = cashCount,
                TotalAmount = cashAmount,
                SharePercent = CalcShare(cashAmount, cashCount),
            },
            new()
            {
                MethodKey = "TRANSFER",
                MethodName = "Chuyển khoản",
                OrderCount = transferCount,
                TotalAmount = transferAmount,
                SharePercent = CalcShare(transferAmount, transferCount),
            },
        };
    }
}
