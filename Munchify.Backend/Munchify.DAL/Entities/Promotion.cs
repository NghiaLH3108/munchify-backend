using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string PromotionCode { get; set; } = null!;

    public string PromotionName { get; set; } = null!;

    public string? Description { get; set; }

    public string DiscountType { get; set; } = null!;

    public decimal DiscountValue { get; set; }

    public decimal? MinimumOrderAmount { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
