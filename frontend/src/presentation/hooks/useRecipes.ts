import { useState } from "react";
import type { Recipe } from "../../core/entities/Recipe";
import {
  GetRecipesUseCase,
  GetRecipeByIdUseCase,
  CreateRecipeUseCase,
  DeleteRecipeUseCase,
  ImportRecipeUseCase,
} from "../../core/use-cases/RecipeUseCases";
import { RecipeRepository } from "../../data/repositories/RecipeRepository";

const recipeRepository = new RecipeRepository();
const getRecipesUseCase = new GetRecipesUseCase(recipeRepository);
const getRecipeByIdUseCase = new GetRecipeByIdUseCase(recipeRepository);
const createRecipeUseCase = new CreateRecipeUseCase(recipeRepository);
const deleteRecipeUseCase = new DeleteRecipeUseCase(recipeRepository);
const importRecipeUseCase = new ImportRecipeUseCase(recipeRepository);

export const useRecipes = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const getRecipes = async (): Promise<Recipe[]> => {
    setIsLoading(true);
    setError(null);
    try {
      return await getRecipesUseCase.execute();
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to fetch recipes";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const getRecipeById = async (id: string): Promise<Recipe> => {
    setIsLoading(true);
    setError(null);
    try {
      return await getRecipeByIdUseCase.execute(id);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to fetch recipe";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const createRecipe = async (recipe: Recipe): Promise<Recipe> => {
    setIsLoading(true);
    setError(null);
    try {
      return await createRecipeUseCase.execute(recipe);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to create recipe";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const deleteRecipe = async (id: string): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await deleteRecipeUseCase.execute(id);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to delete recipe";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const importRecipe = async (url: string): Promise<Recipe> => {
    setIsLoading(true);
    setError(null);
    try {
      return await importRecipeUseCase.execute(url);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Failed to import recipe";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return { getRecipes, getRecipeById, createRecipe, deleteRecipe, importRecipe, isLoading, error };
};
