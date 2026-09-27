using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? ContactName { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<StockImport> StockImports { get; set; } = new List<StockImport>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
}
