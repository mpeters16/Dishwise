import { useEffect, useState, useMemo, useContext } from "react";
import Form from "react-bootstrap/Form";
import { Link, useLocation } from "react-router-dom";
import Pagination from "react-bootstrap/Pagination";
import apiClient from "../../api/apiClient";
import type { Recipe } from "../../types/Recipe";
import type { Category } from "../../types/Category";
import { ImportRecipeModal } from "../../presentation/components/ImportRecipeModal";
import { AddRecipeModal } from "../../presentation/components/AddRecipeModal";
import { AuthContext } from "../../presentation/context/AuthContext";

// The photo to show on a card: the primary image, or the first one if none is marked
const getCoverImage = (recipe: Recipe) =>
  recipe.images?.find((image) => image.isPrimary) ?? recipe.images?.[0];

const Recipes = () => {
  const [recipes, setRecipes] = useState<Recipe[]>([]);
  const [loading, setLoading] = useState<boolean>(false);
  const [showImportModal, setShowImportModal] = useState(false);
  const [showAddModal, setShowAddModal] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [selectedCategory, setSelectedCategory] = useState<string>("");
  const [sortOrder, setSortOrder] = useState<string>("date-desc");
  const [searchQuery, setSearchQuery] = useState<string>("");
  const [currentPage, setCurrentPage] = useState<number>(1);
  const [itemsPerPage, setItemsPerPage] = useState<number>(12);

  const location = useLocation();
  const { user } = useContext(AuthContext);

  useEffect(() => {
    document.title = "Dishwise - Recipes";

    const fetchData = async () => {
      setLoading(true);
      try {
        const [recipesRes, categoriesRes] = await Promise.all([
          apiClient.get("/recipes"),
          apiClient.get("/categories"),
        ]);
        setRecipes(recipesRes.data);
        setCategories(categoriesRes.data);
      } catch (err) {
        console.error("Failed to load recipes or categories:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, []);

  // Reset to first page when filters change
  useEffect(() => {
    setCurrentPage(1);
  }, [selectedCategory, sortOrder, searchQuery, location.search, itemsPerPage]);

  const sortedAndFilteredRecipes = useMemo(() => {
    let result = [...recipes];
    const query = new URLSearchParams(location.search);
    const show = query.get("show");

    if (show === "my" && user) {
      result = result.filter((r) => r.userId === user.id);
    }

    if (selectedCategory) {
      result = result.filter((r) => r.category?.id === selectedCategory);
    }

    // Search filter
    if (searchQuery.trim()) {
      const searchLower = searchQuery.toLowerCase();
      result = result.filter(
        (r) =>
          r.title.toLowerCase().includes(searchLower) ||
          r.description?.toLowerCase().includes(searchLower) ||
          r.category?.name.toLowerCase().includes(searchLower),
      );
    }

    //Sort Order
    switch (sortOrder) {
      case "name-asc":
        result.sort((a, b) => a.title.localeCompare(b.title));
        break;
      case "name-desc":
        result.sort((a, b) => b.title.localeCompare(a.title));
        break;
      case "date-asc":
        result.sort(
          (a, b) =>
            new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
        );
        break;
      case "date-desc":
        result.sort(
          (a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
        );
        break;
      default:
        result.sort(
          (a, b) =>
            new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
        );
        break;
    }

    return result;
  }, [
    recipes,
    selectedCategory,
    sortOrder,
    location.search,
    user,
    searchQuery,
  ]);

  // Calculate pagination
  const totalPages = Math.ceil(sortedAndFilteredRecipes.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const paginatedRecipes = sortedAndFilteredRecipes.slice(startIndex, endIndex);

  const handleImportSuccess = () => {
    // Refresh recipes list after import
    apiClient.get("/recipes").then((res) => {
      setRecipes(res.data);
    });
  };

  const handleAddSuccess = () => {
    // Refresh recipes list after adding a recipe
    apiClient.get("/recipes").then((res) => {
      setRecipes(res.data);
    });
  };

  return (
    <div className="container">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h1>
          {location.search.includes("show=my") ? "My Recipes" : "All Recipes"}
        </h1>
        <div>
          <button
            className="btn btn-success me-2"
            onClick={() => setShowAddModal(true)}
          >
            Add Recipe
          </button>
          <button
            className="btn btn-accent-outline"
            onClick={() => setShowImportModal(true)}
          >
            Import from URL
          </button>
        </div>
      </div>

      <ImportRecipeModal
        show={showImportModal}
        onHide={() => setShowImportModal(false)}
        onSuccess={handleImportSuccess}
      />

      <AddRecipeModal
        show={showAddModal}
        onHide={() => setShowAddModal(false)}
        onSuccess={handleAddSuccess}
      />

      {/* Filter and Sort Controls */}
      <div className="row mb-4">
        <div className="col-md-4">
          <Form.Group controlId="searchFilter">
            <Form.Label>
              <i className="bi bi-search me-2"></i>
              Search Recipes
            </Form.Label>
            <Form.Control
              type="text"
              placeholder="Search by title, description, or category..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
            />
          </Form.Group>
        </div>
        <div className="col-md-4">
          <Form.Group controlId="categoryFilter">
            <Form.Label>Filter by Category</Form.Label>
            <Form.Control
              as="select"
              value={selectedCategory}
              onChange={(e) => setSelectedCategory(e.target.value)}
            >
              <option value="">All Categories</option>
              {categories.map((cat) => (
                <option key={cat.id} value={cat.id}>
                  {cat.name}
                </option>
              ))}
            </Form.Control>
          </Form.Group>
        </div>
        <div className="col-md-4">
          <Form.Group controlId="sortOrder">
            <Form.Label>Sort by</Form.Label>
            <Form.Control
              as="select"
              value={sortOrder}
              onChange={(e) => setSortOrder(e.target.value)}
            >
              <option value="date-desc">Date Added (Newest)</option>
              <option value="date-asc">Date Added (Oldest)</option>
              <option value="name-asc">Name (A-Z)</option>
              <option value="name-desc">Name (Z-A)</option>
            </Form.Control>
          </Form.Group>
        </div>
      </div>

      {/* Recipes List */}
      <div>
        {loading ? (
          <p>Loading recipes...</p>
        ) : sortedAndFilteredRecipes.length > 0 ? (
          <>
            <div className="mb-3">
              <p className="text-muted">
                Showing <strong>{startIndex + 1}</strong> to{" "}
                <strong>
                  {Math.min(endIndex, sortedAndFilteredRecipes.length)}
                </strong>{" "}
                of <strong>{sortedAndFilteredRecipes.length}</strong> recipe
                {sortedAndFilteredRecipes.length !== 1 ? "s" : ""}
                {searchQuery && ` matching "${searchQuery}"`}
              </p>
            </div>
            <div className="row">
              {paginatedRecipes.map((recipe: Recipe) => (
                <div className="col-sm-6 col-lg-4 mb-4" key={recipe.id}>
                  <article className="recipe-card">
                    {getCoverImage(recipe) ? (
                      <img
                        src={getCoverImage(recipe)?.url}
                        className="recipe-card__image"
                        alt={recipe.title}
                      />
                    ) : (
                      <div className="recipe-card__placeholder" aria-hidden="true">
                        No photo
                      </div>
                    )}
                    <div className="recipe-card__body">
                      <h5 className="recipe-card__title">
                        <Link to={`/recipes/${recipe.id}`}>{recipe.title}</Link>
                      </h5>
                      <p className="recipe-card__description">
                        {recipe.description}
                      </p>
                      <div className="recipe-card__meta">
                        <span className="badge-accent-light">
                          {recipe.category ? recipe.category.name : "Uncategorized"}
                        </span>
                      </div>
                      <div className="recipe-card__footer">
                        <Link to={`/recipes/${recipe.id}`} className="btn btn-accent">
                          View Recipe
                        </Link>
                      </div>
                    </div>
                  </article>
                </div>
              ))}
            </div>

            {/* Pagination Controls */}
            {totalPages > 1 && (
              <div className="d-flex justify-content-between align-items-center mt-4 mb-4">
                <div className="d-flex align-items-center gap-2">
                  <label htmlFor="itemsPerPage" className="me-2">
                    Items per page:
                  </label>
                  <Form.Select
                    id="itemsPerPage"
                    value={itemsPerPage}
                    onChange={(e) => setItemsPerPage(parseInt(e.target.value))}
                    style={{ width: "80px" }}
                  >
                    <option value={12}>12</option>
                    <option value={24}>24</option>
                    <option value={36}>36</option>
                    <option value={48}>48</option>
                    <option value={60}>60</option>
                  </Form.Select>
                </div>
                <Pagination>
                  <Pagination.First
                    onClick={() => setCurrentPage(1)}
                    disabled={currentPage === 1}
                  />
                  <Pagination.Prev
                    onClick={() => setCurrentPage(currentPage - 1)}
                    disabled={currentPage === 1}
                  />

                  {/* Show page numbers */}
                  {Array.from({ length: totalPages }, (_, i) => i + 1).map(
                    (page) => {
                      // Show first, last, current, and adjacent pages
                      if (
                        page === 1 ||
                        page === totalPages ||
                        (page >= currentPage - 1 && page <= currentPage + 1)
                      ) {
                        return (
                          <Pagination.Item
                            key={page}
                            active={page === currentPage}
                            onClick={() => setCurrentPage(page)}
                          >
                            {page}
                          </Pagination.Item>
                        );
                      } else if (
                        page === currentPage - 2 ||
                        page === currentPage + 2
                      ) {
                        return <Pagination.Ellipsis key={page} disabled />;
                      }
                      return null;
                    },
                  )}

                  <Pagination.Next
                    onClick={() => setCurrentPage(currentPage + 1)}
                    disabled={currentPage === totalPages}
                  />
                  <Pagination.Last
                    onClick={() => setCurrentPage(totalPages)}
                    disabled={currentPage === totalPages}
                  />
                </Pagination>
              </div>
            )}
          </>
        ) : (
          <p>No recipes found{searchQuery && ` matching "${searchQuery}"`}.</p>
        )}
      </div>
    </div>
  );
};

export default Recipes;
