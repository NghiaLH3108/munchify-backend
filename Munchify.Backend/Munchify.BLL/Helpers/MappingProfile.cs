using AutoMapper;
using Munchify.DAL.DTOs.Categories;
using Munchify.DAL.DTOs.Units;
using Munchify.DAL.Entities;

namespace Munchify.BLL.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));
            
        CreateMap<SaveCategoryRequest, Category>();

        // Unit mappings
        CreateMap<Unit, UnitDto>();
        CreateMap<SaveUnitRequest, Unit>();

        CreateMap<StockMovement, Munchify.DAL.DTOs.Inventory.StockMovementDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductName : string.Empty))
            .ForMember(dest => dest.StaffName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty));
            
        // Promotion mappings
        CreateMap<Product, Munchify.DAL.DTOs.Products.ProductDto>();
        CreateMap<Promotion, Munchify.DAL.DTOs.Promotions.PromotionDto>();
        CreateMap<Promotion, Munchify.DAL.DTOs.Promotions.PromotionDetailDto>();
    }
}
