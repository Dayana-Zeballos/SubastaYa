import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { getToken } from "../api/client";

// El hub es anónimo (igual que GET /api/auctions/{id}). El token va por si
// más adelante hay eventos solo para el usuario autenticado.
export function createAuctionConnection() {
  return new HubConnectionBuilder()
    .withUrl("/hubs/auctions", {
      accessTokenFactory: () => getToken() ?? "",
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}

export const AuctionRealtimeEvents = {
  BidPlaced: "bidPlaced",
  AuctionExtended: "auctionExtended",
  AuctionActivated: "auctionActivated",
  AuctionClosed: "auctionClosed",
};
