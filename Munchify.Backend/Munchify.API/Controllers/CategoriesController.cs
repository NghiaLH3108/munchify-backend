using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Categories;
using Munchify.DAL.DTOs.Common;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<CategoryDto>>>> GetCategories(
        [FromQuery] string searchTerm = "",
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var response = await _categoryService.GetCategoriesAsync(searchTerm, isActive, page, pageSize);
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        var response = await _categoryService.GetCategoryByIdAsync(id);
        if (!response.Success)
            return NotFound(response);
            
        return Ok(response);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] SaveCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.ErrorResult("Validation failed"));

        var response = await _categoryService.CreateCategoryAsync(request);
        if (!response.Success)
            return BadRequest(response);
            
        return CreatedAtAction(nameof(GetCategory), new { id = response.Data?.CategoryId }, response);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] SaveCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<CategoryDto>.ErrorResult("Validation failed"));

        var response = await _categoryService.UpdateCategoryAsync(id, request);
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
    {
        var response = await _categoryService.DeleteCategoryAsync(id);
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }
}
