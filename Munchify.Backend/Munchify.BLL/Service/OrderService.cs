using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.Data;
using Munchify.DAL.DTOs.Common;
using Munchify.DAL.DTOs.Orders;
using Munchify.DAL.Entities;
using Munchify.DAL.Repository.Interfaces;

namespace Munchify.BLL.Service;

/// <summary>
/// Handles order creation (checkout) in a single DB transaction:
///   1. Validate products and stock
///   2. Calculate totals server-side (ignore client amounts)
///   3. BEGIN TRANSACTION
///   4. Create Order → OrderDetails → Payment
///   5. Deduct stock + log StockMovements
///   6. Update customer LoyaltyPoints
///   7. COMMIT
/// </summary>
public class OrderService : IOrderService
{
    private readonly MunchifyDbContext _db;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public OrderService(MunchifyDbContext db, IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _db = db;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    // ─── CreateOrderAsync ────────────────────────────────────────────────────

    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        int staffId,
        CancellationToken cancellationToken = default)
    {
        // ── 0. Validate staff ────────────────────────────────────────────────
        var staff = await _db.Users.FindAsync(new object[] { staffId }, cancellationToken);
        if (staff == null || !staff.IsActive)
            throw new ValidationException("Tài khoản nhân viên không tồn tại hoặc đã bị vô hiệu hóa.");

        // ── 1. Validate and fetch products ────────────────────────────────────
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Include(p => p.Unit)
            .Include(p => p.ProductComboParentProducts)
                .ThenInclude(pc => pc.ComponentProduct)
            .Where(p => productIds.Contains(p.ProductId))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
            return ApiResponse<OrderResponse>.ErrorResult("Một hoặc nhiều sản phẩm không tồn tại.");

        var inactiveProduct = products.FirstOrDefault(p => !p.IsActive);
        if (inactiveProduct != null)
            return ApiResponse<OrderResponse>.ErrorResult(
                $"Sản phẩm '{inactiveProduct.ProductName}' hiện không kinh doanh.");

        // ── 2. Validate stock ─────────────────────────────────────────────────
        foreach (var item in request.Items)
        {
            var product = products.First(p => p.ProductId == item.ProductId);

            if (product.IsCombo)
            {
                // Combo: check each component
                foreach (var combo in product.ProductComboParentProducts)
                {
                    var needed = combo.Quantity * item.Quantity;
                    if (combo.ComponentProduct.StockQuantity < needed)
                        return ApiResponse<OrderResponse>.ErrorResult(
                            $"Không đủ nguyên liệu '{combo.ComponentProduct.ProductName}' cho combo '{product.ProductName}'.");
                }
            }
            else
            {
                if (product.StockQuantity < item.Quantity)
                    return ApiResponse<OrderResponse>.ErrorResult(
                        $"Sản phẩm '{product.ProductName}' không đủ số lượng (còn {product.StockQuantity}).");
            }
        }

        // ── 3. Server-side calculations ───────────────────────────────────────
        decimal subtotal = request.Items.Sum(item =>
        {
            var p = products.First(x => x.ProductId == item.ProductId);
            return p.Price * item.Quantity;
        });

        decimal discountAmount = 0m;
        if (!string.IsNullOrWhiteSpace(request.PromotionCode))
        {
            var promo = await _db.Promotions
                .Include(p => p.Products)
                .FirstOrDefaultAsync(
                    p => p.PromotionCode.ToUpper() == request.PromotionCode.ToUpper(),
                    cancellationToken);

            if (promo != null && promo.IsActive &&
                promo.StartDate <= DateTime.Now && DateTime.Now <= promo.EndDate &&
                (promo.MinimumOrderAmount == null || subtotal >= promo.MinimumOrderAmount))
            {
                if (promo.Products != null && promo.Products.Any())
                {
                    var promoProductIds = promo.Products.Select(p => p.ProductId).ToHashSet();
                    decimal applicableAmount = request.Items
                        .Where(item => promoProductIds.Contains(item.ProductId))
                        .Sum(item =>
                        {
                            var p = products.First(x => x.ProductId == item.ProductId);
                            return p.Price * item.Quantity;
                        });

                    if (applicableAmount > 0)
                    {
                        discountAmount = string.Equals(promo.DiscountType, "PERCENTAGE", StringComparison.OrdinalIgnoreCase)
                            ? Math.Min(applicableAmount * promo.DiscountValue / 100m, applicableAmount)
                            : Math.Min(promo.DiscountValue, applicableAmount);
                    }
                    else
                    {
                        discountAmount = 0m;
                    }
                }
                else
                {
                    discountAmount = string.Equals(promo.DiscountType, "PERCENTAGE", StringComparison.OrdinalIgnoreCase)
                        ? Math.Min(subtotal * promo.DiscountValue / 100m, subtotal)
                        : Math.Min(promo.DiscountValue, subtotal);
                }
            }
        }

        Customer? customer = null;
        if (request.PointsToRedeem > 0)
        {
            if (!request.CustomerId.HasValue)
                throw new ValidationException("Khách vãng lai không thể quy đổi điểm tích lũy.");
        }

        if (request.CustomerId.HasValue)
        {
            customer = await _db.Customers.FindAsync(new object[] { request.CustomerId.Value }, cancellationToken);
            if (customer == null)
                throw new NotFoundException("Không tìm thấy thông tin khách hàng.");

            if (request.PointsToRedeem > 0 && customer.LoyaltyPoints < request.PointsToRedeem)
                throw new ValidationException($"Khách hàng không đủ điểm tích lũy. Điểm hiện tại: {customer.LoyaltyPoints}, yêu cầu đổi: {request.PointsToRedeem}.");
        }

        decimal amountAfterVoucher = Math.Max(0m, subtotal - discountAmount);
        int maxPointsNeeded = (int)Math.Ceiling(amountAfterVoucher / 100m);
        int actualPointsRedeemed = Math.Max(0, Math.Min(request.PointsToRedeem, maxPointsNeeded));
        decimal pointsDiscountAmount = actualPointsRedeemed * 100m;
        decimal totalAmount = Math.Max(0m, amountAfterVoucher - pointsDiscountAmount);
        int pointsEarned = customer != null ? (int)(totalAmount / 10000m) : 0;

        // ── 3.1. Payment method validation & Change calculation ──────────────
        var validPaymentMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CASH", "QR_CODE"
        };

        if (!validPaymentMethods.Contains(request.PaymentMethod))
            throw new ValidationException($"Phương thức thanh toán '{request.PaymentMethod}' không hợp lệ. Chỉ chấp nhận: CASH, QR_CODE.");

        decimal? cashGiven = null;
        decimal? changeAmount = null;

        if (string.Equals(request.PaymentMethod, "CASH", StringComparison.OrdinalIgnoreCase))
        {
            if (!request.CashGiven.HasValue)
                throw new ValidationException("Vui lòng nhập số tiền khách đưa khi chọn thanh toán tiền mặt.");

            if (request.CashGiven.Value < totalAmount)
                throw new ValidationException($"Số tiền khách đưa ({request.CashGiven.Value:N0} đ) không đủ để thanh toán ({totalAmount:N0} đ).");

            cashGiven = request.CashGiven.Value;
            changeAmount = cashGiven.Value - totalAmount;
        }

        // ── 4-7. Atomic transaction ───────────────────────────────────────────
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Order
            var orderCode = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
            var order = new Order
            {
                OrderCode     = orderCode,
                StaffId       = staffId,
                CustomerId    = request.CustomerId,
                Subtotal      = subtotal,
                DiscountAmount = discountAmount,
                TotalAmount   = totalAmount,
                PointsRedeemed = actualPointsRedeemed,
                PointsEarned  = pointsEarned,
                OrderStatus   = "COMPLETED",
                PaymentStatus = "PAID",
                CreatedAt     = DateTime.Now,
                CompletedAt   = DateTime.Now,
            };
            await _db.Orders.AddAsync(order, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // OrderDetails
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);
                _db.OrderDetails.Add(new OrderDetail
                {
                    OrderId       = order.OrderId,
                    ProductId     = item.ProductId,
                    ProductName   = product.ProductName,
                    UnitName      = product.Unit?.UnitName ?? string.Empty,
                    UnitPrice     = product.Price,
                    Quantity      = item.Quantity,
                    DiscountAmount = 0,
                    TotalAmount   = product.Price * item.Quantity,
                });
            }

            // Payment
            _db.Payments.Add(new Payment
            {
                OrderId         = order.OrderId,
                PaymentMethod   = request.PaymentMethod.ToUpper(),
                Amount          = totalAmount,
                PaymentStatus   = "PAID",
                TransactionCode = request.PaymentMethod.ToUpper() == "QR_CODE" ? $"VIETQR-{Guid.NewGuid().ToString("N")[..8].ToUpper()}" : null,
                PaidAt          = DateTime.Now,
                CreatedAt       = DateTime.Now,
            });

            await _db.SaveChangesAsync(cancellationToken);

            // Deduct stock + log StockMovements
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);

                if (product.IsCombo)
                {
                    foreach (var combo in product.ProductComboParentProducts)
                    {
                        var deduct = combo.Quantity * item.Quantity;
                        var prevQty = combo.ComponentProduct.StockQuantity;
                        combo.ComponentProduct.StockQuantity -= deduct;
                        _db.StockMovements.Add(new StockMovement
                        {
                            ProductId        = combo.ComponentProductId,
                            MovementType     = "COMBO_SALE",
                            Quantity         = -deduct,
                            PreviousQuantity = prevQty,
                            NewQuantity      = combo.ComponentProduct.StockQuantity,
                            ReferenceType    = "ORDER",
                            ReferenceId      = order.OrderId,
                            UserId           = staffId,
                            Reason           = $"Bán combo {product.ProductName} [ORD #{order.OrderCode}]",
                            CreatedAt        = DateTime.Now,
                        });
                    }
                }
                else
                {
                    var prevQty = product.StockQuantity;
                    product.StockQuantity -= item.Quantity;
                    _db.StockMovements.Add(new StockMovement
                    {
                        ProductId        = item.ProductId,
                        MovementType     = "SALE",
                        Quantity         = -item.Quantity,
                        PreviousQuantity = prevQty,
                        NewQuantity      = product.StockQuantity,
                        ReferenceType    = "ORDER",
                        ReferenceId      = order.OrderId,
                        UserId           = staffId,
                        Reason           = $"Bán hàng [ORD #{order.OrderCode}]",
                        CreatedAt        = DateTime.Now,
                    });
                }
            }

            // Update loyalty points
            if (customer != null)
            {
                customer.LoyaltyPoints =
                    customer.LoyaltyPoints - actualPointsRedeemed + pointsEarned;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Reload for response
            var created = await _unitOfWork.Orders.GetByIdAsync(order.OrderId, cancellationToken);
            var response = MapToResponse(created!, cashGiven, changeAmount);
            return ApiResponse<OrderResponse>.SuccessResult(response, "Tạo đơn hàng thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ApiResponse<OrderResponse>.ErrorResult(
                $"Lỗi khi tạo đơn hàng: {ex.Message}");
        }
    }

    // ─── VietQR Generation ───────────────────────────────────────────────────

    public ApiResponse<VietQrInfoDto> GenerateVietQr(decimal amount, string? orderCode = null)
    {
        if (amount < 0)
            return ApiResponse<VietQrInfoDto>.ErrorResult("Số tiền thanh toán không được âm.");

        var bankId = _configuration["VietQR:BankId"] ?? "MB";
        var bankName = _configuration["VietQR:BankName"] ?? "MB Bank (Napas 247)";
        var accountNo = _configuration["VietQR:AccountNo"] ?? "0383344686";
        var accountName = _configuration["VietQR:AccountName"] ?? "MUNCHIFY STORE";
        var template = _configuration["VietQR:Template"] ?? "compact2";

        // Chuẩn hóa cú pháp chuyển khoản (Napas tối đa 25 ký tự, không dấu)
        var description = string.IsNullOrWhiteSpace(orderCode)
            ? "MUNCHIFY"
            : $"MUNCHIFY {orderCode.Replace("-", "")}";
        if (description.Length > 25)
            description = description.Substring(0, 25);

        var encodedDescription = UrlEncoder.Default.Encode(description);
        var encodedAccountName = UrlEncoder.Default.Encode(accountName);

        // VietQR image URL chuẩn: https://img.vietqr.io/image/<BANK_ID>-<ACCOUNT_NO>-<TEMPLATE>.png?amount=<AMOUNT>&addInfo=<DESCRIPTION>&accountName=<ACCOUNT_NAME>
        var qrImageUrl = $"https://img.vietqr.io/image/{bankId}-{accountNo}-{template}.png?amount={(long)amount}&addInfo={encodedDescription}&accountName={encodedAccountName}";
        
        // VietQR QuickLink (app ngân hàng mở trực tiếp)
        var qrQuickLink = $"https://api.vietqr.io/{bankId}/{accountNo}/{(long)amount}/{encodedDescription}/vietqr_net_2.jpg?accountName={encodedAccountName}";

        var dto = new VietQrInfoDto
        {
            BankId       = bankId,
            BankName     = bankName,
            AccountNo    = accountNo,
            AccountName  = accountName,
            Amount       = amount,
            Description  = description,
            QrImageUrl   = qrImageUrl,
            QrQuickLink  = qrQuickLink
        };

        return ApiResponse<VietQrInfoDto>.SuccessResult(dto, "Lấy thông tin VietQR thành công.");
    }

    // ─── GetOrdersAsync ──────────────────────────────────────────────────────

    public async Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetOrdersAsync(
        OrderFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Orders.GetPagedAsync(filter, cancellationToken);

        var summaries = paged.Items.Select(o => new OrderSummaryDto
        {
            OrderId       = o.OrderId,
            OrderCode     = o.OrderCode,
            TotalAmount   = o.TotalAmount,
            OrderStatus   = o.OrderStatus,
            PaymentStatus = o.PaymentStatus,
            PaymentMethod = o.Payments.FirstOrDefault()?.PaymentMethod ?? "CASH",
            PointsEarned  = o.PointsEarned,
            CustomerName  = o.Customer?.CustomerName,
            CustomerPhone = o.Customer?.Phone,
            StaffName     = o.Staff?.FullName,
            CreatedAt     = o.CreatedAt,
            ItemCount     = o.OrderDetails.Count,
        }).ToList();

        return ApiResponse<PagedResult<OrderSummaryDto>>.SuccessResult(new PagedResult<OrderSummaryDto>
        {
            Items      = summaries,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize   = paged.PageSize,
        });
    }

    public async Task<ApiResponse<OrderResponse>> GetOrderDetailAsync(
        int orderId,
        CancellationToken cancellationToken = default,
        int? restrictToStaffId = null)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null || (restrictToStaffId.HasValue && order.StaffId != restrictToStaffId.Value))
        {
            return ApiResponse<OrderResponse>.NotFoundResult($"Không tìm thấy đơn hàng #{orderId}.");
        }

        var response = MapToResponse(order);
        return ApiResponse<OrderResponse>.SuccessResult(response);
    }

    // ─── Mapping ─────────────────────────────────────────────────────────────

    private static OrderResponse MapToResponse(Order o, decimal? cashGiven = null, decimal? changeAmount = null) => new()
    {
        OrderId        = o.OrderId,
        OrderCode      = o.OrderCode,
        Subtotal       = o.Subtotal,
        DiscountAmount = o.DiscountAmount,
        TotalAmount    = o.TotalAmount,
        PointsEarned   = o.PointsEarned,
        PointsRedeemed = o.PointsRedeemed,
        OrderStatus    = o.OrderStatus,
        PaymentStatus  = o.PaymentStatus,
        CustomerName   = o.Customer?.CustomerName,
        CustomerPhone  = o.Customer?.Phone,
        StaffName      = o.Staff?.FullName,
        CashGiven      = cashGiven,
        ChangeAmount   = changeAmount,
        CreatedAt      = o.CreatedAt,
        Payment        = o.Payments.Select(p => new PaymentDto
        {
            PaymentId       = p.PaymentId,
            PaymentMethod   = p.PaymentMethod,
            Amount          = p.Amount,
            PaymentStatus   = p.PaymentStatus,
            TransactionCode = p.TransactionCode,
            CashGiven       = cashGiven,
            ChangeAmount    = changeAmount,
            PaidAt          = p.PaidAt,
        }).FirstOrDefault() ?? new PaymentDto(),
        OrderDetails = o.OrderDetails.Select(d => new OrderDetailDto
        {
            ProductName    = d.ProductName,
            UnitName       = d.UnitName,
            UnitPrice      = d.UnitPrice,
            Quantity       = d.Quantity,
            DiscountAmount = d.DiscountAmount,
            TotalAmount    = d.TotalAmount,
        }).ToList(),
    };
}

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
