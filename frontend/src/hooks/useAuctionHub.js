import { useEffect, useRef, useState } from "react";
import { createAuctionConnection } from "../realtime/auctionHub";

function payloadAuctionId(payload) {
  return payload?.auctionId ?? payload?.AuctionId ?? "";
}

// Conecta al hub, entra al grupo de la subasta y avisa cada auctionEvent.
// Si el WebSocket no arranca, dispara onEvent cada 8s para que la sala recargue por GET.
export function useAuctionHub(auctionId, onEvent) {
  const onEventRef = useRef(onEvent);
  onEventRef.current = onEvent;
  const [connected, setConnected] = useState(false);

  useEffect(() => {
    if (!auctionId) {
      return undefined;
    }

    const connection = createAuctionConnection();
    let pollId;

    const startPolling = () => {
      if (pollId) {
        return;
      }

      pollId = setInterval(() => {
        onEventRef.current({ event: "poll" });
      }, 8000);
    };

    const handleEvent = (payload) => {
      if (String(payloadAuctionId(payload)).toLowerCase() !== String(auctionId).toLowerCase()) {
        return;
      }

      onEventRef.current(payload);
    };

    connection.on("auctionEvent", handleEvent);
    connection.onreconnected(() => connection.invoke("JoinAuction", auctionId));
    connection.onclose(() => {
      setConnected(false);
      startPolling();
    });

    connection
      .start()
      .then(() => connection.invoke("JoinAuction", auctionId))
      .then(() => setConnected(true))
      .catch(() => {
        setConnected(false);
        startPolling();
      });

    return () => {
      if (pollId) {
        clearInterval(pollId);
      }

      connection.off("auctionEvent", handleEvent);
      connection.stop().catch(() => {});
      setConnected(false);
    };
  }, [auctionId]);

  return connected;
}
