import React from "react";

export function Layout({ children }: { children: React.ReactNode }) {
  return (
    <div className="container-fluid p-0 w-100" style={{ minHeight: "100px" }}>
      {children}
    </div>
  );
}
