import { useEffect, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { DeviceList } from "./features/devices/components/DeviceList";
import { HeroHeader } from "./features/devices/components/HeroHeader";
import { ModbusLogsTable } from "./features/devices/components/ModbusLogsTable";
import { RegisterOperationsPanel } from "./features/devices/components/RegisterOperationsPanel";
import { RegisterTable } from "./features/devices/components/RegisterTable";
import { getDeviceRegisters, getDevices } from "./features/devices/devicesApi";
import {
  getModbusLogs,
  readRegister,
  writeRegister,
} from "./features/devices/modbusApi";
import type {
  RegisterOperationResult,
  RegisterValueChangedEvent,
} from "./features/devices/modbusTypes";
import type { DeviceDto, RegisterDto } from "./features/devices/types";
import { toOperationResult } from "./features/devices/utils/operationResultMapper";
import { createModbusHubConnection } from "./shared/api/modbusHubConnection";
import "./App.css";

function App() {
  const queryClient = useQueryClient();

  const [realtimeStatus, setRealtimeStatus] = useState("Connecting");
  const [selectedDevice, setSelectedDevice] = useState<DeviceDto | null>(null);
  const [operationResult, setOperationResult] =
    useState<RegisterOperationResult | null>(null);

  const devicesQuery = useQuery({
    queryKey: ["devices"],
    queryFn: getDevices,
  });

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

  useEffect(() => {
    const connection = createModbusHubConnection();

    connection.on(
      "RegisterValueChanged",
      (event: RegisterValueChangedEvent) => {
        queryClient.setQueryData<RegisterDto[]>(
          ["device-registers", event.deviceId],
          (currentRegisters) => {
            if (!currentRegisters) {
              return currentRegisters;
            }

            return currentRegisters.map((register) => {
              if (register.definitionId !== event.registerDefinitionId) {
                return register;
              }

              return {
                ...register,
                currentValue: event.value,
                updatedAtUtc: event.updatedAtUtc,
              };
            });
          }
        );
      }
    );

    connection.onreconnecting(() => {
      setRealtimeStatus("Reconnecting");
    });

    connection.onreconnected(() => {
      setRealtimeStatus("Connected");
    });

    connection.onclose(() => {
      setRealtimeStatus("Disconnected");
    });

    connection
      .start()
      .then(() => {
        setRealtimeStatus("Connected");
      })
      .catch(() => {
        setRealtimeStatus("Disconnected");
      });

    return () => {
      void connection.stop();
    };
  }, [queryClient]);

  const readRegisterMutation = useMutation({
    mutationFn: readRegister,
    onSuccess: async (result) => {
      setOperationResult(result);

      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["device-registers", selectedDevice?.id],
        }),
        queryClient.invalidateQueries({
          queryKey: ["modbus-logs"],
        }),
      ]);
    },
    onError: (error) => {
      setOperationResult(toOperationResult(error));
    },
  });

  const writeRegisterMutation = useMutation({
    mutationFn: writeRegister,
    onSuccess: async (result) => {
      setOperationResult(result);

      await Promise.all([
        queryClient.invalidateQueries({
          queryKey: ["device-registers", selectedDevice?.id],
        }),
        queryClient.invalidateQueries({
          queryKey: ["modbus-logs"],
        }),
      ]);
    },
    onError: (error) => {
      setOperationResult(toOperationResult(error));
    },
  });

  function handleDeviceSelect(device: DeviceDto) {
    setSelectedDevice(device);
    setOperationResult(null);
  }

  function handleReadRegister(registerAddress: string) {
    if (!selectedDevice) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Сначала выбери устройство.",
      });

      return;
    }

    const address = Number(registerAddress);

    if (Number.isNaN(address)) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Адрес регистра должен быть числом.",
      });

      return;
    }

    readRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: address,
    });
  }

  function handleWriteRegister(registerAddress: string, registerValue: string) {
    if (!selectedDevice) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Сначала выбери устройство.",
      });

      return;
    }

    const address = Number(registerAddress);
    const value = Number(registerValue);

    if (Number.isNaN(address) || Number.isNaN(value)) {
      setOperationResult({
        isSuccess: false,
        status: 2,
        value: null,
        message: "Адрес регистра и значение должны быть числами.",
      });

      return;
    }

    writeRegisterMutation.mutate({
      slaveAddress: selectedDevice.slaveAddress,
      registerAddress: address,
      value,
    });
  }

  return (
    <main className="app-shell">
      <HeroHeader realtimeStatus={realtimeStatus} />

      <section className="layout">
        <DeviceList
          devices={devicesQuery.data}
          selectedDeviceId={selectedDevice?.id}
          isLoading={devicesQuery.isLoading}
          isError={devicesQuery.isError}
          onSelect={handleDeviceSelect}
        />

        <section className="panel content-panel">
          <div className="panel-header">
            <div>
              <h2>Регистры</h2>
              <p className="muted">
                {selectedDevice
                  ? `Устройство: ${selectedDevice.name}`
                  : "Выбери устройство слева"}
              </p>
            </div>
          </div>

          {selectedDevice && (
            <RegisterOperationsPanel
              operationResult={operationResult}
              isReading={readRegisterMutation.isPending}
              isWriting={writeRegisterMutation.isPending}
              onRead={handleReadRegister}
              onWrite={handleWriteRegister}
            />
          )}

          <RegisterTable
            selectedDevice={selectedDevice}
            registers={registersQuery.data}
            isLoading={registersQuery.isLoading}
            isError={registersQuery.isError}
          />
        </section>
      </section>

      <ModbusLogsTable
        logs={modbusLogsQuery.data}
        isLoading={modbusLogsQuery.isLoading}
        isError={modbusLogsQuery.isError}
        isFetching={modbusLogsQuery.isFetching}
        onRefresh={() => void modbusLogsQuery.refetch()}
      />
    </main>
  );
}

export default App;