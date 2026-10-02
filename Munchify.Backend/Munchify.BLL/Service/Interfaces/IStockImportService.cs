using System.Collections.Generic;
using System.Threading.Tasks;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.StockImports;

namespace Munchify.BLL.Service.Interfaces
{
    public interface IStockImportService
    {
        Task<ApiResponse<int>> CreateDraftImportAsync(CreateImportRequest request, int userId);
        Task<ApiResponse<IEnumerable<StockImportDto>>> GetImportsAsync();
        Task<ApiResponse<StockImportDto>> GetImportByIdAsync(int id);
        Task<ApiResponse<bool>> CancelImportAsync(int id);
        Task<ApiResponse<bool>> CompleteImportAsync(int id, int userId);
    }
}
