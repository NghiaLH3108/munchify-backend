using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

    public OrderService(MunchifyDbContext db, IUnitOfWork unitOfWork)
    {
        _db = db;
        _unitOfWork = unitOfWork;
    }

    // ─── CreateOrderAsync ────────────────────────────────────────────────────

    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(
        CreateOrderRequest request,
        int staffId,
        CancellationToken cancellationToken = default)
    {
        // ── 1. Validate and fetch products ────────────────────────────────────
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();

        var products = await _db.Products
            .Include(p => p.Unit)
            .Include(p => p.ProductCombos)
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
                foreach (var combo in product.ProductCombos)
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
            var promo = await _unitOfWork.Promotions.GetByCodeAsync(
                request.PromotionCode, cancellationToken);

            if (promo != null && promo.IsActive &&
                promo.StartDate <= DateTime.Now && DateTime.Now <= promo.EndDate &&
                (promo.MinimumOrderAmount == null || subtotal >= promo.MinimumOrderAmount))
            {
                discountAmount = promo.DiscountType == "PERCENTAGE"
                    ? Math.Min(subtotal * promo.DiscountValue / 100m, subtotal)
                    : Math.Min(promo.DiscountValue, subtotal);
            }
        }

        decimal pointsDiscountAmount = request.PointsToRedeem * 1000m;
        decimal totalAmount = Math.Max(0, subtotal - discountAmount - pointsDiscountAmount);
        int pointsEarned = (int)(totalAmount / 100000m);

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
                PointsRedeemed = request.PointsToRedeem,
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
                OrderId       = order.OrderId,
                PaymentMethod = request.PaymentMethod,
                Amount        = totalAmount,
                PaymentStatus = "PAID",
                PaidAt        = DateTime.Now,
                CreatedAt     = DateTime.Now,
            });

            await _db.SaveChangesAsync(cancellationToken);

            // Deduct stock + log StockMovements
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.ProductId == item.ProductId);

                if (product.IsCombo)
                {
                    foreach (var combo in product.ProductCombos)
                    {
                        var deduct = combo.Quantity * item.Quantity;
                        combo.ComponentProduct.StockQuantity -= deduct;
                        _db.StockMovements.Add(new StockMovement
                        {
                            ProductId    = combo.ComponentProductId,
                            MovementType = "COMBO_SALE",
                            Quantity     = -deduct,
                            UserId       = staffId,
                            Note         = $"Bán combo {product.ProductName} [ORD #{order.OrderCode}]",
                            CreatedAt    = DateTime.Now,
                        });
                    }
                }
                else
                {
                    product.StockQuantity -= item.Quantity;
                    _db.StockMovements.Add(new StockMovement
                    {
                        ProductId    = item.ProductId,
                        MovementType = "SALE",
                        Quantity     = -item.Quantity,
                        UserId       = staffId,
                        Note         = $"Bán hàng [ORD #{order.OrderCode}]",
                        CreatedAt    = DateTime.Now,
                    });
                }
            }

            // Update loyalty points
            if (request.CustomerId.HasValue)
            {
                var customer = await _db.Customers.FindAsync(
                    new object[] { request.CustomerId.Value }, cancellationToken);

                if (customer != null)
                {
                    customer.LoyaltyPoints =
                        customer.LoyaltyPoints - request.PointsToRedeem + pointsEarned;
                }
            }

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Reload for response
            var created = await _unitOfWork.Orders.GetByIdAsync(order.OrderId, cancellationToken);
            return ApiResponse<OrderResponse>.SuccessResult(
                MapToResponse(created!), "Tạo đơn hàng thành công.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return ApiResponse<OrderResponse>.ErrorResult(
                $"Lỗi khi tạo đơn hàng: {ex.Message}");
        }
    }

    // ─── GetOrdersAsync ──────────────────────────────────────────────────────

    public async Task<ApiResponse<PagedResult<OrderSummaryDto>>> GetOrdersAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var paged = await _unitOfWork.Orders.GetPagedAsync(page, pageSize, cancellationToken);

        var summaries = paged.Items.Select(o => new OrderSummaryDto
        {
            OrderId       = o.OrderId,
            OrderCode     = o.OrderCode,
            TotalAmount   = o.TotalAmount,
            OrderStatus   = o.OrderStatus,
            PaymentMethod = o.Payments.FirstOrDefault()?.PaymentMethod ?? "CASH",
            PointsEarned  = o.PointsEarned,
            CustomerName  = o.Customer?.CustomerName,
            CreatedAt     = o.CreatedAt,
            ItemCount     = o.OrderDetails.Count,
        }).ToList();

        return ApiResponse<PagedResult<OrderSummaryDto>>.SuccessResult(new PagedResult<OrderSummaryDto>
        {
            Items      = summaries,
            TotalCount = paged.TotalCount,
            PageNumber = page,
            PageSize   = pageSize,
        });
    }

    // ─── Mapping ─────────────────────────────────────────────────────────────

    private static OrderResponse MapToResponse(Order o) => new()
    {
        OrderId        = o.OrderId,
        OrderCode      = o.OrderCode,
        Subtotal       = o.Subtotal,
        DiscountAmount = o.DiscountAmount,
        TotalAmount    = o.TotalAmount,
        PointsEarned   = o.PointsEarned,
        PointsRedeemed = o.PointsRedeemed,
        OrderStatus    = o.OrderStatus,
        CustomerName   = o.Customer?.CustomerName,
        CreatedAt      = o.CreatedAt,
        Payment        = o.Payments.Select(p => new PaymentDto
        {
            PaymentId     = p.PaymentId,
            PaymentMethod = p.PaymentMethod,
            Amount        = p.Amount,
            PaymentStatus = p.PaymentStatus,
            TransactionCode = p.TransactionCode,
            PaidAt        = p.PaidAt,
        }).First(),
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
