using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Inventory
{
    public class StaffReportDamagedExpiredRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn sản phẩm")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sự cố")]
        public string ReportType { get; set; } = "DAMAGED"; // DAMAGED or EXPIRED

        [Required(ErrorMessage = "Vui lòng nhập lý do")]
        [MaxLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = null!;
    }
}
