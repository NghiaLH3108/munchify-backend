using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using System.Threading.Tasks;
using System.Linq;

namespace Munchify.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly MunchifyDbContext _context;

        public ProductsController(MunchifyDbContext context)
        {
            _context = context;
        }

        [HttpGet("for-stock")]
        public async Task<IActionResult> GetProductsForStock([FromQuery] int? supplierId)
        {
            var query = _context.Products.Include(p => p.Unit).AsQueryable();

            if (supplierId.HasValue)
            {
                query = query.Where(p => p.Category.Suppliers.Any(s => s.SupplierId == supplierId.Value));
            }

            var products = await query.Select(p => new {
                p.ProductId,
                p.ProductName,
                Sku = p.ProductCode,
                p.Price,
                UnitName = p.Unit != null ? p.Unit.UnitName : "N/A"
            }).ToListAsync();

            return Ok(new { data = products });
        }
    }
}
