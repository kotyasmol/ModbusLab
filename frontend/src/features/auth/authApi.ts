import { apiGet, apiPost } from "../../shared/api/apiClient";
import type { AuthResponse, AuthUser, LoginRequest, RegisterRequest } from "./types";

export function login(request: LoginRequest): Promise<AuthResponse> {
  return apiPost<LoginRequest, AuthResponse>("/api/auth/login", request);
}

export function register(request: RegisterRequest): Promise<AuthResponse> {
  return apiPost<RegisterRequest, AuthResponse>("/api/auth/register", request);
}

export function getMe(): Promise<AuthUser> {
  return apiGet<AuthUser>("/api/auth/me");
}
