import Modal from "react-bootstrap/Modal";
import { LoadingButton } from "./LoadingButton";

interface ConfirmDeleteModalProps {
  show: boolean;
  recipeTitle: string;
  isDeleting: boolean;
  error?: string;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmDeleteModal = ({
  show,
  recipeTitle,
  isDeleting,
  error,
  onConfirm,
  onCancel,
}: ConfirmDeleteModalProps) => {
  return (
    <Modal
      show={show}
      onHide={onCancel}
      centered
      backdrop={isDeleting ? "static" : true}
      keyboard={!isDeleting}
    >
      <Modal.Header closeButton={!isDeleting} className="border-0 pb-0">
        <Modal.Title as="h5" className="fw-bold">
          Delete this recipe?
        </Modal.Title>
      </Modal.Header>

      <Modal.Body className="pt-2">
        <p className="mb-2">
          <strong>&ldquo;{recipeTitle}&rdquo;</strong> will be permanently removed,
          along with its ingredients, instructions and photos.
        </p>
        <p className="text-muted small mb-0">This can&rsquo;t be undone.</p>

        {error && (
          <p className="text-danger small mt-3 mb-0" role="alert">
            {error}
          </p>
        )}
      </Modal.Body>

      <Modal.Footer className="border-0 pt-0">
        <button
          type="button"
          className="btn btn-outline-secondary"
          onClick={onCancel}
          disabled={isDeleting}
        >
          Cancel
        </button>
        <LoadingButton
          type="button"
          className="btn btn-delete"
          isLoading={isDeleting}
          loadingText="Deleting…"
          onClick={onConfirm}
        >
          Delete recipe
        </LoadingButton>
      </Modal.Footer>
    </Modal>
  );
};
