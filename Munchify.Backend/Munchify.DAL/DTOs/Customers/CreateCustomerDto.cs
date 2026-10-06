using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Customers;

public class CreateCustomerDto
{
    [Required(ErrorMessage = "CustomerName is required.")]
    public string CustomerName { get; set; } = null!;

    [Required(ErrorMessage = "Phone is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(15, ErrorMessage = "Phone number cannot exceed 15 characters.")]
    public string Phone { get; set; } = null!;
}
