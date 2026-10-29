import { useEffect, useMemo, useState, type FormEvent } from "react";
import { Link } from "react-router-dom";
import { formatIngredient } from "../../utils/recipeFormatter";
import {
  addManualItem,
  loadShoppingList,
  saveShoppingList,
  type ShoppingItem,
} from "../../utils/shoppingList";

const ShoppingListPage = () => {
  useEffect(() => {
    document.title = "Dishwise - Shopping list";
  }, []);

  const [items, setItems] = useState<ShoppingItem[]>(() => loadShoppingList());
  const [newItem, setNewItem] = useState("");

  useEffect(() => {
    saveShoppingList(items);
  }, [items]);

  // Items still to buy first, ticked ones move to the bottom
  const sortedItems = useMemo(
    () => [...items].sort((a, b) => Number(a.checked) - Number(b.checked)),
    [items],
  );
  const doneCount = items.filter((item) => item.checked).length;

  const toggleItem = (id: string) =>
    setItems((prev) =>
      prev.map((item) => (item.id === id ? { ...item, checked: !item.checked } : item)),
    );

  const removeItem = (id: string) =>
    setItems((prev) => prev.filter((item) => item.id !== id));

  const clearDone = () => setItems((prev) => prev.filter((item) => !item.checked));

  const clearAll = () => setItems([]);

  const handleAdd = (e: FormEvent) => {
    e.preventDefault();
    setItems((prev) => addManualItem(prev, newItem));
    setNewItem("");
  };

  return (
    <div className="shopping-list">
      <div className="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-4">
        <h1 className="mb-0">Shopping list</h1>
        <Link to="/recipes" className="btn btn-accent-outline btn-sm">
          Browse recipes
        </Link>
      </div>

      <form onSubmit={handleAdd} className="flex-row gap-2 mb-4">
        <input
          type="text"
          className="form-control"
          placeholder="Add an item, e.g. milk"
          aria-label="Add an item"
          value={newItem}
          onChange={(e) => setNewItem(e.target.value)}
        />
        <button type="submit" className="btn btn-accent" disabled={!newItem.trim()}>
          Add
        </button>
      </form>

      {items.length === 0 ? (
        <div className="panel text-center py-5">
          <p className="fw-semibold mb-1">Your shopping list is empty</p>
          <p className="text-muted mb-0">
            Open a recipe and choose “Add to shopping list”.
          </p>
        </div>
      ) : (
        <div className="panel">
          <p className="text-muted small mb-3">
            {doneCount} of {items.length} done
          </p>

          <ul className="list-unstyled mb-0">
            {sortedItems.map((item) => (
              <li
                key={item.id}
                className="d-flex align-items-start gap-3 py-2 border-bottom"
              >
                <input
                  type="checkbox"
                  className="form-check-input mt-1"
                  id={`item-${item.id}`}
                  checked={item.checked}
                  onChange={() => toggleItem(item.id)}
                />
                <label htmlFor={`item-${item.id}`} className="flex-grow-1 mb-0" style={{ cursor: "pointer" }}>
                  <span
                    className={
                      item.checked ? "text-decoration-line-through text-muted" : "fw-medium"
                    }
                  >
                    {formatIngredient(item)}
                  </span>
                  {item.sources.length > 0 && (
                    <div className="small text-muted">From {item.sources.join(", ")}</div>
                  )}
                </label>
                <button
                  type="button"
                  className="btn btn-sm text-muted p-0"
                  aria-label={`Remove ${item.name}`}
                  onClick={() => removeItem(item.id)}
                >
                  ✕
                </button>
              </li>
            ))}
          </ul>

          <div className="d-flex justify-content-end gap-2 mt-3">
            <button
              type="button"
              className="btn btn-outline-secondary btn-sm"
              onClick={clearDone}
              disabled={doneCount === 0}
            >
              Clear done
            </button>
            <button type="button" className="btn btn-outline-secondary btn-sm" onClick={clearAll}>
              Clear all
            </button>
          </div>
        </div>
      )}
    </div>
  );
};

export default ShoppingListPage;
