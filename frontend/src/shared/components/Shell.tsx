import type { ReactNode } from "react";

type Section = "dashboard" | "monitoring" | "testing";

type ShellProps = {
  activeSection: Section;
  onSectionChange: (section: Section) => void;
  realtimeStatus: string;
  children: ReactNode;
};

const sectionTitles: Record<Section, string> = {
  dashboard: "Dashboard",
  monitoring: "Monitoring",
  testing: "Testing",
};

export function Shell({ activeSection, onSectionChange, realtimeStatus, children }: ShellProps) {
  return (
    <main className="shell-root">
      <aside className="shell-sidebar panel">
        <div>
          <p className="shell-eyebrow">Industrial monitoring</p>
          <h1>ModbusLab</h1>
        </div>
        <nav className="shell-nav">
          {(["dashboard", "monitoring", "testing"] as Section[]).map((section) => (
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
          <strong>Admin</strong>
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
