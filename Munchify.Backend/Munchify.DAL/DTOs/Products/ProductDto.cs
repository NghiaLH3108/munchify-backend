using System;

namespace Munchify.DAL.DTOs.Products;

public class ProductDto
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public bool IsCombo { get; set; }
    public bool IsActive { get; set; }
}
