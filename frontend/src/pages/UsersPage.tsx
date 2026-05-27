import { type FormEvent, useState } from "react";
import { useUsers } from "../features/users/useUsers";

type UsersPageProps = {
  formatTimestamp: (value: string) => string;
};

function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message;
  }

  if (typeof error === "object" && error !== null && "message" in error) {
    const message = (error as { message?: unknown }).message;
    if (typeof message === "string") {
      return message;
    }
  }

  return "Request failed.";
}

export function UsersPage({ formatTimestamp }: UsersPageProps) {
  const {
    usersQuery,
    rolesQuery,
    createUser,
    creatingUser,
    changeUserRole,
    changingRole,
    changeUserStatus,
    changingStatus,
  } = useUsers();

  const roles = rolesQuery.data ?? ["Viewer", "Engineer", "Admin"];
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [role, setRole] = useState("Viewer");
  const [formError, setFormError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const handleCreateUser = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setFormError(null);

    try {
      await createUser({
        userName,
        email: email.trim() || null,
        password,
        role,
      });

      setUserName("");
      setEmail("");
      setPassword("");
      setRole("Viewer");
    } catch (error) {
      setFormError(getErrorMessage(error));
    }
  };

  const handleRoleChange = async (userId: string, nextRole: string) => {
    setActionError(null);

    try {
      await changeUserRole(userId, nextRole);
    } catch (error) {
      setActionError(getErrorMessage(error));
    }
  };

  const handleStatusChange = async (userId: string, isEnabled: boolean) => {
    setActionError(null);

    try {
      await changeUserStatus(userId, isEnabled);
    } catch (error) {
      setActionError(getErrorMessage(error));
    }
  };

  return (
    <section className="users-layout">
      <div className="panel">
        <div className="panel-header">
          <h2>Users</h2>
          <span className="panel-counter">{usersQuery.data?.length ?? 0}</span>
        </div>

        {usersQuery.isLoading && <p className="muted">Loading users...</p>}
        {usersQuery.isError && (
          <div className="form-error">{getErrorMessage(usersQuery.error)}</div>
        )}
        {actionError && <div className="form-error">{actionError}</div>}

        <div className="table-wrapper">
          <table>
            <thead>
              <tr>
                <th>User name</th>
                <th>Email</th>
                <th>Role</th>
                <th>Status</th>
                <th>Created</th>
              </tr>
            </thead>
            <tbody>
              {(usersQuery.data ?? []).map((user) => (
                <tr key={user.id} data-testid={`user-row-${user.userName}`}>
                  <td>{user.userName}</td>
                  <td>{user.email ?? "-"}</td>
                  <td>
                    <select
                      data-testid={`user-role-${user.userName}`}
                      value={user.role}
                      disabled={changingRole}
                      onChange={(event) => void handleRoleChange(user.id, event.target.value)}
                    >
                      {roles.map((availableRole) => (
                        <option key={availableRole} value={availableRole}>
                          {availableRole}
                        </option>
                      ))}
                    </select>
                  </td>
                  <td>
                    <label className="inline-toggle">
                      <input
                        data-testid={`user-enabled-${user.userName}`}
                        type="checkbox"
                        checked={user.isEnabled}
                        disabled={changingStatus}
                        onChange={(event) =>
                          void handleStatusChange(user.id, event.target.checked)
                        }
                      />
                      <span className={user.isEnabled ? "log-status success" : "log-status failed"}>
                        {user.isEnabled ? "Enabled" : "Disabled"}
                      </span>
                    </label>
                  </td>
                  <td>{formatTimestamp(user.createdAtUtc)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <aside className="panel">
        <div className="panel-header">
          <h2>Create user</h2>
        </div>

        <form className="auth-form" data-testid="create-user-form" onSubmit={handleCreateUser}>
          <label>
            User name
            <input
              data-testid="create-user-name"
              autoComplete="off"
              value={userName}
              onChange={(event) => setUserName(event.target.value)}
            />
          </label>
          <label>
            Email
            <input
              data-testid="create-user-email"
              autoComplete="off"
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
            />
          </label>
          <label>
            Password
            <input
              data-testid="create-user-password"
              autoComplete="new-password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
            />
          </label>
          <label>
            Role
            <select
              data-testid="create-user-role"
              value={role}
              onChange={(event) => setRole(event.target.value)}
            >
              {roles.map((availableRole) => (
                <option key={availableRole} value={availableRole}>
                  {availableRole}
                </option>
              ))}
            </select>
          </label>

          {formError && <div className="form-error">{formError}</div>}

          <button className="primary-button full-width" disabled={creatingUser}>
            {creatingUser ? "Creating..." : "Create user"}
          </button>
        </form>

        <div className="permission-grid">
          <h3>Role access</h3>
          <div className="permission-row">
            <span className="badge">Viewer</span>
            <span className="muted">Read dashboards, devices, registers, and test history.</span>
          </div>
          <div className="permission-row">
            <span className="badge">Engineer</span>
            <span className="muted">Viewer access plus register writes and test execution.</span>
          </div>
          <div className="permission-row">
            <span className="badge">Admin</span>
            <span className="muted">Full access, user management, audit logs, and profile setup.</span>
          </div>
        </div>
      </aside>
    </section>
  );
}
