export function getFunctionCodeLabel(functionCode: number): string {
  if (functionCode === 3) {
    return "FC03 Read";
  }

  if (functionCode === 6) {
    return "FC06 Write";
  }

  return `FC${functionCode}`;
}

export function getStatusLabel(status: number): string {
  if (status === 0) {
    return "Success";
  }

  if (status === 1) {
    return "Failed";
  }

  if (status === 2) {
    return "Rejected";
  }

  return "Unknown";
}

export function getStatusClass(status: number): string {
  if (status === 0) {
    return "log-status success";
  }

  if (status === 1) {
    return "log-status failed";
  }

  if (status === 2) {
    return "log-status rejected";
  }

  return "log-status";
}

export function formatTimestamp(timestampUtc: string): string {
  return new Date(timestampUtc).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
}