using System;

namespace Munchify.DAL.DTOs.Products;

public class ProductFilterParams
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public int? UnitId { get; set; }
    public bool? IsCombo { get; set; }
    
    // Mặc định là true cho Staff/POS theo BR-01
    public bool? IsActive { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
