import { PlaceholderPage } from "./PlaceholderPage";

export function WalletPage() {
  return (
    <PlaceholderPage
      eyebrow="Cuenta"
      title="Billetera"
      summary="Saldo total, retenido y disponible. Depósito simulado y ledger de movimientos."
      endpoints={[
        "GET /api/wallet/balance",
        "POST /api/wallet/deposit",
        "GET /api/wallet/transactions",
      ]}
    />
  );
}
