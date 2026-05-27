import { useEffect, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useMonitoring } from "./features/devices/useMonitoring";
import type { RegisterValueChangedEvent } from "./features/devices/modbusTypes";
import type { RegisterDto } from "./features/devices/types";
import { useTesting } from "./features/testing/useTesting";
import { AuditLogsPage } from "./pages/AuditLogsPage";
import { DashboardPage } from "./pages/DashboardPage";
import { LoginPage } from "./pages/LoginPage";
import { MonitoringPage } from "./pages/MonitoringPage";
import { RegisterPage } from "./pages/RegisterPage";
import { TestingPage } from "./pages/TestingPage";
import { UsersPage } from "./pages/UsersPage";
import { createModbusHubConnection } from "./shared/api/modbusHubConnection";
import { useAuth } from "./shared/auth/useAuth";
import {
  canManageTestProfiles,
  canManageUsers,
  canRunTests,
  canViewAuditLogs,
  canWriteRegister,
} from "./shared/auth/permissions";
import { Shell } from "./shared/components/Shell";
import "./App.css";

type ActiveSection = "dashboard" | "monitoring" | "testing" | "users" | "audit";
type AuthSection = "login" | "register";

function formatTimestamp(timestampUtc: string): string {
  return new Date(timestampUtc).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}

function getStatusClass(status: number | string): string {
  if (status === 0 || status === "Passed" || status === "Success") {
    return "log-status success";
  }

  if (status === 1 || status === "Failed") {
    return "log-status failed";
  }

  if (status === 2 || status === "Rejected") {
    return "log-status rejected";
  }

  return "log-status";
}

export default function App() {
  const { user, isLoading } = useAuth();
  const [authSection, setAuthSection] = useState<AuthSection>("login");

  if (isLoading) {
    return (
      <main className="auth-page">
        <section className="auth-card">
          <p className="shell-eyebrow">ModbusLab</p>
          <h1>Загрузка...</h1>
        </section>
      </main>
    );
  }

  if (!user) {
    return authSection === "login" ? (
      <LoginPage onRegisterClick={() => setAuthSection("register")} />
    ) : (
      <RegisterPage onLoginClick={() => setAuthSection("login")} />
    );
  }

  return <AuthenticatedApp />;
}

function AuthenticatedApp() {
  const queryClient = useQueryClient();
  const { accessToken, logout, user } = useAuth();
  const [activeSection, setActiveSection] = useState<ActiveSection>("dashboard");
  const [realtimeStatus, setRealtimeStatus] = useState("Connecting");

  const monitoring = useMonitoring({ formatTimestamp, getStatusClass });
  const testing = useTesting();

  const handleSectionChange = (section: ActiveSection) => {
    if (
      (section === "audit" && !canViewAuditLogs(user)) ||
      (section === "users" && !canManageUsers(user))
    ) {
      setActiveSection("dashboard");
      return;
    }

    setActiveSection(section);
  };

  useEffect(() => {
    const connection = createModbusHubConnection(accessToken);

    connection.on("RegisterValueChanged", (event: RegisterValueChangedEvent) => {
      queryClient.setQueryData<RegisterDto[]>(
        ["device-registers", event.deviceId],
        (currentRegisters) =>
          currentRegisters?.map((register) =>
            register.definitionId === event.registerDefinitionId
              ? {
                  ...register,
                  currentValue: event.value,
                  updatedAtUtc: event.updatedAtUtc,
                }
              : register
          )
      );
    });

    connection.onreconnecting(() => setRealtimeStatus("Reconnecting"));
    connection.onreconnected(() => setRealtimeStatus("Connected"));
    connection.onclose(() => setRealtimeStatus("Disconnected"));

    connection
      .start()
      .then(() => setRealtimeStatus("Connected"))
      .catch(() => setRealtimeStatus("Disconnected"));

    return () => {
      void connection.stop();
    };
  }, [accessToken, queryClient]);

  return (
    <Shell
      activeSection={activeSection}
      onSectionChange={handleSectionChange}
      realtimeStatus={realtimeStatus}
      user={user}
      onLogout={logout}
    >
      {activeSection === "dashboard" && (
        <DashboardPage
          devicesQuery={monitoring.devicesQuery}
          logsQuery={monitoring.modbusLogsQuery}
          testProfilesQuery={testing.testProfilesQuery}
          testRunsQuery={testing.testRunsQuery}
          formatTimestamp={formatTimestamp}
          getStatusClass={getStatusClass}
        />
      )}

      {activeSection === "monitoring" && (
        <MonitoringPage
          {...monitoring}
          canWriteRegister={canWriteRegister(user)}
          unavailableReason={`Недоступно для роли ${user?.role ?? "Viewer"}`}
        />
      )}

      {activeSection === "testing" && (
        <TestingPage
          {...testing}
          canManageProfiles={canManageTestProfiles(user)}
          canRunTests={canRunTests(user)}
          unavailableReason={`Недоступно для роли ${user?.role ?? "Viewer"}`}
          getStatusClass={getStatusClass}
          formatTimestamp={formatTimestamp}
        />
      )}

      {activeSection === "audit" && canViewAuditLogs(user) && (
        <AuditLogsPage formatTimestamp={formatTimestamp} />
      )}

      {activeSection === "users" && canManageUsers(user) && (
        <UsersPage formatTimestamp={formatTimestamp} />
      )}
    </Shell>
  );
}
