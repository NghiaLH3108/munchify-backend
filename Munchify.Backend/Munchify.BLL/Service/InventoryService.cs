using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;
using Munchify.DAL.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<ApiResponse<IEnumerable<ForecastItemDto>>> GetForecastingAsync(int daysHistory = 30)
    {
        // 1. Fetch sales data for the last N days
        var salesData = await _unitOfWork.StockMovements.GetSalesDataForForecastingAsync(daysHistory);

        // 1.5. Fetch pending import quantities to avoid duplicate drafts
        var pendingImports = await _unitOfWork.StockImports.GetPendingImportQuantitiesAsync();

        // 2. Fetch all active single products
        var activeProducts = await _unitOfWork.Products.GetProductsQuery()
            .Where(p => !p.IsCombo && p.IsActive)
            .Include(p => p.Category)
                .ThenInclude(c => c.Suppliers)
            .Include(p => p.Unit)
            .ToListAsync();

        var forecastList = new List<ForecastItemDto>();
        int leadTime = 3;
        int bufferDays = 7;

        foreach (var p in activeProducts)
        {
            double totalSold = salesData.ContainsKey(p.ProductId) ? salesData[p.ProductId] : 0;
            double vAvg = totalSold / daysHistory;
            double daysRemaining = vAvg > 0 ? p.StockQuantity / vAvg : 999;
            
            string riskLevel = "SAFE";
            if (daysRemaining <= leadTime || p.StockQuantity <= p.MinStockLevel)
            {
                riskLevel = daysRemaining <= 1 || p.StockQuantity == 0 ? "CRITICAL" : "WARNING";
            }

            // Đề xuất thông minh (Smart Recommendation) bằng Buffer-days
            double suggestedQty = 0;
            if (riskLevel != "SAFE")
            {
                suggestedQty = Math.Max(0, Math.Ceiling(vAvg * bufferDays + p.MinStockLevel - p.StockQuantity));
                
                // Trừ đi số lượng đang được lên đơn (Draft/Pending)
                int incomingQty = pendingImports.ContainsKey(p.ProductId) ? pendingImports[p.ProductId] : 0;
                
                if (incomingQty > 0)
                {
                    if (incomingQty >= suggestedQty)
                    {
                        // Đã lên đơn đủ số lượng, không gợi ý nhập thêm và đổi trạng thái
                        suggestedQty = 0;
                        riskLevel = "ORDERED";
                    }
                    else
                    {
                        // Chỉ đề xuất nhập thêm phần còn thiếu
                        suggestedQty -= incomingQty;
                    }
                }
            }

            // Determine default supplier from Category
            var defaultSupplier = p.Category?.Suppliers?.FirstOrDefault();

            // Only add items that need warning or import
            if (riskLevel != "SAFE" || suggestedQty > 0)
            {
                forecastList.Add(new ForecastItemDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    UnitName = p.Unit?.UnitName ?? "Item",
                    StockQuantity = p.StockQuantity,
                    MinStockLevel = p.MinStockLevel,
                    AvgDailyUsage = Math.Round(vAvg, 2),
                    DaysRemaining = Math.Round(daysRemaining, 1),
                    RiskLevel = riskLevel,
                    SuggestedImportQty = suggestedQty,
                    DefaultSupplierId = defaultSupplier?.SupplierId,
                    DefaultSupplierName = defaultSupplier?.SupplierName
                });
            }
        }

        // Sort by risk (CRITICAL first, then WARNING, then ORDERED) and DaysRemaining
        var sortedList = forecastList
            .OrderBy(x => x.RiskLevel == "CRITICAL" ? 0 : (x.RiskLevel == "WARNING" ? 1 : 2))
            .ThenBy(x => x.DaysRemaining)
            .ToList();

        return ApiResponse<IEnumerable<ForecastItemDto>>.SuccessResult(sortedList);
    }

    public async Task<ApiResponse<bool>> GenerateDraftImportsFromForecastAsync(List<ForecastItemDto> items, int userId = 1)
    {
        if (items == null || !items.Any()) return ApiResponse<bool>.ErrorResult("Không có sản phẩm nào để tạo phiếu nhập.");

        // Group by supplier (assuming default supplier id is 1 if not provided for now, as DB design requires SupplierId)
        var groupedItems = items.Where(i => i.SuggestedImportQty > 0).GroupBy(i => i.DefaultSupplierId ?? 1);

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            foreach (var group in groupedItems)
            {
                var stockImport = new DAL.Entities.StockImport
                {
                    ImportCode = $"IMP-AUTO-{DateTime.Now:yyyyMMddHHmmss}-{group.Key}",
                    SupplierId = group.Key,
                    UserId = userId,
                    Status = "DRAFT",
                    ImportDate = DateTime.Now,
                    Notes = "Generated automatically by AI Low-Stock Forecast",
                    TotalAmount = 0m // Draft, prices will be updated later
                };

                await _unitOfWork.StockImports.AddAsync(stockImport);
                await _unitOfWork.CompleteAsync(); // Save to get ImportId

                decimal totalAmount = 0;
                foreach (var item in group)
                {
                    // TÌM GIÁ NHẬP LẦN GẦN NHẤT
                    var latestImportPrice = await _unitOfWork.StockImports.GetLatestImportPriceAsync(item.ProductId);
                    
                    decimal estimatedImportPrice = 0m;

                    if (latestImportPrice.HasValue && latestImportPrice.Value > 0)
                    {
                        // Lấy giá của lần nhập gần nhất
                        estimatedImportPrice = latestImportPrice.Value;
                    }
                    else
                    {
                        // Fallback: Chưa nhập bao giờ thì lấy 70% giá bán lẻ
                        var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                        if (product != null)
                        {
                            estimatedImportPrice = product.Price * 0.7m;
                        }
                    }

                    int qty = (int)item.SuggestedImportQty;
                    decimal rowTotal = qty * estimatedImportPrice;

                    var detail = new DAL.Entities.StockImportDetail
                    {
                        ImportId = stockImport.ImportId,
                        ProductId = item.ProductId,
                        Quantity = qty, 
                        ImportPrice = estimatedImportPrice, 
                        TotalAmount = rowTotal 
                    };
                    totalAmount += detail.TotalAmount;
                    stockImport.StockImportDetails.Add(detail);
                }
                
                stockImport.TotalAmount = totalAmount;
                _unitOfWork.StockImports.Update(stockImport);
            }

            await _unitOfWork.CompleteAsync();
            await transaction.CommitAsync();

            return ApiResponse<bool>.SuccessResult(true, "Đã tạo các phiếu nhập nháp thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ApiResponse<bool>.ErrorResult($"Lỗi khi tạo phiếu nhập: {ex.Message}");
        }
    }
}
