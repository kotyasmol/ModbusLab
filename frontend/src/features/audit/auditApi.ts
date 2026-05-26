import { apiGet } from "../../shared/api/apiClient";
import type { AuditLogDto } from "./types";

export function getAuditLogs(count = 100): Promise<AuditLogDto[]> {
  return apiGet<AuditLogDto[]>(`/api/audit-logs?count=${count}`);
}
