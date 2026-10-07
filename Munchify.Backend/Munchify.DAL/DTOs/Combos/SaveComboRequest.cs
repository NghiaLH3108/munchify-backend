using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Combos;

public class SaveComboRequest
{
    [StringLength(50)]
    public string? ProductCode { get; set; }

    [Required]
    [StringLength(200)]
    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>
    /// Giá bán sau khi áp dụng % giảm giá (do frontend tính từ BasePrice).
    /// </summary>
    [Range(1, 1000000000)]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public List<ComboItemRequest> Items { get; set; } = new();
}
