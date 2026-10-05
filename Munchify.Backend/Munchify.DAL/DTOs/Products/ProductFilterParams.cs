using System;

namespace Munchify.DAL.DTOs.Products;

public class ProductFilterParams
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? UnitId { get; set; }
    public bool? IsCombo { get; set; }
    
    // Lọc theo trạng thái (chú ý: API cho POS cần chủ động truyền true)
    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
