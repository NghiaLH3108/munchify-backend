using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class ProductCombo
{
    public int ComboId { get; set; }

    public int ParentProductId { get; set; }

    public int ComponentProductId { get; set; }

    public int Quantity { get; set; }

    public virtual Product ComponentProduct { get; set; } = null!;

    public virtual Product ParentProduct { get; set; } = null!;
}
