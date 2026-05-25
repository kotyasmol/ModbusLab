const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5199";

export async function apiGet<T>(url: string): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${url}`);

  if (!response.ok) {
    throw new Error(`API request failed: ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export async function apiPost<TRequest, TResponse>(
  url: string,
  body: TRequest
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${url}`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  const data = (await response.json()) as TResponse;

  if (!response.ok) {
    throw data;
  }

  return data;
}
