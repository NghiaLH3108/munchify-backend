namespace Munchify.DAL.DTOs.Inventory
{
    public class AiExpirySummaryDto
    {
        public int TotalPendingAlerts { get; set; }
        public int UrgentPendingAlerts { get; set; }
        public decimal EstFinancialLoss { get; set; }
        public decimal LossTrendPercent { get; set; }
        public int AutoResolvedThisMonth { get; set; }
        public decimal RecoveredCapital { get; set; }
    }
}
