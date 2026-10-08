using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class PromotionRepository : IPromotionRepository
{
    private readonly MunchifyDbContext _context;

    public PromotionRepository(MunchifyDbContext context)
    {
        _context = context;
    }

    public async Task<Promotion?> GetByCodeAsync(string promotionCode, CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .FirstOrDefaultAsync(
                p => p.PromotionCode.ToUpper() == promotionCode.ToUpper(),
                cancellationToken);
    }

    public async Task<List<Promotion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Promotions
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
