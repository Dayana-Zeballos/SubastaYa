import { apiFetch } from "./client";

export function getBalance() {
  return apiFetch("/api/wallet/balance");
}

export function deposit(amount) {
  return apiFetch("/api/wallet/deposit", {
    method: "POST",
    body: JSON.stringify({ amount }),
  });
}

export function getTransactions() {
  return apiFetch("/api/wallet/transactions");
}
