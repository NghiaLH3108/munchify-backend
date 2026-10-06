using System;

namespace Munchify.DAL.Entities;

public partial class DamagedExpiredRecord
{
    public int RecordId { get; set; }
    public int ImportDetailId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string ReportType { get; set; } = "EXPIRED";
    public string Status { get; set; } = "PENDING"; // PENDING, APPROVED, REJECTED
    public string? AiAnalysis { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? ProcessedAt { get; set; }

    public virtual StockImportDetail ImportDetail { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
