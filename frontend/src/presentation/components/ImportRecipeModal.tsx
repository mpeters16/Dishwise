import { useContext, useState, type FormEvent } from "react";
import axios from "axios";
import apiClient from "../../api/apiClient";
import Modal from "react-bootstrap/Modal";
import Button from "react-bootstrap/Button";
import Form from "react-bootstrap/Form";
import { AuthContext } from "../context/AuthContext";

interface ImportRecipeModalProps {
  show: boolean;
  onHide: () => void;
  onSuccess: () => void;
}

export const ImportRecipeModal = ({
  show,
  onHide,
  onSuccess,
}: ImportRecipeModalProps) => {
  const { user } = useContext(AuthContext);
  const [url, setUrl] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string>("");

  const handleImport = async (e: FormEvent) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      if (!user) {
        setError("User not authenticated");
        setLoading(false);
        return;
      }

      // Validate user ID
      if (!user.id || user.id.trim() === "") {
        setError("User ID is missing. Please login again.");
        setLoading(false);
        return;
      }

      if (!url.trim()) {
        setError("Please enter a recipe URL");
        setLoading(false);
        return;
      }

      const result = await apiClient.post(`/recipesimport`, {
        url: url.trim(),
        userId: user.id.trim(),
      });

      // Handle successful response (200 or 201)
      if (result.status === 200 || result.status === 201) {
        setUrl("");
        onSuccess();
        onHide();
      }
    } catch (err: unknown) {
      let errorMsg = "Failed to import recipe";
      const response = axios.isAxiosError<string>(err) ? err.response : undefined;

      // Extract error message from different error types
      if (response?.data) {
        errorMsg = response.data;
      } else if (response?.status === 400) {
        errorMsg = "Invalid request. Check your URL and try again.";
      } else if (response?.status === 401) {
        errorMsg = "Please login again";
      } else if (response?.status === 404) {
        errorMsg = "Recipe URL not found";
      } else if (response?.status === 500) {
        errorMsg = "Server error. Please try again later.";
      } else if (err instanceof Error) {
        errorMsg = err.message;
      }

      setError(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <Modal show={show} onHide={onHide} centered>
      <Modal.Header closeButton>
        <Modal.Title>Import Recipe from URL</Modal.Title>
      </Modal.Header>
      <Modal.Body>
        <Form onSubmit={handleImport}>
          <Form.Group className="mb-3">
            <Form.Label>Recipe URL</Form.Label>
            <Form.Control
              type="url"
              placeholder="Enter recipe URL"
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              required
            />
          </Form.Group>

          {error && (
            <div className="alert alert-danger" role="alert">
              {error}
            </div>
          )}

          <div className="d-grid gap-2">
            <Button variant="primary" type="submit" disabled={loading}>
              {loading ? "Importing..." : "Import Recipe"}
            </Button>
          </div>
        </Form>
      </Modal.Body>
    </Modal>
  );
};
