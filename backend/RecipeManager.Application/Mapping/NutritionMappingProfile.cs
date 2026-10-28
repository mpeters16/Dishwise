using AutoMapper;
using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Mapping;

public class NutritionMappingProfile : Profile
{
    public NutritionMappingProfile()
    {
        CreateMap<Nutrition, NutritionDto>().ReverseMap();
    }
}
