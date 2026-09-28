import { apiFetch } from "./client";

function mineQuery(page) {
  return `page=${page}&pageSize=12`;
}

export function getMyAuctions(page = 1) {
  return apiFetch(`/api/me/auctions?${mineQuery(page)}`);
}

export function getMyBids(page = 1) {
  return apiFetch(`/api/me/bids?${mineQuery(page)}`);
}

export function getMyPurchases(page = 1) {
  return apiFetch(`/api/me/purchases?${mineQuery(page)}`);
}
