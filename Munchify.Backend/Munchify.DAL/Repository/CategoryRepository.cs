using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.DAL.Data;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.DAL.Repository;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(MunchifyDbContext context) : base(context)
    {
    }

    public IQueryable<Category> GetCategoriesQuery()
    {
        return _context.Categories
            .Include(c => c.Products)
            .AsQueryable();
    }

    public async Task<Category?> GetCategoryWithProductsByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }
}
