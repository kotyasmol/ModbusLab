import { useState } from "react";
import type { RegisterOperationResult } from "../modbusTypes";

type RegisterOperationsPanelProps = {
  operationResult: RegisterOperationResult | null;
  isReading: boolean;
  isWriting: boolean;
  onRead: (registerAddress: string) => void;
  onWrite: (registerAddress: string, value: string) => void;
};

export function RegisterOperationsPanel({
  operationResult,
  isReading,
  isWriting,
  onRead,
  onWrite,
}: RegisterOperationsPanelProps) {
  const [readRegisterAddress, setReadRegisterAddress] = useState("1305");
  const [writeRegisterAddress, setWriteRegisterAddress] = useState("1300");
  const [writeRegisterValue, setWriteRegisterValue] = useState("1");

  return (
    <section className="operation-panel">
      <div>
        <h3>Операции с регистрами</h3>
        <p className="muted">Чтение выполняет FC03, запись выполняет FC06.</p>
      </div>

      <div className="operation-sections">
        <div className="operation-box">
          <div>
            <h4>Чтение регистра</h4>
            <p className="muted">Например: адрес 1305 — Output voltage.</p>
          </div>

          <div className="read-operation-form">
            <label>
              Адрес регистра
              <input
                value={readRegisterAddress}
                onChange={(event) => setReadRegisterAddress(event.target.value)}
                inputMode="numeric"
              />
            </label>

            <button
              className="secondary-button"
              onClick={() => onRead(readRegisterAddress)}
              disabled={isReading}
            >
              {isReading ? "Чтение..." : "Прочитать"}
            </button>
          </div>
        </div>

        <div className="operation-box">
          <div>
            <h4>Запись регистра</h4>
            <p className="muted">
              Например: адрес 1300, значение 1 — Power control.
            </p>
          </div>

          <div className="operation-form">
            <label>
              Адрес регистра
              <input
                value={writeRegisterAddress}
                onChange={(event) => setWriteRegisterAddress(event.target.value)}
                inputMode="numeric"
              />
            </label>

            <label>
              Значение
              <input
                value={writeRegisterValue}
                onChange={(event) => setWriteRegisterValue(event.target.value)}
                inputMode="numeric"
              />
            </label>

            <button
              className="primary-button"
              onClick={() => onWrite(writeRegisterAddress, writeRegisterValue)}
              disabled={isWriting}
            >
              {isWriting ? "Запись..." : "Записать"}
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
  );
}