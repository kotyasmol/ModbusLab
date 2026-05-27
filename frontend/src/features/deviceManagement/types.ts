export type DeviceTypeDto = {
  id: string;
  name: string;
  description: string | null;
};

export type ManagedDeviceDto = {
  id: string;
  name: string;
  slaveAddress: number;
  deviceTypeId: string;
  isEnabled: boolean;
};

export type RegisterDefinitionDto = {
  id: string;
  deviceTypeId: string;
  address: number;
  name: string;
  accessMode: string;
  unit: string | null;
  minValue: number | null;
  maxValue: number | null;
  description: string | null;
};

export type CreateDeviceTypeRequest = {
  name: string;
  description: string | null;
};

export type CreateDeviceRequest = {
  name: string;
  slaveAddress: number;
  deviceTypeId: string;
};

export type ChangeDeviceStatusRequest = {
  isEnabled: boolean;
};

export type CreateRegisterDefinitionRequest = {
  deviceTypeId: string;
  address: number;
  name: string;
  accessMode: string;
  unit: string | null;
  minValue: number | null;
  maxValue: number | null;
  description: string | null;
  initialValue: number | null;
};
