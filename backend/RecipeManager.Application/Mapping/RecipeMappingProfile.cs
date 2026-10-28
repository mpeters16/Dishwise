using System.Runtime.InteropServices;
using AutoMapper;
using RecipeManager.Application.Dto;


namespace RecipeManager.Application.Mapping
{
    public class RecipeMappingProfile : Profile
    {
        public RecipeMappingProfile()
        {
            CreateMap<Recipe, RecipesDto>();
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.MapFrom(src => src.SubCategories));
            CreateMap<CategoryDto, Category>()
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Recipes, opt => opt.Ignore());
            CreateMap<CreateCategoryDto, Category>()
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore())
                .ForMember(dest => dest.ParentCategory, opt => opt.Ignore())
                .ForMember(dest => dest.Recipes, opt => opt.Ignore());
            CreateMap<Ingredient, IngredientsDto>();
            CreateMap<Instruction, InstructionsDto>();
            CreateMap<RecipeImage, RecipeImageDto>();
            CreateMap<Nutrition, NutritionDto>();
            CreateMap<User, UserDto>();

            CreateMap<RecipesDto, Recipe>();
        }
    }
}