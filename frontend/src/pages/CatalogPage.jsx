import { PlaceholderPage } from "./PlaceholderPage";

export function CatalogPage() {
  return (
    <PlaceholderPage
      eyebrow="Público"
      title="Catálogo"
      summary="Listado con filtros, orden y paginación. Desde acá se entra a cada sala."
      endpoints={[
        "GET /api/auctions",
        "GET /api/auctions/{id}",
        "GET /api/categories",
      ]}
    />
  );
}
