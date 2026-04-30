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