import apiClient from "./apiClient";
import type { Recipe } from "../types/Recipe";

export const getRecipes = async (): Promise<Recipe[]> => {
  const response = await apiClient.get<Recipe[]>("/recipes");
  return response.data;
};

export const getRecipe = async (id: number): Promise<Recipe> => {
  const response = await apiClient.get<Recipe>(`/recipes/${id}`);
  return response.data;
};

export const createRecipe = async (recipe: Recipe): Promise<Recipe> => {
  const response = await apiClient.post<Recipe>("/recipes", recipe);
  return response.data;
};

export const updateRecipe = async (
  id: number,
  recipe: Recipe,
): Promise<Recipe> => {
  const response = await apiClient.put<Recipe>(`/recipes/${id}`, recipe);
  return response.data;
};

export const deleteRecipe = async (id: number): Promise<Recipe> => {
  const response = await apiClient.delete<Recipe>(`/recipes/${id}`);
  return response.data;
};
