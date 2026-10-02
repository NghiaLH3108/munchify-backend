using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.StockImports
{
    public class CreateImportRequest
    {
        [Required]
        public int SupplierId { get; set; }

        public string? Notes { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one product must be imported.")]
        public List<ImportDetailDto> Items { get; set; } = new List<ImportDetailDto>();
    }
}
