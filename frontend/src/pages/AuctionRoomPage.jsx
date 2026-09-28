import { useCallback, useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { getAuction, getAuctionBids } from "../api/auctions";
import { useAuth } from "../auth/AuthContext";
import { BidPanel } from "../components/BidPanel";
import { useAuctionHub } from "../hooks/useAuctionHub";
import { useSecondsRemaining } from "../hooks/useSecondsRemaining";
import { productBlurb, productName } from "../lib/productCopy";
import { formatCountdown, formatDateTime, formatMoney, statusLabel } from "../lib/format";
import { categoryImage, productImage } from "../lib/productImage";

export function AuctionRoomPage() {
  const { id } = useParams();
  const { user } = useAuth();
  const [auction, setAuction] = useState(null);
  const [bids, setBids] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const remaining = useSecondsRemaining(auction?.secondsRemaining ?? 0);
  const [brokenImage, setBrokenImage] = useState(false);
  const [imageSrc, setImageSrc] = useState("");

  const reload = useCallback(() => {
    if (!id) {
      return Promise.resolve();
    }

    return Promise.all([getAuction(id), getAuctionBids(id)])
      .then(([detail, history]) => {
        setAuction(detail);
        setBids(history);
        setError("");
      })
      .catch((err) => {
        setError(err.status === 404 ? "Esa subasta no está." : err.message || "No se pudo abrir el lote.");
        setAuction(null);
      });
  }, [id]);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setBrokenImage(false);

    reload().finally(() => {
      if (!cancelled) {
        setLoading(false);
      }
    });

    return () => {
      cancelled = true;
    };
  }, [reload]);

  useAuctionHub(id, reload);

  useEffect(() => {
    if (!auction) {
      return;
    }

    setImageSrc(productImage(auction));
    setBrokenImage(false);
  }, [auction]);

  if (loading) {
    return <p className="muted">Cargando…</p>;
  }

  if (error) {
    return (
      <section className="stack">
        <p className="error">{error}</p>
        <Link to="/">Volver al catálogo</Link>
      </section>
    );
  }

  if (!auction) {
    return null;
  }

  const closed = remaining <= 0 || ["finished", "deserted", "cancelled", "closing"].includes(auction.status);
  const isSeller = Boolean(user && user.userName === auction.sellerUserName);
  const winning = isSeller ? null : auction.isCurrentUserWinning;
  const name = productName(auction.title);
  const blurb = productBlurb(auction.description);

  function handleImageError() {
    const fallback = categoryImage(auction.categorySlug);
    if (imageSrc !== fallback) {
      setImageSrc(fallback);
      return;
    }

    setBrokenImage(true);
  }

  return (
    <section className="room stack">
      {auction.status === "active" && !closed ? <p className="eyebrow">En vivo</p> : null}
      <div className="room-heading">
        <h1>{name}</h1>
        <p className="muted">
          {auction.categoryName} · {statusLabel(auction.status)} · {auction.sellerUserName}
        </p>
      </div>

      <div className="room-hero">
        {imageSrc && !brokenImage ? (
          <img src={imageSrc} alt={name} onError={handleImageError} />
        ) : (
          <div className="auction-card-fallback room-fallback" aria-hidden="true">
            {name.slice(0, 1)}
          </div>
        )}

        <div className="stack">
          <div className="panel">
            {closed ? null : <p className="eyebrow">Cierra en</p>}
            <p className="countdown">
              {closed ? statusLabel(auction.status) : formatCountdown(remaining)}
            </p>
            {winning === true ? <p className="badge badge-win">Vas ganando</p> : null}
            {winning === false ? <p className="badge">Te superaron</p> : null}
            <p className="price">{formatMoney(auction.currentPrice)}</p>
            <p className="muted">
              Salía {formatMoney(auction.startingPrice)}. La próxima oferta va de{" "}
              {formatMoney(auction.nextMinimumBid)}.
            </p>
            {auction.highestBidderAlias ? (
              <p className="muted">Va ganando {auction.highestBidderAlias}</p>
            ) : (
              <p className="muted">Todavía no hubo ofertas.</p>
            )}
          </div>
          <BidPanel
            auctionId={id}
            nextMinimumBid={auction.nextMinimumBid}
            status={auction.status}
            onPlaced={reload}
          />
        </div>
      </div>

      {blurb ? <p className="lede room-blurb">{blurb}</p> : null}

      <div className="panel">
        <h2>Ofertas</h2>
        {bids.length === 0 ? (
          <p className="muted">Nadie ofreció todavía.</p>
        ) : (
          <ol className="bid-list">
            {bids.map((bid) => (
              <li key={bid.id}>
                <span>{bid.bidderAlias}</span>
                <span>{formatMoney(bid.amount)}</span>
                <span className="muted">{formatDateTime(bid.createdAt)}</span>
              </li>
            ))}
          </ol>
        )}
      </div>
    </section>
  );
}
