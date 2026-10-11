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
}
