export interface User {
  id: string;
  username: string;
  email: string;
  displayName: string;
  role: string;
}

export interface AuthToken {
  token: string;
}

export interface LoginCredentials {
  username: string;
  password: string;
}

export interface RegisterCredentials extends LoginCredentials {
  email: string;
  displayName: string;
}

export interface ResetPasswordCredentials {
  username: string;
  newPassword: string;
}
