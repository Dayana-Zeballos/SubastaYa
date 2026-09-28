import { apiFetch } from "./client";

export function searchAuctions({
  status,
  categorySlug,
  minPrice,
  maxPrice,
  search,
  sort = "EndingSoon",
  page = 1,
  pageSize = 12,
} = {}) {
  const query = new URLSearchParams();

  if (status) {
    query.set("status", status);
  }

  if (categorySlug) {
    query.set("categorySlug", categorySlug);
  }

  if (minPrice) {
    query.set("minPrice", minPrice);
  }

  if (maxPrice) {
    query.set("maxPrice", maxPrice);
  }

  if (search) {
    query.set("search", search);
  }

  query.set("sort", sort);
  query.set("page", String(page));
  query.set("pageSize", String(pageSize));

  return apiFetch(`/api/auctions?${query}`);
}

export function createAuction(payload) {
  return apiFetch("/api/auctions", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export function getAuction(id) {
  return apiFetch(`/api/auctions/${id}`);
}

export function getAuctionBids(id) {
  return apiFetch(`/api/auctions/${id}/bids`);
}
