using RecipeManager.Application.Interfaces;
using Microsoft.AspNetCore.Routing;
using RecipeManager.Application.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RecipeManager.Api.Endpoints;

public static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        // GET all categories
        app.MapGet("/api/v1/categories", GetAllCategories)
            .WithName("GetAllCategories");

        // GET category by id
        app.MapGet("/api/v1/categories/{id}", GetCategoryById)
            .WithName("GetCategoryById");

        // POST create category
        app.MapPost("/api/v1/categories", CreateCategory)
            .RequireAuthorization()
            .WithName("CreateCategory");

        // PUT update category
        app.MapPut("/api/v1/categories/{id}", UpdateCategory)
            .RequireAuthorization()
            .WithName("UpdateCategory");

        // DELETE category
        app.MapDelete("/api/v1/categories/{id}", DeleteCategory)
            .RequireAuthorization()
            .WithName("DeleteCategory");
    }

    private static async Task<IResult> GetAllCategories(
        [FromServices] ICategoryService categoryService,
        [FromServices] ILogger<object> logger)
    {
        try
        {
            logger.LogInformation("Fetching all categories");
            var categories = await categoryService.GetAllCategoriesAsync();
            logger.LogInformation($"Successfully fetched {categories.Count()} categories");
            return Results.Ok(categories);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error fetching categories: {ex.Message}");
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetCategoryById(
        Guid id,
        [FromServices] ICategoryService categoryService)
    {
        try
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return Results.NotFound(new { error = "Category not found" });

            return Results.Ok(category);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> CreateCategory(
        CreateCategoryDto request,
        [FromServices] ICategoryService categoryService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Category name is required" });

            await categoryService.AddCategoryAsync(new CategoryDto 
            { 
                Name = request.Name,
                
                ParentCategoryId = request.ParentCategoryId
            });
            return Results.Created($"/api/v1/categories", request);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> UpdateCategory(
        Guid id,
        CreateCategoryDto request,
        [FromServices] ICategoryService categoryService)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Category name is required" });

            var categoryDto = new CategoryDto 
            { 
                Id = id, 
                Name = request.Name,
                ParentCategoryId = request.ParentCategoryId
            };
            await categoryService.UpdateCategoryAsync(id, categoryDto);
            return Results.Ok(new { message = "Category updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound(new { error = "Category not found" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> DeleteCategory(
        Guid id,
        [FromServices] ICategoryService categoryService)
    {
        try
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
            if (category == null)
                return Results.NotFound(new { error = "Category not found" });

            await categoryService.DeleteCategoryAsync(id);
            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
