using System;
using System.Collections.Generic;

namespace Munchify.DAL.Entities;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<StockImport> StockImports { get; set; } = new List<StockImport>();

    public virtual ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
