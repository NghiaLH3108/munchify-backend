using System;

namespace Munchify.DAL.DTOs.Products;

public class ComboItemDto
{
    public int ComponentProductId { get; set; }
    public string ComponentProductName { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string UnitName { get; set; } = null!;
    public int ComponentStockQuantity { get; set; }
}
