import { apiFetch } from "./client";

export function getCategories() {
  return apiFetch("/api/categories");
}
