import type { User } from "../../core/entities/User";

const STORAGE_KEYS = {
  AUTH_TOKEN: "authToken",
  CURRENT_USER: "currentUser",
};

export interface ILocalDataSource {
  saveToken(token: string): void;
  getToken(): string | null;
  removeToken(): void;
  saveUser(user: User): void;
  getUser(): User | null;
  removeUser(): void;
}

export class LocalDataSource implements ILocalDataSource {
  saveToken(token: string): void {
    localStorage.setItem(STORAGE_KEYS.AUTH_TOKEN, token);
  }

  getToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.AUTH_TOKEN);
  }

  removeToken(): void {
    localStorage.removeItem(STORAGE_KEYS.AUTH_TOKEN);
  }

  saveUser(user: User): void {
    localStorage.setItem(STORAGE_KEYS.CURRENT_USER, JSON.stringify(user));
  }

  getUser(): User | null {
    const userJson = localStorage.getItem(STORAGE_KEYS.CURRENT_USER);
    if (!userJson) return null;
    try {
      return JSON.parse(userJson);
    } catch {
      return null;
    }
  }

  removeUser(): void {
    localStorage.removeItem(STORAGE_KEYS.CURRENT_USER);
  }
}

export const localDataSource = new LocalDataSource();
