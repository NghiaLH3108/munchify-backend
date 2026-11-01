using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Munchify.API.Hubs;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;

namespace Munchify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.AdminOrStaff)]
    public class DamagedExpiredRecordsController : ControllerBase
    {
        private readonly IDamagedExpiredService _service;
        private readonly IHubContext<InventoryHub> _hubContext;

        public DamagedExpiredRecordsController(IDamagedExpiredService service, IHubContext<InventoryHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpPost("report")]
        public async Task<ActionResult<ApiResponse<DamagedExpiredRecordDto>>> CreateStaffReport([FromBody] StaffReportDamagedExpiredRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<DamagedExpiredRecordDto>.ErrorResult("Dữ liệu báo cáo không hợp lệ."));
            }

            int userId = User.GetUserId() ?? 0;
            var response = await _service.CreateStaffReportAsync(request, userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            // Real-time broadcast to Web Admin
            if (response.Data != null)
            {
                await _hubContext.Clients.All.SendAsync("ReceiveExpiryAlert", 
                    $"Nhân viên vừa báo cáo hàng hỏng/hết hạn: {response.Data.ProductName} (SL: {response.Data.Quantity}) - Lý do: {response.Data.Reason}");
            }

            return Ok(response);
        }

        [HttpGet]
        [HttpGet("pending")]
        [Authorize(Roles = AppRoles.AdminOrStaff)]
        public async Task<ActionResult<ApiResponse<IEnumerable<DamagedExpiredRecordDto>>>> GetPendingRecords([FromQuery] string? status = null)
        {
            var response = await _service.GetPendingRecordsAsync(status);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpGet("summary-metrics")]
        public async Task<ActionResult<ApiResponse<AiExpirySummaryDto>>> GetSummaryMetrics()
        {
            var response = await _service.GetSummaryMetricsAsync();
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{id}/approve")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveRecord(int id)
        {
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            // {
            //     return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Lỗi xác thực người dùng." });
            // }
            int userId = User.GetUserId() ?? 0;

            var response = await _service.ApproveRecordAsync(id, userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<ApiResponse<bool>>> RejectRecord(int id)
        {
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            // {
            //     return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Lỗi xác thực người dùng." });
            // }
            int userId = User.GetUserId() ?? 0;

            var response = await _service.RejectRecordAsync(id, userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPost("{id}/analyze")]
        [Authorize(Roles = AppRoles.AdminOrStaff)]
        public async Task<ActionResult<ApiResponse<DamagedExpiredRecordDto>>> AnalyzeRecord(int id)
        {
            var response = await _service.AnalyzeRecordAsync(id);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
