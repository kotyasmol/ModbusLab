export type DeviceDto = {
  id: string;
  name: string;
  slaveAddress: number;
  deviceTypeId: string;
  isEnabled: boolean;
};

export type RegisterDto = {
  definitionId: string;
  address: number;
  name: string;
  accessMode: string;
  unit: string | null;
  minValue: number | null;
  maxValue: number | null;
  currentValue: number | null;
  updatedAtUtc: string | null;
};