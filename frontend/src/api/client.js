export const TOKEN_KEY = "subastaya.token";

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token);
    return;
  }

  localStorage.removeItem(TOKEN_KEY);
}

export class ApiError extends Error {
  constructor(status, title, detail) {
    super(detail || title || `Error ${status}`);
    this.name = "ApiError";
    this.status = status;
    this.title = title;
    this.detail = detail;
  }
}

// Wrapper único: Bearer desde localStorage y errores ProblemDetails de la API.
export async function apiFetch(path, options = {}) {
  const headers = { ...(options.headers ?? {}) };
  const token = getToken();

  if (options.body !== undefined && !headers["Content-Type"]) {
    headers["Content-Type"] = "application/json";
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  const response = await fetch(path, { ...options, headers });

  if (response.status === 204) {
    return null;
  }

  const text = await response.text();
  const data = text ? JSON.parse(text) : null;

  if (!response.ok) {
    throw new ApiError(response.status, data?.title, data?.detail);
  }

  return data;
}
