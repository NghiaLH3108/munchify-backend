using System.Linq;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    IQueryable<Category> GetCategoriesQuery();
    Task<Category?> GetCategoryWithProductsByIdAsync(int id);
}
