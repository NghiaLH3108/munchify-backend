using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class PromotionRepository : GenericRepository<Promotion>, IPromotionRepository
{
    public PromotionRepository(MunchifyDbContext context) : base(context)
    {
    }

    public async Task<Promotion?> GetPromotionWithDetailsAsync(int id)
    {
        return await _context.Set<Promotion>()
            .Include(p => p.Products)
            .FirstOrDefaultAsync(p => p.PromotionId == id);
    }

    public async Task<bool> ExistsCodeAsync(string code, int? excludeId = null)
    {
        var query = _context.Set<Promotion>().AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PromotionId != excludeId.Value);
        }
        return await query.AnyAsync(p => p.PromotionCode.ToLower() == code.ToLower());
    }
}
