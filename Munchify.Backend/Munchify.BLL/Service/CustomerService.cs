using System.Threading;
using System.Threading.Tasks;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Customers;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ApiResponse<CustomerDto>> GetCustomerByPhoneAsync(string phone, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByPhoneAsync(phone, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<CustomerDto>.ErrorResult($"Không tìm thấy khách hàng với số điện thoại '{phone}'");
        }

        return ApiResponse<CustomerDto>.SuccessResult(MapToDto(customer));
    }

    public async Task<ApiResponse<CustomerDto>> CreateCustomerAsync(CreateCustomerDto request, CancellationToken cancellationToken = default)
    {
        if (await _customerRepository.ExistsPhoneAsync(request.Phone, cancellationToken))
        {
            return ApiResponse<CustomerDto>.ErrorResult($"Số điện thoại '{request.Phone}' đã được đăng ký bởi khách hàng khác.");
        }

        var newCustomer = new Customer
        {
            CustomerName = request.CustomerName.Trim(),
            Phone = request.Phone.Trim(),
            LoyaltyPoints = 0
        };

        var createdCustomer = await _customerRepository.AddAsync(newCustomer, cancellationToken);
        return ApiResponse<CustomerDto>.SuccessResult(MapToDto(createdCustomer), "Thêm khách hàng thành công.");
    }

    private static CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerName = customer.CustomerName,
            Phone = customer.Phone,
            LoyaltyPoints = customer.LoyaltyPoints,
            CreatedAt = customer.CreatedAt
        };
    }

    public async Task<ApiResponse<PagedResult<CustomerDto>>> GetAllCustomersAsync(int pageIndex, int pageSize, string? searchKeyword, CancellationToken cancellationToken = default)
    {
        var (customers, totalCount) = await _customerRepository.GetAllAsync(pageIndex, pageSize, searchKeyword, cancellationToken);
        
        var dtoList = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Select(customers, c => MapToDto(c)));
        var pagedResult = new PagedResult<CustomerDto>
        {
            Items = dtoList,
            TotalCount = totalCount,
            PageNumber = pageIndex,
            PageSize = pageSize
        };
        
        return ApiResponse<PagedResult<CustomerDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<PagedResult<CustomerPointHistoryDto>>> GetCustomerPointsHistoryAsync(int customerId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
        {
            return ApiResponse<PagedResult<CustomerPointHistoryDto>>.ErrorResult($"Không tìm thấy khách hàng với ID {customerId}");
        }

        var (history, totalCount) = await _customerRepository.GetPointHistoryAsync(customerId, pageIndex, pageSize, cancellationToken);
        
        var pagedResult = new PagedResult<CustomerPointHistoryDto>
        {
            Items = System.Linq.Enumerable.ToList(history),
            TotalCount = totalCount,
            PageNumber = pageIndex,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<CustomerPointHistoryDto>>.SuccessResult(pagedResult);
    }
}
