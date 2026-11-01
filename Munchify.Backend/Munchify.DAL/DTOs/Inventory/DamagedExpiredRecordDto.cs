using System;

namespace Munchify.DAL.DTOs.Inventory
{
    public class DamagedExpiredRecordDto
    {
        public int RecordId { get; set; }
        public int? ImportDetailId { get; set; }
        public string? BatchCode { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UnitName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string ReportType { get; set; } = "EXPIRED";
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? AiAnalysis { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
