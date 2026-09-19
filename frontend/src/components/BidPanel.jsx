import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { apiFetch, getToken } from "../api/client";
import { placeBid } from "../api/bids";
import { getBalance } from "../api/wallet";
import { useAuth } from "../auth/AuthContext";

const money = new Intl.NumberFormat("es-AR", {
  style: "currency",
  currency: "ARS",
  maximumFractionDigits: 2,
});

export function BidPanel({ auctionId, nextMinimumBid, status, onPlaced }) {
  const { ready, isAuthenticated, user } = useAuth();
  const location = useLocation();
  const [auction, setAuction] = useState(null);
  const [available, setAvailable] = useState(null);
  const [amount, setAmount] = useState("");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError("");
      setSuccess("");

      try {
        const detail = await apiFetch(`/api/auctions/${auctionId}`);
        if (cancelled) {
          return;
        }

        setAuction(detail);
        const minimum = nextMinimumBid ?? detail.nextMinimumBid;
        setAmount(String(minimum));

        if (getToken()) {
          const wallet = await getBalance();
          if (!cancelled) {
            setAvailable(wallet.available);
          }
        } else {
          setAvailable(null);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err.message || "No se pudo cargar la consola de puja.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    if (auctionId) {
      load();
    }

    return () => {
      cancelled = true;
    };
  }, [auctionId, nextMinimumBid, isAuthenticated]);

  const resolvedStatus = status ?? auction?.status ?? "";
  const minimum = nextMinimumBid ?? auction?.nextMinimumBid;
  const isSeller = Boolean(
    user && auction?.sellerUserName && user.userName === auction.sellerUserName,
  );
  const canBid = resolvedStatus === "active" && isAuthenticated && !isSeller;

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setSuccess("");
    setSubmitting(true);

    const parsed = Number(amount);
    if (!Number.isFinite(parsed) || parsed <= 0) {
      setError("El monto de la puja tiene que ser mayor a cero.");
      setSubmitting(false);
      return;
    }

    try {
      const bid = await placeBid(auctionId, parsed);
      const wallet = await getBalance();
      setAvailable(wallet.available);

      const detail = await apiFetch(`/api/auctions/${auctionId}`);
      setAuction(detail);
      setAmount(String(detail.nextMinimumBid));

      setSuccess(
        bid.antiSnipingApplied
          ? "Oferta enviada. La subasta se extendió 2 minutos (anti-sniping)."
          : "Oferta enviada. Vas liderando y el monto quedó retenido en la billetera.",
      );
      onPlaced?.(bid, detail);
    } catch (err) {
      setError(err.message || "No se pudo enviar la oferta.");
    } finally {
      setSubmitting(false);
    }
  }

  if (!ready || loading) {
    return (
      <section className="panel">
        <p className="muted">Cargando consola de puja…</p>
      </section>
    );
  }

  return (
    <section className="panel">
      <p className="eyebrow">Ofertar</p>
      <h2>Consola de puja</h2>
      {auction ? (
        <p className="lede">
          Precio actual {money.format(auction.currentPrice)}. Mínimo admitido{" "}
          {money.format(minimum)}.
          {auction.isCurrentUserWinning ? " Estás liderando." : null}
        </p>
      ) : null}
      {available !== null ? (
        <p className="muted">Disponible en billetera: {money.format(available)}</p>
      ) : null}

      {!isAuthenticated ? (
        <p className="muted">
          <Link to="/ingresar" state={{ from: location.pathname }}>
            Ingresá
          </Link>{" "}
          para ofertar. El postor sale del token, no del formulario.
        </p>
      ) : null}
      {isSeller ? (
        <p className="error">El vendedor no puede pujar en su propia subasta.</p>
      ) : null}
      {resolvedStatus && resolvedStatus !== "active" ? (
        <p className="error">Esta subasta no está activa ({resolvedStatus}).</p>
      ) : null}

      <form className="form" onSubmit={handleSubmit}>
        <label>
          Monto
          <input
            type="number"
            min={minimum ?? 0.01}
            step="0.01"
            value={amount}
            onChange={(event) => setAmount(event.target.value)}
            disabled={!canBid || submitting}
            required
          />
        </label>
        {error ? <p className="error">{error}</p> : null}
        {success ? <p className="lede">{success}</p> : null}
        <button type="submit" className="primary" disabled={!canBid || submitting}>
          {submitting ? "Enviando…" : "Pujar"}
        </button>
      </form>
    </section>
  );
}
