import { useContext, useEffect, useState, type FormEvent } from "react";
import apiClient from "../../api/apiClient";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import Form from "react-bootstrap/Form";
import { AuthContext } from "../context/AuthContext";

interface AddRecipeModalProps {
  show: boolean;
  onHide: () => void;
  onSuccess: () => void;
}

export const AddRecipeModal = ({
  show,
  onHide,
  onSuccess,
}: AddRecipeModalProps) => {
  const { user } = useContext(AuthContext);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [ingredients, setIngredients] = useState("");
  const [instructions, setInstructions] = useState("");
  const [prepTimeHours, setPrepTimeHours] = useState(0);
  const [prepTimeMinutes, setPrepTimeMinutes] = useState(0);
  const [cookTimeHours, setCookTimeHours] = useState(0);
  const [cookTimeMinutes, setCookTimeMinutes] = useState(0);
  const [categoryId, setCategoryId] = useState("");
  const [image, setImage] = useState<File | null>(null);
  const [categories, setCategories] = useState<{ id: string; name: string }[]>(
    [],
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>("");

  useEffect(() => {
    if (show) {
      apiClient.get(`/categories`).then((res) => {
        setCategories(res.data);
      });
    }
  }, [show]);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      if (!user) {
        setError("User not authenticated");
        setLoading(false);
        return;
      }

      const prepTimeMinutesTotal = prepTimeHours * 60 + prepTimeMinutes;
      const cookTimeMinutesTotal = cookTimeHours * 60 + cookTimeMinutes;

      const formData = new FormData();
      formData.append("title", title);
      formData.append("description", description);
      formData.append("ingredients", ingredients);
      formData.append("instructions", instructions);
      formData.append("prepTimeMinutes", prepTimeMinutesTotal.toString());
      formData.append("cookTimeMinutes", cookTimeMinutesTotal.toString());
      formData.append("categoryId", categoryId);
      formData.append("userId", user.id);

      if (image) formData.append("image", image);

      await apiClient.post(`/recipes`, formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      // Reset form
      setTitle("");
      setDescription("");
      setIngredients("");
      setInstructions("");
      setPrepTimeHours(0);
      setPrepTimeMinutes(0);
      setCookTimeHours(0);
      setCookTimeMinutes(0);
      setCategoryId("");
      setImage(null);

      onSuccess();
      onHide();
    } catch (err) {
      const errorMsg =
        err instanceof Error ? err.message : "Failed to add recipe";
      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Create Recipe</Modal.Title>
      </Modal.Header>
      <Modal.Body style={{ maxHeight: "70vh", overflowY: "auto" }}>
        <Form onSubmit={handleSubmit}>
          {/* IMAGE UPLOAD */}
          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Image</Form.Label>
            <Form.Control
              type="file"
              accept="image/*"
              onChange={(e) => {
                const target = e.target as HTMLInputElement;
                setImage(target.files?.[0] || null);
              }}
            />
          </Form.Group>

          {/* BASIC INFO */}
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

          {/* INGREDIENTS */}
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

          {/* INSTRUCTIONS */}
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

          {/* TIMING */}
          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Prep Time</Form.Label>
            <div className="d-flex gap-2">
              <div className="flex-grow-1">
                <Form.Select
                  value={prepTimeHours}
                  onChange={(e) => setPrepTimeHours(Number(e.target.value))}
                >
                  {Array.from({ length: 24 }, (_, i) => (
                    <option key={i} value={i}>
                      {i} hour{i !== 1 ? "s" : ""}
                    </option>
                  ))}
                </Form.Select>
              </div>
              <div className="flex-grow-1">
                <Form.Select
                  value={prepTimeMinutes}
                  onChange={(e) => setPrepTimeMinutes(Number(e.target.value))}
                >
                  {Array.from({ length: 60 }, (_, i) => (
                    <option key={i} value={i}>
                      {i} minute{i !== 1 ? "s" : ""}
                    </option>
                  ))}
                </Form.Select>
              </div>
            </div>
          </Form.Group>

          <Form.Group className="mb-3">
            <Form.Label className="fw-bold">Cook Time</Form.Label>
            <div className="d-flex gap-2">
              <div className="flex-grow-1">
                <Form.Select
                  value={cookTimeHours}
                  onChange={(e) => setCookTimeHours(Number(e.target.value))}
                >
                  {Array.from({ length: 24 }, (_, i) => (
                    <option key={i} value={i}>
                      {i} hour{i !== 1 ? "s" : ""}
                    </option>
                  ))}
                </Form.Select>
              </div>
              <div className="flex-grow-1">
                <Form.Select
                  value={cookTimeMinutes}
                  onChange={(e) => setCookTimeMinutes(Number(e.target.value))}
                >
                  {Array.from({ length: 60 }, (_, i) => (
                    <option key={i} value={i}>
                      {i} minute{i !== 1 ? "s" : ""}
                    </option>
                  ))}
                </Form.Select>
              </div>
            </div>
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
          {loading ? "Saving..." : "Save Recipe"}
        </Button>
      </Modal.Footer>
    </Modal>
  );
};
