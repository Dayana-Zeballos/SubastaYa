import { PlaceholderPage } from "./PlaceholderPage";

export function MyActivityPage() {
  return (
    <PlaceholderPage
      eyebrow="Cuenta"
      title="Mi actividad"
      summary="Publicaciones, pujas y compras del usuario del token."
      endpoints={[
        "GET /api/me/auctions",
        "GET /api/me/bids",
        "GET /api/me/purchases",
      ]}
    />
  );
}
