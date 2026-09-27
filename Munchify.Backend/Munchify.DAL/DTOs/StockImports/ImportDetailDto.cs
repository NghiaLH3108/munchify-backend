using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.StockImports
{
    public class ImportDetailDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Import price cannot be negative.")]
        public decimal ImportPrice { get; set; }
    }
}
