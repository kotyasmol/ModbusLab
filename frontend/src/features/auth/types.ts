export type AuthUser = {
  id: string;
  userName: string;
  email: string | null;
  role: string;
};

export type AuthResponse = {
  accessToken: string;
  user: AuthUser;
};

export type LoginRequest = {
  userName: string;
  password: string;
};

export type RegisterRequest = {
  userName: string;
  email: string | null;
  password: string;
};
