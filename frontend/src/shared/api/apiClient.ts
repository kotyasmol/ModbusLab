const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5199";
const ACCESS_TOKEN_KEY = "modbuslab.accessToken";

let unauthorizedHandler: (() => void) | null = null;

export function getAccessToken(): string | null {
  return localStorage.getItem(ACCESS_TOKEN_KEY);
}

export function setAccessToken(token: string): void {
  localStorage.setItem(ACCESS_TOKEN_KEY, token);
}

export function clearAccessToken(): void {
  localStorage.removeItem(ACCESS_TOKEN_KEY);
}

export function setUnauthorizedHandler(handler: (() => void) | null): void {
  unauthorizedHandler = handler;
}

async function handleResponse<T>(response: Response): Promise<T> {
  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  if (response.status === 401) {
    clearAccessToken();
    unauthorizedHandler?.();
    throw new Error("Unauthorized");
  }

  if (!response.ok) {
    throw data ?? new Error(`API request failed: ${response.status}`);
  }

  return data as T;
}

function createHeaders(hasBody: boolean): HeadersInit {
  const headers: Record<string, string> = {};
  const token = getAccessToken();

  if (hasBody) {
    headers["Content-Type"] = "application/json";
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  return headers;
}

export async function apiGet<T>(url: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    headers: createHeaders(false),
  });

  return handleResponse<T>(response);
}

export async function apiPost<TRequest, TResponse>(
  url: string,
  body: TRequest
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: "POST",
    headers: createHeaders(true),
    body: JSON.stringify(body),
  });

  return handleResponse<TResponse>(response);
}
