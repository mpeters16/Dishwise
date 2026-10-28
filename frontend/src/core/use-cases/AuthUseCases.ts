import type { AuthToken, LoginCredentials, User } from "../../core/entities/User";

export interface IAuthRepository {
  login(credentials: LoginCredentials): Promise<AuthToken>;
  logout(): Promise<void>;
  getCurrentUser(): User | null;
  setCurrentUser(user: User | null): void;
}

export class LoginUseCase {
  authRepository: IAuthRepository;

  constructor(authRepository: IAuthRepository) {
    this.authRepository = authRepository;
  }

  async execute(credentials: LoginCredentials): Promise<AuthToken> {
    return this.authRepository.login(credentials);
  }
}

export class LogoutUseCase {
  authRepository: IAuthRepository;

  constructor(authRepository: IAuthRepository) {
    this.authRepository = authRepository;
  }

  async execute(): Promise<void> {
    return this.authRepository.logout();
  }
}

export class GetCurrentUserUseCase {
  authRepository: IAuthRepository;

  constructor(authRepository: IAuthRepository) {
    this.authRepository = authRepository;
  }

  execute(): User | null {
    return this.authRepository.getCurrentUser();
  }
}
