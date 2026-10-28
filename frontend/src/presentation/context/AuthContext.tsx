import { createContext, useEffect, useState, type ReactNode } from "react";
import type { User } from "../../core/entities/User";
import { decodeJwt } from "../../utils/jwt";
import { localDataSource } from "../../data/datasources/LocalDataSource";

export interface AuthContextType {
  isLoggedIn: boolean;
  username: string | null;
  user: User | null;
  logout: () => void;
  setLoginState: (token: string) => void;
}

// eslint-disable-next-line react-refresh/only-export-components -- context object must be exported alongside its provider
export const AuthContext = createContext<AuthContextType>({
  isLoggedIn: false,
  username: null,
  user: null,
  logout: () => {},
  setLoginState: () => {},
});

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [username, setUsername] = useState<string | null>(null);
  const [user, setUser] = useState<User | null>(null);

  useEffect(() => {
    const token = localDataSource.getToken();
    const storedUser = localDataSource.getUser();

    if (token && storedUser) {
      // Use displayName if available, otherwise username, otherwise id as fallback
      const displayName = storedUser.displayName || storedUser.username;
      setUsername(displayName);
      setUser(storedUser);
      setIsLoggedIn(true);
    }
  }, []);

  const logout = () => {
    localDataSource.removeToken();
    localDataSource.removeUser();
    setIsLoggedIn(false);
    setUsername(null);
    setUser(null);
    window.location.href = "/login";
  };

  const setLoginState = (token: string) => {
    const decoded = decodeJwt(token);
    const userData: User = {
      id: decoded?.sub || "",
      username:
        decoded?.username || decoded?.unique_name || decoded?.name || "",
      email: decoded?.email || "",
      displayName:
        decoded?.displayName || decoded?.unique_name || decoded?.name || "",
      role: decoded?.role || "user",
    };
    // Use displayName if available, otherwise username
    const displayName =
      userData.displayName || userData.username || userData.id;
    setUsername(displayName);
    setUser(userData);
    setIsLoggedIn(true);
    localDataSource.saveToken(token);
    localDataSource.saveUser(userData);
  };

  return (
    <AuthContext.Provider
      value={{ isLoggedIn, username, user, logout, setLoginState }}
    >
      {children}
    </AuthContext.Provider>
  );
};
