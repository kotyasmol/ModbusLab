import { useQuery } from "@tanstack/react-query";
import { getAuditLogs } from "../features/audit/auditApi";

type AuditLogsPageProps = {
  formatTimestamp: (value: string) => string;
};

export function AuditLogsPage({ formatTimestamp }: AuditLogsPageProps) {
  const auditLogsQuery = useQuery({
    queryKey: ["audit-logs"],
    queryFn: () => getAuditLogs(150),
    refetchInterval: 10000,
  });

  return (
    <section className="panel">
      <div className="panel-header">
        <h2>Audit logs</h2>
        <span className="panel-counter">{auditLogsQuery.data?.length ?? 0}</span>
      </div>

      {auditLogsQuery.isError && (
        <div className="form-error">Не удалось загрузить audit log.</div>
      )}

      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Time</th>
              <th>User</th>
              <th>Role</th>
              <th>Action</th>
              <th>Entity</th>
              <th>Success</th>
              <th>Details</th>
            </tr>
          </thead>
          <tbody>
            {(auditLogsQuery.data ?? []).map((log) => (
              <tr key={log.id}>
                <td>{formatTimestamp(log.timestampUtc)}</td>
                <td>{log.userName ?? "-"}</td>
                <td>{log.userRole ?? "-"}</td>
                <td>{log.action}</td>
                <td>{log.entityType ?? "-"}</td>
                <td>
                  <span className={log.isSuccess ? "log-status success" : "log-status failed"}>
                    {log.isSuccess ? "Yes" : "No"}
                  </span>
                </td>
                <td>{log.details ?? "-"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </section>
  );
}
