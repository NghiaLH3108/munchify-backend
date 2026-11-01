using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Munchify.API.Hubs;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;

namespace Munchify.API.BackgroundServices
{
    public class ExpiryCheckBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiryCheckBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

        public ExpiryCheckBackgroundService(IServiceProvider serviceProvider, ILogger<ExpiryCheckBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expiry Check Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Running expiry check at: {time}", DateTimeOffset.Now);

                try
                {
                    await CheckExpiriesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing ExpiryCheckBackgroundService.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
            
            _logger.LogInformation("Expiry Check Background Service is stopping.");
        }

        private async Task CheckExpiriesAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MunchifyDbContext>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<InventoryHub>>();

            var thresholdDate = DateTime.Now.AddDays(7); // Check items expiring in 7 days or less

            var expiringItems = await dbContext.StockImportDetails
                .Include(d => d.Product)
                .Include(d => d.Import)
                .Where(d => d.Import.Status == "COMPLETED")
                .Where(d => d.ExpiryDate != null && d.ExpiryDate <= thresholdDate && d.Quantity > 0)
                .Where(d => !dbContext.DamagedExpiredRecords.Any(r => r.ImportDetailId == d.ImportDetailId && (r.Status == "PENDING" || r.Status == "APPROVED")))
                .ToListAsync();

            if (!expiringItems.Any())
            {
                _logger.LogInformation("No expiring items found to process.");
                return;
            }

            foreach (var item in expiringItems)
            {
                if (item.Product == null) continue;

                var record = new DamagedExpiredRecord
                {
                    ImportDetailId = item.ImportDetailId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ReportType = "EXPIRED",
                    Status = "PENDING",
                    AiAnalysis = null, // Will be analyzed on-demand when user clicks
                    CreatedAt = DateTime.Now
                };

                dbContext.DamagedExpiredRecords.Add(record);
            }

            await dbContext.SaveChangesAsync();
            
            // Broadcast real-time event to Flutter frontend
            await hubContext.Clients.All.SendAsync("ReceiveExpiryAlert", "Phát hiện lô hàng sắp hết hạn cần xử lý!");
            
            _logger.LogInformation($"Processed {expiringItems.Count} expiring items to AI check and sent SignalR alert.");
        }
    }
}
