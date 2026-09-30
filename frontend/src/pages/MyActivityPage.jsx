import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getMyAuctions, getMyBids, getMyPurchases } from "../api/me";
import { formatCountdown } from "../lib/format";
import { productName } from "../lib/productCopy";

const money = new Intl.NumberFormat("es-AR", {
  style: "currency",
  currency: "ARS",
  maximumFractionDigits: 2,
});

const TABS = [
  {
    id: "auctions",
    label: "Publicaciones",
    empty: "Todavía no publicaste nada.",
    load: getMyAuctions,
  },
  {
    id: "bids",
    label: "Ofertas",
    empty: "Todavía no ofertaste.",
    load: getMyBids,
  },
  {
    id: "purchases",
    label: "Compras",
    empty: "Todavía no te quedó ningún lote.",
    load: getMyPurchases,
  },
];

const STATUS_LABELS = {
  active: "Abierta",
  scheduled: "Próxima",
  closing: "Cerrando",
  finished: "Cerrada",
  deserted: "Nadie ofreció",
  cancelled: "Cancelada",
};

export function MyActivityPage() {
  const [tab, setTab] = useState("auctions");
  const [page, setPage] = useState({});
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  const current = TABS.find((item) => item.id === tab);
  const result = page[tab];

  useEffect(() => {
    let cancelled = false;

    if (page[tab]) {
      setLoading(false);
      return undefined;
    }

    setLoading(true);
    setError("");

    current
      .load(1)
      .then((data) => {
        if (!cancelled) {
          setPage((prev) => ({ ...prev, [tab]: data }));
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err.message || "No se pudo cargar tu actividad.");
        }
      })
      .finally(() => {
        if (!cancelled) {
          setLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [tab, current, page]);

  async function loadMore() {
    if (!result?.hasNextPage) {
      return;
    }

    setError("");
    const next = await current.load(result.page + 1);
    setPage((prev) => ({
      ...prev,
      [tab]: {
        ...next,
        items: [...result.items, ...next.items],
      },
    }));
  }

  return (
    <section className="stack">
      <h1>Mi actividad</h1>
      <p className="lede">Lo que publicaste, lo que ofertaste y lo que te quedó.</p>

      <div className="chips">
        {TABS.map((item) => (
          <button
            key={item.id}
            type="button"
            className={item.id === tab ? "chip chip-active" : "chip"}
            onClick={() => setTab(item.id)}
          >
            {item.label}
            {page[item.id] ? ` (${page[item.id].totalItems})` : ""}
          </button>
        ))}
      </div>

      {error ? <p className="error">{error}</p> : null}
      {loading ? <p className="muted">Cargando…</p> : null}

      {!loading && result ? (
        result.items.length === 0 ? (
          <p className="muted">{current.empty}</p>
        ) : (
          <div className="stack">
            <ul className="activity-list">
              {result.items.map((auction) => {
                const outcome = activityOutcome(tab, auction);

                return (
                <li key={auction.id}>
                  <Link className="activity-row" to={`/subastas/${auction.id}`}>
                    <span>
                      <strong>{productName(auction.title)}</strong>
                      <span className="activity-meta">
                        <span className={`activity-outcome ${outcome.tone}`}>{outcome.label}</span>
                        <span className="muted">
                          {auction.categoryName}
                          {auction.status === "active" && auction.secondsRemaining > 0
                            ? ` · cierra en ${formatCountdown(auction.secondsRemaining)}`
                            : ""}
                          {tab === "auctions" ? ` · recaudación ${money.format(auction.currentPrice)}` : ""}
                        </span>
                      </span>
                    </span>
                    <span className="activity-price">
                      {money.format(auction.currentPrice)}
                      <span className="muted">
                        {" "}
                        · {auction.bidCount} {auction.bidCount === 1 ? "oferta" : "ofertas"}
                      </span>
                    </span>
                  </Link>
                </li>
                );
              })}
            </ul>
            {result.hasNextPage ? (
              <button type="button" className="primary" onClick={loadMore}>
                Ver más
              </button>
            ) : null}
          </div>
        )
      ) : null}
    </section>
  );
}

function activityOutcome(tab, auction) {
  if (tab === "purchases") {
    return { label: "Ganaste", tone: "activity-outcome-win" };
  }

  if (tab === "auctions") {
    if (auction.status === "finished") {
      return { label: "Adjudicada", tone: "activity-outcome-win" };
    }

    if (auction.status === "deserted") {
      return { label: "Desierta", tone: "" };
    }

    if (auction.status === "scheduled") {
      return { label: "Próxima", tone: "" };
    }

    if (auction.status === "closing") {
      return { label: "Cerrando", tone: "" };
    }

    return { label: "Abierta", tone: "" };
  }

  if (auction.status === "active") {
    return auction.isCurrentUserWinning
      ? { label: "Vas ganando · sigue abierta", tone: "activity-outcome-win" }
      : { label: "Te superaron · sigue abierta", tone: "activity-outcome-out" };
  }

  if (auction.status === "finished") {
    return auction.isCurrentUserWinning
      ? { label: "Ganaste", tone: "activity-outcome-win" }
      : { label: "No ganaste", tone: "activity-outcome-out" };
  }

  if (auction.status === "deserted") {
    return { label: "Desierta", tone: "" };
  }

  return { label: STATUS_LABELS[auction.status] ?? auction.status, tone: "" };
}
