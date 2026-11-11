using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Suppliers;

namespace Munchify.BLL.Service.Interfaces;

public interface ISupplierService
{
    Task<ApiResponse<IEnumerable<SupplierDto>>> GetSuppliersAsync(string? search = null, bool? isActive = null);
    Task<ApiResponse<SupplierDto>> GetSupplierByIdAsync(int id);
    Task<ApiResponse<SupplierDto>> CreateSupplierAsync(SaveSupplierRequest request);
    Task<ApiResponse<SupplierDto>> UpdateSupplierAsync(int id, SaveSupplierRequest request);
    Task<ApiResponse<bool>> DeleteSupplierAsync(int id);
}
