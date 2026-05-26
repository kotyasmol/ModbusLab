import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDeviceRegisters, getDevices } from "./devicesApi";
import { getModbusLogs, readRegister, writeRegister } from "./modbusApi";
import type { ModbusLogDto, RegisterOperationResult } from "./modbusTypes";
import type { DeviceDto } from "./types";

type UseMonitoringOptions = {
  formatTimestamp: (value: string) => string;
  getStatusClass: (status: number | string) => string;
};

function toOperationResult(error: unknown): RegisterOperationResult {
  if (typeof error === "object" && error !== null && "message" in error) {
    return {
      isSuccess: false,
      status: 1,
      value: null,
      message:
        typeof (error as { message?: unknown }).message === "string"
          ? (error as { message: string }).message
          : "Неизвестная ошибка.",
    };
  }

  if (error instanceof Error) {
    return {
      isSuccess: false,
      status: 1,
      value: null,
      message: error.message,
    };
  }

  return {
    isSuccess: false,
    status: 1,
    value: null,
    message: "Неизвестная ошибка.",
  };
}

export function useMonitoring({ formatTimestamp, getStatusClass }: UseMonitoringOptions) {
  const queryClient = useQueryClient();
  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);
  const [readRegisterAddress, setReadRegisterAddress] = useState("1305");
  const [writeRegisterAddress, setWriteRegisterAddress] = useState("1300");
  const [writeRegisterValue, setWriteRegisterValue] = useState("1");
  const [operationResult, setOperationResult] = useState<RegisterOperationResult | null>(null);

  const devicesQuery = useQuery({ queryKey: ["devices"], queryFn: getDevices });
  const registersQuery = useQuery({
    queryKey: ["device-registers", selectedDevice?.id],
    queryFn: () => getDeviceRegisters(selectedDevice!.id),
    enabled: selectedDevice !== null,
  });
  const modbusLogsQuery = useQuery({
    queryKey: ["modbus-logs"],
    queryFn: getModbusLogs,
    refetchInterval: 5000,
  });

  const readRegisterMutation = useMutation({
    mutationFn: readRegister,
    onSuccess: async (result) => {
      setOperationResult(result);
      await queryClient.invalidateQueries({ queryKey: ["modbus-logs"] });
    },
    onError: (error) => setOperationResult(toOperationResult(error)),
  });

  const writeRegisterMutation = useMutation({
    mutationFn: writeRegister,
    onSuccess: async (result) => {
      setOperationResult(result);
      await queryClient.invalidateQueries({ queryKey: ["modbus-logs"] });
    },
    onError: (error) => setOperationResult(toOperationResult(error)),
  });

  const handleReadRegister = () => {
    if (!selectedDevice) return;
    readRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: Number(readRegisterAddress),
    });
  };

  const handleWriteRegister = () => {
    if (!selectedDevice) return;
    writeRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: Number(writeRegisterAddress),
      value: Number(writeRegisterValue),
    });
  };

  const renderLogRows = useMemo(
    () =>
      (logs: ModbusLogDto[]) =>
        logs.map((log) => (
          <tr key={log.id}>
            <td>{formatTimestamp(log.timestampUtc)}</td>
            <td>{log.slaveAddress}</td>
            <td>{log.functionCode}</td>
            <td>{log.registerAddress}</td>
            <td>{log.value ?? "-"}</td>
            <td>
              <span className={getStatusClass(log.status)}>{log.status}</span>
            </td>
            <td>{log.message}</td>
          </tr>
        )),
    [formatTimestamp, getStatusClass]
  );

  return {
    devicesQuery,
    registersQuery,
    modbusLogsQuery,
    selectedDevice,
    setSelectedDevice,
    operationResult,
    readRegisterAddress,
    setReadRegisterAddress,
    writeRegisterAddress,
    setWriteRegisterAddress,
    writeRegisterValue,
    setWriteRegisterValue,
    handleReadRegister,
    handleWriteRegister,
    readRegisterPending: readRegisterMutation.isPending,
    writeRegisterPending: writeRegisterMutation.isPending,
    renderLogRows,
  };
}
