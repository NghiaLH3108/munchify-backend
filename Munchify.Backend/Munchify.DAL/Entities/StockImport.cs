using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class StockImport
{
    public int ImportId { get; set; }

    public string ImportCode { get; set; } = null!;

    public int SupplierId { get; set; }

    public int UserId { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public DateTime ImportDate { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<StockImportDetail> StockImportDetails { get; set; } = new List<StockImportDetail>();

    public virtual Supplier Supplier { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
