using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Units;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class UnitService : IUnitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly MunchifyDbContext _context;

    public UnitService(IUnitOfWork unitOfWork, IMapper mapper, MunchifyDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ApiResponse<PagedResult<UnitDto>>> GetUnitsAsync(
        string searchTerm = "", bool? isActive = null, int page = 1, int pageSize = 10)
    {
        var allUnits = await _unitOfWork.Units.GetAllAsync();

        var query = allUnits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.UnitName.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase) ||
                                     (u.Description != null && u.Description.Contains(searchTerm, System.StringComparison.OrdinalIgnoreCase)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        int totalCount = query.Count();

        var items = query.OrderBy(u => u.UnitName)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize)
                         .ToList();

        var mappedItems = _mapper.Map<System.Collections.Generic.IEnumerable<UnitDto>>(items);
        var pagedResult = new PagedResult<UnitDto>
        {
            Items = mappedItems,
            TotalCount = totalCount,
            PageNumber = page,
            PageSize = pageSize
        };

        return ApiResponse<PagedResult<UnitDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<UnitDto>> GetUnitByIdAsync(int id)
    {
        var unit = await _unitOfWork.Units.GetByIdAsync(id);

        if (unit == null)
            return ApiResponse<UnitDto>.ErrorResult("Unit not found.");

        var dto = _mapper.Map<UnitDto>(unit);
        return ApiResponse<UnitDto>.SuccessResult(dto);
    }

    public async Task<ApiResponse<UnitDto>> CreateUnitAsync(SaveUnitRequest request)
    {
        var unit = _mapper.Map<Unit>(request);

        await _unitOfWork.Units.AddAsync(unit);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<UnitDto>(unit);
        return ApiResponse<UnitDto>.SuccessResult(dto, "Unit created successfully.");
    }

    public async Task<ApiResponse<UnitDto>> UpdateUnitAsync(int id, SaveUnitRequest request)
    {
        var unit = await _unitOfWork.Units.GetByIdAsync(id);

        if (unit == null)
            return ApiResponse<UnitDto>.ErrorResult("Unit not found.");

        _mapper.Map(request, unit);

        _unitOfWork.Units.Update(unit);
        await _unitOfWork.CompleteAsync();

        var dto = _mapper.Map<UnitDto>(unit);
        return ApiResponse<UnitDto>.SuccessResult(dto, "Unit updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteUnitAsync(int id)
    {
        var unit = await _unitOfWork.Units.GetByIdAsync(id);

        if (unit == null)
            return ApiResponse<bool>.ErrorResult("Unit not found.");

        // Check if the unit is referenced by any products before hard-deleting
        bool hasProducts = await _context.Products.AnyAsync(p => p.UnitId == id);
        if (hasProducts)
            return ApiResponse<bool>.ErrorResult("Không thể xóa đơn vị đang được sử dụng bởi sản phẩm.");

        _unitOfWork.Units.Remove(unit);
        await _unitOfWork.CompleteAsync();

        return ApiResponse<bool>.SuccessResult(true, "Unit deleted successfully.");
    }
}
