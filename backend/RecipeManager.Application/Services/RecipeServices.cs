using AutoMapper;
using RecipeManager.Application.Interfaces;
using RecipeManager.Application.Dto;

namespace RecipeManager.Application.Services
{
    public class RecipeServices : IRecipeServices
    {
        private readonly IRecipeRepository _recipeRepository;
        private readonly IMapper _mapper;

        public RecipeServices(IRecipeRepository recipeRepository, IMapper mapper)
        {
            _recipeRepository = recipeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RecipesDto>> GetAllRecipesAsync()
        {
            var recipes = await _recipeRepository.GetAllRecipesAsync();
            return _mapper.Map<IEnumerable<RecipesDto>>(recipes);
        }

        public async Task<RecipesDto?> GetRecipeByIdAsync(Guid id)
        {
            var recipe = await _recipeRepository.GetRecipeByIdAsync(id);
            if (recipe == null)
            {
                return null;
            }
            return _mapper.Map<RecipesDto>(recipe);
        }

        public async Task CreateRecipeAsync(RecipesDto recipeDto)
        {
            var recipe = new Recipe
            {
                Title = recipeDto.Title,
                Description = recipeDto.Description,
                PrepTimeMinutes = recipeDto.PrepTimeMinutes,
                CookTimeMinutes = recipeDto.CookTimeMinutes,
                CategoryId = recipeDto.CategoryId,   // IMPORTANT: use FK, not entity
                UserId = recipeDto.UserId,
                SourceUrl = recipeDto.SourceUrl,

                Ingredients = recipeDto.Ingredients.Select(i => new Ingredient
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit
                }).ToList(),

                Instructions = recipeDto.Instructions.Select(i => new Instruction
                {
                    Step = i.Step,
                    Order = i.Order
                }).ToList(),
                Images = recipeDto.Images.Select(i => new RecipeImage
                {
                    Url = i.Url ?? "",
                    FileName = i.FileName,
                    ContentType = i.ContentType,
                    Size = i.Size,
                    IsPrimary = i.IsPrimary
                }).ToList(),
                Nutrition = recipeDto.Nutrition != null ? new Nutrition
                {
                    Calories = recipeDto.Nutrition.Calories,
                    Protein = recipeDto.Nutrition.Protein,
                    Carbohydrates = recipeDto.Nutrition.Carbohydrates,
                    Fat = recipeDto.Nutrition.Fat,
                    Fiber = recipeDto.Nutrition.Fiber,
                    Sugar = recipeDto.Nutrition.Sugar,
                    Sodium = recipeDto.Nutrition.Sodium,
                    Potassium = recipeDto.Nutrition.Potassium,
                    Calcium = recipeDto.Nutrition.Calcium,
                    Iron = recipeDto.Nutrition.Iron,
                    Vitamin_A = recipeDto.Nutrition.Vitamin_A,
                    Vitamin_C = recipeDto.Nutrition.Vitamin_C,
                    Vitamin_D = recipeDto.Nutrition.Vitamin_D,
                    Cholesterol = recipeDto.Nutrition.Cholesterol
                } : null
            };

            await _recipeRepository.AddRecipeAsync(recipe);
        }


        public async Task UpdateRecipeAsync(Guid id, RecipesDto recipeDto)
        {
            var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(id);
            if (existingRecipe == null)
            {
                throw new KeyNotFoundException("Recipe not found");
            }
            _mapper.Map(recipeDto, existingRecipe);
            await _recipeRepository.UpdateRecipeAsync(existingRecipe);
        }

        public async Task DeleteRecipeAsync(Guid id)
        {
            var existingRecipe = await _recipeRepository.GetRecipeByIdAsync(id);
            if (existingRecipe == null)
            {
                throw new KeyNotFoundException("Recipe not found");
            }

            await _recipeRepository.DeleteRecipeAsync(id);
        }
    }
}