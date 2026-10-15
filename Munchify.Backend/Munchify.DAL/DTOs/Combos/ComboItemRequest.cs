using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Combos;

public class ComboItemRequest
{
    [Range(1, int.MaxValue)]
    public int ComponentProductId { get; set; }

    [Range(1, 1000)]
    public int Quantity { get; set; }
}
