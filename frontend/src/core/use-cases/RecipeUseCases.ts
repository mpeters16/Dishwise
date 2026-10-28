import type { Recipe } from "../../core/entities/Recipe";

export interface IRecipeRepository {
  getRecipes(): Promise<Recipe[]>;
  getRecipeById(id: string): Promise<Recipe>;
  createRecipe(recipe: Recipe): Promise<Recipe>;
  updateRecipe(id: string, recipe: Recipe): Promise<Recipe>;
  deleteRecipe(id: string): Promise<void>;
  importRecipeFromUrl(url: string): Promise<Recipe>;
}

export class GetRecipesUseCase {
  recipeRepository: IRecipeRepository;

  constructor(recipeRepository: IRecipeRepository) {
    this.recipeRepository = recipeRepository;
  }

  async execute(): Promise<Recipe[]> {
    return this.recipeRepository.getRecipes();
  }
}

export class GetRecipeByIdUseCase {
  recipeRepository: IRecipeRepository;

  constructor(recipeRepository: IRecipeRepository) {
    this.recipeRepository = recipeRepository;
  }

  async execute(id: string): Promise<Recipe> {
    return this.recipeRepository.getRecipeById(id);
  }
}

export class CreateRecipeUseCase {
  recipeRepository: IRecipeRepository;

  constructor(recipeRepository: IRecipeRepository) {
    this.recipeRepository = recipeRepository;
  }

  async execute(recipe: Recipe): Promise<Recipe> {
    return this.recipeRepository.createRecipe(recipe);
  }
}

export class DeleteRecipeUseCase {
  recipeRepository: IRecipeRepository;

  constructor(recipeRepository: IRecipeRepository) {
    this.recipeRepository = recipeRepository;
  }

  async execute(id: string): Promise<void> {
    return this.recipeRepository.deleteRecipe(id);
  }
}

export class ImportRecipeUseCase {
  recipeRepository: IRecipeRepository;

  constructor(recipeRepository: IRecipeRepository) {
    this.recipeRepository = recipeRepository;
  }

  async execute(url: string): Promise<Recipe> {
    return this.recipeRepository.importRecipeFromUrl(url);
  }
}
