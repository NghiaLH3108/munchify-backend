using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Categories;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<CategoryDto>>> GetCategoriesAsync(string searchTerm = "", bool? isActive = null, int page = 1, int pageSize = 10)
    {
        var allCategories = await _unitOfWork.Categories.GetAllAsync();

        var query = allCategories.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                                     (c.Description != null && c.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        int totalCount = query.Count();
        
        // Sorting by newest (mock, can be dynamic later)
        var items = query.OrderByDescending(c => c.CreatedAt)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

        var mappedItems = _mapper.Map<IEnumerable<CategoryDto>>(items);
        var pagedResult = new PagedResult<CategoryDto>(mappedItems, totalCount, page, pageSize);

        return ApiResponse<PagedResult<CategoryDto>>.Ok(pagedResult);
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        
        if (category == null)
        {
            return ApiResponse<CategoryDto>.Error("Category not found.");
        }

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(dto);
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(SaveCategoryRequest request)
    {
        var category = _mapper.Map<Category>(request);
        category.CreatedAt = DateTime.Now;
        category.UpdatedAt = DateTime.Now;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(dto, "Category created successfully.");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, SaveCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        
        if (category == null)
        {
            return ApiResponse<CategoryDto>.Error("Category not found.");
        }

        _mapper.Map(request, category);
        category.UpdatedAt = DateTime.Now;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(dto, "Category updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        
        if (category == null)
        {
            return ApiResponse<bool>.Error("Category not found.");
        }

        // Using Soft Delete as it is generally safer for referential integrity
        category.IsActive = false;
        category.UpdatedAt = DateTime.Now;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.CompleteAsync();

        return ApiResponse<bool>.Ok(true, "Category deleted (deactivated) successfully.");
    }
}
