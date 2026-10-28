export interface Recipe {
  id: string;
  title: string;
  description?: string;
  ingredients: Ingredient[];
  instructions: Instruction[];
  category?: Category;
  images?: RecipeImage[];
  createdAt?: string;
  updatedAt?: string;
}

export interface Ingredient {
  id: string;
  name: string;
  quantity: number;
  unit: string;
}

export interface Instruction {
  id: string;
  step: number;
  description: string;
}

export interface RecipeImage {
  id: string;
  url: string;
  altText?: string;
}

export interface Category {
  id: string;
  name: string;
}
