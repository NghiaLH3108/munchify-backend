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

    public async Task<ApiResponse<PagedResult<ProductDto>>> GetProductsAsync(ProductFilterParams filter)
    {
        try
        {
            var query = _productRepository.GetProductsQuery();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where(p => p.ProductName.ToLower().Contains(searchTerm) || 
                                         p.ProductCode.ToLower().Contains(searchTerm));
            }

            if (filter.CategoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
            }

            if (filter.UnitId.HasValue)
            {
                query = query.Where(p => p.UnitId == filter.UnitId.Value);
            }

            if (filter.IsCombo.HasValue)
            {
                query = query.Where(p => p.IsCombo == filter.IsCombo.Value);
            }

            if (filter.IsActive.HasValue)
            {
                query = query.Where(p => p.IsActive == filter.IsActive.Value);
            }

            var totalCount = query.Count();

            var items = query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

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
                MinStockLevel = p.MinStockLevel,
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
    public async Task<ApiResponse<ProductDetailDto>> CreateProductAsync(ProductRequest request)
    {
        try
        {
            var product = new Munchify.DAL.Entities.Product
            {
                ProductCode = string.IsNullOrWhiteSpace(request.ProductCode) ? $"SP{DateTime.Now.Ticks.ToString().Substring(10)}" : request.ProductCode,
                ProductName = request.ProductName,
                CategoryId = request.CategoryId,
                UnitId = request.UnitId,
                Description = request.Description,
                Price = request.Price,
                MinStockLevel = request.MinStockLevel,
                ImageUrl = request.ImageUrl,
                IsCombo = request.IsCombo,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                StockQuantity = 0
            };

            if (request.IsCombo && request.ComboItems != null)
            {
                foreach (var item in request.ComboItems)
                {
                    product.ProductComboParentProducts.Add(new Munchify.DAL.Entities.ProductCombo
                    {
                        ComponentProductId = item.ComponentProductId,
                        Quantity = item.Quantity
                    });
                }
            }

            await _productRepository.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            return await GetProductDetailAsync(product.ProductId);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDetailDto>.ErrorResult($"Lỗi khi tạo sản phẩm: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ProductDetailDto>> UpdateProductAsync(int id, ProductRequest request)
    {
        try
        {
            var product = await _productRepository.GetProductByIdWithDetailsAsync(id);
            if (product == null)
            {
                return ApiResponse<ProductDetailDto>.ErrorResult("Không tìm thấy sản phẩm");
            }

            product.ProductName = request.ProductName;
            product.CategoryId = request.CategoryId;
            product.UnitId = request.UnitId;
            if (!string.IsNullOrWhiteSpace(request.ProductCode))
                product.ProductCode = request.ProductCode;
            product.Description = request.Description;
            product.Price = request.Price;
            product.MinStockLevel = request.MinStockLevel;
            product.ImageUrl = request.ImageUrl;
            product.IsCombo = request.IsCombo;
            product.IsActive = request.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            if (request.IsCombo)
            {
                product.ProductComboParentProducts.Clear();
                if (request.ComboItems != null)
                {
                    foreach (var item in request.ComboItems)
                    {
                        product.ProductComboParentProducts.Add(new Munchify.DAL.Entities.ProductCombo
                        {
                            ParentProductId = product.ProductId,
                            ComponentProductId = item.ComponentProductId,
                            Quantity = item.Quantity
                        });
                    }
                }
            }
            else
            {
                product.ProductComboParentProducts.Clear();
            }

            _productRepository.Update(product);
            await _unitOfWork.CompleteAsync();

            return await GetProductDetailAsync(product.ProductId);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDetailDto>.ErrorResult($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ToggleProductStatusAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return ApiResponse<bool>.ErrorResult("Không tìm thấy sản phẩm");
            }

            product.IsActive = !product.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            _productRepository.Update(product);
            await _unitOfWork.CompleteAsync();

            return ApiResponse<bool>.SuccessResult(product.IsActive, "Cập nhật trạng thái thành công");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.ErrorResult($"Lỗi khi cập nhật trạng thái: {ex.Message}");
        }
    }
}
