using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResult<StockMovementDto>>> GetStockMovementsAsync(
        int? productId = null, 
        string? movementType = null, 
        DateTime? from = null, 
        DateTime? to = null, 
        int page = 1, 
        int pageSize = 20)
    {
        var pagedEntities = await _unitOfWork.StockMovements.GetPagedAsync(productId, movementType, from, to, page, pageSize);

        var mappedItems = _mapper.Map<IEnumerable<StockMovementDto>>(pagedEntities.Items);

        var pagedResult = new PagedResult<StockMovementDto>
        {
            Items = mappedItems,
            TotalCount = pagedEntities.TotalCount,
            PageNumber = pagedEntities.PageNumber,
            PageSize = pagedEntities.PageSize
        };

        return ApiResponse<PagedResult<StockMovementDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<StockMovementDto>> AdjustStockAsync(AdjustStockRequest request, int userId = 1)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                return ApiResponse<StockMovementDto>.ErrorResult("Sản phẩm không tồn tại");
            }

            if (product.StockQuantity + request.QuantityChange < 0)
            {
                return ApiResponse<StockMovementDto>.ErrorResult("Số lượng tồn kho không đủ (không thể âm)");
            }

            var previousQuantity = product.StockQuantity;
            product.StockQuantity += request.QuantityChange;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Products.Update(product);

            var movement = new Munchify.DAL.Entities.StockMovement
            {
                ProductId = request.ProductId,
                MovementType = request.MovementType,
                Quantity = request.QuantityChange,
                PreviousQuantity = previousQuantity,
                NewQuantity = product.StockQuantity,
                ReferenceType = "MANUAL",
                Reason = request.Reason,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.StockMovements.AddAsync(movement);
            await _unitOfWork.CompleteAsync();
            await transaction.CommitAsync();

            var resultDto = _mapper.Map<StockMovementDto>(movement);
            resultDto.ProductName = product.ProductName;
            return ApiResponse<StockMovementDto>.SuccessResult(resultDto);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<StockMovementDto>.ErrorResult($"Lỗi khi điều chỉnh tồn kho: {ex.Message}");
        }
    }
}
