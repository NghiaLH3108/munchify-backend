using System;

namespace Munchify.DAL.DTOs.Orders;

public class OrderFilterRequest
{
    public string? SearchQuery { get; set; }
    public string? OrderStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
