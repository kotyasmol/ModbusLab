import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import {
  getModbusLogs,
  readRegister,
  writeRegister,
} from "./features/devices/modbusApi";
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
import type {
  CreateTestStepRequest,
  TestProfileDto,
  TestRunDto,
} from "./features/testing/types";
import { createModbusHubConnection } from "./shared/api/modbusHubConnection";
import "./App.css";

type ActiveTab = "monitoring" | "testing";

function toOperationResult(error: unknown): RegisterOperationResult {
  if (
    typeof error === "object" &&
    error !== null &&
    "isSuccess" in error &&
    "message" in error
  ) {
    return error as RegisterOperationResult;
  }

  if (
    typeof error === "object" &&
    error !== null &&
    "message" in error &&
    typeof (error as { message?: unknown }).message === "string"
  ) {
    return {
      isSuccess: false,
      status: 1,
      value: null,
      message: (error as { message: string }).message,
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
    message: "Неизвестная ошибка при выполнении операции.",
  };
}

function getFunctionCodeLabel(functionCode: number): string {
  if (functionCode === 3) return "FC03 Read";
  if (functionCode === 6) return "FC06 Write";
  return `FC${functionCode}`;
}

function getStatusLabel(status: number): string {
  if (status === 0) return "Success";
  if (status === 1) return "Failed";
  if (status === 2) return "Rejected";
  return "Unknown";
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

function toNullableNumber(value: string): number | null {
  if (value.trim().length === 0) return null;

  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
}

function App() {
  const queryClient = useQueryClient();

  const [activeTab, setActiveTab] = useState<ActiveTab>("monitoring");
  const [realtimeStatus, setRealtimeStatus] = useState("Connecting");
  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);

  const [readRegisterAddress, setReadRegisterAddress] = useState("1305");
  const [writeRegisterAddress, setWriteRegisterAddress] = useState("1300");
  const [writeRegisterValue, setWriteRegisterValue] = useState("1");
  const [operationResult, setOperationResult] =
    useState<RegisterOperationResult | null>(null);

  const [selectedProfileId, setSelectedProfileId] = useState<string | null>(
    null
  );
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

  const devicesQuery = useQuery({
    queryKey: ["devices"],
    queryFn: getDevices,
  });

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
        (currentRegisters) => {
          if (!currentRegisters) return currentRegisters;

          return currentRegisters.map((register) => {
            if (register.definitionId !== event.registerDefinitionId) {
              return register;
            }

            return {
              ...register,
              currentValue: event.value,
              updatedAtUtc: event.updatedAtUtc,
            };
          });
        }
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
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["device-registers", selectedDevice?.id],
        }),
        queryClient.invalidateQueries({ queryKey: ["modbus-logs"] }),
      ]);
    },
    onError: (error) => setOperationResult(toOperationResult(error)),
  });

  const writeRegisterMutation = useMutation({
    mutationFn: writeRegister,
    onSuccess: async (result) => {
      setOperationResult(result);
      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["device-registers", selectedDevice?.id],
        }),
        queryClient.invalidateQueries({ queryKey: ["modbus-logs"] }),
      ]);
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
    mutationFn: (request: CreateTestStepRequest) =>
      addTestStep(selectedProfileId!, request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["test-profiles"] }),
        queryClient.invalidateQueries({
          queryKey: ["test-profile", selectedProfileId],
        }),
      ]);
    },
  });

  const runProfileMutation = useMutation({
    mutationFn: runTestProfile,
    onSuccess: async (run) => {
      setLastRun(run);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["test-runs"] }),
        queryClient.invalidateQueries({ queryKey: ["modbus-logs"] }),
        queryClient.invalidateQueries({
          queryKey: ["device-registers", selectedDevice?.id],
        }),
      ]);
    },
  });

  function handleReadRegister() {
    if (!selectedDevice) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Сначала выбери устройство.",
      });
      return;
    }

    const address = Number(readRegisterAddress);

    if (Number.isNaN(address)) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Адрес регистра должен быть числом.",
      });
      return;
    }

    readRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: address,
    });
  }

  function handleWriteRegister() {
    if (!selectedDevice) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Сначала выбери устройство.",
      });
      return;
    }

    const address = Number(writeRegisterAddress);
    const value = Number(writeRegisterValue);

    if (Number.isNaN(address) || Number.isNaN(value)) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Адрес регистра и значение должны быть числами.",
      });
      return;
    }

    writeRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: address,
      value,
    });
  }

  function handleCreateProfile() {
    createProfileMutation.mutate({
      name: newProfileName,
      description: newProfileDescription.trim() || null,
    });
  }

  function handleAddStep() {
    if (!selectedProfileId) return;

    addStepMutation.mutate({
      type: stepType,
      name: stepName,
      slaveAddress:
        stepType === "Delay" ? null : toNullableNumber(stepSlaveAddress),
      registerAddress:
        stepType === "Delay" ? null : toNullableNumber(stepRegisterAddress),
      value: stepType === "WriteRegister" ? toNullableNumber(stepValue) : null,
      minValue:
        stepType === "CheckRegisterRange" ? toNullableNumber(stepMinValue) : null,
      maxValue:
        stepType === "CheckRegisterRange" ? toNullableNumber(stepMaxValue) : null,
      delayMs: stepType === "Delay" ? toNullableNumber(stepDelayMs) : null,
    });
  }

  function renderLogRows(logs: ModbusLogDto[]) {
    return logs.map((log) => (
      <tr key={log.id}>
        <td>{formatTimestamp(log.timestampUtc)}</td>
        <td>{log.slaveAddress}</td>
        <td>{getFunctionCodeLabel(log.functionCode)}</td>
        <td>{log.registerAddress}</td>
        <td>{log.value ?? "—"}</td>
        <td>
          <span className={getStatusClass(log.status)}>
            {getStatusLabel(log.status)}
          </span>
        </td>
        <td>{log.message}</td>
      </tr>
    ));
  }

  function renderMonitoringTab() {
    return (
      <>
        <section className="layout">
          <aside className="panel">
            <div className="panel-header">
              <h2>Устройства</h2>
              <span className="panel-counter">
                {devicesQuery.data?.length ?? 0}
              </span>
            </div>

            {devicesQuery.isLoading && (
              <p className="muted padded">Загрузка устройств...</p>
            )}

            {devicesQuery.isError && (
              <p className="error">
                Не удалось получить устройства. Проверь, что backend запущен.
              </p>
            )}

            <div className="device-list">
              {devicesQuery.data?.map((device) => (
                <button
                  key={device.id}
                  className={
                    selectedDevice?.id === device.id
                      ? "device-card selected"
                      : "device-card"
                  }
                  onClick={() => {
                    setSelectedDevice(device);
                    setOperationResult(null);
                  }}
                >
                  <span className="device-title">{device.name}</span>
                  <span className="device-meta">
                    Slave address: {device.slaveAddress}
                  </span>
                  <span
                    className={device.isEnabled ? "badge online" : "badge offline"}
                  >
                    {device.isEnabled ? "Enabled" : "Disabled"}
                  </span>
                </button>
              ))}
            </div>
          </aside>

          <section className="panel content-panel">
            <div className="panel-header">
              <div>
                <h2>Регистры</h2>
                <p className="muted">
                  {selectedDevice
                    ? `Устройство: ${selectedDevice.name}`
                    : "Выбери устройство слева"}
                </p>
              </div>
            </div>

            {selectedDevice && (
              <section className="operation-panel">
                <div>
                  <h3>Операции с регистрами</h3>
                  <p className="muted">
                    Чтение выполняет FC03, запись выполняет FC06.
                  </p>
                </div>

                <div className="operation-sections">
                  <div className="operation-box">
                    <h4>Чтение регистра</h4>
                    <div className="read-operation-form">
                      <label>
                        Адрес регистра
                        <input
                          value={readRegisterAddress}
                          onChange={(event) =>
                            setReadRegisterAddress(event.target.value)
                          }
                          inputMode="numeric"
                        />
                      </label>

                      <button
                        className="secondary-button"
                        onClick={handleReadRegister}
                        disabled={readRegisterMutation.isPending}
                      >
                        {readRegisterMutation.isPending
                          ? "Чтение..."
                          : "Прочитать"}
                      </button>
                    </div>
                  </div>

                  <div className="operation-box">
                    <h4>Запись регистра</h4>
                    <div className="operation-form">
                      <label>
                        Адрес
                        <input
                          value={writeRegisterAddress}
                          onChange={(event) =>
                            setWriteRegisterAddress(event.target.value)
                          }
                          inputMode="numeric"
                        />
                      </label>

                      <label>
                        Значение
                        <input
                          value={writeRegisterValue}
                          onChange={(event) =>
                            setWriteRegisterValue(event.target.value)
                          }
                          inputMode="numeric"
                        />
                      </label>

                      <button
                        className="primary-button"
                        onClick={handleWriteRegister}
                        disabled={writeRegisterMutation.isPending}
                      >
                        {writeRegisterMutation.isPending ? "Запись..." : "Записать"}
                      </button>
                    </div>
                  </div>
                </div>

                {operationResult && (
                  <div
                    className={
                      operationResult.isSuccess
                        ? "operation-result success"
                        : "operation-result rejected"
                    }
                  >
                    <strong>
                      {operationResult.isSuccess ? "Успешно" : "Операция отклонена"}
                    </strong>
                    <span>{operationResult.message}</span>
                    {operationResult.value !== null && (
                      <span>Значение: {operationResult.value}</span>
                    )}
                  </div>
                )}
              </section>
            )}

            {!selectedDevice && (
              <div className="empty-state">
                Выбери Modbus-устройство, чтобы посмотреть карту регистров.
              </div>
            )}

            {selectedDevice && registersQuery.isLoading && (
              <p className="muted padded">Загрузка регистров...</p>
            )}

            {selectedDevice && registersQuery.isError && (
              <p className="error">Не удалось получить регистры устройства.</p>
            )}

            {selectedDevice && registersQuery.data && (
              <div className="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>Адрес</th>
                      <th>Название</th>
                      <th>Доступ</th>
                      <th>Значение</th>
                      <th>Ед.</th>
                      <th>Диапазон</th>
                    </tr>
                  </thead>
                  <tbody>
                    {registersQuery.data.map((register) => (
                      <tr key={register.definitionId}>
                        <td>{register.address}</td>
                        <td>{register.name}</td>
                        <td>
                          <span
                            className={
                              register.accessMode === "ReadWrite"
                                ? "badge writable"
                                : "badge readonly"
                            }
                          >
                            {register.accessMode}
                          </span>
                        </td>
                        <td>{register.currentValue ?? "—"}</td>
                        <td>{register.unit ?? "—"}</td>
                        <td>
                          {register.minValue ?? "—"} / {register.maxValue ?? "—"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </section>
        </section>

        <section className="panel logs-panel">
          <div className="panel-header">
            <div>
              <h2>Журнал Modbus-операций</h2>
              <p className="muted">
                Последние операции чтения, записи и отклонённые запросы.
              </p>
            </div>

            <button
              className="secondary-button"
              onClick={() => modbusLogsQuery.refetch()}
              disabled={modbusLogsQuery.isFetching}
            >
              {modbusLogsQuery.isFetching ? "Обновление..." : "Обновить"}
            </button>
          </div>

          {modbusLogsQuery.isLoading && (
            <p className="muted padded">Загрузка журнала...</p>
          )}

          {modbusLogsQuery.isError && (
            <p className="error">Не удалось загрузить журнал Modbus-операций.</p>
          )}

          {modbusLogsQuery.data && modbusLogsQuery.data.length > 0 && (
            <div className="table-wrapper">
              <table>
                <thead>
                  <tr>
                    <th>Время</th>
                    <th>Slave</th>
                    <th>Функция</th>
                    <th>Регистр</th>
                    <th>Значение</th>
                    <th>Статус</th>
                    <th>Сообщение</th>
                  </tr>
                </thead>
                <tbody>{renderLogRows(modbusLogsQuery.data)}</tbody>
              </table>
            </div>
          )}
        </section>
      </>
    );
  }

  function renderStepLabel(step: TestProfileDto["steps"][number]) {
    if (step.type === "Delay") return `${step.delayMs} мс`;
    if (step.type === "WriteRegister") {
      return `Slave ${step.slaveAddress}, R${step.registerAddress} = ${step.value}`;
    }

    return `Slave ${step.slaveAddress}, R${step.registerAddress}: ${step.minValue}–${step.maxValue}`;
  }

  function renderTestingTab() {
    const selectedProfile = selectedProfileQuery.data;

    return (
      <section className="layout">
        <aside className="panel">
          <div className="panel-header">
            <h2>Тестовые профили</h2>
            <span className="panel-counter">
              {testProfilesQuery.data?.length ?? 0}
            </span>
          </div>

          <div className="create-card">
            <label>
              Название
              <input
                value={newProfileName}
                onChange={(event) => setNewProfileName(event.target.value)}
              />
            </label>
            <label>
              Описание
              <textarea
                value={newProfileDescription}
                onChange={(event) => setNewProfileDescription(event.target.value)}
                rows={3}
              />
            </label>
            <button
              className="primary-button full-width"
              onClick={handleCreateProfile}
              disabled={createProfileMutation.isPending}
            >
              {createProfileMutation.isPending ? "Создание..." : "Создать профиль"}
            </button>
          </div>

          <div className="device-list">
            {testProfilesQuery.data?.map((profile) => (
              <button
                key={profile.id}
                className={
                  selectedProfileId === profile.id
                    ? "device-card selected"
                    : "device-card"
                }
                onClick={() => {
                  setSelectedProfileId(profile.id);
                  setLastRun(null);
                }}
              >
                <span className="device-title">{profile.name}</span>
                <span className="device-meta">
                  Шагов: {profile.steps.length}
                </span>
                <span
                  className={profile.isEnabled ? "badge online" : "badge offline"}
                >
                  {profile.isEnabled ? "Enabled" : "Disabled"}
                </span>
              </button>
            ))}
          </div>
        </aside>

        <section className="panel content-panel">
          <div className="panel-header">
            <div>
              <h2>Сценарий проверки</h2>
              <p className="muted">
                {selectedProfile
                  ? selectedProfile.description ?? selectedProfile.name
                  : "Выбери профиль слева или создай новый"}
              </p>
            </div>

            {selectedProfile && (
              <button
                className="primary-button"
                disabled={runProfileMutation.isPending}
                onClick={() => runProfileMutation.mutate(selectedProfile.id)}
              >
                {runProfileMutation.isPending ? "Выполнение..." : "Запустить тест"}
              </button>
            )}
          </div>

          {!selectedProfile && (
            <div className="empty-state">
              Здесь будет список шагов теста и результат последнего запуска.
            </div>
          )}

          {selectedProfile && (
            <>
              <section className="operation-panel">
                <h3>Добавить шаг</h3>
                <div className="step-form">
                  <label>
                    Тип шага
                    <select
                      value={stepType}
                      onChange={(event) => setStepType(event.target.value)}
                    >
                      <option value="WriteRegister">Записать регистр</option>
                      <option value="Delay">Пауза</option>
                      <option value="CheckRegisterRange">
                        Проверить диапазон регистра
                      </option>
                    </select>
                  </label>

                  <label>
                    Название
                    <input
                      value={stepName}
                      onChange={(event) => setStepName(event.target.value)}
                    />
                  </label>

                  {stepType !== "Delay" && (
                    <>
                      <label>
                        Slave
                        <input
                          value={stepSlaveAddress}
                          onChange={(event) =>
                            setStepSlaveAddress(event.target.value)
                          }
                          inputMode="numeric"
                        />
                      </label>
                      <label>
                        Регистр
                        <input
                          value={stepRegisterAddress}
                          onChange={(event) =>
                            setStepRegisterAddress(event.target.value)
                          }
                          inputMode="numeric"
                        />
                      </label>
                    </>
                  )}

                  {stepType === "WriteRegister" && (
                    <label>
                      Значение
                      <input
                        value={stepValue}
                        onChange={(event) => setStepValue(event.target.value)}
                        inputMode="numeric"
                      />
                    </label>
                  )}

                  {stepType === "CheckRegisterRange" && (
                    <>
                      <label>
                        Min
                        <input
                          value={stepMinValue}
                          onChange={(event) => setStepMinValue(event.target.value)}
                          inputMode="numeric"
                        />
                      </label>
                      <label>
                        Max
                        <input
                          value={stepMaxValue}
                          onChange={(event) => setStepMaxValue(event.target.value)}
                          inputMode="numeric"
                        />
                      </label>
                    </>
                  )}

                  {stepType === "Delay" && (
                    <label>
                      Задержка, мс
                      <input
                        value={stepDelayMs}
                        onChange={(event) => setStepDelayMs(event.target.value)}
                        inputMode="numeric"
                      />
                    </label>
                  )}

                  <button
                    className="secondary-button"
                    onClick={handleAddStep}
                    disabled={addStepMutation.isPending}
                  >
                    {addStepMutation.isPending ? "Добавление..." : "Добавить шаг"}
                  </button>
                </div>
              </section>

              <div className="table-wrapper">
                <table>
                  <thead>
                    <tr>
                      <th>№</th>
                      <th>Шаг</th>
                      <th>Тип</th>
                      <th>Параметры</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedProfile.steps.map((step) => (
                      <tr key={step.id}>
                        <td>{step.orderIndex}</td>
                        <td>{step.name}</td>
                        <td>{step.type}</td>
                        <td>{renderStepLabel(step)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {lastRun && (
                <section className="run-result">
                  <div className="panel-header">
                    <div>
                      <h3>Последний запуск</h3>
                      <p className="muted">{lastRun.summary}</p>
                    </div>
                    <span className={getStatusClass(lastRun.status)}>
                      {lastRun.status}
                    </span>
                  </div>

                  <div className="table-wrapper">
                    <table>
                      <thead>
                        <tr>
                          <th>№</th>
                          <th>Шаг</th>
                          <th>Статус</th>
                          <th>Факт</th>
                          <th>Сообщение</th>
                        </tr>
                      </thead>
                      <tbody>
                        {lastRun.steps.map((step) => (
                          <tr key={step.id}>
                            <td>{step.orderIndex}</td>
                            <td>{step.stepName}</td>
                            <td>
                              <span className={getStatusClass(step.status)}>
                                {step.status}
                              </span>
                            </td>
                            <td>{step.actualValue ?? "—"}</td>
                            <td>{step.message}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                </section>
              )}

              <section className="run-result">
                <div className="panel-header">
                  <div>
                    <h3>История запусков</h3>
                    <p className="muted">Последние результаты тестов.</p>
                  </div>
                  <button
                    className="secondary-button"
                    onClick={() => testRunsQuery.refetch()}
                  >
                    Обновить
                  </button>
                </div>

                {testRunsQuery.data && testRunsQuery.data.length > 0 && (
                  <div className="table-wrapper">
                    <table>
                      <thead>
                        <tr>
                          <th>Время</th>
                          <th>Профиль</th>
                          <th>Статус</th>
                          <th>Итог</th>
                        </tr>
                      </thead>
                      <tbody>
                        {testRunsQuery.data.map((run) => (
                          <tr key={run.id}>
                            <td>{formatTimestamp(run.startedAtUtc)}</td>
                            <td>{run.profileName}</td>
                            <td>
                              <span className={getStatusClass(run.status)}>
                                {run.status}
                              </span>
                            </td>
                            <td>{run.summary ?? "—"}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                )}
              </section>
            </>
          )}
        </section>
      </section>
    );
  }

  return (
    <main className="app-shell">
      <section className="hero">
        <div className="hero-main">
          <p className="eyebrow">Industrial Fullstack Platform</p>
          <h1>ModbusLab</h1>
          <p className="hero-description">
            Web-платформа для симуляции, мониторинга и автоматизированного
            тестирования Modbus-устройств.
          </p>
        </div>

        <div className="status-card">
          <div>
            <span className="status-label">Backend</span>
            <strong>ASP.NET Core + PostgreSQL</strong>
          </div>
          <span className="status-dot">SignalR: {realtimeStatus}</span>
        </div>
      </section>

      <nav className="tabs">
        <button
          className={activeTab === "monitoring" ? "tab active" : "tab"}
          onClick={() => setActiveTab("monitoring")}
        >
          Мониторинг регистров
        </button>
        <button
          className={activeTab === "testing" ? "tab active" : "tab"}
          onClick={() => setActiveTab("testing")}
        >
          Тестовые сценарии
        </button>
      </nav>

      {activeTab === "monitoring" ? renderMonitoringTab() : renderTestingTab()}
    </main>
  );
}

export default App;
