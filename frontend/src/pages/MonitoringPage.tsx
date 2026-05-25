import type { UseMutationResult, UseQueryResult } from "@tanstack/react-query";
import type { ModbusLogDto, RegisterOperationResult } from "../features/devices/modbusTypes";
import type { DeviceDto, RegisterDto } from "../features/devices/types";

type Props = {
  devicesQuery: UseQueryResult<DeviceDto[], Error>;
  registersQuery: UseQueryResult<RegisterDto[], Error>;
  modbusLogsQuery: UseQueryResult<ModbusLogDto[], Error>;
  selectedDevice: DeviceDto | null;
  setSelectedDevice: (device: DeviceDto) => void;
  operationResult: RegisterOperationResult | null;
  readRegisterAddress: string;
  setReadRegisterAddress: (v: string) => void;
  writeRegisterAddress: string;
  setWriteRegisterAddress: (v: string) => void;
  writeRegisterValue: string;
  setWriteRegisterValue: (v: string) => void;
  handleReadRegister: () => void;
  handleWriteRegister: () => void;
  readRegisterPending: boolean;
  writeRegisterPending: boolean;
  renderLogRows: (logs: ModbusLogDto[]) => React.ReactNode;
};

export function MonitoringPage(props: Props) {
  const {
    devicesQuery, registersQuery, modbusLogsQuery, selectedDevice, setSelectedDevice, operationResult,
    readRegisterAddress, setReadRegisterAddress, writeRegisterAddress, setWriteRegisterAddress,
    writeRegisterValue, setWriteRegisterValue, handleReadRegister, handleWriteRegister,
    readRegisterPending, writeRegisterPending, renderLogRows,
  } = props;

  return <>{/* same layout */}
    <section className="layout"><aside className="panel"><div className="panel-header"><h2>Устройства</h2><span className="panel-counter">{devicesQuery.data?.length ?? 0}</span></div>
    <div className="device-list">{devicesQuery.data?.map((device)=><button key={device.id} className={selectedDevice?.id===device.id?"device-card selected":"device-card"} onClick={()=>setSelectedDevice(device)}><span className="device-title">{device.name}</span><span className="device-meta">Slave address: {device.slaveAddress}</span><span className={device.isEnabled?"badge online":"badge offline"}>{device.isEnabled?"Enabled":"Disabled"}</span></button>)}</div></aside>
    <section className="panel content-panel"><div className="panel-header"><h2>Регистры</h2></div>
    {selectedDevice && <section className="operation-panel"><div className="operation-sections"><div className="operation-box"><h4>Чтение регистра</h4><label>Адрес регистра<input value={readRegisterAddress} onChange={(e)=>setReadRegisterAddress(e.target.value)} /></label><button className="secondary-button" onClick={handleReadRegister} disabled={readRegisterPending}>{readRegisterPending?"Чтение...":"Прочитать"}</button></div><div className="operation-box"><h4>Запись регистра</h4><label>Адрес<input value={writeRegisterAddress} onChange={(e)=>setWriteRegisterAddress(e.target.value)} /></label><label>Значение<input value={writeRegisterValue} onChange={(e)=>setWriteRegisterValue(e.target.value)} /></label><button className="primary-button" onClick={handleWriteRegister} disabled={writeRegisterPending}>{writeRegisterPending?"Запись...":"Записать"}</button></div></div>{operationResult && <div className={operationResult.isSuccess?"operation-result success":"operation-result rejected"}>{operationResult.message}</div>}</section>}
    {registersQuery.data && <div className="table-wrapper"><table><thead><tr><th>Адрес</th><th>Название</th><th>Доступ</th><th>Значение</th></tr></thead><tbody>{registersQuery.data.map((r)=><tr key={r.definitionId}><td>{r.address}</td><td>{r.name}</td><td>{r.accessMode}</td><td>{r.currentValue ?? "—"}</td></tr>)}</tbody></table></div>}
    </section></section>
    <section className="panel logs-panel"><div className="panel-header"><h2>Журнал Modbus-операций</h2></div>{modbusLogsQuery.data && <div className="table-wrapper"><table><thead><tr><th>Время</th><th>Slave</th><th>Функция</th><th>Регистр</th><th>Значение</th><th>Статус</th><th>Сообщение</th></tr></thead><tbody>{renderLogRows(modbusLogsQuery.data)}</tbody></table></div>}</section>
  </>;
}
