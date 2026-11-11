using System;
using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Suppliers;

public class SupplierDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<int> CategoryIds { get; set; } = new();
    public List<string> CategoryNames { get; set; } = new();
}
