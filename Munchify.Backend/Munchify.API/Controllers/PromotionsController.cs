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
    /// POST /api/promotions/apply – Kiểm tra mã khuyến mãi và trả về số tiền giảm.
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

    /// <summary>
    /// GET /api/promotions – Danh sách tất cả khuyến mãi (Admin).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<PromotionDto>>>> GetPromotions(
        CancellationToken cancellationToken)
    {
        var response = await _promotionService.GetAllPromotionsAsync(cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// POST /api/promotions – Tạo khuyến mãi mới (Admin).
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<PromotionDto>>> CreatePromotion(
        [FromBody] SavePromotionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PromotionDto>.ErrorResult("Dữ liệu không hợp lệ."));

        var response = await _promotionService.CreatePromotionAsync(request, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    /// <summary>
    /// PUT /api/promotions/{id} – Cập nhật khuyến mãi (Admin).
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PromotionDto>>> UpdatePromotion(
        int id,
        [FromBody] SavePromotionRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<PromotionDto>.ErrorResult("Dữ liệu không hợp lệ."));

        var response = await _promotionService.UpdatePromotionAsync(id, request, cancellationToken);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
