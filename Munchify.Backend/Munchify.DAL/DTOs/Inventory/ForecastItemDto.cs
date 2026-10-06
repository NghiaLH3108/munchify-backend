namespace Munchify.DAL.DTOs.Inventory
{
    public class ForecastItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public double StockQuantity { get; set; }
        public double MinStockLevel { get; set; }
        public double AvgDailyUsage { get; set; } 
        public double DaysRemaining { get; set; } 
        public string RiskLevel { get; set; } = string.Empty; 
        public double SuggestedImportQty { get; set; } 
        public int? DefaultSupplierId { get; set; } 
        public string? DefaultSupplierName { get; set; }
    }
}
