import type { ReactNode } from "react";
import type { AuthUser } from "../../features/auth/types";

type Section = "dashboard" | "monitoring" | "testing" | "audit";

type ShellProps = {
  activeSection: Section;
  onSectionChange: (section: Section) => void;
  realtimeStatus: string;
  user: AuthUser | null;
  onLogout: () => void;
  children: ReactNode;
};

const sectionTitles: Record<Section, string> = {
  dashboard: "Dashboard",
  monitoring: "Monitoring",
  testing: "Testing",
  audit: "Audit logs",
};

export function Shell({
  activeSection,
  onSectionChange,
  realtimeStatus,
  user,
  onLogout,
  children,
}: ShellProps) {
  return (
    <main className="shell-root">
      <aside className="shell-sidebar panel">
        <div>
          <p className="shell-eyebrow">Industrial monitoring</p>
          <h1>ModbusLab</h1>
        </div>
        <nav className="shell-nav">
          {(["dashboard", "monitoring", "testing", ...(user?.role === "Admin" ? ["audit" as const] : [])] as Section[]).map((section) => (
            <button
              key={section}
              className={activeSection === section ? "shell-nav-item active" : "shell-nav-item"}
              onClick={() => onSectionChange(section)}
            >
              {sectionTitles[section]}
            </button>
          ))}
        </nav>
        <div className="shell-user panel-soft">
          <span className="muted">User</span>
          <strong>{user?.userName ?? "Unknown"}</strong>
          <span className="muted">{user?.role ?? "User"}</span>
          <button className="secondary-button full-width" onClick={onLogout}>
            Logout
          </button>
        </div>
      </aside>
      <section className="shell-main">
        <header className="shell-topbar panel">
          <div>
            <p className="muted">Current section</p>
            <h2>{sectionTitles[activeSection]}</h2>
          </div>
          <span className="signal-pill">SignalR: {realtimeStatus}</span>
        </header>
        <div className="page-content">{children}</div>
      </section>
    </main>
  );
}
