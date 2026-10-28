using RecipeManager.Application.Interfaces;
using RecipeManager.Application.Dto;
using Microsoft.AspNetCore.Mvc;

namespace RecipeManager.Api.Endpoints;

public static class RecipeEndpoints
{
    public static void MapRecipeEndpoints(this IEndpointRouteBuilder app)
    {
        // =============================
        // Auth Endpoints (no auth required)
        // =============================

        app.MapPost("/api/v1/register", async (
            UserDto request,
            IUserService userService,
            IAuthService authService) =>
        {
            var existing = await userService.GetByUsernameAsync(request.Username);
            if (existing != null)
                return Results.BadRequest("Email already registered.");

            var user = await userService.CreateUserAsync(request.DisplayName, request.Username, request.Email, request.Password);

            var token = authService.GenerateToken(
                user.Id,
                user.DisplayName,
                user.Email,
                user.Role
            );

            return Results.Ok(new { token });
        });

        app.MapPost("/api/v1/login", async (
            UserDto request,
            IUserService userService,
            IAuthService authService) =>
        {
            var user = await userService.ValidateCredentialsAsync(request.Username, request.Password);

            if (user == null)
                return Results.Unauthorized();

            var token = authService.GenerateToken(user.Id, user.DisplayName, user.Username, user.Role);

            return Results.Ok(new { token });
        });

        app.MapPost("/api/v1/reset-password", async (
            ResetPasswordDto request,
            IUserService userService) =>
        {
            var success = await userService.ResetPasswordAsync(request.Username, request.NewPassword);

            return success ? Results.Ok() : Results.NotFound("User not found");
        });

        // =============================
        // Recipe Endpoints
        // =============================

        app.MapGet("/api/v1/recipes", async ([FromServices] IRecipeServices recipeService) =>
        {
            var recipes = await recipeService.GetAllRecipesAsync();
            return Results.Ok(recipes);
        });

        app.MapPost("/api/v1/recipes", async (HttpContext context, [FromServices] IRecipeServices recipeService) =>
        {
            var form = await context.Request.ReadFormAsync();

            var ingredientsText = form["ingredients"].ToString();
            var instructionsText = form["instructions"].ToString();

            // Parse ingredients (one per line)
            var ingredients = ingredientsText
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select((ing, index) => new IngredientsDto
                {
                    Name = ing.Trim(),
                    Quantity = 1,
                    Unit = null
                })
                .ToList();

            // Parse instructions (one per line)
            var instructions = instructionsText
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select((ins, index) => new InstructionsDto
                {
                    Step = ins.Trim(),
                    Order = index + 1
                })
                .ToList();

            if (!int.TryParse(form["prepTimeMinutes"].ToString(), out var prepTimeMinutes) ||
                !int.TryParse(form["cookTimeMinutes"].ToString(), out var cookTimeMinutes) ||
                !Guid.TryParse(form["categoryId"].ToString(), out var categoryId) ||
                !Guid.TryParse(form["userId"].ToString(), out var userId))
            {
                return Results.BadRequest("prepTimeMinutes and cookTimeMinutes must be integers; categoryId and userId must be GUIDs.");
            }

            var recipesDto = new RecipesDto
            {
                Title = form["title"].ToString(),
                Description = form["description"].ToString(),
                PrepTimeMinutes = prepTimeMinutes,
                CookTimeMinutes = cookTimeMinutes,
                CategoryId = categoryId,
                UserId = userId,
                Ingredients = ingredients,
                Instructions = instructions,
                Images = new List<RecipeImageDto>()
            };

            // Handle image upload if present
            if (form.Files.Count > 0)
            {
                var imageFile = form.Files[0];
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();
                var base64Image = Convert.ToBase64String(imageBytes);

                recipesDto.Images.Add(new RecipeImageDto
                {
                    Url = $"data:{imageFile.ContentType};base64,{base64Image}",
                    Size = imageFile.Length,
                    ContentType = imageFile.ContentType
                });
            }

            await recipeService.CreateRecipeAsync(recipesDto);
            return Results.Ok("Recipe created successfully");
        }).RequireAuthorization();

        app.MapGet("/api/v1/recipes/{id}", async (Guid id, [FromServices] IRecipeServices recipeService) =>
        {
            var recipe = await recipeService.GetRecipeByIdAsync(id);
            if (recipe == null)
                return Results.NotFound();

            return Results.Ok(recipe);
        }).RequireAuthorization();

        app.MapPut("/api/v1/recipes/{id}", async (Guid id, RecipesDto recipeDto, [FromServices] IRecipeServices recipeService) =>
        {
            try
            {
                await recipeService.UpdateRecipeAsync(id, recipeDto);
                return Results.Ok(new { message = "Recipe updated successfully" });
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).RequireAuthorization();

        app.MapDelete("/api/v1/recipes/{id}", async (Guid id, IRecipeServices recipeService) =>
        {
            var existingRecipe = await recipeService.GetRecipeByIdAsync(id);
            if (existingRecipe == null)
                return Results.NotFound();

            await recipeService.DeleteRecipeAsync(id);
            return Results.NoContent();
        }).RequireAuthorization();

        // =============================
        // Recipe Import Endpoint (uses consolidated RecipesImportService)
        // =============================

        app.MapPost("/api/v1/recipesimport", async (HttpContext context, IRecipesImportService importService, ILogger<IRecipesImportService> logger) =>
        {
            try
            {
                var body = await context.Request.ReadFromJsonAsync<Dictionary<string, string>>();

                if (body == null)
                    return Results.BadRequest("Invalid request body");

                var url = body.ContainsKey("url") ? body["url"] : null;
                var userIdStr = body.ContainsKey("userId") ? body["userId"] : null;

                if (string.IsNullOrWhiteSpace(url))
                    return Results.BadRequest("URL is required");

                if (string.IsNullOrWhiteSpace(userIdStr))
                    return Results.BadRequest("UserId is required");

                if (!Guid.TryParse(userIdStr, out var userId))
                    return Results.BadRequest("Invalid UserId format");

                if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                    return Results.BadRequest("Invalid URL format");

                var recipe = await importService.ImportRecipeFromUrlAsync(url, userId);

                logger.LogInformation("Recipe imported successfully: {Title}, Ingredients: {IngredientCount}, Instructions: {InstructionCount}",
                    recipe.Title,
                    recipe.Ingredients?.Count ?? 0,
                    recipe.Instructions?.Count ?? 0);

                return Results.Ok(new { message = "Recipe imported successfully with AI enhancement", recipe });
            }
            catch (HttpRequestException ex)
            {
                return Results.BadRequest($"Failed to fetch the URL: {ex.Message}");
            }
            catch (Exception ex)
            {
                return Results.BadRequest($"Failed to import recipe: {ex.Message}");
            }
        }).RequireAuthorization();
    }
}