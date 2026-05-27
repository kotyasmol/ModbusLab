import { apiGet, apiPatch, apiPost } from "../../shared/api/apiClient";
import type {
  ChangeDeviceStatusRequest,
  CreateDeviceRequest,
  CreateDeviceTypeRequest,
  CreateRegisterDefinitionRequest,
  DeviceTypeDto,
  ManagedDeviceDto,
  RegisterDefinitionDto,
} from "./types";

export function getDeviceTypes(): Promise<DeviceTypeDto[]> {
  return apiGet<DeviceTypeDto[]>("/api/device-management/types");
}

export function createDeviceType(request: CreateDeviceTypeRequest): Promise<DeviceTypeDto> {
  return apiPost<CreateDeviceTypeRequest, DeviceTypeDto>("/api/device-management/types", request);
}

export function createDevice(request: CreateDeviceRequest): Promise<ManagedDeviceDto> {
  return apiPost<CreateDeviceRequest, ManagedDeviceDto>("/api/device-management/devices", request);
}

export function changeDeviceStatus(
  deviceId: string,
  request: ChangeDeviceStatusRequest
): Promise<ManagedDeviceDto> {
  return apiPatch<ChangeDeviceStatusRequest, ManagedDeviceDto>(
    `/api/device-management/devices/${deviceId}/status`,
    request
  );
}

export function createRegisterDefinition(
  request: CreateRegisterDefinitionRequest
): Promise<RegisterDefinitionDto> {
  return apiPost<CreateRegisterDefinitionRequest, RegisterDefinitionDto>(
    "/api/device-management/registers",
    request
  );
}
