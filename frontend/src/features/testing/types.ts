export type TestStepDto = {
  id: string;
  orderIndex: number;
  name: string;
  type: string;
  slaveAddress: number | null;
  registerAddress: number | null;
  value: number | null;
  minValue: number | null;
  maxValue: number | null;
  delayMs: number | null;
};

export type TestProfileDto = {
  id: string;
  name: string;
  description: string | null;
  isEnabled: boolean;
  createdAtUtc: string;
  steps: TestStepDto[];
};

export type CreateTestProfileRequest = {
  name: string;
  description: string | null;
};

export type CreateTestStepRequest = {
  type: string;
  name: string;
  slaveAddress: number | null;
  registerAddress: number | null;
  value: number | null;
  minValue: number | null;
  maxValue: number | null;
  delayMs: number | null;
};

export type TestStepResultDto = {
  id: string;
  orderIndex: number;
  stepName: string;
  stepType: string;
  status: string;
  message: string;
  expectedValue: number | null;
  actualValue: number | null;
  startedAtUtc: string;
  finishedAtUtc: string;
};

export type TestRunDto = {
  id: string;
  testProfileId: string;
  profileName: string;
  status: string;
  startedAtUtc: string;
  finishedAtUtc: string | null;
  summary: string | null;
  steps: TestStepResultDto[];
};

export type TestRunProgressEvent = {
  testRunId: string;
  testProfileId: string;
  profileName: string;
  status: string;
  completedSteps: number;
  totalSteps: number;
  message: string;
  timestampUtc: string;
};
