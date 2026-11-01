using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces
{
    public interface IDamagedExpiredRecordRepository : IGenericRepository<DamagedExpiredRecord>
    {
        Task<IEnumerable<DamagedExpiredRecord>> GetPendingRecordsWithDetailsAsync(string? status = null);
        Task<DamagedExpiredRecord?> GetRecordWithDetailsAsync(int recordId);
        Task<Munchify.DAL.DTOs.Inventory.AiExpirySummaryDto> GetSummaryMetricsAsync();
    }
}
