import type { Category } from "../../core/entities/Category";
import type { ICategoryRepository } from "../../core/use-cases/CategoryUseCases";
import apiClient from "../../api/apiClient";

export class CategoryRepository implements ICategoryRepository {
  async getCategories(): Promise<Category[]> {
    const response = await apiClient.get("/categories");
    return response.data;
  }

  async getCategoryById(id: string): Promise<Category> {
    const response = await apiClient.get(`/categories/${id}`);
    return response.data;
  }

  async createCategory(category: Category): Promise<Category> {
    const payload = {
      name: category.name,
      parentCategoryId: category.parentCategoryId ?? null,
    };
    const response = await apiClient.post("/categories", payload);
    return response.data;
  }

  async updateCategory(id: string, category: Category): Promise<Category> {
    const payload = {
      name: category.name,
      parentCategoryId: category.parentCategoryId ?? null,
    };
    const response = await apiClient.put(`/categories/${id}`, payload);
    return response.data;
  }

  async deleteCategory(id: string): Promise<void> {
    await apiClient.delete(`/categories/${id}`);
  }
}
