namespace Munchify.DAL.DTOs.Products;

public class ProductForStockDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public string UnitName { get; set; }
}
