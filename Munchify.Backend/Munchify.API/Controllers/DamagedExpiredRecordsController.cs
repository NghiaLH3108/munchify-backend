using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;

namespace Munchify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class DamagedExpiredRecordsController : ControllerBase
    {
        private readonly IDamagedExpiredService _service;

        public DamagedExpiredRecordsController(IDamagedExpiredService service)
        {
            _service = service;
        }

        [HttpGet]
        [HttpGet("pending")]
        //[Authorize(Roles = "ADMIN,STAFF")]
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
        //[Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<bool>>> ApproveRecord(int id)
        {
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            // {
            //     return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Lỗi xác thực người dùng." });
            // }
            int userId = 1; // Hardcode userId để test

            var response = await _service.ApproveRecordAsync(id, userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }

        [HttpPut("{id}/reject")]
        //[Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<ApiResponse<bool>>> RejectRecord(int id)
        {
            // var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            // {
            //     return Unauthorized(new ApiResponse<bool> { Success = false, Message = "Lỗi xác thực người dùng." });
            // }
            int userId = 1; // Hardcode userId để test

            var response = await _service.RejectRecordAsync(id, userId);
            if (!response.Success)
            {
                return BadRequest(response);
            }
            return Ok(response);
        }
    }
}
