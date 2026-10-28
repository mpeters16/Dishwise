import type { Nutrition } from "../../types/Nutrition";

interface NutritionInfoProps {
  nutrition?: Nutrition;
  className?: string;
}

interface NutrientRow {
  label: string;
  value: number | undefined;
  unit: string;
  decimals?: number;
}

export const NutritionInfo = ({
  nutrition,
  className = "",
}: NutritionInfoProps) => {
  if (!nutrition) {
    return (
      <div className={`text-muted ${className}`}>
        Nutrition information not available
      </div>
    );
  }

  const macros: NutrientRow[] = [
    { label: "Calories", value: nutrition.calories, unit: "kcal", decimals: 0 },
    { label: "Protein", value: nutrition.protein, unit: "g", decimals: 1 },
    {
      label: "Carbohydrates",
      value: nutrition.carbohydrates,
      unit: "g",
      decimals: 1,
    },
    { label: "Fat", value: nutrition.fat, unit: "g", decimals: 1 },
    { label: "Fiber", value: nutrition.fiber, unit: "g", decimals: 1 },
    { label: "Sugar", value: nutrition.sugar, unit: "g", decimals: 1 },
  ];

  const micros: NutrientRow[] = [
    { label: "Sodium", value: nutrition.sodium, unit: "mg", decimals: 0 },
    { label: "Potassium", value: nutrition.potassium, unit: "mg", decimals: 0 },
    { label: "Calcium", value: nutrition.calcium, unit: "mg", decimals: 0 },
    { label: "Iron", value: nutrition.iron, unit: "mg", decimals: 1 },
    { label: "Vitamin A", value: nutrition.vitamin_A, unit: "IU", decimals: 0 },
    { label: "Vitamin C", value: nutrition.vitamin_C, unit: "mg", decimals: 1 },
    { label: "Vitamin D", value: nutrition.vitamin_D, unit: "µg", decimals: 1 },
    {
      label: "Cholesterol",
      value: nutrition.cholesterol,
      unit: "mg",
      decimals: 0,
    },
  ].filter((row) => row.value !== undefined && row.value !== null);

  return (
    <div className={className}>
      <h5 className="subheading mb-3">Nutrition Facts (Per Serving)</h5>

      {/* Macronutrients table */}
      <div className="table-responsive mb-4">
        <table className="table align-middle accent-table">
          <thead>
            <tr>
              <th>Nutrient</th>
              <th style={{ textAlign: "right" }}>Amount</th>
              <th>Unit</th>
            </tr>
          </thead>
          <tbody>
            {macros.map((row) => (
              <tr key={row.label}>
                <td className="fw-semibold">{row.label}</td>
                <td style={{ textAlign: "right" }}>
                  {row.value?.toFixed(row.decimals ?? 1)}
                </td>
                <td className="text-muted">{row.unit}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Micronutrients table */}
      {micros.length > 0 && (
        <div className="table-responsive">
          <table className="table align-middle accent-table">
            <thead>
              <tr>
                <th>Micronutrient</th>
                <th style={{ textAlign: "right" }}>Amount</th>
                <th>Unit</th>
              </tr>
            </thead>
            <tbody>
              {micros.map((row) => (
                <tr key={row.label}>
                  <td className="fw-semibold">{row.label}</td>
                  <td style={{ textAlign: "right" }}>
                    {row.value?.toFixed(row.decimals ?? 1)}
                  </td>
                  <td className="text-muted">{row.unit}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
