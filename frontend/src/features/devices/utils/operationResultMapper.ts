import type { RegisterOperationResult } from "../modbusTypes";

export function toOperationResult(error: unknown): RegisterOperationResult {
  if (
    typeof error === "object" &&
    error !== null &&
    "isSuccess" in error &&
    "message" in error
  ) {
    return error as RegisterOperationResult;
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
    message: "Неизвестная ошибка при выполнении операции.",
  };
}