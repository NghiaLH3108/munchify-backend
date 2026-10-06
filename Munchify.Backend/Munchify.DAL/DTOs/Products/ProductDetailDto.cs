using System;
using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Products;

public class ProductDetailDto : ProductDto
{
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ComboItemDto> ComboItems { get; set; } = new List<ComboItemDto>();
}
