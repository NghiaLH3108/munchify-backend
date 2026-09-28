using System;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IStockMovementRepository : IGenericRepository<StockMovement>
{
    Task<PagedResult<StockMovement>> GetPagedAsync(
        int? productId = null, 
        string? movementType = null, 
        DateTime? from = null, 
        DateTime? to = null, 
        int page = 1, 
        int pageSize = 20);
}
