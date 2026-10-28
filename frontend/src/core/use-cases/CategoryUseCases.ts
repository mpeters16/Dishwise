import type { Category } from "../../core/entities/Category";

export interface ICategoryRepository {
  getCategories(): Promise<Category[]>;
  getCategoryById(id: string): Promise<Category>;
  createCategory(category: Category): Promise<Category>;
  updateCategory(id: string, category: Category): Promise<Category>;
  deleteCategory(id: string): Promise<void>;
}

export class GetCategoriesUseCase {
  categoryRepository: ICategoryRepository;

  constructor(categoryRepository: ICategoryRepository) {
    this.categoryRepository = categoryRepository;
  }

  async execute(): Promise<Category[]> {
    return this.categoryRepository.getCategories();
  }
}

export class GetCategoryByIdUseCase {
  categoryRepository: ICategoryRepository;

  constructor(categoryRepository: ICategoryRepository) {
    this.categoryRepository = categoryRepository;
  }

  async execute(id: string): Promise<Category> {
    return this.categoryRepository.getCategoryById(id);
  }
}

export class CreateCategoryUseCase {
  categoryRepository: ICategoryRepository;

  constructor(categoryRepository: ICategoryRepository) {
    this.categoryRepository = categoryRepository;
  }

  async execute(category: Category): Promise<Category> {
    return this.categoryRepository.createCategory(category);
  }
}

export class UpdateCategoryUseCase {
  categoryRepository: ICategoryRepository;

  constructor(categoryRepository: ICategoryRepository) {
    this.categoryRepository = categoryRepository;
  }

  async execute(id: string, category: Category): Promise<Category> {
    return this.categoryRepository.updateCategory(id, category);
  }
}

export class DeleteCategoryUseCase {
  categoryRepository: ICategoryRepository;

  constructor(categoryRepository: ICategoryRepository) {
    this.categoryRepository = categoryRepository;
  }

  async execute(id: string): Promise<void> {
    return this.categoryRepository.deleteCategory(id);
  }
}
