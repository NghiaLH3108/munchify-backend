using System;

namespace Munchify.DAL.DTOs.Orders;

public class OrderFilterRequest
{
    public string? SearchQuery { get; set; }
    public string? OrderStatus { get; set; }
    /// <summary>Giới hạn đơn theo nhân viên tạo (STAFF chỉ xem đơn của mình – controller tự gán).</summary>
    public int? StaffId { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
