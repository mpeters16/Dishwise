using AutoMapper;
using Dishwise.Application.Dto;

namespace Dishwise.Application.Mapping;

public class NutritionMappingProfile : Profile
{
    public NutritionMappingProfile()
    {
        CreateMap<Nutrition, NutritionDto>().ReverseMap();
    }
}
