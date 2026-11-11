using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.StockImports;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace Munchify.API.Controllers
{
    [Route("api/stock-imports")]
    [ApiController]
    [Authorize(Roles = AppRoles.AdminOrStaff)]
    public class StockImportsController : ControllerBase
    {
        private readonly IStockImportService _stockImportService;

        public StockImportsController(IStockImportService stockImportService)
        {
            _stockImportService = stockImportService;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<int>>> CreateDraftImport([FromBody] CreateImportRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = User.GetUserId() ?? 0;

            var result = await _stockImportService.CreateDraftImportAsync(request, userId);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<StockImportDto>>>> GetAllImports()
        {
            var result = await _stockImportService.GetImportsAsync();
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<StockImportDto>>> GetImportById(int id)
        {
            var result = await _stockImportService.GetImportByIdAsync(id);
            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                    return NotFound(result);
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelImport(int id)
        {
            var result = await _stockImportService.CancelImportAsync(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = AppRoles.Admin)]
        public async Task<ActionResult<ApiResponse<bool>>> CompleteImport(int id)
        {
            int userId = User.GetUserId() ?? 0;

            var result = await _stockImportService.CompleteImportAsync(id, userId);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
