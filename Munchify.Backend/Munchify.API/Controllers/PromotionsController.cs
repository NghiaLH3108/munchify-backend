using System.Collections.Generic;
using System.Threading;
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

    /// <summary>
    /// GET /api/promotions – Danh sách khuyến mãi.
    /// Hỗ trợ cả phân trang cho Admin và trả về danh sách đầy đủ cho POS nếu không truyền tham số phân trang.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PromotionFilterParams filterParams, [FromQuery] int? pageIndex, CancellationToken cancellationToken)
    {
        if (Request.Query.ContainsKey("pageIndex") || Request.Query.ContainsKey("pageNumber") || Request.Query.ContainsKey("searchTerm") || Request.Query.ContainsKey("searchKeyword") || Request.Query.ContainsKey("isActive"))
        {
            if (pageIndex.HasValue) filterParams.PageNumber = pageIndex.Value;
            if (Request.Query.TryGetValue("searchKeyword", out var kw)) filterParams.SearchTerm = kw;
            var response = await _promotionService.GetAllPromotionsAsync(filterParams);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        var listResponse = await _promotionService.GetAllPromotionsListAsync(cancellationToken);
        return Ok(listResponse);
    }

    /// <summary>
    /// GET /api/promotions/{id} – Chi tiết khuyến mãi theo ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var response = await _promotionService.GetPromotionByIdAsync(id);
        if (!response.Success)
            return NotFound(response);

        return Ok(response);
    }

    /// <summary>
    /// POST /api/promotions – Tạo khuyến mãi mới (Admin).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request)
    {
        var response = await _promotionService.CreatePromotionAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return CreatedAtAction(nameof(GetById), new { id = response.Data?.PromotionId }, response);
    }

    /// <summary>
    /// PUT /api/promotions/{id} – Cập nhật khuyến mãi (Admin).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePromotionRequest request)
    {
        var response = await _promotionService.UpdatePromotionAsync(id, request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// PATCH /api/promotions/{id}/status – Bật/tắt trạng thái hoạt động của khuyến mãi.
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var response = await _promotionService.TogglePromotionStatusAsync(id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// POST /api/promotions/apply – Kiểm tra mã khuyến mãi và trả về số tiền giảm (POS).
    /// </summary>
    [HttpPost("apply")]
    public async Task<ActionResult<ApiResponse<ApplyPromotionResponse>>> ApplyPromotion(
        [FromBody] ApplyPromotionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<ApplyPromotionResponse>.ErrorResult("Dữ liệu không hợp lệ."));

        var response = await _promotionService.ApplyPromotionAsync(request, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
