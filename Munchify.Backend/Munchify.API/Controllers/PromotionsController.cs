using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Promotions;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PromotionFilterParams filterParams)
    {
        var response = await _promotionService.GetAllPromotionsAsync(filterParams);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _promotionService.GetPromotionByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request)
    {
        var response = await _promotionService.CreatePromotionAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.PromotionId }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePromotionRequest request)
    {
        var response = await _promotionService.UpdatePromotionAsync(id, request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var response = await _promotionService.TogglePromotionStatusAsync(id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
