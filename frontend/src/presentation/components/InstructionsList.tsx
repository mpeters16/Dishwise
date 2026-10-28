import {
  formatInstruction,
  isValidInstruction,
} from "../../utils/recipeFormatter";
import type { Instruction } from "../../types/Instruction";
import type { RecipeImage } from "../../types/RecipeImage";

interface InstructionsListProps {
  instructions?: Instruction[];
  stepImages?: RecipeImage[];
  className?: string;
}

export const InstructionsList = ({
  instructions = [],
  stepImages = [],
  className = "",
}: InstructionsListProps) => {
  const validInstructions = instructions.filter(isValidInstruction);

  if (validInstructions.length === 0) {
    return <p className="text-muted">No instructions available</p>;
  }

  // Sort by order if available
  const sorted = [...validInstructions].sort(
    (a, b) => (a.order || 0) - (b.order || 0),
  );

  // Build a lookup map: stepOrder → image url
  // If images have stepOrder set, use that for precise matching;
  // otherwise fall back to index-based matching.
  const hasOrderedImages = stepImages.some((img) => img.stepOrder != null);
  const stepImageMap = new Map<number, string>();
  if (hasOrderedImages) {
    for (const img of stepImages) {
      if (img.stepOrder != null) stepImageMap.set(img.stepOrder, img.url);
    }
  }

  return (
    <div className={className}>
      <ol className="list-group">
        {sorted.map((ins, idx) => {
          const stepNum = ins.order || idx + 1;
          const imgUrl = hasOrderedImages
            ? stepImageMap.get(stepNum)
            : stepImages[idx]?.url;

          return (
            <li key={idx} className="list-group-item align-items-start" style={{ padding: "1rem 1.25rem" }}>
              <div className="fw-bold mb-1" style={{ color: "#333" }}>
                Step {stepNum}
              </div>
              <p className="mb-0" style={{ color: "#666" }}>
                {formatInstruction(ins)}
              </p>
              {imgUrl && (
                <img
                  src={imgUrl}
                  alt={`Step ${stepNum}`}
                  className="img-fluid rounded mt-3"
                  style={{ maxHeight: "220px", objectFit: "cover", width: "100%" }}
                />
              )}
            </li>
          );
        })}
      </ol>
    </div>
  );
};
