using System;

namespace Munchify.DAL.DTOs.Promotions;

public class PromotionFilterParams
{
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
