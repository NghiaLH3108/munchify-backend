using System;
using System.Collections.Generic;

namespace Munchify.DAL.DTOs.Products;

public class ProductDetailDto : ProductDto
{
    public string? Description { get; set; }
    public int MinStockLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ComboItemDto> ComboItems { get; set; } = new List<ComboItemDto>();

    /// <summary>
    /// Tổng giá gốc các sản phẩm thành phần (SUM of component.Price × quantity).
    /// Frontend dùng để hiển thị và tính % giảm giá cho Admin.
    /// Chỉ có giá trị khi IsCombo = true.
    /// </summary>
    public decimal BasePrice { get; set; }
}
