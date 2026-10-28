export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5000";

export const ENDPOINTS = {
  AUTH: {
    LOGIN: "/api/v1/login",
    REGISTER: "/api/v1/register",
    LOGOUT: "/api/v1/logout",
    RESET_PASSWORD: "/api/v1/reset-password",
  },
  RECIPES: {
    BASE: "/api/v1/recipes",
    IMPORT: "/api/v1/recipes/import",
  },
};

export const STORAGE_KEYS = {
  AUTH_TOKEN: "authToken",
  CURRENT_USER: "currentUser",
};
