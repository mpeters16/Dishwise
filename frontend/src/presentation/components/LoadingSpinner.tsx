import type { ReactNode } from "react";

interface LoadingSpinnerProps {
  isLoading: boolean;
  loadingText?: string;
  children: ReactNode;
}

export const LoadingSpinner = ({
  isLoading,
  loadingText = "Loading...",
  children,
}: LoadingSpinnerProps) => {
  if (!isLoading) {
    return <>{children}</>;
  }

  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        gap: "8px",
      }}
    >
      <span
        className="spinner-border spinner-border-sm"
        role="status"
        aria-hidden="true"
      ></span>
      <span>{loadingText}</span>
    </div>
  );
};
