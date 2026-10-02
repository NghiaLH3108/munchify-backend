using System;

namespace Munchify.DAL.DTOs.Customers;

public class CustomerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public int LoyaltyPoints { get; set; }
    public DateTime CreatedAt { get; set; }
}
