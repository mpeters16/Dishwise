import { useState } from "react";
import type { RecipeImage } from "../../types/RecipeImage";

interface ImageSliderProps {
  images: RecipeImage[];
  title: string;
}

export const ImageSlider = ({ images, title }: ImageSliderProps) => {
  const [current, setCurrent] = useState(0);

  if (!images || images.length === 0) return null;

  if (images.length === 1) {
    return (
      <img
        src={images[0].url}
        alt={title}
        className="img-fluid rounded mb-4"
        style={{ maxHeight: "450px", objectFit: "cover", width: "100%" }}
      />
    );
  }

  const prev = () => setCurrent((c) => (c - 1 + images.length) % images.length);
  const next = () => setCurrent((c) => (c + 1) % images.length);

  return (
    <div className="image-slider mb-4" style={{ position: "relative" }}>
      <img
        src={images[current].url}
        alt={`${title} – image ${current + 1}`}
        className="img-fluid rounded"
        style={{ maxHeight: "450px", objectFit: "cover", width: "100%" }}
      />

      {/* Prev / Next arrows */}
      <button
        onClick={prev}
        aria-label="Previous image"
        style={{
          position: "absolute",
          top: "50%",
          left: "12px",
          transform: "translateY(-50%)",
          background: "rgba(0,0,0,0.45)",
          border: "none",
          borderRadius: "50%",
          width: "40px",
          height: "40px",
          color: "#fff",
          fontSize: "1.2rem",
          cursor: "pointer",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          zIndex: 2,
        }}
      >
        ‹
      </button>
      <button
        onClick={next}
        aria-label="Next image"
        style={{
          position: "absolute",
          top: "50%",
          right: "12px",
          transform: "translateY(-50%)",
          background: "rgba(0,0,0,0.45)",
          border: "none",
          borderRadius: "50%",
          width: "40px",
          height: "40px",
          color: "#fff",
          fontSize: "1.2rem",
          cursor: "pointer",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          zIndex: 2,
        }}
      >
        ›
      </button>

      {/* Dot indicators */}
      <div
        style={{
          position: "absolute",
          bottom: "12px",
          left: "50%",
          transform: "translateX(-50%)",
          display: "flex",
          gap: "6px",
          zIndex: 2,
        }}
      >
        {images.map((_, i) => (
          <button
            key={i}
            onClick={() => setCurrent(i)}
            aria-label={`Go to image ${i + 1}`}
            style={{
              width: "10px",
              height: "10px",
              borderRadius: "50%",
              border: "none",
              padding: 0,
              background:
                i === current ? "var(--accent)" : "rgba(255,255,255,0.7)",
              cursor: "pointer",
              transition: "background 0.2s",
            }}
          />
        ))}
      </div>

      {/* Counter */}
      <span
        style={{
          position: "absolute",
          top: "12px",
          right: "12px",
          background: "rgba(0,0,0,0.5)",
          color: "#fff",
          borderRadius: "12px",
          padding: "2px 10px",
          fontSize: "0.8rem",
          zIndex: 2,
        }}
      >
        {current + 1} / {images.length}
      </span>
    </div>
  );
};
