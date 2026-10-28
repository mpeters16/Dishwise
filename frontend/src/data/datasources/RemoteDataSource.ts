import type { AuthToken, LoginCredentials } from "../../core/entities/User";
import apiClient from "../../api/apiClient";

export interface IRemoteDataSource {
  login(credentials: LoginCredentials): Promise<AuthToken>;
}

export class RemoteDataSource implements IRemoteDataSource {
  async login(credentials: LoginCredentials): Promise<AuthToken> {
    const response = await apiClient.post("/login", credentials);
    return { token: response.data.token };
  }
}

export const remoteDataSource = new RemoteDataSource();
