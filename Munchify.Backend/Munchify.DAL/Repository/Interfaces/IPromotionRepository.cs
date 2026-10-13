using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IPromotionRepository : IGenericRepository<Promotion>
{
    Task<Promotion?> GetPromotionWithDetailsAsync(int id);
    Task<bool> ExistsCodeAsync(string code, int? excludeId = null);
}
