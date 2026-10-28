import { useState } from "react";
import type { LoginCredentials } from "../../core/entities/User";
import { LoginUseCase } from "../../core/use-cases/AuthUseCases";
import { AuthRepository } from "../../data/repositories/AuthRepository";
import { remoteDataSource } from "../../data/datasources/RemoteDataSource";
import { localDataSource } from "../../data/datasources/LocalDataSource";

const authRepository = new AuthRepository(remoteDataSource, localDataSource);
const loginUseCase = new LoginUseCase(authRepository);

export const useLogin = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const login = async (credentials: LoginCredentials) => {
    setIsLoading(true);
    setError(null);
    try {
      return await loginUseCase.execute(credentials);
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Login failed";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  };

  return { login, isLoading, error };
};
