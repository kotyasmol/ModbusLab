import { apiGet, apiPatch, apiPost } from "../../shared/api/apiClient";
import type {
  AppUserDto,
  ChangeUserRoleRequest,
  ChangeUserStatusRequest,
  CreateUserRequest,
} from "./types";

export function getUsers(): Promise<AppUserDto[]> {
  return apiGet<AppUserDto[]>("/api/users/");
}

export function getRoles(): Promise<string[]> {
  return apiGet<string[]>("/api/users/roles");
}

export function createUser(request: CreateUserRequest): Promise<AppUserDto> {
  return apiPost<CreateUserRequest, AppUserDto>("/api/users/", request);
}

export function changeUserRole(
  userId: string,
  request: ChangeUserRoleRequest
): Promise<AppUserDto> {
  return apiPatch<ChangeUserRoleRequest, AppUserDto>(`/api/users/${userId}/role`, request);
}

export function changeUserStatus(
  userId: string,
  request: ChangeUserStatusRequest
): Promise<AppUserDto> {
  return apiPatch<ChangeUserStatusRequest, AppUserDto>(`/api/users/${userId}/status`, request);
}
