export type AuditLogDto = {
  id: string;
  timestampUtc: string;
  userId: string | null;
  userName: string | null;
  userRole: string | null;
  action: string;
  entityType: string | null;
  entityId: string | null;
  details: string | null;
  ipAddress: string | null;
  isSuccess: boolean;
};
