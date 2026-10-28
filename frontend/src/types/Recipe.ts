import type { Category } from "./Category";
import type { Ingredient } from "./Ingredient";
import type { Instruction } from "./Instruction";
import type { RecipeImage } from "./RecipeImage";
import type { Nutrition } from "./Nutrition";

export interface Recipe {
  id: string;                    
  title: string;
  description: string;
  prepTimeMinutes: number;
  cookTimeMinutes: number;
  categoryId: string;             
  category?: Category;            
  userId: string;               
  ingredients: Ingredient[];
  instructions: Instruction[];
  images: RecipeImage[];
  nutrition?: Nutrition;
  createdAt: string;
  sourceUrl?: string;
}
