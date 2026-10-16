using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    public async Task<Promotion?> GetByCodeAsync(string promotionCode, CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .FirstOrDefaultAsync(
                p => p.PromotionCode.ToUpper() == promotionCode.ToUpper(),
                cancellationToken);
    }

    public async Task<List<Promotion>> GetAllListAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
