using System.Threading;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Customers;

namespace Munchify.BLL.Service.Interfaces;

public interface ICustomerService
{
    Task<ApiResponse<CustomerDto>> GetCustomerByPhoneAsync(string phone, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto request, CancellationToken cancellationToken = default);
}
