import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import { writeRegister } from "./features/devices/modbusApi";
import type { RegisterOperationResult } from "./features/devices/modbusTypes";
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

  const writeRegisterMutation = useMutation({
    mutationFn: writeRegister,
    onSuccess: async (result) => {
      setOperationResult(result);

      await queryClient.invalidateQueries({
        queryKey: ["device-registers", selectedDevice?.id],
      });
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
    </main>
  );
}

export default App;