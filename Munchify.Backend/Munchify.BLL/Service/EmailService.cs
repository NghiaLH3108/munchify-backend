using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Munchify.BLL.Service.Interfaces;
using Munchify.DAL.DTOs.Common;

namespace Munchify.BLL.Service
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _logger = logger;
            _settings = configuration.GetSection("EmailSettings").Get<EmailSettings>() ?? new EmailSettings();
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_settings.SenderEmail) || string.IsNullOrWhiteSpace(_settings.AppPassword))
            {
                _logger.LogWarning("[EmailService] Chưa cấu hình SenderEmail hoặc AppPassword trong appsettings.json. Bỏ qua gửi email.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("[EmailService] Địa chỉ email nhận rỗng.");
                return false;
            }

            try
            {
                using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
                {
                    Credentials = new NetworkCredential(_settings.SenderEmail, _settings.AppPassword),
                    EnableSsl = _settings.EnableSsl
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = htmlBody,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("[EmailService] Đã gửi email thành công tới {ToEmail} với tiêu đề: {Subject}", toEmail, subject);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailService] Gặp lỗi khi gửi email qua SMTP: {Message}", ex.Message);
                return false;
            }
        }

        public async Task SendDamagedExpiredAlertAsync(
            string productName, 
            string productCode, 
            int quantity, 
            string unitName, 
            string reportType, 
            string reason, 
            string reporterName)
        {
            var adminEmail = _settings.AdminEmail;
            if (string.IsNullOrWhiteSpace(adminEmail)) return;

            string typeTitle = reportType == "EXPIRED" ? "HÀNG HẾT HẠN" : "HÀNG HƯ HỎNG / LỖI";
            string colorHeader = reportType == "EXPIRED" ? "#DC2626" : "#EA580C"; // Đỏ hoặc Cam

            string subject = $"[MUNCHIFY CẢNH BÁO] Sự cố {typeTitle}: {productName} ({productCode})";

            string htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: {colorHeader}; color: #ffffff; padding: 20px; text-align: center; }}
        .header h2 {{ margin: 0; font-size: 20px; text-transform: uppercase; }}
        .content {{ padding: 24px; color: #374151; line-height: 1.6; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
        .info-table td {{ padding: 10px 12px; border-bottom: 1px solid #e5e7eb; }}
        .info-table td.label {{ font-weight: 600; width: 35%; color: #4b5563; }}
        .badge {{ display: inline-block; padding: 4px 10px; border-radius: 6px; font-weight: bold; background: #fee2e2; color: #991b1b; }}
        .footer {{ padding: 16px; background-color: #f9fafb; text-align: center; font-size: 12px; color: #9ca3af; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>⚠️ Báo Cáo Sự Cố Kho Hàng</h2>
        </div>
        <div class='content'>
            <p>Chào Quản trị viên,</p>
            <p>Nhân viên <b>{reporterName}</b> vừa gửi báo cáo sự cố hàng hóa cần kiểm tra và phê duyệt xử lý:</p>
            <table class='info-table'>
                <tr>
                    <td class='label'>Mã sản phẩm:</td>
                    <td><b>{productCode}</b></td>
                </tr>
                <tr>
                    <td class='label'>Tên sản phẩm:</td>
                    <td><b>{productName}</b></td>
                </tr>
                <tr>
                    <td class='label'>Phân loại sự cố:</td>
                    <td><span class='badge'>{typeTitle}</span></td>
                </tr>
                <tr>
                    <td class='label'>Số lượng ảnh hưởng:</td>
                    <td><b style='color: #dc2626; font-size: 16px;'>{quantity} {unitName}</b></td>
                </tr>
                <tr>
                    <td class='label'>Lý do ghi nhận:</td>
                    <td>{reason}</td>
                </tr>
                <tr>
                    <td class='label'>Thời gian báo:</td>
                    <td>{DateTime.Now:dd/MM/yyyy HH:mm:ss}</td>
                </tr>
            </table>
            <p style='margin-top: 20px;'>Vui lòng đăng nhập vào trang quản trị để xem xét và duyệt phiếu giảm trừ tồn kho.</p>
        </div>
        <div class='footer'>
            Hệ thống quản lý kho Munchify F&B Store &bull; Thông báo tự động
        </div>
    </div>
</body>
</html>";

            // Gửi email background để không chặn luồng chính
            _ = Task.Run(async () =>
            {
                await SendEmailAsync(adminEmail, subject, htmlBody);
            });
        }

        public async Task SendLowStockAlertAsync(
            string productName, 
            string productCode, 
            int currentStock, 
            int minStock, 
            string unitName)
        {
            var adminEmail = _settings.AdminEmail;
            if (string.IsNullOrWhiteSpace(adminEmail)) return;

            string statusText = currentStock == 0 ? "CHÁY HÀNG (0 TỒN)" : "SẮP HẾT HÀNG";
            string colorHeader = currentStock == 0 ? "#DC2626" : "#D97706"; // Đỏ hoặc Vàng Cam

            string subject = $"[MUNCHIFY CẢNH BÁO KHO] {statusText}: {productName} ({productCode})";

            string htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); }}
        .header {{ background-color: {colorHeader}; color: #ffffff; padding: 20px; text-align: center; }}
        .header h2 {{ margin: 0; font-size: 20px; text-transform: uppercase; }}
        .content {{ padding: 24px; color: #374151; line-height: 1.6; }}
        .info-table {{ width: 100%; border-collapse: collapse; margin-top: 15px; }}
        .info-table td {{ padding: 10px 12px; border-bottom: 1px solid #e5e7eb; }}
        .info-table td.label {{ font-weight: 600; width: 35%; color: #4b5563; }}
        .footer {{ padding: 16px; background-color: #f9fafb; text-align: center; font-size: 12px; color: #9ca3af; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>📦 Cảnh Báo Cạn Kho Hàng</h2>
        </div>
        <div class='content'>
            <p>Chào Quản trị viên,</p>
            <p>Hệ thống kho phát hiện mặt hàng sau đang có số lượng tồn kho chạm mức báo động:</p>
            <table class='info-table'>
                <tr>
                    <td class='label'>Mã sản phẩm:</td>
                    <td><b>{productCode}</b></td>
                </tr>
                <tr>
                    <td class='label'>Tên sản phẩm:</td>
                    <td><b>{productName}</b></td>
                </tr>
                <tr>
                    <td class='label'>Tồn kho hiện tại:</td>
                    <td><b style='color: #dc2626; font-size: 18px;'>{currentStock} {unitName}</b></td>
                </tr>
                <tr>
                    <td class='label'>Mức tối thiểu quy định:</td>
                    <td>{minStock} {unitName}</td>
                </tr>
                <tr>
                    <td class='label'>Trạng thái:</td>
                    <td><b>{statusText}</b></td>
                </tr>
            </table>
            <p style='margin-top: 20px;'>Hệ thống khuyến nghị lên kế hoạch nhập hàng sớm để tránh gián đoạn phục vụ khách hàng.</p>
        </div>
        <div class='footer'>
            Hệ thống quản lý kho Munchify F&B Store &bull; Thông báo tự động
        </div>
    </div>
</body>
</html>";

            _ = Task.Run(async () =>
            {
                await SendEmailAsync(adminEmail, subject, htmlBody);
            });
        }

        public async Task<bool> SendPasswordResetOtpAsync(string toEmail, string fullName, string otpCode)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
            {
                _logger.LogWarning("[EmailService] Không thể gửi OTP vì địa chỉ email nhận rỗng.");
                return false;
            }

            string targetEmail = toEmail.Trim();
            if (targetEmail.EndsWith("@fnbstore.com", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(_settings.AdminEmail))
            {
                _logger.LogInformation("[EmailService] Phát hiện email seed {OriginalEmail}, chuyển tiếp email OTP sang AdminEmail: {AdminEmail}", targetEmail, _settings.AdminEmail);
                targetEmail = _settings.AdminEmail;
            }

            string subject = $"[MUNCHIFY] Mã xác thực đặt lại mật khẩu (OTP): {otpCode}";

            string htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f3f4f6; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: #ffffff; border-radius: 12px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.05); border: 1px solid #e2e8f0; }}
        .header {{ background: linear-gradient(135deg, #FF6B00 0%, #FF8800 100%); color: #ffffff; padding: 25px 20px; text-align: center; }}
        .header h1 {{ margin: 0; font-size: 26px; font-weight: 700; letter-spacing: 1px; }}
        .header p {{ color: #fff3e0; margin: 6px 0 0; font-size: 13px; }}
        .content {{ padding: 28px 24px; color: #374151; line-height: 1.6; }}
        .otp-box {{ text-align: center; margin: 25px 0; }}
        .otp-code {{ display: inline-block; background-color: #fff7ed; border: 2px dashed #f97316; border-radius: 8px; padding: 14px 32px; font-size: 32px; font-weight: 800; letter-spacing: 8px; color: #ea580c; font-family: 'Courier New', monospace; }}
        .note {{ font-size: 13px; color: #64748b; line-height: 1.5; margin-top: 15px; }}
        .footer {{ padding: 16px; background-color: #f9fafb; text-align: center; font-size: 12px; color: #9ca3af; border-top: 1px solid #e5e7eb; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>MUNCHIFY</h1>
            <p>Hệ thống Quản lý F&B Thông minh</p>
        </div>
        <div class='content'>
            <h2 style='color: #1e293b; font-size: 18px; margin-top: 0;'>Yêu Cầu Đặt Lại Mật Khẩu</h2>
            <p>Xin chào <b>{fullName}</b>,</p>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản liên kết với địa chỉ email <b>{toEmail}</b>. Vui lòng sử dụng mã xác thực OTP bên dưới để hoàn tất việc xác thực:</p>
            <div class='otp-box'>
                <span class='otp-code'>{otpCode}</span>
            </div>
            <p class='note'>⏱ Mã xác thực có hiệu lực trong vòng <b>10 phút</b>. Vì lý do bảo mật, vui lòng không chia sẻ mã này với bất kỳ ai.</p>
            <p class='note'>Nếu bạn không yêu cầu hành động này, xin hãy bỏ qua email này hoặc liên hệ ngay với Quản trị viên hệ thống.</p>
        </div>
        <div class='footer'>
            Hệ thống quản lý Munchify F&B Store &bull; Thông báo tự động
        </div>
    </div>
</body>
</html>";

            return await SendEmailAsync(targetEmail, subject, htmlBody);
        }
    }
}
