using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Customers;

namespace Munchify.BLL.Service.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerDto>> GetCustomerByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<CustomerDto>>> GetAllCustomersAsync(int pageIndex, int pageSize, string? searchKeyword, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResult<CustomerPointHistoryDto>>> GetCustomerPointsHistoryAsync(int customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);
}
