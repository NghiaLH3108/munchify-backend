using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IPromotionRepository : IGenericRepository<Promotion>
{
    Task<Promotion?> GetPromotionWithDetailsAsync(int id);
    Task<bool> ExistsCodeAsync(string code, int? excludeId = null);
    Task<Promotion?> GetByCodeAsync(string promotionCode, CancellationToken cancellationToken = default);
    Task<List<Promotion>> GetAllListAsync(CancellationToken cancellationToken = default);
}
