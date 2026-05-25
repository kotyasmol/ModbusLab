import type { DeviceDto } from "../types";

type DeviceListProps = {
  devices: DeviceDto[] | undefined;
  selectedDeviceId: string | undefined;
  isLoading: boolean;
  isError: boolean;
  onSelect: (device: DeviceDto) => void;
};

export function DeviceList({
  devices,
  selectedDeviceId,
  isLoading,
  isError,
  onSelect,
}: DeviceListProps) {
  return (
    <aside className="panel">
      <div className="panel-header">
        <h2>Устройства</h2>
        <span className="panel-counter">{devices?.length ?? 0}</span>
      </div>

      {isLoading && <p className="muted padded">Загрузка устройств...</p>}

      {isError && (
        <p className="error">
          Не удалось получить устройства. Проверь, что backend запущен.
        </p>
      )}

      <div className="device-list">
        {devices?.map((device) => (
          <button
            key={device.id}
            className={
              selectedDeviceId === device.id
                ? "device-card selected"
                : "device-card"
            }
            onClick={() => onSelect(device)}
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
  );
}