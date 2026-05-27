import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { getDevices } from "../devices/devicesApi";
import {
  changeDeviceStatus,
  createDevice,
  createDeviceType,
  createRegisterDefinition,
  getDeviceTypes,
} from "./deviceManagementApi";
import type {
  CreateDeviceRequest,
  CreateDeviceTypeRequest,
  CreateRegisterDefinitionRequest,
} from "./types";

export function useDeviceManagement() {
  const queryClient = useQueryClient();

  const deviceTypesQuery = useQuery({
    queryKey: ["device-types"],
    queryFn: getDeviceTypes,
  });

  const devicesQuery = useQuery({
    queryKey: ["devices"],
    queryFn: getDevices,
  });

  const createDeviceTypeMutation = useMutation({
    mutationFn: createDeviceType,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["device-types"] });
    },
  });

  const createDeviceMutation = useMutation({
    mutationFn: createDevice,
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["devices"] });
    },
  });

  const changeDeviceStatusMutation = useMutation({
    mutationFn: ({ deviceId, isEnabled }: { deviceId: string; isEnabled: boolean }) =>
      changeDeviceStatus(deviceId, { isEnabled }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["devices"] });
    },
  });

  const createRegisterMutation = useMutation({
    mutationFn: createRegisterDefinition,
    onSuccess: async (_, request) => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["devices"] }),
        queryClient.invalidateQueries({ queryKey: ["device-registers"] }),
        queryClient.invalidateQueries({ queryKey: ["device-registers", request.deviceTypeId] }),
      ]);
    },
  });

  return {
    deviceTypesQuery,
    devicesQuery,
    createDeviceType: (request: CreateDeviceTypeRequest) =>
      createDeviceTypeMutation.mutateAsync(request),
    creatingDeviceType: createDeviceTypeMutation.isPending,
    createDevice: (request: CreateDeviceRequest) => createDeviceMutation.mutateAsync(request),
    creatingDevice: createDeviceMutation.isPending,
    changeDeviceStatus: (deviceId: string, isEnabled: boolean) =>
      changeDeviceStatusMutation.mutateAsync({ deviceId, isEnabled }),
    changingDeviceStatus: changeDeviceStatusMutation.isPending,
    createRegisterDefinition: (request: CreateRegisterDefinitionRequest) =>
      createRegisterMutation.mutateAsync(request),
    creatingRegisterDefinition: createRegisterMutation.isPending,
  };
}
