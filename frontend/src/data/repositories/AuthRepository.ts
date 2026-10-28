import type { AuthToken, LoginCredentials, User } from "../../core/entities/User";
import type { IAuthRepository } from "../../core/use-cases/AuthUseCases";
import type { IRemoteDataSource } from "../datasources/RemoteDataSource";
import type { ILocalDataSource } from "../datasources/LocalDataSource";
import { decodeJwt } from "../../utils/jwt";

export class AuthRepository implements IAuthRepository {
  remoteDataSource: IRemoteDataSource;
  localDataSource: ILocalDataSource;
  currentUser: User | null;

  constructor(remoteDataSource: IRemoteDataSource, localDataSource: ILocalDataSource) {
    this.remoteDataSource = remoteDataSource;
    this.localDataSource = localDataSource;
    this.currentUser = localDataSource.getUser();
  }

  async login(credentials: LoginCredentials): Promise<AuthToken> {
    const token = await this.remoteDataSource.login(credentials);
    this.localDataSource.saveToken(token.token);

    // Decode JWT to extract user info
    const decoded = decodeJwt(token.token);
    const user: User = {
      id: decoded?.sub || "",
      username: decoded?.unique_name || decoded?.name || "",
      email: decoded?.email || "",
      displayName: decoded?.displayName || "",
      role: decoded?.role || "user",
    };

    this.localDataSource.saveUser(user);
    this.currentUser = user;
    return token;
  }

  async logout(): Promise<void> {
    this.localDataSource.removeToken();
    this.localDataSource.removeUser();
    this.currentUser = null;
  }

  getCurrentUser(): User | null {
    return this.currentUser;
  }

  setCurrentUser(user: User | null): void {
    this.currentUser = user;
    if (user) {
      this.localDataSource.saveUser(user);
    } else {
      this.localDataSource.removeUser();
    }
  }
}
