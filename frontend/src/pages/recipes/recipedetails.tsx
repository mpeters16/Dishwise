import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import apiClient from "../../api/apiClient";
import { IngredientsList } from "../../presentation/components/IngredientsList";
import { InstructionsList } from "../../presentation/components/InstructionsList";
import { NutritionInfo } from "../../presentation/components/NutritionInfo";
import { EditRecipeModal } from "../../presentation/components/EditRecipeModal";
import { ImageSlider } from "../../presentation/components/ImageSlider";
import { ConfirmDeleteModal } from "../../presentation/components/ConfirmDeleteModal";
import { toast } from "react-toastify";
import { addIngredientsToList, loadShoppingList, saveShoppingList } from "../../utils/shoppingList";
import type { Recipe } from "../../types/Recipe";

const RecipeDetails = () => {
  useEffect(() => {
    document.title = "Dishwise - Details";
  }, []);

  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const [recipe, setRecipe] = useState<Recipe | null>(null);
  const [loading, setLoading] = useState<boolean>(false);

  const [activeTab, setActiveTab] = useState<
    "ingredients" | "instructions" | "nutrition"
  >("ingredients");
  const [showEditModal, setShowEditModal] = useState(false);
  const [showDeleteModal, setShowDeleteModal] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState("");
  const [servings, setServings] = useState(1);

  // Start from the recipe's own servings once it loads
  useEffect(() => {
    if (recipe?.servings) setServings(recipe.servings);
  }, [recipe?.servings]);

  // Quantities are multiplied by this when the servings differ from the recipe's
  const scale = recipe?.servings ? servings / recipe.servings : 1;

  const addToShoppingList = () => {
    if (!recipe) return;
    const updated = addIngredientsToList(loadShoppingList(), recipe.ingredients, recipe.title, scale);
    saveShoppingList(updated);
    toast.success(`Added ${recipe.title} to your shopping list`);
  };

  useEffect(() => {
    const fetchRecipe = async () => {
      setLoading(true);
      try {
        const response = await apiClient.get<Recipe>(`/recipes/${id}`);
        setRecipe(response.data);
      } catch (err) {
        console.error("Failed to fetch recipe", err);
      } finally {
        setLoading(false);
      }
    };

    fetchRecipe();
  }, [id]);

  const openDeleteModal = () => {
    setDeleteError("");
    setShowDeleteModal(true);
  };

  const confirmDelete = async () => {
    setIsDeleting(true);
    setDeleteError("");
    try {
      await apiClient.delete(`/recipes/${id}`);
      navigate("/recipes");
    } catch {
      setDeleteError("Couldn't delete the recipe. Please try again.");
      setIsDeleting(false);
    }
  };

  if (loading) {
    return (
      <div className="container py-4">
        <p>Loading...</p>
      </div>
    );
  }

  if (!recipe) {
    return (
      <div className="container py-4">
        <p>Recipe not found.</p>
      </div>
    );
  }

  return (
    <div className="container">
      {/* Back link */}
      <div className="mb-3">
        <a href="/recipes" className="text-muted" style={{ fontSize: "0.9rem" }}>
          ← Back to Recipes
        </a>
      </div>

      <h1 className="mb-3">{recipe.title}</h1>

      <ImageSlider images={recipe.images ?? []} title={recipe.title} />

      {/* Meta info panel */}
      <div className="panel mb-4">
        <div className="row g-3">
          <div className="col-sm-4">
            <span className="label">Category</span>
            <p className="mb-0 mt-1">
              <span className="badge-accent">
                {recipe.category ? recipe.category.name : "Unknown"}
              </span>
            </p>
          </div>
          <div className="col-sm-4">
            <span className="label">Prep Time</span>
            <p className="mb-0 mt-1">{recipe.prepTimeMinutes} min</p>
          </div>
          <div className="col-sm-4">
            <span className="label">Cook Time</span>
            <p className="mb-0 mt-1">{recipe.cookTimeMinutes} min</p>
          </div>
        </div>
        {recipe.description?.trim() && (
          <p className="mt-3 mb-0" style={{ color: "var(--text-dark)" }}>
            {recipe.description}
          </p>
        )}
        {recipe.sourceUrl && (
          <p className="mt-2 mb-0" style={{ fontSize: "0.78rem", color: "var(--text-muted, #888)" }}>
            Source:{" "}
            <a href={recipe.sourceUrl} target="_blank" rel="noopener noreferrer" style={{ color: "#b36a00", wordBreak: "break-all" }}>
              {recipe.sourceUrl}
            </a>
          </p>
        )}
      </div>

      {/* Tabs */}
      <div className="d-flex mb-0" style={{ borderBottom: "2px solid var(--panel-border)" }}>
        {(["ingredients", "instructions", ...(recipe.nutrition ? ["nutrition"] : [])] as Array<"ingredients" | "instructions" | "nutrition">).map((tab) => (
          <button
            key={tab}
            className="btn me-1"
            onClick={() => setActiveTab(tab)}
            style={{
              borderRadius: "8px 8px 0 0",
              border: "none",
              borderBottom: activeTab === tab ? "3px solid var(--accent)" : "3px solid transparent",
              color: activeTab === tab ? "var(--accent)" : "var(--text-dark)",
              fontWeight: activeTab === tab ? 700 : 500,
              background: activeTab === tab ? "var(--panel-bg)" : "transparent",
              padding: "8px 20px",
              transition: "all 0.15s",
            }}
          >
            {tab.charAt(0).toUpperCase() + tab.slice(1)}
          </button>
        ))}
      </div>

      {/* Tab Content */}
      {activeTab === "ingredients" && (
        <div className="panel" style={{ borderRadius: "0 var(--radius) var(--radius) var(--radius)" }}>
          <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-4">
            <h4 className="subheading mb-0">Ingredients</h4>
            <div className="d-flex flex-wrap align-items-center gap-2">
              {recipe.servings ? (
                <div className="d-flex align-items-center gap-1" role="group" aria-label="Servings">
                  <button
                    type="button"
                    className="btn btn-outline-secondary btn-sm"
                    onClick={() => setServings((s) => Math.max(1, s - 1))}
                    disabled={servings <= 1}
                    aria-label="Fewer servings"
                  >
                    −
                  </button>
                  <span className="px-2 fw-semibold" aria-live="polite">
                    {servings} serving{servings !== 1 ? "s" : ""}
                  </span>
                  <button
                    type="button"
                    className="btn btn-outline-secondary btn-sm"
                    onClick={() => setServings((s) => Math.min(100, s + 1))}
                    disabled={servings >= 100}
                    aria-label="More servings"
                  >
                    +
                  </button>
                  {servings !== recipe.servings && (
                    <button
                      type="button"
                      className="btn btn-link btn-sm p-0 ms-1"
                      onClick={() => setServings(recipe.servings ?? 1)}
                    >
                      Reset
                    </button>
                  )}
                </div>
              ) : null}
              <button type="button" className="btn btn-accent btn-sm" onClick={addToShoppingList}>
                + Shopping list
              </button>
            </div>
          </div>
          <IngredientsList ingredients={recipe.ingredients} scale={scale} />
        </div>
      )}

      {activeTab === "instructions" && (
        <div className="panel" style={{ borderRadius: "0 var(--radius) var(--radius) var(--radius)" }}>
          <h4 className="subheading mb-4">Instructions</h4>
          <InstructionsList
            instructions={recipe.instructions}
            stepImages={recipe.images?.filter((img) => img.stepOrder != null) ?? []}
          />
        </div>
      )}

      {activeTab === "nutrition" && recipe.nutrition && (
        <div className="panel" style={{ borderRadius: "0 var(--radius) var(--radius) var(--radius)" }}>
          <NutritionInfo nutrition={recipe.nutrition} />
        </div>
      )}

      {/* Action Buttons */}
      <div className="d-flex gap-2 mt-4">
        <button className="btn btn-accent" onClick={() => setShowEditModal(true)}>
          Edit Recipe
        </button>
        <button className="btn btn-delete" onClick={openDeleteModal}>
          Delete Recipe
        </button>
      </div>

      <ConfirmDeleteModal
        show={showDeleteModal}
        recipeTitle={recipe?.title ?? ""}
        isDeleting={isDeleting}
        error={deleteError}
        onConfirm={confirmDelete}
        onCancel={() => setShowDeleteModal(false)}
      />

      {/* Edit Modal */}
      <EditRecipeModal
        show={showEditModal}
        onHide={() => setShowEditModal(false)}
        onSuccess={(updated: Recipe) => setRecipe(updated)}
        recipe={recipe}
      />
    </div>
  );
};

export default RecipeDetails;
