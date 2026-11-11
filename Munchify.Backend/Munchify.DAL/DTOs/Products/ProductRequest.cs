using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Products;

public class ProductRequest
{
    public string ProductName { get; set; } = null!;
    public int CategoryId { get; set; }
    public int UnitId { get; set; }
    public string? ProductCode { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int MinStockLevel { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsCombo { get; set; }
    public bool IsActive { get; set; }
    
    // For Combo Products
    public List<ComboItemDto> ComboItems { get; set; } = new();
}
