using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class StockMovement
{
    public int StockMovementId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public string MovementType { get; set; } = null!;

    public int Quantity { get; set; }

    public int PreviousQuantity { get; set; }

    public int NewQuantity { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
