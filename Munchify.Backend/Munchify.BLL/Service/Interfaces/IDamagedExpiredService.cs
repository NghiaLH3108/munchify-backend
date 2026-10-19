using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;

namespace Munchify.BLL.Service.Interfaces
{
    public interface IDamagedExpiredService
    {
        Task<ApiResponse<IEnumerable<DamagedExpiredRecordDto>>> GetPendingRecordsAsync(string? status = null);
        Task<ApiResponse<AiExpirySummaryDto>> GetSummaryMetricsAsync();
        Task<ApiResponse<bool>> ApproveRecordAsync(int recordId, int userId);
        Task<ApiResponse<bool>> RejectRecordAsync(int recordId, int userId);
        Task<ApiResponse<DamagedExpiredRecordDto>> CreateStaffReportAsync(StaffReportDamagedExpiredRequest request, int userId);
    }
}
