import { useCallback, useState } from "react";
import type { Category } from "../../core/entities/Category";
import {
  GetCategoriesUseCase,
  GetCategoryByIdUseCase,
  CreateCategoryUseCase,
  UpdateCategoryUseCase,
  DeleteCategoryUseCase,
} from "../../core/use-cases/CategoryUseCases";
import { CategoryRepository } from "../../data/repositories/CategoryRepository";

const categoryRepository = new CategoryRepository();
const getCategoriesUseCase = new GetCategoriesUseCase(categoryRepository);
const getCategoryByIdUseCase = new GetCategoryByIdUseCase(categoryRepository);
const createCategoryUseCase = new CreateCategoryUseCase(categoryRepository);
const updateCategoryUseCase = new UpdateCategoryUseCase(categoryRepository);
const deleteCategoryUseCase = new DeleteCategoryUseCase(categoryRepository);

export const useCategories = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // Memoized so callers can list it as an effect dependency without re-running on every render
  const getCategories = useCallback(async (): Promise<Category[]> => {
    setIsLoading(true);
    setError(null);
    try {
      return await getCategoriesUseCase.execute();
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch categories";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const getCategoryById = async (id: string): Promise<Category> => {
    setIsLoading(true);
    setError(null);
    try {
      return await getCategoryByIdUseCase.execute(id);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to fetch category";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const createCategory = async (category: Category): Promise<Category> => {
    setIsLoading(true);
    setError(null);
    try {
      return await createCategoryUseCase.execute(category);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to create category";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const updateCategory = async (
    id: string,
    category: Category,
  ): Promise<Category> => {
    setIsLoading(true);
    setError(null);
    try {
      return await updateCategoryUseCase.execute(id, category);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to update category";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  const deleteCategory = async (id: string): Promise<void> => {
    setIsLoading(true);
    setError(null);
    try {
      await deleteCategoryUseCase.execute(id);
    } catch (err) {
      const errorMessage =
        err instanceof Error ? err.message : "Failed to delete category";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    getCategories,
    getCategoryById,
    createCategory,
    updateCategory,
    deleteCategory,
    isLoading,
    error,
  };
};
