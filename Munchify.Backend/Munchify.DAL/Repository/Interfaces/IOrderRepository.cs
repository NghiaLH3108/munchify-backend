using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Orders;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface IOrderRepository
{
    Task<Order> AddAsync(Order order, CancellationToken cancellationToken = default);
    Task<PagedResult<Order>> GetPagedAsync(OrderFilterRequest filter, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int orderId, CancellationToken cancellationToken = default);
}
