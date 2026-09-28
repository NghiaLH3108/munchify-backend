using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Products;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IProductRepository _productRepository;

    public ProductService(IUnitOfWork unitOfWork, IProductRepository productRepository)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
    }

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsForStaffAsync(ProductFilterParams filter)
    {
        try
        {
            var (items, totalCount) = await _productRepository.GetProductsAsync(filter);

            var productDtos = items.Select(p => new ProductDto
            {
                ProductId = p.ProductId,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName ?? string.Empty,
                UnitId = p.UnitId,
                UnitName = p.Unit?.UnitName ?? string.Empty,
                ImageUrl = p.ImageUrl,
                IsCombo = p.IsCombo,
                IsActive = p.IsActive
            }).ToList();

            var pagedResult = new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageNumber = filter.Page,
                PageSize = filter.PageSize
            };

            return ApiResponse<PagedResult<ProductDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponse<PagedResult<ProductDto>>.ErrorResult($"Lỗi khi lấy danh sách sản phẩm: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductDetailDto>> GetProductDetailAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetProductByIdWithDetailsAsync(id);

            if (product == null)
            {
                return ApiResponse<ProductDetailDto>.ErrorResult("Không tìm thấy sản phẩm");
            }

            var detailDto = new ProductDetailDto
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName ?? string.Empty,
                UnitId = product.UnitId,
                UnitName = product.Unit?.UnitName ?? string.Empty,
                ImageUrl = product.ImageUrl,
                IsCombo = product.IsCombo,
                IsActive = product.IsActive,
                Description = product.Description,
                MinStockLevel = product.MinStockLevel,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                ComboItems = product.ProductComboParentProducts.Select(pc => new ComboItemDto
                {
                    ComponentProductId = pc.ComponentProductId,
                    ComponentProductName = pc.ComponentProduct?.ProductName ?? string.Empty,
                    Quantity = pc.Quantity,
                    UnitPrice = pc.ComponentProduct?.Price ?? 0,
                    UnitName = pc.ComponentProduct?.Unit?.UnitName ?? string.Empty
                }).ToList()
            };

            return ApiResponse<ProductDetailDto>.SuccessResult(detailDto);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDetailDto>.ErrorResult($"Lỗi khi lấy chi tiết sản phẩm: {ex.Message}");
        }
    }
    public async Task<ApiResponse<IEnumerable<ProductForStockDto>>> GetProductsForStockAsync(int? supplierId)
    {
        try
        {
            var products = await _productRepository.GetProductsForStockAsync(supplierId);
            
            var productDtos = products.Select(p => new ProductForStockDto
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Sku = p.ProductCode,
                Price = p.Price,
                UnitName = p.Unit != null ? p.Unit.UnitName : "N/A"
            }).ToList();

            return ApiResponse<IEnumerable<ProductForStockDto>>.SuccessResult(productDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ProductForStockDto>>.ErrorResult($"Lỗi khi lấy danh sách sản phẩm theo nhà cung cấp: {ex.Message}");
        }
    }
}
