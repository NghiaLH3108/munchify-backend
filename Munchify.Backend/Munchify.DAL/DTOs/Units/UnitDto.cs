namespace Munchify.DAL.DTOs.Units;

public class UnitDto
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
