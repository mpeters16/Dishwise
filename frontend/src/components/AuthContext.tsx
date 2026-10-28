import { createContext, useEffect, useState, type ReactNode } from "react";
import { decodeJwt } from "../utils/jwt";

interface AuthContextType {
  isLoggedIn: boolean;
  username: string | null;
  logout: () => void;
  setLoginState: (token: string) => void;
}

// eslint-disable-next-line react-refresh/only-export-components -- context object must be exported alongside its provider
export const AuthContext = createContext<AuthContextType>({
  isLoggedIn: false,
  username: null,
  logout: () => {},
  setLoginState: () => {},
});

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [username, setUsername] = useState<string | null>(null);

  useEffect(() => {
    const token = localStorage.getItem("authToken");

    if (token) {
      const decoded = decodeJwt(token);
      setUsername(
        decoded?.unique_name || decoded?.name || decoded?.username || null,
      );
      setIsLoggedIn(true);
    }
  }, []);

  const logout = () => {
    localStorage.removeItem("authToken");
    setIsLoggedIn(false);
    setUsername(null);
    window.location.href = "/login";
  };

  const setLoginState = (token: string) => {
    const decoded = decodeJwt(token);
    setUsername(
      decoded?.unique_name || decoded?.name || decoded?.username || null,
    );
    setIsLoggedIn(true);
  };

  return (
    <AuthContext.Provider
      value={{ isLoggedIn, username, logout, setLoginState }}
    >
      {children}
    </AuthContext.Provider>
  );
};
