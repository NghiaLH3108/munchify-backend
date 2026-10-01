using System.ComponentModel.DataAnnotations;

namespace Munchify.DAL.DTOs.Units;

public class SaveUnitRequest
{
    [Required(ErrorMessage = "Unit name is required")]
    [StringLength(100, ErrorMessage = "Unit name cannot exceed 100 characters")]
    public string UnitName { get; set; } = null!;

    [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
