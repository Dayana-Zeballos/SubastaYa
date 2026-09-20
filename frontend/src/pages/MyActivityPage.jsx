import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getMyAuctions, getMyBids, getMyPurchases } from "../api/me";

const money = new Intl.NumberFormat("es-AR", {
  style: "currency",
  currency: "ARS",
  maximumFractionDigits: 2,
});

const TABS = [
  {
    id: "auctions",
    label: "Publicaciones",
    empty: "Todavía no publicaste ninguna subasta.",
    load: getMyAuctions,
  },
  {
    id: "bids",
    label: "Pujas",
    empty: "Todavía no ofertaste en ninguna subasta.",
    load: getMyBids,
  },
  {
    id: "purchases",
    label: "Compras",
    empty: "Todavía no ganaste ninguna subasta.",
    load: getMyPurchases,
  },
];

const STATUS_LABELS = {
  active: "Activa",
  scheduled: "Programada",
  closing: "Cerrando",
  finished: "Finalizada",
  deserted: "Desierta",
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
      <p className="eyebrow">Cuenta</p>
      <h1>Mi actividad</h1>
      <p className="lede">
        Publicaciones, ofertas y compras del usuario del token. Desde cada fila se entra a la sala.
      </p>

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
      {loading ? <p className="muted">Cargando {current.label.toLowerCase()}…</p> : null}

      {!loading && result ? (
        result.items.length === 0 ? (
          <p className="muted">{current.empty}</p>
        ) : (
          <div className="stack">
            <ul className="activity-list">
              {result.items.map((auction) => (
                <li key={auction.id}>
                  <Link className="activity-row" to={`/subastas/${auction.id}`}>
                    <span>
                      <strong>{auction.title}</strong>
                      <span className="muted">
                        {" "}
                        · {auction.categoryName} · {STATUS_LABELS[auction.status] ?? auction.status}
                        {auction.status === "active" && auction.secondsRemaining > 0
                          ? ` · ${formatRemaining(auction.secondsRemaining)}`
                          : ""}
                      </span>
                    </span>
                    <span className="activity-price">
                      {money.format(auction.currentPrice)}
                      <span className="muted"> · {auction.bidCount} pujas</span>
                    </span>
                  </Link>
                </li>
              ))}
            </ul>
            {result.hasNextPage ? (
              <button type="button" className="primary" onClick={loadMore}>
                Cargar más
              </button>
            ) : null}
          </div>
        )
      ) : null}
    </section>
  );
}

function formatRemaining(seconds) {
  const minutes = Math.ceil(seconds / 60);
  return minutes <= 1 ? "cierra en 1 min" : `cierra en ${minutes} min`;
}
