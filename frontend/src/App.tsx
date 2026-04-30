import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import type { DeviceDto } from "./features/devices/types";
import "./App.css";

function App() {
  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);

  const devicesQuery = useQuery({
    queryKey: ["devices"],
    queryFn: getDevices,
  });

  const registersQuery = useQuery({
    queryKey: ["device-registers", selectedDevice?.id],
    queryFn: () => getDeviceRegisters(selectedDevice!.id),
    enabled: selectedDevice !== null,
  });

  return (
    <main className="app-shell">
      <section className="hero">
        <div>
          <p className="eyebrow">Industrial Fullstack Platform</p>
          <h1>ModbusLab</h1>
          <p className="hero-description">
            Web-платформа для симуляции, мониторинга и автоматизированного
            тестирования Modbus-устройств.
          </p>
        </div>

        <div className="status-card">
          <span className="status-label">Backend</span>
          <strong>ASP.NET Core + PostgreSQL</strong>
        </div>
      </section>

      <section className="layout">
        <aside className="panel">
          <div className="panel-header">
            <h2>Устройства</h2>
            <span>{devicesQuery.data?.length ?? 0}</span>
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
                onClick={() => setSelectedDevice(device)}
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