import type { ButtonHTMLAttributes, ReactNode } from "react";

interface LoadingButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  isLoading?: boolean;
  loadingText?: string;
  children: ReactNode;
}

export const LoadingButton = ({
  isLoading = false,
  loadingText = "Loading...",
  disabled,
  children,
  ...props
}: LoadingButtonProps) => {
  return (
    <button
      disabled={isLoading || disabled}
      {...props}
      style={{
        position: "relative",
        minWidth: "100px",
        ...props.style,
      }}
    >
      {isLoading ? (
        <>
          <span
            className="spinner-border spinner-border-sm"
            role="status"
            aria-hidden="true"
            style={{ marginRight: "8px" }}
          ></span>
          <span>{loadingText}</span>
        </>
      ) : (
        children
      )}
    </button>
  );
};
