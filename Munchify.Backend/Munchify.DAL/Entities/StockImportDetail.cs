using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class StockImportDetail
{
    public int ImportDetailId { get; set; }

    public int ImportId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    public decimal ImportPrice { get; set; }

    public decimal TotalAmount { get; set; }

    public virtual StockImport Import { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
