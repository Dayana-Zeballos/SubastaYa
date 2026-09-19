import { useParams } from "react-router-dom";
import { PlaceholderPage } from "./PlaceholderPage";

export function AuctionRoomPage() {
  const { id } = useParams();

  return (
    <PlaceholderPage
      eyebrow="Sala en vivo"
      title="Subasta"
      summary={`Detalle, historial, pujas y SignalR para ${id}. JoinAuction(auctionId) y evento auctionEvent.`}
      endpoints={[
        "GET /api/auctions/{id}",
        "GET /api/auctions/{id}/bids",
        "POST /api/auctions/{id}/bids",
        "GET /api/wallet/balance",
        "Hub /hubs/auctions",
      ]}
    />
  );
}
