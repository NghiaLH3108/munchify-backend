using System.Threading.Tasks;

namespace Munchify.BLL.Service.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendDamagedExpiredAlertAsync(string productName, string productCode, int quantity, string unitName, string reportType, string reason, string reporterName);
        Task SendLowStockAlertAsync(string productName, string productCode, int currentStock, int minStock, string unitName);
        Task<bool> SendPasswordResetOtpAsync(string toEmail, string fullName, string otpCode);
    }
}
