using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IPromotionRepository
{
    Task<Promotion?> GetByCodeAsync(string promotionCode, CancellationToken cancellationToken = default);
    Task<System.Collections.Generic.List<Promotion>> GetAllAsync(CancellationToken cancellationToken = default);
}
