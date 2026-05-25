import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import { getModbusLogs, readRegister, writeRegister } from "./features/devices/modbusApi";
import type { ModbusLogDto, RegisterOperationResult, RegisterValueChangedEvent } from "./features/devices/modbusTypes";
import type { DeviceDto, RegisterDto } from "./features/devices/types";
import { addTestStep, createTestProfile, getTestProfile, getTestProfiles, getTestRuns, runTestProfile } from "./features/testing/testingApi";
import type { CreateTestStepRequest, TestProfileDto, TestRunDto } from "./features/testing/types";
import { DashboardPage } from "./pages/DashboardPage";
import { MonitoringPage } from "./pages/MonitoringPage";
import { TestingPage } from "./pages/TestingPage";
import { createModbusHubConnection } from "./shared/api/modbusHubConnection";
import { Shell } from "./shared/components/Shell";
import "./App.css";

type ActiveSection = "dashboard" | "monitoring" | "testing";
const formatTimestamp = (t: string) => new Date(t).toLocaleString("ru-RU");
const getStatusClass = (status: number | string) => status === 0 || status === "Passed" || status === "Success" ? "log-status success" : status === 1 || status === "Failed" ? "log-status failed" : "log-status rejected";
const toNullableNumber = (v: string) => (v.trim() ? Number(v) : null);

export default function App() {
  const queryClient = useQueryClient();
  const [activeSection, setActiveSection] = useState<ActiveSection>("dashboard");
  const [realtimeStatus, setRealtimeStatus] = useState("Connecting");
  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);
  const [readRegisterAddress, setReadRegisterAddress] = useState("1305");
  const [writeRegisterAddress, setWriteRegisterAddress] = useState("1300");
  const [writeRegisterValue, setWriteRegisterValue] = useState("1");
  const [operationResult, setOperationResult] = useState<RegisterOperationResult | null>(null);
  const [selectedProfileId, setSelectedProfileId] = useState<string | null>(null);
  const [newProfileName, setNewProfileName] = useState("Новый тестовый профиль");
  const [newProfileDescription, setNewProfileDescription] = useState("");
  const [stepType, setStepType] = useState("CheckRegisterRange");
  const [stepName, setStepName] = useState("Проверить регистр");
  const [stepSlaveAddress, setStepSlaveAddress] = useState("1"); const [stepRegisterAddress, setStepRegisterAddress] = useState("1305");
  const [stepValue, setStepValue] = useState("1"); const [stepMinValue, setStepMinValue] = useState("11700"); const [stepMaxValue, setStepMaxValue] = useState("12300"); const [stepDelayMs, setStepDelayMs] = useState("1000");
  const [lastRun, setLastRun] = useState<TestRunDto | null>(null);

  const devicesQuery = useQuery({ queryKey: ["devices"], queryFn: getDevices });
  const registersQuery = useQuery({ queryKey: ["device-registers", selectedDevice?.id], queryFn: () => getDeviceRegisters(selectedDevice!.id), enabled: selectedDevice !== null });
  const modbusLogsQuery = useQuery({ queryKey: ["modbus-logs"], queryFn: getModbusLogs, refetchInterval: 5000 });
  const testProfilesQuery = useQuery({ queryKey: ["test-profiles"], queryFn: getTestProfiles });
  const selectedProfileQuery = useQuery({ queryKey: ["test-profile", selectedProfileId], queryFn: () => getTestProfile(selectedProfileId!), enabled: selectedProfileId !== null });
  const testRunsQuery = useQuery({ queryKey: ["test-runs"], queryFn: getTestRuns, refetchInterval: 7000 });

  useEffect(() => { const c = createModbusHubConnection(); c.on("RegisterValueChanged", (event: RegisterValueChangedEvent) => { queryClient.setQueryData<RegisterDto[]>(["device-registers", event.deviceId], (current) => current?.map((r) => r.definitionId === event.registerDefinitionId ? { ...r, currentValue: event.value, updatedAtUtc: event.updatedAtUtc } : r)); }); c.onreconnecting(() => setRealtimeStatus("Reconnecting")); c.onreconnected(() => setRealtimeStatus("Connected")); c.onclose(() => setRealtimeStatus("Disconnected")); c.start().then(() => setRealtimeStatus("Connected")).catch(() => setRealtimeStatus("Disconnected")); return () => { void c.stop(); }; }, [queryClient]);

  const readRegisterMutation = useMutation({ mutationFn: readRegister, onSuccess: (r) => { setOperationResult(r); void queryClient.invalidateQueries({ queryKey: ["modbus-logs"] }); } });
  const writeRegisterMutation = useMutation({ mutationFn: writeRegister, onSuccess: (r) => { setOperationResult(r); void queryClient.invalidateQueries({ queryKey: ["modbus-logs"] }); } });
  const createProfileMutation = useMutation({ mutationFn: createTestProfile, onSuccess: (p) => { setSelectedProfileId(p.id); void queryClient.invalidateQueries({ queryKey: ["test-profiles"] }); } });
  const addStepMutation = useMutation({ mutationFn: (request: CreateTestStepRequest) => addTestStep(selectedProfileId!, request), onSuccess: () => void queryClient.invalidateQueries({ queryKey: ["test-profile", selectedProfileId] }) });
  const runProfileMutation = useMutation({ mutationFn: runTestProfile, onSuccess: (run) => { setLastRun(run); void queryClient.invalidateQueries({ queryKey: ["test-runs"] }); } });

  const renderLogRows = (logs: ModbusLogDto[]) => logs.map((log) => <tr key={log.id}><td>{formatTimestamp(log.timestampUtc)}</td><td>{log.slaveAddress}</td><td>{log.functionCode}</td><td>{log.registerAddress}</td><td>{log.value ?? "—"}</td><td><span className={getStatusClass(log.status)}>{log.status}</span></td><td>{log.message}</td></tr>);

  return <Shell activeSection={activeSection} onSectionChange={setActiveSection} realtimeStatus={realtimeStatus}>{activeSection === "dashboard" && <DashboardPage devicesQuery={devicesQuery} logsQuery={modbusLogsQuery} testProfilesQuery={testProfilesQuery} testRunsQuery={testRunsQuery} formatTimestamp={formatTimestamp} getStatusClass={getStatusClass} />}{activeSection === "monitoring" && <MonitoringPage devicesQuery={devicesQuery} registersQuery={registersQuery} modbusLogsQuery={modbusLogsQuery} selectedDevice={selectedDevice} setSelectedDevice={setSelectedDevice} operationResult={operationResult} readRegisterAddress={readRegisterAddress} setReadRegisterAddress={setReadRegisterAddress} writeRegisterAddress={writeRegisterAddress} setWriteRegisterAddress={setWriteRegisterAddress} writeRegisterValue={writeRegisterValue} setWriteRegisterValue={setWriteRegisterValue} handleReadRegister={() => selectedDevice && readRegisterMutation.mutate({ slaveAddress: selectedDevice.slaveAddress, registerAddress: Number(readRegisterAddress) })} handleWriteRegister={() => selectedDevice && writeRegisterMutation.mutate({ slaveAddress: selectedDevice.slaveAddress, registerAddress: Number(writeRegisterAddress), value: Number(writeRegisterValue) })} readRegisterPending={readRegisterMutation.isPending} writeRegisterPending={writeRegisterMutation.isPending} renderLogRows={renderLogRows} />}{activeSection === "testing" && <TestingPage testProfilesQuery={testProfilesQuery} selectedProfile={selectedProfileQuery.data} selectedProfileId={selectedProfileId} setSelectedProfileId={setSelectedProfileId} newProfileName={newProfileName} setNewProfileName={setNewProfileName} newProfileDescription={newProfileDescription} setNewProfileDescription={setNewProfileDescription} handleCreateProfile={() => createProfileMutation.mutate({ name: newProfileName, description: newProfileDescription || null })} creatingProfile={createProfileMutation.isPending} stepType={stepType} setStepType={setStepType} stepName={stepName} setStepName={setStepName} stepSlaveAddress={stepSlaveAddress} setStepSlaveAddress={setStepSlaveAddress} stepRegisterAddress={stepRegisterAddress} setStepRegisterAddress={setStepRegisterAddress} stepValue={stepValue} setStepValue={setStepValue} stepMinValue={stepMinValue} setStepMinValue={setStepMinValue} stepMaxValue={stepMaxValue} setStepMaxValue={setStepMaxValue} stepDelayMs={stepDelayMs} setStepDelayMs={setStepDelayMs} handleAddStep={() => selectedProfileId && addStepMutation.mutate({ type: stepType, name: stepName, slaveAddress: stepType === "Delay" ? null : toNullableNumber(stepSlaveAddress), registerAddress: stepType === "Delay" ? null : toNullableNumber(stepRegisterAddress), value: stepType === "WriteRegister" ? toNullableNumber(stepValue) : null, minValue: stepType === "CheckRegisterRange" ? toNullableNumber(stepMinValue) : null, maxValue: stepType === "CheckRegisterRange" ? toNullableNumber(stepMaxValue) : null, delayMs: stepType === "Delay" ? toNullableNumber(stepDelayMs) : null })} addingStep={addStepMutation.isPending} runProfile={(id) => runProfileMutation.mutate(id)} runPending={runProfileMutation.isPending} lastRun={lastRun} testRunsQuery={testRunsQuery} getStatusClass={getStatusClass} formatTimestamp={formatTimestamp} />}</Shell>;
}
