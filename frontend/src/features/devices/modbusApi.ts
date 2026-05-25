import { apiGet, apiPost } from "../../shared/api/apiClient";
import type {
  ModbusLogDto,
  ReadRegisterRequest,
  RegisterOperationResult,
  WriteRegisterRequest,
} from "./modbusTypes";

export function readRegister(
  request: ReadRegisterRequest
): Promise<RegisterOperationResult> {
  return apiPost<ReadRegisterRequest, RegisterOperationResult>(
    "/api/modbus/read",
    request
  );
}

export function writeRegister(
  request: WriteRegisterRequest
): Promise<RegisterOperationResult> {
  return apiPost<WriteRegisterRequest, RegisterOperationResult>(
    "/api/modbus/write",
    request
  );
}

export function getModbusLogs(): Promise<ModbusLogDto[]> {
  return apiGet<ModbusLogDto[]>("/api/modbus/logs");
}
