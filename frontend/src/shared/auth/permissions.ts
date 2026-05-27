import type { AuthUser } from "../../features/auth/types";

export function canWriteRegister(user: AuthUser | null): boolean {
  return user?.role === "Engineer" || user?.role === "Admin";
}

export function canRunTests(user: AuthUser | null): boolean {
  return user?.role === "Engineer" || user?.role === "Admin";
}

export function canManageTestProfiles(user: AuthUser | null): boolean {
  return user?.role === "Admin";
}

export function canViewAuditLogs(user: AuthUser | null): boolean {
  return user?.role === "Admin";
}

export function canManageUsers(user: AuthUser | null): boolean {
  return user?.role === "Admin";
}

export function canManageDevices(user: AuthUser | null): boolean {
  return user?.role === "Engineer" || user?.role === "Admin";
}
