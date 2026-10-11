namespace Munchify.DAL.DTOs.Combos;

public class ComboFilterParams
{
    public string? SearchTerm { get; set; }

    // null = all (Admin sees both active and inactive combos)
    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
