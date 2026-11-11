using System;
using System.Collections.Generic;
using Munchify.DAL.DTOs.Products;

namespace Munchify.DAL.DTOs.Promotions;

public class PromotionDetailDto : PromotionDto
{
    public List<ProductDto> Products { get; set; } = new();
}
