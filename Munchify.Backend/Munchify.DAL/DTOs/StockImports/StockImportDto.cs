using System;
using System.Collections.Generic;

namespace Munchify.DAL.DTOs.StockImports
{
    public class StockImportDto
    {
        public int ImportId { get; set; }
        public string ImportCode { get; set; } = null!;
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = null!;
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = null!;
        public DateTime ImportDate { get; set; }
        public string? Notes { get; set; }

        public List<ImportDetailResponseDto> Details { get; set; } = new List<ImportDetailResponseDto>();
    }

    public class ImportDetailResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string UnitName { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal ImportPrice { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
