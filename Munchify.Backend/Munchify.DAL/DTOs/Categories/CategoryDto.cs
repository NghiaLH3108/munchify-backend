using System;

namespace Munchify.DAL.DTOs.Categories;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public DateTime? CreatedAt { get; set; }
}
