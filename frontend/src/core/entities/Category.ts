export interface Category {
  id: string;
  name: string;
  description?: string;
  parentCategoryId?: string | null;
  createdAt?: string;
  updatedAt?: string;
}
