import { apiGet } from "../../shared/api/apiClient";
import type { AuditLogDto } from "./types";

export type AuditLogFilters = {
  count?: number;
  action?: string;
  userName?: string;
  isSuccess?: string;
};

export function getAuditLogs(filters: AuditLogFilters = {}): Promise<AuditLogDto[]> {
  const params = new URLSearchParams();
  params.set("count", String(filters.count ?? 100));

  if (filters.action?.trim()) {
    params.set("action", filters.action.trim());
  }

  if (filters.userName?.trim()) {
    params.set("userName", filters.userName.trim());
  }

  if (filters.isSuccess === "true" || filters.isSuccess === "false") {
    params.set("isSuccess", filters.isSuccess);
  }

  return apiGet<AuditLogDto[]>(`/api/audit-logs?${params.toString()}`);
}
