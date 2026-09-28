import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useSecondsRemaining } from "../hooks/useSecondsRemaining";
import { productBlurb, productName } from "../lib/productCopy";
import { formatCountdown, formatMoney, statusLabel } from "../lib/format";
import { categoryImage, productImage } from "../lib/productImage";

export function AuctionCard({ auction }) {
  const remaining = useSecondsRemaining(auction.secondsRemaining);
  const [brokenImage, setBrokenImage] = useState(false);
  const closed = remaining <= 0 || auction.status === "finished" || auction.status === "deserted";
  const image = productImage(auction);
  const [src, setSrc] = useState(image);
  const name = productName(auction.title);
  const blurb = productBlurb(auction.description);

  useEffect(() => {
    setSrc(image);
    setBrokenImage(false);
  }, [image]);

  function handleImageError() {
    const fallback = categoryImage(auction.categorySlug);
    if (src !== fallback) {
      setSrc(fallback);
      return;
    }

    setBrokenImage(true);
  }

  return (
    <Link to={`/subastas/${auction.id}`} className="auction-card">
      <div className="auction-card-media">
        {src && !brokenImage ? (
          <img src={src} alt={name} onError={handleImageError} />
        ) : (
          <div className="auction-card-fallback" aria-hidden="true">
            {name.slice(0, 1)}
          </div>
        )}
      </div>
      <div className="auction-card-body">
        <p className="eyebrow">
          {auction.categoryName} · {statusLabel(auction.status)}
        </p>
        <h2>{name}</h2>
        {blurb ? <p className="auction-card-blurb">{blurb}</p> : null}
        <p className="price">{formatMoney(auction.currentPrice)}</p>
        <p className="muted">
          {auction.bidCount} {auction.bidCount === 1 ? "oferta" : "ofertas"}
          {" · "}
          {closed ? "Cerrada" : `Cierra en ${formatCountdown(remaining)}`}
        </p>
      </div>
    </Link>
  );
}
