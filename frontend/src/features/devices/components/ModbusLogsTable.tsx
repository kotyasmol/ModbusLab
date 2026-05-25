import type { ModbusLogDto } from "../modbusTypes";
import {
  formatTimestamp,
  getFunctionCodeLabel,
  getStatusClass,
  getStatusLabel,
} from "../utils/modbusFormatters";

type ModbusLogsTableProps = {
  logs: ModbusLogDto[] | undefined;
  isLoading: boolean;
  isError: boolean;
  isFetching: boolean;
  onRefresh: () => void;
};

export function ModbusLogsTable({
  logs,
  isLoading,
  isError,
  isFetching,
  onRefresh,
}: ModbusLogsTableProps) {
  return (
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
          onClick={onRefresh}
          disabled={isFetching}
        >
          {isFetching ? "Обновление..." : "Обновить"}
        </button>
      </div>

      {isLoading && <p className="muted padded">Загрузка журнала...</p>}

      {isError && (
        <p className="error">Не удалось загрузить журнал Modbus-операций.</p>
      )}

      {logs && logs.length === 0 && (
        <div className="empty-state">
          Журнал пока пуст. Выполни чтение или запись регистра.
        </div>
      )}

      {logs && logs.length > 0 && (
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
            <tbody>
              {logs.map((log) => (
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
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}