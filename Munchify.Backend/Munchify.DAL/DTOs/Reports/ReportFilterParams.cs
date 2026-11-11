namespace Munchify.DAL.DTOs.Reports;

/// <summary>
/// Query-string filter shared by every report endpoint (F-07.x).
/// period=day   &amp; date=yyyy-MM-dd
/// period=month &amp; year=yyyy &amp; month=1..12
/// period=year  &amp; year=yyyy
/// </summary>
public class ReportFilterParams
{
    /// <summary>"day" | "month" | "year"</summary>
    public string Period { get; set; } = "day";

    /// <summary>Used when Period = "day". Defaults to today.</summary>
    public DateTime? Date { get; set; }

    /// <summary>Used when Period = "month" or "year". Defaults to the current year.</summary>
    public int? Year { get; set; }

    /// <summary>Used when Period = "month" (1..12). Defaults to the current month.</summary>
    public int? Month { get; set; }
}
