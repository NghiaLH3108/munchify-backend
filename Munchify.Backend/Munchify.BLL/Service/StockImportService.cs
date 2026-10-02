using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.StockImports;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service
{
    public class StockImportService : IStockImportService
    {
        private readonly IStockImportRepository _stockImportRepository;
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<StockMovement> _stockMovementRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StockImportService(
            IStockImportRepository stockImportRepository,
            IGenericRepository<Product> productRepository,
            IGenericRepository<StockMovement> stockMovementRepository,
            IUnitOfWork unitOfWork)
        {
            _stockImportRepository = stockImportRepository;
            _productRepository = productRepository;
            _stockMovementRepository = stockMovementRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<int>> CreateDraftImportAsync(CreateImportRequest request, int userId)
        {
            var response = new ApiResponse<int>();

            try
            {
                // Generate a unique ImportCode
                string importCode = $"IMP{DateTime.Now:yyyyMMddHHmmss}";

                var newImport = new StockImport
                {
                    ImportCode = importCode,
                    SupplierId = request.SupplierId,
                    UserId = userId,
                    Status = "DRAFT", // defined in Enums/StockImportStatus.cs
                    ImportDate = DateTime.Now,
                    Notes = request.Notes,
                    StockImportDetails = new List<StockImportDetail>()
                };

                decimal totalAmount = 0;

                foreach (var item in request.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || !product.IsActive)
                    {
                        response.Success = false;
                        response.Message = $"Product with ID {item.ProductId} not found or inactive.";
                        return response;
                    }

                    var detailTotalAmount = item.Quantity * item.ImportPrice;
                    totalAmount += detailTotalAmount;

                    newImport.StockImportDetails.Add(new StockImportDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        ImportPrice = item.ImportPrice,
                        TotalAmount = detailTotalAmount
                    });
                }

                newImport.TotalAmount = totalAmount;

                await _stockImportRepository.AddAsync(newImport);
                await _unitOfWork.CompleteAsync();

                response.Success = true;
                response.Message = "Draft stock import created successfully.";
                response.Data = newImport.ImportId;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error occurred while creating stock import.";
                response.Errors = new List<string> { ex.Message };
            }

            return response;
        }

        public async Task<ApiResponse<IEnumerable<StockImportDto>>> GetImportsAsync()
        {
            var response = new ApiResponse<IEnumerable<StockImportDto>>();
            try
            {
                var imports = await _stockImportRepository.GetAllWithDetailsAsync();
                var importDtos = imports.Select(i => new StockImportDto
                {
                    ImportId = i.ImportId,
                    ImportCode = i.ImportCode,
                    SupplierId = i.SupplierId,
                    SupplierName = i.Supplier?.SupplierName ?? "Unknown",
                    UserId = i.UserId,
                    UserName = i.User?.FullName ?? "Unknown",
                    TotalAmount = i.TotalAmount,
                    Status = i.Status,
                    ImportDate = i.ImportDate,
                    Notes = i.Notes,
                    Details = i.StockImportDetails.Select(d => new ImportDetailResponseDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.ProductName ?? "Unknown",
                        UnitName = d.Product?.Unit?.UnitName ?? "Unknown",
                        Quantity = d.Quantity,
                        ImportPrice = d.ImportPrice,
                        TotalAmount = d.TotalAmount
                    }).ToList()
                }).ToList();

                response.Success = true;
                response.Data = importDtos;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error occurred while fetching stock imports.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ApiResponse<StockImportDto>> GetImportByIdAsync(int id)
        {
            var response = new ApiResponse<StockImportDto>();
            try
            {
                var import = await _stockImportRepository.GetByIdWithDetailsAsync(id);
                if (import == null)
                {
                    response.Success = false;
                    response.Message = $"Stock import with ID {id} not found.";
                    return response;
                }

                var importDto = new StockImportDto
                {
                    ImportId = import.ImportId,
                    ImportCode = import.ImportCode,
                    SupplierId = import.SupplierId,
                    SupplierName = import.Supplier?.SupplierName ?? "Unknown",
                    UserId = import.UserId,
                    UserName = import.User?.FullName ?? "Unknown",
                    TotalAmount = import.TotalAmount,
                    Status = import.Status,
                    ImportDate = import.ImportDate,
                    Notes = import.Notes,
                    Details = import.StockImportDetails.Select(d => new ImportDetailResponseDto
                    {
                        ProductId = d.ProductId,
                        ProductName = d.Product?.ProductName ?? "Unknown",
                        UnitName = d.Product?.Unit?.UnitName ?? "Unknown",
                        Quantity = d.Quantity,
                        ImportPrice = d.ImportPrice,
                        TotalAmount = d.TotalAmount
                    }).ToList()
                };

                response.Success = true;
                response.Data = importDto;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error occurred while fetching stock import details.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }
        public async Task<ApiResponse<bool>> CancelImportAsync(int id)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var import = await _stockImportRepository.GetByIdAsync(id);
                if (import == null)
                {
                    response.Success = false;
                    response.Message = $"Stock import with ID {id} not found.";
                    return response;
                }

                if (import.Status != "DRAFT")
                {
                    response.Success = false;
                    response.Message = "Only DRAFT imports can be cancelled.";
                    return response;
                }

                import.Status = "CANCELLED";
                _stockImportRepository.Update(import);
                await _unitOfWork.CompleteAsync();

                response.Success = true;
                response.Data = true;
                response.Message = "Import cancelled successfully.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error occurred while cancelling stock import.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ApiResponse<bool>> CompleteImportAsync(int id, int userId)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var import = await _stockImportRepository.GetByIdWithDetailsAsync(id);
                if (import == null)
                {
                    response.Success = false;
                    response.Message = $"Stock import with ID {id} not found.";
                    return response;
                }

                if (import.Status != "DRAFT")
                {
                    response.Success = false;
                    response.Message = "Only DRAFT imports can be completed.";
                    return response;
                }

                using var transaction = await _unitOfWork.BeginTransactionAsync();
                
                try 
                {
                    // 1. Update status
                    import.Status = "COMPLETED";
                    _stockImportRepository.Update(import);

                    // 2. Update stock & add movements
                    foreach (var detail in import.StockImportDetails)
                    {
                        var product = await _productRepository.GetByIdAsync(detail.ProductId);
                        if (product != null)
                        {
                            int prevQty = product.StockQuantity;
                            int newQty = prevQty + detail.Quantity;

                            product.StockQuantity = newQty;
                            _productRepository.Update(product);

                            var movement = new StockMovement
                            {
                                ProductId = product.ProductId,
                                UserId = userId,
                                MovementType = "IMPORT",
                                Quantity = detail.Quantity,
                                PreviousQuantity = prevQty,
                                NewQuantity = newQty,
                                ReferenceType = "STOCK_IMPORT",
                                ReferenceId = import.ImportId,
                                Reason = $"Import completed for {import.ImportCode}",
                                CreatedAt = DateTime.Now
                            };
                            
                            await _stockMovementRepository.AddAsync(movement);
                        }
                    }

                    // Save all changes in the transaction
                    await _unitOfWork.CompleteAsync();
                    await transaction.CommitAsync();

                    response.Success = true;
                    response.Data = true;
                    response.Message = "Import completed and stock updated successfully.";
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Error occurred while completing stock import.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }
    }
}
