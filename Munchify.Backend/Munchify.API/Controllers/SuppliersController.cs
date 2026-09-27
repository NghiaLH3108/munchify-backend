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
    public class SuppliersController : ControllerBase
    {
        private readonly MunchifyDbContext _context;

        public SuppliersController(MunchifyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuppliers()
        {
            var suppliers = await _context.Suppliers.Select(s => new {
                s.SupplierId,
                s.SupplierName,
                s.ContactName,
                s.Phone,
                s.Address,
                s.IsActive
            }).ToListAsync();
            return Ok(new { data = suppliers });
        }
    }
}
