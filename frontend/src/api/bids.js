import { apiFetch } from "./client";

export function placeBid(auctionId, amount) {
  return apiFetch(`/api/auctions/${auctionId}/bids`, {
    method: "POST",
    body: JSON.stringify({ amount }),
  });
}
