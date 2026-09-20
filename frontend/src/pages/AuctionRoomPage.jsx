import { useParams } from "react-router-dom";
import { BidPanel } from "../components/BidPanel";
import { PlaceholderPage } from "./PlaceholderPage";

export function AuctionRoomPage() {
  const { id } = useParams();

  return (
    <section className="stack">
      <PlaceholderPage
        eyebrow="Sala en vivo"
        title="Subasta"
        summary={`Detalle, historial y SignalR para ${id}. La consola de puja ya está abajo.`}
        endpoints={[
          "GET /api/auctions/{id}",
          "GET /api/auctions/{id}/bids",
          "Hub /hubs/auctions",
        ]}
      />
      <BidPanel auctionId={id} />
    </section>
  );
}
