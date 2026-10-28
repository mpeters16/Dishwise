import { useState } from "react";
import {
  formatIngredient,
  isValidIngredient,
} from "../../utils/recipeFormatter";
import type { Ingredient } from "../../types/Ingredient";

interface IngredientsListProps {
  ingredients?: Ingredient[];
  className?: string;
}

export const IngredientsList = ({
  ingredients = [],
  className = "",
}: IngredientsListProps) => {
  const validIngredients = ingredients.filter(isValidIngredient);
  const [checkedItems, setCheckedItems] = useState<Set<number>>(new Set());

  const toggleIngredient = (idx: number) => {
    setCheckedItems((prev) => {
      const updated = new Set(prev);
      if (updated.has(idx)) {
        updated.delete(idx);
      } else {
        updated.add(idx);
      }
      return updated;
    });
  };

  if (validIngredients.length === 0) {
    return <p className="text-muted">No ingredients available</p>;
  }

  return (
    <div className={className}>
      <ul className="list-unstyled">
        {validIngredients.map((ing, idx) => (
          <li key={idx} className="">
            <div className="d-flex">
              <input
                type="checkbox"
                className="form-check-input me-2"
                id={`ingredient-${idx}`}
                checked={checkedItems.has(idx)}
                onChange={() => toggleIngredient(idx)}
                aria-label={`Check off ingredient: ${formatIngredient(ing)}`}
              />
              <label
                htmlFor={`ingredient-${idx}`}
                className={`mb-0 cursor-pointer ${
                  checkedItems.has(idx)
                    ? "text-decoration-line-through text-muted"
                    : ""
                }`}
                style={{ cursor: "pointer" }}
              >
                {formatIngredient(ing)}
              </label>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};
