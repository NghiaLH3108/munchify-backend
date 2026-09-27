using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;

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
        var response = await _productService.GetProductsForStaffAsync(filter);
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
}
