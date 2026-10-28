import apiClient from "./apiClient";

export const login = async (username: string, password: string) => {
  const response = await apiClient.post("/login", { username, password });
  const token = response.data.token;
  localStorage.setItem("authToken", token);
  return token;
};
