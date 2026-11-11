using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        var query = _unitOfWork.Categories.GetCategoriesQuery();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(c => c.CategoryName.ToLower().Contains(term) || 
                                     (c.Description != null && c.Description.ToLower().Contains(term)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        int totalCount = await query.CountAsync();
        
        var items = await query.OrderByDescending(c => c.CreatedAt)
                               .Skip((page - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync();

        var mappedItems = _mapper.Map<IEnumerable<CategoryDto>>(items);
        var pagedResult = new PagedResult<CategoryDto> 
        { 
            Items = mappedItems, 
            TotalCount = totalCount, 
            PageNumber = page, 
            PageSize = pageSize 
        };

        return ApiResponse<PagedResult<CategoryDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetCategoryWithProductsByIdAsync(id);
        
        if (category == null)
        {
            return ApiResponse<CategoryDto>.ErrorResult("Category not found.");
        }

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(SaveCategoryRequest request)
    {
        var category = _mapper.Map<Category>(request);
        category.CreatedAt = DateTime.Now;
        category.UpdatedAt = DateTime.Now;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.SuccessResult(dto, "Category created successfully.");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, SaveCategoryRequest request)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        
        if (category == null)
        {
            return ApiResponse<CategoryDto>.ErrorResult("Category not found.");
        }

        _mapper.Map(request, category);
        category.UpdatedAt = DateTime.Now;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.SuccessResult(dto, "Category updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        
        if (category == null)
        {
            return ApiResponse<bool>.ErrorResult("Category not found.");
        }

        // Using Soft Delete as it is generally safer for referential integrity
        category.IsActive = false;
        category.UpdatedAt = DateTime.Now;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.CompleteAsync();

        return ApiResponse<bool>.SuccessResult(true, "Category deleted (deactivated) successfully.");
    }
}
