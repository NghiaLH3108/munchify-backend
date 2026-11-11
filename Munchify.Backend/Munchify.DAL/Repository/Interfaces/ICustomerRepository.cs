using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Customers;
using Munchify.DAL.Entities;

namespace Munchify.DAL.Repository.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);
    Task<(System.Collections.Generic.IEnumerable<Customer> Customers, int TotalCount)> GetAllAsync(int pageIndex, int pageSize, string? searchKeyword, CancellationToken cancellationToken = default);
    Task<(System.Collections.Generic.IEnumerable<CustomerPointHistoryDto> History, int TotalCount)> GetPointHistoryAsync(int customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
}
