import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import { getModbusLogs, writeRegister } from "./features/devices/modbusApi";
import type { ModbusLogDto, RegisterOperationResult } from "./features/devices/modbusTypes";
import type { DeviceDto } from "./features/devices/types";
import "./App.css";

function toOperationResult(error: unknown): RegisterOperationResult {
  if (
    typeof error === "object" &&
    error !== null &&
    "isSuccess" in error &&
    "message" in error
  ) {
    return error as RegisterOperationResult;
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
  if (functionCode === 3) {
    return "FC03 Read";
  }

  if (functionCode === 6) {
    return "FC06 Write";
  }

  return `FC${functionCode}`;
}

function getStatusLabel(status: number): string {
  if (status === 0) {
    return "Success";
  }

  if (status === 1) {
    return "Failed";
  }

  if (status === 2) {
    return "Rejected";
  }

  return "Unknown";
}

function getStatusClass(status: number): string {
  if (status === 0) {
    return "log-status success";
  }

  if (status === 1) {
    return "log-status failed";
  }

  if (status === 2) {
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

function App() {
  const queryClient = useQueryClient();

  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);
  const [registerAddress, setRegisterAddress] = useState("1300");
  const [registerValue, setRegisterValue] = useState("1");
  const [operationResult, setOperationResult] =
    useState<RegisterOperationResult | null>(null);

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

  const writeRegisterMutation = useMutation({
    mutationFn: writeRegister,
    onSuccess: async (result) => {
      setOperationResult(result);

      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["device-registers", selectedDevice?.id],
        }),
        queryClient.invalidateQueries({
          queryKey: ["modbus-logs"],
        }),
      ]);
    },
    onError: (error) => {
      setOperationResult(toOperationResult(error));
    },
  });

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

    const address = Number(registerAddress);
    const value = Number(registerValue);

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
          <span className="status-dot">Online-ready</span>
        </div>
      </section>

      <section className="layout">
        <aside className="panel">
          <div className="panel-header">
            <h2>Устройства</h2>
            <span className="panel-counter">{devicesQuery.data?.length ?? 0}</span>
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
                <span className={device.isEnabled ? "badge online" : "badge offline"}>
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
                <h3>Запись регистра</h3>
                <p className="muted">
                  Например: адрес 1300, значение 1 — включение Power control.
                </p>
              </div>

              <div className="operation-form">
                <label>
                  Адрес регистра
                  <input
                    value={registerAddress}
                    onChange={(event) => setRegisterAddress(event.target.value)}
                    inputMode="numeric"
                  />
                </label>

                <label>
                  Значение
                  <input
                    value={registerValue}
                    onChange={(event) => setRegisterValue(event.target.value)}
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

        {modbusLogsQuery.data && modbusLogsQuery.data.length === 0 && (
          <div className="empty-state">
            Журнал пока пуст. Выполни чтение или запись регистра.
          </div>
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
    </main>
  );
}

export default App;