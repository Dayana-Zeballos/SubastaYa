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
          setError(err.message || "No se pudo cargar.");
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
      setError("Poné un monto.");
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
          ? "Oferta enviada. Se alargó un par de minutos el cierre."
          : "Listo, vas ganando. El dinero quedó retenido.",
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
        <p className="muted">Cargando…</p>
      </section>
    );
  }

  return (
    <section className="panel">
      <p className="eyebrow">Ofertar</p>
      <h2>Hacé tu oferta</h2>
      {auction ? (
        <p className="lede">
          Ahora va {money.format(auction.currentPrice)}. Tenés que ofrecer al menos{" "}
          {money.format(minimum)}.
          {auction.isCurrentUserWinning ? " Vas ganando." : null}
        </p>
      ) : null}
      {available !== null ? (
        <p className="muted">Tenés {money.format(available)} disponibles.</p>
      ) : null}

      {!isAuthenticated ? (
        <p className="muted">
          <Link to="/ingresar" state={{ from: location.pathname }}>
            Ingresá
          </Link>{" "}
          para ofertar.
        </p>
      ) : null}
      {isSeller ? (
        <p className="error">No podés ofertar en un lote tuyo.</p>
      ) : null}
      {resolvedStatus && resolvedStatus !== "active" ? (
        <p className="muted">Esta ya no está abierta.</p>
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
          {submitting ? "Enviando…" : "Ofertar"}
        </button>
      </form>
    </section>
  );
}
