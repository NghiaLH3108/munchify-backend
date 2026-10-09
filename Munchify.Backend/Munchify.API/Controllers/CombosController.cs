using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Combos;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CombosController : ControllerBase
{
    private readonly IComboService _comboService;

    public CombosController(IComboService comboService)
    {
        _comboService = comboService;
    }

    /// <summary>GET /api/combos — Paged combo list for Admin (active + inactive).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDetailDto>>>> GetCombos(
        [FromQuery] ComboFilterParams filter, CancellationToken cancellationToken)
    {
        var response = await _comboService.GetCombosAsync(filter, cancellationToken);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>GET /api/combos/{id} — Combo detail with components.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetCombo(int id, CancellationToken cancellationToken)
    {
        var response = await _comboService.GetComboByIdAsync(id, cancellationToken);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>POST /api/combos — Create combo with component products and quantities.</summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> CreateCombo(
        [FromBody] SaveComboRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ProductDetailDto>.ErrorResult("Dữ liệu không hợp lệ."));

        var response = await _comboService.CreateComboAsync(request, cancellationToken);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetCombo), new { id = response.Data?.ProductId }, response);
    }

    /// <summary>PUT /api/combos/{id} — Update components, selling price and info.</summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> UpdateCombo(
        int id, [FromBody] SaveComboRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ProductDetailDto>.ErrorResult("Dữ liệu không hợp lệ."));

        var response = await _comboService.UpdateComboAsync(id, request, cancellationToken);
        if (!response.Success)
            return response.StatusCode == 404 ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }

    /// <summary>DELETE /api/combos/{id} — Soft-delete if has history, otherwise hard-delete.</summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCombo(int id, CancellationToken cancellationToken)
    {
        var response = await _comboService.DeleteComboAsync(id, cancellationToken);
        if (!response.Success)
            return response.StatusCode == 404 ? NotFound(response) : BadRequest(response);

        return Ok(response);
    }
}
