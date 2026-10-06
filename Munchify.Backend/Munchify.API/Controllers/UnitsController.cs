using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Units;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    /// <summary>GET /api/units — List all units with optional filtering and pagination.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UnitDto>>>> GetUnits(
        [FromQuery] string searchTerm = "",
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _unitService.GetUnitsAsync(searchTerm, isActive, page, pageSize);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>GET /api/units/{id} — Get a single unit by ID.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UnitDto>>> GetUnit(int id)
    {
        var response = await _unitService.GetUnitByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>POST /api/units — Create a new selling unit.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UnitDto>>> CreateUnit([FromBody] SaveUnitRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UnitDto>.ErrorResult("Validation failed"));

        var response = await _unitService.CreateUnitAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetUnit), new { id = response.Data?.UnitId }, response);
    }

    /// <summary>PUT /api/units/{id} — Update an existing selling unit.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<UnitDto>>> UpdateUnit(int id, [FromBody] SaveUnitRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<UnitDto>.ErrorResult("Validation failed"));

        var response = await _unitService.UpdateUnitAsync(id, request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>DELETE /api/units/{id} — Hard-delete a unit (guarded by product FK check).</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteUnit(int id)
    {
        var response = await _unitService.DeleteUnitAsync(id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
