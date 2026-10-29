using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Dishwise.Application.Interfaces;
using Dishwise.Application.Services;
using Dishwise.Application.Mapping;

namespace Dishwise.Application
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