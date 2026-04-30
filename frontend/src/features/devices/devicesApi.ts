import { apiGet } from "../../shared/api/apiClient";
import type { DeviceDto, RegisterDto } from "./types";

export function getDevices(): Promise<DeviceDto[]> {
  return apiGet<DeviceDto[]>("/api/devices");
}

export function getDeviceRegisters(deviceId: string): Promise<RegisterDto[]> {
  return apiGet<RegisterDto[]>(`/api/devices/${deviceId}/registers`);
}