using System.Threading.Tasks;
using Munchify.DAL.DTOs.Categories;
using Munchify.DAL.DTOs.Common;

namespace Munchify.BLL.Service.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<PagedResult<CategoryDto>>> GetCategoriesAsync(string searchTerm = "", bool? isActive = null, int page = 1, int pageSize = 10);
    Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> CreateCategoryAsync(SaveCategoryRequest request);
    Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, SaveCategoryRequest request);
    Task<ApiResponse<bool>> DeleteCategoryAsync(int id);
}
