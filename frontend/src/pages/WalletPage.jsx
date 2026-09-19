import { useEffect, useState } from "react";
import { deposit, getBalance, getTransactions } from "../api/wallet";

const money = new Intl.NumberFormat("es-AR", {
  style: "currency",
  currency: "ARS",
  maximumFractionDigits: 2,
});

const TYPE_LABELS = {
  deposit: "Depósito",
  reserve: "Retención",
  release: "Liberación",
  capture: "Cobro",
  refund: "Reembolso",
  payout: "Acreditación",
};

export function WalletPage() {
  const [balance, setBalance] = useState(null);
  const [transactions, setTransactions] = useState([]);
  const [amount, setAmount] = useState("1000");
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);

  async function load() {
    const [nextBalance, nextTransactions] = await Promise.all([
      getBalance(),
      getTransactions(),
    ]);
    setBalance(nextBalance);
    setTransactions(nextTransactions);
  }

  useEffect(() => {
    let cancelled = false;

    setLoading(true);
    setError("");

    load()
      .catch((err) => {
        if (!cancelled) {
          setError(err.message || "No se pudo cargar la billetera.");
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
  }, []);

  async function handleDeposit(event) {
    event.preventDefault();
    setError("");
    setSuccess("");

    const parsed = Number(amount);
    if (!Number.isFinite(parsed) || parsed <= 0) {
      setError("El monto a depositar tiene que ser mayor a cero.");
      return;
    }

    setSubmitting(true);

    try {
      const nextBalance = await deposit(parsed);
      setBalance(nextBalance);
      setTransactions(await getTransactions());
      setSuccess(`Se acreditaron ${money.format(parsed)} al disponible.`);
      setAmount("1000");
    } catch (err) {
      setError(err.message || "No se pudo depositar.");
    } finally {
      setSubmitting(false);
    }
  }

  if (loading) {
    return <p className="muted">Cargando billetera…</p>;
  }

  return (
    <section className="stack">
      <p className="eyebrow">Cuenta</p>
      <h1>Billetera</h1>
      <p className="lede">
        El dueño sale del token. Total es disponible más retenido por ofertas en curso.
      </p>

      {balance ? (
        <div className="balance-grid">
          <article className="panel balance-card">
            <p className="eyebrow">Total</p>
            <p className="balance-amount">{money.format(balance.total)}</p>
          </article>
          <article className="panel balance-card">
            <p className="eyebrow">Disponible</p>
            <p className="balance-amount">{money.format(balance.available)}</p>
          </article>
          <article className="panel balance-card">
            <p className="eyebrow">Retenido</p>
            <p className="balance-amount">{money.format(balance.reserved)}</p>
          </article>
        </div>
      ) : null}

      <section className="panel">
        <h2>Depositar</h2>
        <form className="form" onSubmit={handleDeposit}>
          <label>
            Monto
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={amount}
              onChange={(event) => setAmount(event.target.value)}
              disabled={submitting}
              required
            />
          </label>
          {error ? <p className="error">{error}</p> : null}
          {success ? <p className="lede">{success}</p> : null}
          <button type="submit" className="primary" disabled={submitting}>
            {submitting ? "Acreditando…" : "Acreditar"}
          </button>
        </form>
      </section>

      <section className="panel">
        <h2>Movimientos</h2>
        {transactions.length === 0 ? (
          <p className="muted">Todavía no hay movimientos en esta billetera.</p>
        ) : (
          <div className="ledger-wrap">
            <table className="ledger">
              <thead>
                <tr>
                  <th>Fecha</th>
                  <th>Tipo</th>
                  <th>Detalle</th>
                  <th>Monto</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((item) => (
                  <tr key={item.id}>
                    <td>{formatWhen(item.createdAt)}</td>
                    <td>{TYPE_LABELS[item.type] ?? item.type}</td>
                    <td>{item.description}</td>
                    <td>{money.format(item.amount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </section>
  );
}

function formatWhen(value) {
  return new Date(value).toLocaleString("es-AR", {
    dateStyle: "short",
    timeStyle: "short",
  });
}
