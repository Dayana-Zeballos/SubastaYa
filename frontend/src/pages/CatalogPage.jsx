import { useEffect, useMemo, useState } from "react";
import { useSearchParams } from "react-router-dom";
import { searchAuctions } from "../api/auctions";
import { getCategories } from "../api/categories";
import { AuctionCard } from "../components/AuctionCard";

const SORTS = [
  { value: "EndingSoon", label: "Cierran antes" },
  { value: "HighestBid", label: "Las más caras" },
  { value: "LowestPrice", label: "Las más baratas" },
];

const STATUSES = [
  { value: "Active", label: "Abiertas" },
  { value: "Scheduled", label: "Próximas" },
  { value: "Finished", label: "Ya cerraron" },
  { value: "all", label: "Todas" },
];

function readFilters(params) {
  const statusParam = params.get("status");

  return {
    // Sin query: en curso. ?status=all: todas. Así el home no se llena de cerradas.
    status: statusParam === "all" ? "all" : (statusParam ?? "Active"),
    categorySlug: params.get("categorySlug") ?? "",
    minPrice: params.get("minPrice") ?? "",
    maxPrice: params.get("maxPrice") ?? "",
    search: params.get("search") ?? "",
    sort: params.get("sort") ?? "EndingSoon",
    page: Number(params.get("page") ?? "1") || 1,
  };
}

function toSearchParams(filters) {
  const next = new URLSearchParams();

  if (filters.status === "all") {
    next.set("status", "all");
  } else if (filters.status && filters.status !== "Active") {
    next.set("status", filters.status);
  }

  if (filters.categorySlug) {
    next.set("categorySlug", filters.categorySlug);
  }

  if (filters.minPrice) {
    next.set("minPrice", filters.minPrice);
  }

  if (filters.maxPrice) {
    next.set("maxPrice", filters.maxPrice);
  }

  if (filters.search) {
    next.set("search", filters.search);
  }

  if (filters.sort && filters.sort !== "EndingSoon") {
    next.set("sort", filters.sort);
  }

  if (filters.page > 1) {
    next.set("page", String(filters.page));
  }

  return next;
}

export function CatalogPage() {
  const [params, setParams] = useSearchParams();
  const filtersKey = params.toString();
  const filters = useMemo(() => readFilters(new URLSearchParams(filtersKey)), [filtersKey]);
  const [searchInput, setSearchInput] = useState(filters.search);
  const [categories, setCategories] = useState([]);
  const [result, setResult] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    setSearchInput(filters.search);
  }, [filters.search]);

  useEffect(() => {
    const timeout = setTimeout(() => {
      if (searchInput === filters.search) {
        return;
      }

      setParams(toSearchParams({ ...filters, search: searchInput, page: 1 }), { replace: true });
    }, 400);

    return () => clearTimeout(timeout);
  }, [searchInput, filters, setParams]);

  useEffect(() => {
    let cancelled = false;

    getCategories()
      .then((items) => {
        if (!cancelled) {
          setCategories(items);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setCategories([]);
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    let cancelled = false;
    setLoading(true);
    setError("");

    searchAuctions({
      ...filters,
      status: filters.status === "all" ? "" : filters.status,
    })
      .then((data) => {
        if (!cancelled) {
          setResult(data);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err.message || "No se pudo cargar el listado.");
          setResult(null);
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
  }, [filters]);

  function update(patch) {
    setParams(toSearchParams({ ...filters, ...patch }), { replace: true });
  }

  const items = result?.items ?? [];
  const totalItems = result?.totalItems ?? 0;
  const totalPages = result?.totalPages ?? 0;

  return (
    <section className="catalog stack">
      <h1>Catálogo</h1>
      <p className="lede">Lotes abiertos, los que vienen y los que ya cerraron.</p>

      <form
        className="panel filters"
        onSubmit={(event) => {
          event.preventDefault();
          update({ search: searchInput, page: 1 });
        }}
      >
        <label>
          Buscar
          <input
            type="search"
            placeholder="Qué estás buscando"
            value={searchInput}
            onChange={(event) => setSearchInput(event.target.value)}
          />
        </label>
        <label>
          Mostrar
          <select value={filters.status} onChange={(event) => update({ status: event.target.value, page: 1 })}>
            {STATUSES.map((item) => (
              <option key={item.value || "all"} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        </label>
        <label>
          Categoría
          <select
            value={filters.categorySlug}
            onChange={(event) => update({ categorySlug: event.target.value, page: 1 })}
          >
            <option value="">Todas</option>
            {categories.map((category) => (
              <option key={category.id} value={category.slug}>
                {category.name}
              </option>
            ))}
          </select>
        </label>
        <label>
          Precio mínimo
          <input
            type="number"
            min="0"
            step="1"
            value={filters.minPrice}
            onChange={(event) => update({ minPrice: event.target.value, page: 1 })}
          />
        </label>
        <label>
          Precio máximo
          <input
            type="number"
            min="0"
            step="1"
            value={filters.maxPrice}
            onChange={(event) => update({ maxPrice: event.target.value, page: 1 })}
          />
        </label>
        <label>
          Orden
          <select value={filters.sort} onChange={(event) => update({ sort: event.target.value, page: 1 })}>
            {SORTS.map((item) => (
              <option key={item.value} value={item.value}>
                {item.label}
              </option>
            ))}
          </select>
        </label>
      </form>

      {error ? <p className="error">{error}</p> : null}
      {loading ? <p className="muted">Cargando…</p> : null}

      {!loading && !error && items.length === 0 ? (
        <p className="muted">No hay nada con esa búsqueda.</p>
      ) : null}

      {!loading && items.length > 0 ? (
        <>
          <p className="muted">
            {totalItems} {totalItems === 1 ? "resultado" : "resultados"}
          </p>
          <div className="catalog-grid">
            {items.map((auction) => (
              <AuctionCard key={auction.id} auction={auction} />
            ))}
          </div>
        </>
      ) : null}

      {totalPages > 1 ? (
        <div className="pager">
          <button
            type="button"
            className="chip"
            disabled={!result?.hasPreviousPage}
            onClick={() => update({ page: filters.page - 1 })}
          >
            Anterior
          </button>
          <span className="muted">
            Página {filters.page} de {totalPages}
          </span>
          <button
            type="button"
            className="chip"
            disabled={!result?.hasNextPage}
            onClick={() => update({ page: filters.page + 1 })}
          >
            Siguiente
          </button>
        </div>
      ) : null}
    </section>
  );
}
