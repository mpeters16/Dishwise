import { useEffect, useState, type FormEvent } from "react";
import apiClient from "../../api/apiClient";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import Form from "react-bootstrap/Form";
import type { Recipe } from "../../types/Recipe";

interface EditRecipeModalProps {
  show: boolean;
  onHide: () => void;
  onSuccess: (updated: Recipe) => void;
  recipe: Recipe;
}

export const EditRecipeModal = ({
  show,
  onHide,
  onSuccess,
  recipe,
}: EditRecipeModalProps) => {
  const [title, setTitle] = useState(recipe.title || "");
  const [description, setDescription] = useState(recipe.description || "");
  const [prepTimeHours, setPrepTimeHours] = useState(
    Math.floor((recipe.prepTimeMinutes || 0) / 60),
  );
  const [prepTimeMinutes, setPrepTimeMinutes] = useState(
    (recipe.prepTimeMinutes || 0) % 60,
  );
  const [cookTimeHours, setCookTimeHours] = useState(
    Math.floor((recipe.cookTimeMinutes || 0) / 60),
  );
  const [cookTimeMinutes, setCookTimeMinutes] = useState(
    (recipe.cookTimeMinutes || 0) % 60,
  );
  const [servings, setServings] = useState<number | "">(recipe.servings ?? "");
  const [categoryId, setCategoryId] = useState(recipe.category?.id || "");
  const [ingredients, setIngredients] = useState(
    (recipe.ingredients || [])
      .map((i) => `${i.quantity} ${i.unit} ${i.name}`.trim())
      .join("\n"),
  );
  const [instructions, setInstructions] = useState(
    (recipe.instructions || [])
      .sort((a, b) => a.order - b.order)
      .map((i) => i.step)
      .join("\n"),
  );
  const [categories, setCategories] = useState<{ id: string; name: string }[]>(
    [],
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    if (show) {
      setTitle(recipe.title || "");
      setDescription(recipe.description || "");
      setPrepTimeHours(Math.floor((recipe.prepTimeMinutes || 0) / 60));
      setPrepTimeMinutes((recipe.prepTimeMinutes || 0) % 60);
      setCookTimeHours(Math.floor((recipe.cookTimeMinutes || 0) / 60));
      setCookTimeMinutes((recipe.cookTimeMinutes || 0) % 60);
      setServings(recipe.servings ?? "");
      setCategoryId(recipe.category?.id || "");
      setIngredients(
        (recipe.ingredients || [])
          .map((i) => `${i.quantity} ${i.unit} ${i.name}`.trim())
          .join("\n"),
      );
      setInstructions(
        (recipe.instructions || [])
          .sort((a, b) => a.order - b.order)
          .map((i) => i.step)
          .join("\n"),
      );
      apiClient.get("/categories").then((res) => setCategories(res.data));
    }
  }, [show, recipe]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const prepTotal = prepTimeHours * 60 + prepTimeMinutes;
      const cookTotal = cookTimeHours * 60 + cookTimeMinutes;

      const formData = new FormData();
      formData.append("title", title);
      formData.append("description", description);
      formData.append("ingredients", ingredients);
      formData.append("instructions", instructions);
      formData.append("prepTimeMinutes", prepTotal.toString());
      formData.append("cookTimeMinutes", cookTotal.toString());
      if (servings) formData.append("servings", servings.toString());
      formData.append("categoryId", categoryId);

      const res = await apiClient.put<Recipe>(
        `/recipes/${recipe.id}`,
        formData,
        {
          headers: { "Content-Type": "multipart/form-data" },
        },
      );

      onSuccess(res.data);
      onHide();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update recipe");
    } finally {
      setLoading(false);
    }
  }

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Edit Recipe</Modal.Title>
      </Modal.Header>
      <Modal.Body style={{ maxHeight: "70vh", overflowY: "auto" }}>
        <Form onSubmit={handleSubmit}>
          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Name</Form.Label>
            <Form.Control
              type="text"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Description</Form.Label>
            <Form.Control
              as="textarea"
              rows={3}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Category</Form.Label>
            <Form.Select
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              required
            >
              <option value="">Select Category</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </Form.Select>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Ingredients</Form.Label>
            <Form.Control
              as="textarea"
              rows={4}
              value={ingredients}
              onChange={(e) => setIngredients(e.target.value)}
              placeholder="One ingredient per line"
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Instructions</Form.Label>
            <Form.Control
              as="textarea"
              rows={4}
              value={instructions}
              onChange={(e) => setInstructions(e.target.value)}
              placeholder="One step per line"
              required
            />
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Prep Time</Form.Label>
            <div className="d-flex gap-2">
              <Form.Select
                className="flex-grow-1"
                value={prepTimeHours}
                onChange={(e) => setPrepTimeHours(Number(e.target.value))}
              >
                {Array.from({ length: 24 }, (_, i) => (
                  <option key={i} value={i}>
                    {i} hour{i !== 1 ? "s" : ""}
                  </option>
                ))}
              </Form.Select>
              <Form.Select
                className="flex-grow-1"
                value={prepTimeMinutes}
                onChange={(e) => setPrepTimeMinutes(Number(e.target.value))}
              >
                {Array.from({ length: 60 }, (_, i) => (
                  <option key={i} value={i}>
                    {i} min{i !== 1 ? "s" : ""}
                  </option>
                ))}
              </Form.Select>
            </div>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Cook Time</Form.Label>
            <div className="d-flex gap-2">
              <Form.Select
                className="flex-grow-1"
                value={cookTimeHours}
                onChange={(e) => setCookTimeHours(Number(e.target.value))}
              >
                {Array.from({ length: 24 }, (_, i) => (
                  <option key={i} value={i}>
                    {i} hour{i !== 1 ? "s" : ""}
                  </option>
                ))}
              </Form.Select>
              <Form.Select
                className="flex-grow-1"
                value={cookTimeMinutes}
                onChange={(e) => setCookTimeMinutes(Number(e.target.value))}
              >
                {Array.from({ length: 60 }, (_, i) => (
                  <option key={i} value={i}>
                    {i} min{i !== 1 ? "s" : ""}
                  </option>
                ))}
              </Form.Select>
            </div>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold" htmlFor="edit-recipe-servings">
              Servings
            </Form.Label>
            <Form.Control
              id="edit-recipe-servings"
              type="number"
              min={1}
              max={100}
              value={servings}
              onChange={(e) =>
                setServings(e.target.value === "" ? "" : Math.min(100, Number(e.target.value)))
              }
              placeholder="How many does it serve?"
            />
          </Form.Group>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}
        </Form>
      </Modal.Body>
      <Modal.Footer>
        <Button variant="secondary" onClick={onHide}>
          Cancel
        </Button>
        <Button variant="primary" onClick={handleSubmit} disabled={loading}>
          {loading ? "Saving..." : "Save Changes"}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};
