import { PlaceholderPage } from "./PlaceholderPage";

export function PublishPage() {
  return (
    <PlaceholderPage
      eyebrow="Vendedor"
      title="Publicar subasta"
      summary="El vendedor sale del JWT. Sin startsAt nace active; si el inicio es a futuro, scheduled."
      endpoints={["POST /api/auctions", "GET /api/categories"]}
    />
  );
}
