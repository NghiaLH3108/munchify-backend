using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class Product
{
    public int ProductId { get; set; }

    public int CategoryId { get; set; }

    public int UnitId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int MinStockLevel { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsCombo { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ProductCombo> ProductComboComponentProducts { get; set; } = new List<ProductCombo>();

    public virtual ICollection<ProductCombo> ProductComboParentProducts { get; set; } = new List<ProductCombo>();

    public virtual ICollection<StockImportDetail> StockImportDetails { get; set; } = new List<StockImportDetail>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();

    public virtual Unit Unit { get; set; } = null!;

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();
}
