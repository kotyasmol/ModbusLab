export type ReadRegisterRequest = {
  slaveAddress: number;
  registerAddress: number;
};

export type WriteRegisterRequest = {
  slaveAddress: number;
  registerAddress: number;
  value: number;
};

export type RegisterOperationResult = {
  isSuccess: boolean;
  status: number;
  value: number | null;
  message: string;
};

export type ModbusLogDto = {
  id: string;
  timestampUtc: string;
  slaveAddress: number;
  functionCode: number;
  registerAddress: number;
  value: number | null;
  status: number;
  message: string;
  slaveDeviceId: string | null;
};

export type RegisterValueChangedEvent = {
  deviceId: string;
  registerDefinitionId: string;
  registerAddress: number;
  value: number;
  updatedAtUtc: string;
};
