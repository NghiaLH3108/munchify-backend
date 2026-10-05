using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Entities;

namespace Munchify.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetProducts([FromQuery] ProductFilterParams filter)
    {
        var response = await _productService.GetProductsAsync(filter);
        if (!response.Success)
            return BadRequest(response);
            
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> GetProduct(int id)
    {
        var response = await _productService.GetProductDetailAsync(id);
        if (!response.Success)
            return NotFound(response);
            
        return Ok(response);
    }

    [HttpGet("for-stock")]
    public async Task<IActionResult> GetProductsForStock([FromQuery] int? supplierId)
    {
        var response = await _productService.GetProductsForStockAsync(supplierId);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> CreateProduct([FromBody] ProductRequest request)
    {
        var response = await _productService.CreateProductAsync(request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDetailDto>>> UpdateProduct(int id, [FromBody] ProductRequest request)
    {
        var response = await _productService.UpdateProductAsync(id, request);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<ApiResponse<bool>>> ToggleProductStatus(int id)
    {
        var response = await _productService.ToggleProductStatusAsync(id);
        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
