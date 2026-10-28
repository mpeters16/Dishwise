import { useState, useEffect, useCallback } from "react";
import { useCategories } from "../../presentation/hooks/useCategories";
import type { Category } from "../../core/entities/Category";
import {
  organizeCategories,
  type HierarchicalCategory,
} from "../../utils/categoryHelper";
import "../../App.css";

const Categories = () => {
  useEffect(() => {
    document.title = "Recipe Manager - Manage Categories";
  }, []);

  const {
    getCategories,
    createCategory,
    updateCategory,
    deleteCategory,
    isLoading,
    error,
  } = useCategories();

  const [categories, setCategories] = useState<Category[]>([]);
  const [name, setName] = useState("");
  const [parentCategoryId, setParentCategoryId] = useState<string>("");
  const [editingId, setEditingId] = useState<string | null>(null);
  const [showForm, setShowForm] = useState(false);
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(
    new Set(),
  );

  const loadCategories = useCallback(async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch {
      console.error("Failed to load categories");
    }
  }, [getCategories]);

  useEffect(() => {
    loadCategories();
  }, [loadCategories]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!name.trim()) {
      alert("Category name is required");
      return;
    }

    try {
      const resolvedParentId =
        parentCategoryId && parentCategoryId.trim() !== ""
          ? parentCategoryId
          : null;
      if (editingId) {
        await updateCategory(editingId, {
          id: editingId,
          name,
          parentCategoryId: resolvedParentId,
        });
      } else {
        await createCategory({
          id: "",
          name,
          parentCategoryId: resolvedParentId,
        });
      }
      setName("");
      setParentCategoryId("");
      setEditingId(null);
      setShowForm(false);
      await loadCategories();
    } catch {
      console.error("Failed to save category");
    }
  };

  const handleEdit = (category: Category) => {
    setName(category.name);
    setParentCategoryId(category.parentCategoryId || "");
    setEditingId(category.id);
    setShowForm(true);
  };

  const handleDelete = async (id: string) => {
    if (window.confirm("Are you sure you want to delete this category?")) {
      try {
        await deleteCategory(id);
        await loadCategories();
      } catch {
        console.error("Failed to delete category");
      }
    }
  };

  const handleCancel = () => {
    setName("");
    setParentCategoryId("");
    setEditingId(null);
    setShowForm(false);
  };

  const toggleExpand = (id: string) => {
    setExpandedCategories((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  // Get only root categories (no parent) for the parent dropdown
  const rootCategories = categories.filter((c) => !c.parentCategoryId);

  // Organize into hierarchy for the table
  const hierarchicalCategories = organizeCategories(categories);

  const renderCategoryRow = (
    category: HierarchicalCategory,
    depth: number = 0,
  ): React.ReactNode[] => {
    const hasChildren = category.subcategories.length > 0;
    const isExpanded = expandedCategories.has(category.id);
    const indent = depth * 24;

    const rows: React.ReactNode[] = [
      <tr key={category.id} className={depth === 0 ? "parent-row" : ""}>
        <td style={{ paddingLeft: `${indent + 12}px` }}>
          <div className="d-flex align-items-center">
            {hasChildren ? (
              <button
                className="btn btn-sm btn-link p-0 me-2 text-decoration-none"
                onClick={() => toggleExpand(category.id)}
                title={isExpanded ? "Collapse" : "Expand"}
              >
                {isExpanded ? "▼" : "▶"}
              </button>
            ) : (
              <span
                className="me-2"
                style={{ width: "16px", display: "inline-block" }}
              >
                {depth > 0 ? "└" : ""}
              </span>
            )}
            <span className={depth === 0 ? "fw-bold" : ""}>
              {category.name}
            </span>
            {hasChildren && (
              <span className="badge badge-accent ms-2">
                {category.subcategories.length}
              </span>
            )}
          </div>
        </td>
        <td>
          {depth === 0 ? (
            <span className="badge badge-accent">Main</span>
          ) : (
            <span className="badge badge-accent-light">Sub</span>
          )}
        </td>
        <td>
          <div className="d-flex gap-1">
            <button
              className="btn btn-sm btn-accent-outline"
              onClick={() => handleEdit(category)}
            >
              Edit
            </button>
            <button
              className="btn btn-sm btn-outline-danger"
              onClick={() => handleDelete(category.id)}
            >
              Delete
            </button>
          </div>
        </td>
      </tr>,
    ];

    if (hasChildren && isExpanded) {
      category.subcategories.forEach((sub) => {
        rows.push(...renderCategoryRow(sub, depth + 1));
      });
    }

    return rows;
  };

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Manage Categories</h2>
        <button
          className="btn btn-accent"
          onClick={() => setShowForm(!showForm)}
        >
          {showForm ? "Cancel" : "Add Category"}
        </button>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {showForm && (
        <div className="panel mb-4">
          <h5 className="subheading">
            {editingId ? "Edit Category" : "Add New Category"}
          </h5>
          <form onSubmit={handleSubmit}>
            <div className="mb-3">
              <label className="label form-label">Category Name</label>
              <input
                type="text"
                className="form-control"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g., Desserts, Main Course"
                required
              />
            </div>
            <div className="mb-3">
              <label className="label form-label">Parent Category</label>
              <select
                className="form-select"
                value={parentCategoryId}
                onChange={(e) => setParentCategoryId(e.target.value)}
              >
                <option value="">None (Main Category)</option>
                {rootCategories
                  .filter((c) => c.id !== editingId)
                  .map((cat) => (
                    <option key={cat.id} value={cat.id}>
                      {cat.name}
                    </option>
                  ))}
              </select>
              <div className="form-text">
                Leave empty to create a main category, or select a parent to
                create a subcategory.
              </div>
            </div>
            <div className="d-flex gap-2">
              <button
                type="submit"
                className="btn btn-accent"
                disabled={isLoading}
              >
                {isLoading ? "Saving..." : editingId ? "Update" : "Create"}
              </button>
              <button
                type="button"
                className="btn btn-accent-outline"
                onClick={handleCancel}
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {isLoading && <div className="alert alert-info">Loading...</div>}

      {categories.length === 0 ? (
        <div className="alert alert-info">No categories yet. Create one!</div>
      ) : (
        <>
          <div className="d-flex justify-content-between align-items-center mb-2">
            <p className="text-muted mb-0">
              <strong>{rootCategories.length}</strong> main categor
              {rootCategories.length !== 1 ? "ies" : "y"},{" "}
              <strong>{categories.length - rootCategories.length}</strong>{" "}
              subcategor
              {categories.length - rootCategories.length !== 1 ? "ies" : "y"}
            </p>
            <button
              className="btn btn-sm btn-accent-outline"
              onClick={() => {
                if (expandedCategories.size === rootCategories.length) {
                  setExpandedCategories(new Set());
                } else {
                  setExpandedCategories(
                    new Set(rootCategories.map((c) => c.id)),
                  );
                }
              }}
            >
              {expandedCategories.size === rootCategories.length
                ? "Collapse All"
                : "Expand All"}
            </button>
          </div>
          <div className="table-responsive">
            <table className="table table-hover align-middle accent-table">
              <thead>
                <tr>
                  <th style={{ width: "50%" }}>Category</th>
                  <th style={{ width: "15%" }}>Type</th>
                  <th style={{ width: "35%" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {hierarchicalCategories.map((cat) => renderCategoryRow(cat, 0))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
};

export default Categories;
