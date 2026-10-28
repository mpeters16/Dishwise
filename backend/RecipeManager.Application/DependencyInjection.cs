using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using RecipeManager.Application.Interfaces;
using RecipeManager.Application.Services;
using RecipeManager.Application.Mapping;

namespace RecipeManager.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(DependencyInjection).Assembly);
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRecipeServices, RecipeServices>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IBlobServices, BlobServices>();
            services.AddScoped<IRecipesImportService, RecipesImportService>();
            services.AddScoped<IOpenAiService, OpenAiService>();
            //services.AddScoped<IOpenAiRecipeImportService, OpenAiRecipeImportService>();
            services.AddScoped<IAuthService, AuthService>();

            return services;
        }
    }
}