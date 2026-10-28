import type { Recipe } from "../../core/entities/Recipe";
import type { IRecipeRepository } from "../../core/use-cases/RecipeUseCases";
import apiClient from "../../api/apiClient";

export class RecipeRepository implements IRecipeRepository {
  async getRecipes(): Promise<Recipe[]> {
    const response = await apiClient.get("/recipes");
    return response.data;
  }

  async getRecipeById(id: string): Promise<Recipe> {
    const response = await apiClient.get(`/recipes/${id}`);
    return response.data;
  }

  async createRecipe(recipe: Recipe): Promise<Recipe> {
    const response = await apiClient.post("/recipes", recipe);
    return response.data;
  }

  async updateRecipe(id: string, recipe: Recipe): Promise<Recipe> {
    const response = await apiClient.put(`/recipes/${id}`, recipe);
    return response.data;
  }

  async deleteRecipe(id: string): Promise<void> {
    await apiClient.delete(`/recipes/${id}`);
  }

  async importRecipeFromUrl(url: string): Promise<Recipe> {
    const response = await apiClient.post("/recipes/import", { url });
    return response.data;
  }
}
