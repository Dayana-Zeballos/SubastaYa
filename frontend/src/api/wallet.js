import { apiFetch } from "./client";

export function getBalance() {
  return apiFetch("/api/wallet/balance");
}
