using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Inventory;

public class AdjustStockRequest
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    public int QuantityChange { get; set; }
    
    [Required]
    public string Reason { get; set; } = null!;

    [Required]
    public string MovementType { get; set; } = null!;
}
