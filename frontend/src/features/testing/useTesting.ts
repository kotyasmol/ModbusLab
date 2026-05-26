import { useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  addTestStep,
  createTestProfile,
  getTestProfile,
  getTestProfiles,
  getTestRuns,
  runTestProfile,
} from "./testingApi";
import type { CreateTestStepRequest, TestRunDto } from "./types";

function toNullableNumber(value: string): number | null {
  if (value.trim().length === 0) return null;
  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
}

export function useTesting() {
  const queryClient = useQueryClient();
  const [selectedProfileId, setSelectedProfileId] = useState<string | null>(null);
  const [newProfileName, setNewProfileName] = useState("Новый тестовый профиль");
  const [newProfileDescription, setNewProfileDescription] = useState("");
  const [stepType, setStepType] = useState("CheckRegisterRange");
  const [stepName, setStepName] = useState("Проверить регистр");
  const [stepSlaveAddress, setStepSlaveAddress] = useState("1");
  const [stepRegisterAddress, setStepRegisterAddress] = useState("1305");
  const [stepValue, setStepValue] = useState("1");
  const [stepMinValue, setStepMinValue] = useState("11700");
  const [stepMaxValue, setStepMaxValue] = useState("12300");
  const [stepDelayMs, setStepDelayMs] = useState("1000");
  const [lastRun, setLastRun] = useState<TestRunDto | null>(null);

  const testProfilesQuery = useQuery({
    queryKey: ["test-profiles"],
    queryFn: getTestProfiles,
  });
  const selectedProfileQuery = useQuery({
    queryKey: ["test-profile", selectedProfileId],
    queryFn: () => getTestProfile(selectedProfileId!),
    enabled: selectedProfileId !== null,
  });
  const testRunsQuery = useQuery({
    queryKey: ["test-runs"],
    queryFn: getTestRuns,
    refetchInterval: 7000,
  });

  const createProfileMutation = useMutation({
    mutationFn: createTestProfile,
    onSuccess: async (profile) => {
      setSelectedProfileId(profile.id);
      await queryClient.invalidateQueries({ queryKey: ["test-profiles"] });
    },
  });

  const addStepMutation = useMutation({
    mutationFn: (request: CreateTestStepRequest) => addTestStep(selectedProfileId!, request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["test-profiles"] }),
        queryClient.invalidateQueries({ queryKey: ["test-profile", selectedProfileId] }),
      ]);
    },
  });

  const runProfileMutation = useMutation({
    mutationFn: runTestProfile,
    onSuccess: async (run) => {
      setLastRun(run);
      await queryClient.invalidateQueries({ queryKey: ["test-runs"] });
    },
  });

  const handleCreateProfile = () => {
    createProfileMutation.mutate({
      name: newProfileName,
      description: newProfileDescription.trim() || null,
    });
  };

  const handleAddStep = () => {
    if (!selectedProfileId) return;

    addStepMutation.mutate({
      type: stepType,
      name: stepName,
      slaveAddress: stepType === "Delay" ? null : toNullableNumber(stepSlaveAddress),
      registerAddress: stepType === "Delay" ? null : toNullableNumber(stepRegisterAddress),
      value: stepType === "WriteRegister" ? toNullableNumber(stepValue) : null,
      minValue: stepType === "CheckRegisterRange" ? toNullableNumber(stepMinValue) : null,
      maxValue: stepType === "CheckRegisterRange" ? toNullableNumber(stepMaxValue) : null,
      delayMs: stepType === "Delay" ? toNullableNumber(stepDelayMs) : null,
    });
  };

  return {
    testProfilesQuery,
    selectedProfile: selectedProfileQuery.data,
    selectedProfileId,
    setSelectedProfileId,
    newProfileName,
    setNewProfileName,
    newProfileDescription,
    setNewProfileDescription,
    handleCreateProfile,
    creatingProfile: createProfileMutation.isPending,
    stepType,
    setStepType,
    stepName,
    setStepName,
    stepSlaveAddress,
    setStepSlaveAddress,
    stepRegisterAddress,
    setStepRegisterAddress,
    stepValue,
    setStepValue,
    stepMinValue,
    setStepMinValue,
    stepMaxValue,
    setStepMaxValue,
    stepDelayMs,
    setStepDelayMs,
    handleAddStep,
    addingStep: addStepMutation.isPending,
    runProfile: (id: string) => runProfileMutation.mutate(id),
    runPending: runProfileMutation.isPending,
    lastRun,
    testRunsQuery,
  };
}
