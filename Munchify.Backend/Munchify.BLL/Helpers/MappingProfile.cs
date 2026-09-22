using AutoMapper;
using Munchify.DAL.DTOs.Categories;
using Munchify.DAL.Entities;

namespace Munchify.BLL.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));
            
        CreateMap<SaveCategoryRequest, Category>();
    }
}
