import type { DeviceDto, RegisterDto } from "../types";

type RegisterTableProps = {
  selectedDevice: DeviceDto | null;
  registers: RegisterDto[] | undefined;
  isLoading: boolean;
  isError: boolean;
};

export function RegisterTable({
  selectedDevice,
  registers,
  isLoading,
  isError,
}: RegisterTableProps) {
  if (!selectedDevice) {
    return (
      <div className="empty-state">
        Выбери Modbus-устройство, чтобы посмотреть карту регистров.
      </div>
    );
  }

  if (isLoading) {
    return <p className="muted padded">Загрузка регистров...</p>;
  }

  if (isError) {
    return <p className="error">Не удалось получить регистры устройства.</p>;
  }

  if (!registers || registers.length === 0) {
    return <div className="empty-state">У устройства нет регистров.</div>;
  }

  return (
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
          {registers.map((register) => (
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
  );
}