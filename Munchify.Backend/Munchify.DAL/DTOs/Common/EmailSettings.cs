namespace Munchify.DAL.DTOs.Common
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = "smtp.gmail.com";
        public int Port { get; set; } = 587;
        public string SenderName { get; set; } = "Munchify Inventory System";
        public string SenderEmail { get; set; } = "";
        public string AppPassword { get; set; } = "";
        public string AdminEmail { get; set; } = "";
        public bool EnableSsl { get; set; } = true;
    }
}
