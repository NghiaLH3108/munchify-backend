using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository
{
    public class DamagedExpiredRecordRepository : GenericRepository<DamagedExpiredRecord>, IDamagedExpiredRecordRepository
    {
        public DamagedExpiredRecordRepository(MunchifyDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<DamagedExpiredRecord>> GetPendingRecordsWithDetailsAsync(string? status = null)
        {
            var query = _context.DamagedExpiredRecords
                .Include(r => r.Product)
                .ThenInclude(p => p.Unit)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && !status.Equals("ALL", System.StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(r => r.Status == status.ToUpper());
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Munchify.DAL.DTOs.Inventory.AiExpirySummaryDto> GetSummaryMetricsAsync()
        {
            var now = System.DateTime.Now;
            var startOfMonth = new System.DateTime(now.Year, now.Month, 1);

            var pendingRecords = await _context.DamagedExpiredRecords
                .Include(r => r.Product)
                .Include(r => r.ImportDetail)
                .Where(r => r.Status == "PENDING")
                .ToListAsync();

            var resolvedThisMonth = await _context.DamagedExpiredRecords
                .Include(r => r.Product)
                .Where(r => r.Status == "APPROVED" && r.ProcessedAt >= startOfMonth)
                .ToListAsync();

            int totalPending = pendingRecords.Count;
            // Define urgent as expiry within 3 days or already expired
            int urgentPending = pendingRecords.Count(r => r.ImportDetail != null && r.ImportDetail.ExpiryDate.HasValue && (r.ImportDetail.ExpiryDate.Value.Date - now.Date).Days <= 3);
            
            decimal estLoss = pendingRecords.Sum(r => r.Quantity * r.Product.Price);
            
            // Mock trend for now
            decimal lossTrend = 12.4m;

            int autoResolved = resolvedThisMonth.Count;
            decimal recovered = resolvedThisMonth.Sum(r => r.Quantity * r.Product.Price * 0.5m); // assuming 50% recovery on average for flash sales

            return new Munchify.DAL.DTOs.Inventory.AiExpirySummaryDto
            {
                TotalPendingAlerts = totalPending,
                UrgentPendingAlerts = urgentPending,
                EstFinancialLoss = estLoss,
                LossTrendPercent = lossTrend,
                AutoResolvedThisMonth = autoResolved,
                RecoveredCapital = recovered
            };
        }
    }
}
