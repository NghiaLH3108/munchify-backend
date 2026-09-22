using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Categories;

public class SaveCategoryRequest
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string CategoryName { get; set; } = null!;

    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
    public string? Description { get; set; }
    
    public bool IsActive { get; set; } = true;
}
