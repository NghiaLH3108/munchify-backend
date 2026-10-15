using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Inventory;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service
{
    public class DamagedExpiredService : IDamagedExpiredService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DamagedExpiredService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<IEnumerable<DamagedExpiredRecordDto>>> GetPendingRecordsAsync(string? status = null)
        {
            var response = new ApiResponse<IEnumerable<DamagedExpiredRecordDto>>();
            try
            {
                var records = await _unitOfWork.DamagedExpiredRecords.GetPendingRecordsWithDetailsAsync(status);
                
                var dtos = records.Select(r => new DamagedExpiredRecordDto
                {
                    RecordId = r.RecordId,
                    ImportDetailId = r.ImportDetailId,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.ProductName ?? "Unknown",
                    UnitName = r.Product?.Unit?.UnitName ?? "Unknown",
                    Quantity = r.Quantity,
                    Status = r.Status,
                    AiAnalysis = r.AiAnalysis,
                    CreatedAt = r.CreatedAt,
                    ProcessedAt = r.ProcessedAt
                }).ToList();

                response.Success = true;
                response.Data = dtos;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi tải danh sách cảnh báo.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ApiResponse<AiExpirySummaryDto>> GetSummaryMetricsAsync()
        {
            var response = new ApiResponse<AiExpirySummaryDto>();
            try
            {
                var summary = await _unitOfWork.DamagedExpiredRecords.GetSummaryMetricsAsync();
                response.Success = true;
                response.Data = summary;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi tính toán metrics.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ApiResponse<bool>> ApproveRecordAsync(int recordId, int userId)
        {
            var response = new ApiResponse<bool>();
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var record = await _unitOfWork.DamagedExpiredRecords.GetByIdAsync(recordId);
                if (record == null || record.Status != "PENDING")
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy record hoặc trạng thái không hợp lệ.";
                    return response;
                }

                var product = await _unitOfWork.Products.GetByIdAsync(record.ProductId);
                if (product != null)
                {
                    int prevQty = product.StockQuantity;
                    int newQty = Math.Max(0, prevQty - record.Quantity);
                    int deduction = prevQty - newQty;

                    if (deduction > 0)
                    {
                        product.StockQuantity = newQty;
                        _unitOfWork.Products.Update(product);

                        var movement = new StockMovement
                        {
                            ProductId = product.ProductId,
                            UserId = userId,
                            MovementType = "DAMAGE",
                            Quantity = -deduction,
                            PreviousQuantity = prevQty,
                            NewQuantity = newQty,
                            ReferenceType = "MANUAL",
                            ReferenceId = record.ImportDetailId,
                            Reason = $"Hủy hàng cận/hết hạn. Theo record ID: {record.RecordId}",
                            CreatedAt = DateTime.Now
                        };
                        await _unitOfWork.StockMovements.AddAsync(movement);
                    }
                }

                record.Status = "APPROVED";
                record.ProcessedAt = DateTime.Now;
                _unitOfWork.DamagedExpiredRecords.Update(record);

                await _unitOfWork.CompleteAsync();
                await transaction.CommitAsync();

                response.Success = true;
                response.Data = true;
                response.Message = "Đã duyệt và trừ kho thành công.";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                response.Success = false;
                response.Message = "Lỗi khi duyệt.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }

        public async Task<ApiResponse<bool>> RejectRecordAsync(int recordId, int userId)
        {
            var response = new ApiResponse<bool>();
            try
            {
                var record = await _unitOfWork.DamagedExpiredRecords.GetByIdAsync(recordId);
                if (record == null || record.Status != "PENDING")
                {
                    response.Success = false;
                    response.Message = "Không tìm thấy record hoặc trạng thái không hợp lệ.";
                    return response;
                }

                record.Status = "REJECTED";
                record.ProcessedAt = DateTime.Now;
                _unitOfWork.DamagedExpiredRecords.Update(record);
                await _unitOfWork.CompleteAsync();

                response.Success = true;
                response.Data = true;
                response.Message = "Đã từ chối.";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Lỗi khi từ chối.";
                response.Errors = new List<string> { ex.Message };
            }
            return response;
        }
    }
}
