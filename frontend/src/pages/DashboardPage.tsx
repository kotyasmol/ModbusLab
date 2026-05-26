import { useMemo } from "react";
import type { UseQueryResult } from "@tanstack/react-query";
import type { DeviceDto } from "../features/devices/types";
import type { ModbusLogDto } from "../features/devices/modbusTypes";
import type { TestProfileDto, TestRunDto } from "../features/testing/types";
import { MiniLineChart } from "../shared/components/MiniCharts";

type Props = {
  devicesQuery: UseQueryResult<DeviceDto[], Error>;
  logsQuery: UseQueryResult<ModbusLogDto[], Error>;
  testProfilesQuery: UseQueryResult<TestProfileDto[], Error>;
  testRunsQuery: UseQueryResult<TestRunDto[], Error>;
  formatTimestamp: (v: string) => string;
  getStatusClass: (status: number | string) => string;
};

export function DashboardPage({ devicesQuery, logsQuery, testProfilesQuery, testRunsQuery, formatTimestamp, getStatusClass }: Props) {
  const runs = useMemo(() => testRunsQuery.data ?? [], [testRunsQuery.data]);
  const passRate = runs.length ? Math.round((runs.filter((r) => r.status === "Passed").length / runs.length) * 100) : 0;
  const avgDuration = useMemo(() => {
    const durations = runs.map((r) => r.finishedAtUtc ? new Date(r.finishedAtUtc).getTime() - new Date(r.startedAtUtc).getTime() : 0).filter((d) => d > 0);
    return durations.length ? Math.round(durations.reduce((a, b) => a + b, 0) / durations.length / 1000) : 0;
  }, [runs]);
  const activeTests = runs.filter((r) => r.finishedAtUtc === null).length;

  const logPoints = (logsQuery.data ?? []).slice(-20).map((l, i) => ({ label: String(i + 1), value: l.value ?? 0 }));
  const qualityPoints = runs.slice(-20).map((r, i) => ({ label: String(i + 1), value: r.status === "Passed" ? 100 : 0 }));

  return <>
    <section className="kpi-grid">
      <article className="panel kpi-card"><h3>Test runs</h3><strong>{runs.length}</strong></article>
      <article className="panel kpi-card"><h3>PASS rate</h3><strong>{passRate}%</strong></article>
      <article className="panel kpi-card"><h3>Avg test time</h3><strong>{avgDuration}s</strong></article>
      <article className="panel kpi-card"><h3>Active tests</h3><strong>{activeTests}</strong></article>
      <article className="panel kpi-card"><h3>Devices</h3><strong>{devicesQuery.data?.length ?? 0}</strong></article>
      <article className="panel kpi-card"><h3>Test profiles</h3><strong>{testProfilesQuery.data?.length ?? 0}</strong></article>
    </section>
    <section className="dashboard-grid">
      <MiniLineChart title="Modbus values" points={logPoints} />
      <MiniLineChart title="Test quality" points={qualityPoints} color="#34d399" />
      <section className="panel"><h3>Latest runs</h3><div className="table-wrapper"><table><tbody>{runs.slice(0, 6).map((run) => <tr key={run.id}><td>{formatTimestamp(run.startedAtUtc)}</td><td>{run.profileName}</td><td><span className={getStatusClass(run.status)}>{run.status}</span></td></tr>)}</tbody></table></div></section>
      <section className="panel"><h3>Latest errors</h3><div className="table-wrapper"><table><tbody>{(logsQuery.data ?? []).filter((l) => l.status !== 0).slice(0, 6).map((log) => <tr key={log.id}><td>{formatTimestamp(log.timestampUtc)}</td><td>R{log.registerAddress}</td><td>{log.message}</td></tr>)}</tbody></table></div></section>
    </section>
  </>;
}
