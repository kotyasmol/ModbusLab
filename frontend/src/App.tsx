import { useEffect, useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import { getModbusLogs, readRegister, writeRegister } from "./features/devices/modbusApi";
import type {
  ModbusLogDto,
  RegisterOperationResult,
  RegisterValueChangedEvent,
} from "./features/devices/modbusTypes";
import type { DeviceDto, RegisterDto } from "./features/devices/types";
import {
  addTestStep,
  createTestProfile,
  getTestProfile,
  getTestProfiles,
  getTestRuns,
  runTestProfile,
} from "./features/testing/testingApi";
import type { CreateTestStepRequest, TestRunDto } from "./features/testing/types";
import { DashboardPage } from "./pages/DashboardPage";
import { MonitoringPage } from "./pages/MonitoringPage";
import { TestingPage } from "./pages/TestingPage";
import { createModbusHubConnection } from "./shared/api/modbusHubConnection";
import { Shell } from "./shared/components/Shell";
import "./App.css";

type ActiveSection = "dashboard" | "monitoring" | "testing";

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

function toOperationResult(error: unknown): RegisterOperationResult {
  if (typeof error === "object" && error !== null && "message" in error) {
    return {
      isSuccess: false,
      status: 1,
      value: null,
      message:
        typeof (error as { message?: unknown }).message === "string"
          ? (error as { message: string }).message
          : "Неизвестная ошибка.",
    };
  }

  if (error instanceof Error) {
    return {
      isSuccess: false,
      status: 1,
      value: null,
      message: error.message,
    };
  }

  return {
    isSuccess: false,
    status: 1,
    value: null,
    message: "Неизвестная ошибка.",
  };
}

function toNullableNumber(value: string): number | null {
  if (value.trim().length === 0) return null;
  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
}

export default function App() {
  const queryClient = useQueryClient();

  const [activeSection, setActiveSection] = useState<ActiveSection>("dashboard");
  const [realtimeStatus, setRealtimeStatus] = useState("Connecting");
  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);

  const [readRegisterAddress, setReadRegisterAddress] = useState("1305");
  const [writeRegisterAddress, setWriteRegisterAddress] = useState("1300");
  const [writeRegisterValue, setWriteRegisterValue] = useState("1");
  const [operationResult, setOperationResult] =
    useState<RegisterOperationResult | null>(null);

  const [selectedProfileId, setSelectedProfileId] = useState<string | null>(null);
  const [newProfileName, setNewProfileName] = useState("Новый тестовый профиль");
  const [newProfileDescription, setNewProfileDescription] = useState("");

  const [stepType, setStepType] = useState("CheckRegisterRange");
  const [stepName, setStepName] = useState("Проверить регистр");
  const [stepSlaveAddress, setStepSlaveAddress] = useState("1");
  const [stepRegisterAddress, setStepRegisterAddress] = useState("1305");
  const [stepValue, setStepValue] = useState("1");
  const [stepMinValue, setStepMinValue] = useState("11700");
  const [stepMaxValue, setStepMaxValue] = useState("12300");
  const [stepDelayMs, setStepDelayMs] = useState("1000");
  const [lastRun, setLastRun] = useState<TestRunDto | null>(null);

  const devicesQuery = useQuery({ queryKey: ["devices"], queryFn: getDevices });
  const registersQuery = useQuery({
    queryKey: ["device-registers", selectedDevice?.id],
    queryFn: () => getDeviceRegisters(selectedDevice!.id),
    enabled: selectedDevice !== null,
  });
  const modbusLogsQuery = useQuery({
    queryKey: ["modbus-logs"],
    queryFn: getModbusLogs,
    refetchInterval: 5000,
  });
  const testProfilesQuery = useQuery({
    queryKey: ["test-profiles"],
    queryFn: getTestProfiles,
  });
  const selectedProfileQuery = useQuery({
    queryKey: ["test-profile", selectedProfileId],
    queryFn: () => getTestProfile(selectedProfileId!),
    enabled: selectedProfileId !== null,
  });
  const testRunsQuery = useQuery({
    queryKey: ["test-runs"],
    queryFn: getTestRuns,
    refetchInterval: 7000,
  });

  useEffect(() => {
    const connection = createModbusHubConnection();

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
  }, [queryClient]);

  const readRegisterMutation = useMutation({
    mutationFn: readRegister,
    onSuccess: async (result) => {
      setOperationResult(result);
      await queryClient.invalidateQueries({ queryKey: ["modbus-logs"] });
    },
    onError: (error) => setOperationResult(toOperationResult(error)),
  });

  const writeRegisterMutation = useMutation({
    mutationFn: writeRegister,
    onSuccess: async (result) => {
      setOperationResult(result);
      await queryClient.invalidateQueries({ queryKey: ["modbus-logs"] });
    },
    onError: (error) => setOperationResult(toOperationResult(error)),
  });

  const createProfileMutation = useMutation({
    mutationFn: createTestProfile,
    onSuccess: async (profile) => {
      setSelectedProfileId(profile.id);
      await queryClient.invalidateQueries({ queryKey: ["test-profiles"] });
    },
  });

  const addStepMutation = useMutation({
    mutationFn: (request: CreateTestStepRequest) => addTestStep(selectedProfileId!, request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["test-profiles"] }),
        queryClient.invalidateQueries({ queryKey: ["test-profile", selectedProfileId] }),
      ]);
    },
  });

  const runProfileMutation = useMutation({
    mutationFn: runTestProfile,
    onSuccess: async (run) => {
      setLastRun(run);
      await queryClient.invalidateQueries({ queryKey: ["test-runs"] });
    },
  });

  const handleReadRegister = () => {
    if (!selectedDevice) return;
    readRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: Number(readRegisterAddress),
    });
  };

  const handleWriteRegister = () => {
    if (!selectedDevice) return;
    writeRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: Number(writeRegisterAddress),
      value: Number(writeRegisterValue),
    });
  };

  const handleAddStep = () => {
    if (!selectedProfileId) return;

    addStepMutation.mutate({
      type: stepType,
      name: stepName,
      slaveAddress: stepType === "Delay" ? null : toNullableNumber(stepSlaveAddress),
      registerAddress:
        stepType === "Delay" ? null : toNullableNumber(stepRegisterAddress),
      value: stepType === "WriteRegister" ? toNullableNumber(stepValue) : null,
      minValue:
        stepType === "CheckRegisterRange" ? toNullableNumber(stepMinValue) : null,
      maxValue:
        stepType === "CheckRegisterRange" ? toNullableNumber(stepMaxValue) : null,
      delayMs: stepType === "Delay" ? toNullableNumber(stepDelayMs) : null,
    });
  };

  const renderLogRows = useMemo(
    () =>
      (logs: ModbusLogDto[]) =>
        logs.map((log) => (
          <tr key={log.id}>
            <td>{formatTimestamp(log.timestampUtc)}</td>
            <td>{log.slaveAddress}</td>
            <td>{log.functionCode}</td>
            <td>{log.registerAddress}</td>
            <td>{log.value ?? "—"}</td>
            <td>
              <span className={getStatusClass(log.status)}>{log.status}</span>
            </td>
            <td>{log.message}</td>
          </tr>
        )),
    []
  );

  return (
    <Shell
      activeSection={activeSection}
      onSectionChange={setActiveSection}
      realtimeStatus={realtimeStatus}
    >
      {activeSection === "dashboard" && (
        <DashboardPage
          devicesQuery={devicesQuery}
          logsQuery={modbusLogsQuery}
          testProfilesQuery={testProfilesQuery}
          testRunsQuery={testRunsQuery}
          formatTimestamp={formatTimestamp}
          getStatusClass={getStatusClass}
        />
      )}

      {activeSection === "monitoring" && (
        <MonitoringPage
          devicesQuery={devicesQuery}
          registersQuery={registersQuery}
          modbusLogsQuery={modbusLogsQuery}
          selectedDevice={selectedDevice}
          setSelectedDevice={setSelectedDevice}
          operationResult={operationResult}
          readRegisterAddress={readRegisterAddress}
          setReadRegisterAddress={setReadRegisterAddress}
          writeRegisterAddress={writeRegisterAddress}
          setWriteRegisterAddress={setWriteRegisterAddress}
          writeRegisterValue={writeRegisterValue}
          setWriteRegisterValue={setWriteRegisterValue}
          handleReadRegister={handleReadRegister}
          handleWriteRegister={handleWriteRegister}
          readRegisterPending={readRegisterMutation.isPending}
          writeRegisterPending={writeRegisterMutation.isPending}
          renderLogRows={renderLogRows}
        />
      )}

      {activeSection === "testing" && (
        <TestingPage
          testProfilesQuery={testProfilesQuery}
          selectedProfile={selectedProfileQuery.data}
          selectedProfileId={selectedProfileId}
          setSelectedProfileId={setSelectedProfileId}
          newProfileName={newProfileName}
          setNewProfileName={setNewProfileName}
          newProfileDescription={newProfileDescription}
          setNewProfileDescription={setNewProfileDescription}
          handleCreateProfile={() =>
            createProfileMutation.mutate({
              name: newProfileName,
              description: newProfileDescription.trim() || null,
            })
          }
          creatingProfile={createProfileMutation.isPending}
          stepType={stepType}
          setStepType={setStepType}
          stepName={stepName}
          setStepName={setStepName}
          stepSlaveAddress={stepSlaveAddress}
          setStepSlaveAddress={setStepSlaveAddress}
          stepRegisterAddress={stepRegisterAddress}
          setStepRegisterAddress={setStepRegisterAddress}
          stepValue={stepValue}
          setStepValue={setStepValue}
          stepMinValue={stepMinValue}
          setStepMinValue={setStepMinValue}
          stepMaxValue={stepMaxValue}
          setStepMaxValue={setStepMaxValue}
          stepDelayMs={stepDelayMs}
          setStepDelayMs={setStepDelayMs}
          handleAddStep={handleAddStep}
          addingStep={addStepMutation.isPending}
          runProfile={(id) => runProfileMutation.mutate(id)}
          runPending={runProfileMutation.isPending}
          lastRun={lastRun}
          testRunsQuery={testRunsQuery}
          getStatusClass={getStatusClass}
          formatTimestamp={formatTimestamp}
        />
      )}
    </Shell>
  );
}
