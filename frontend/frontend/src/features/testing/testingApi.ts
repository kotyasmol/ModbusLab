import { apiGet, apiPost } from "../../shared/api/apiClient";
import type {
  CreateTestProfileRequest,
  CreateTestStepRequest,
  TestProfileDto,
  TestRunDto,
} from "./types";

export function getTestProfiles(): Promise<TestProfileDto[]> {
  return apiGet<TestProfileDto[]>("/api/test-profiles");
}

export function getTestProfile(profileId: string): Promise<TestProfileDto> {
  return apiGet<TestProfileDto>(`/api/test-profiles/${profileId}`);
}

export function createTestProfile(
  request: CreateTestProfileRequest
): Promise<TestProfileDto> {
  return apiPost<CreateTestProfileRequest, TestProfileDto>(
    "/api/test-profiles",
    request
  );
}

export function addTestStep(
  profileId: string,
  request: CreateTestStepRequest
): Promise<TestProfileDto> {
  return apiPost<CreateTestStepRequest, TestProfileDto>(
    `/api/test-profiles/${profileId}/steps`,
    request
  );
}

export function runTestProfile(profileId: string): Promise<TestRunDto> {
  return apiPost<Record<string, never>, TestRunDto>(
    `/api/test-profiles/${profileId}/run`,
    {}
  );
}

export function getTestRuns(): Promise<TestRunDto[]> {
  return apiGet<TestRunDto[]>("/api/test-runs");
}
