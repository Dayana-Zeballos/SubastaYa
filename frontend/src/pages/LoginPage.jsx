import { useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const DEMO_USERS = [
  { email: "vendedor@test.com", label: "Vendedor" },
  { email: "comprador1@test.com", label: "Comprador 1" },
  { email: "comprador2@test.com", label: "Comprador 2" },
];

const DEMO_PASSWORD = "Subasta2026!";

export function LoginPage() {
  const { isAuthenticated, ready, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState("comprador2@test.com");
  const [password, setPassword] = useState(DEMO_PASSWORD);
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  if (ready && isAuthenticated) {
    return <Navigate to={location.state?.from ?? "/"} replace />;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setSubmitting(true);

    try {
      await login(email, password);
      navigate(location.state?.from ?? "/", { replace: true });
    } catch (err) {
      setError(err.message || "No se pudo ingresar.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="auth-card">
      <p className="eyebrow">Sesión</p>
      <h1>Ingresar</h1>
      <p className="lede">
        El token se guarda en el navegador y todas las peticiones lo mandan en el header.
        El id del usuario nunca viaja por body ni por URL.
      </p>
      <form className="form" onSubmit={handleSubmit}>
        <label>
          Email
          <input
            type="email"
            value={email}
            autoComplete="username"
            onChange={(event) => setEmail(event.target.value)}
            required
          />
        </label>
        <label>
          Contraseña
          <input
            type="password"
            value={password}
            autoComplete="current-password"
            onChange={(event) => setPassword(event.target.value)}
            required
          />
        </label>
        {error ? <p className="error">{error}</p> : null}
        <button type="submit" className="primary" disabled={submitting}>
          {submitting ? "Ingresando…" : "Entrar"}
        </button>
      </form>
      <div className="chips">
        {DEMO_USERS.map((demo) => (
          <button
            key={demo.email}
            type="button"
            className="chip"
            onClick={() => {
              setEmail(demo.email);
              setPassword(DEMO_PASSWORD);
            }}
          >
            {demo.label}
          </button>
        ))}
      </div>
      <p className="muted">
        Usuarios de prueba del seeder. Contraseña: <code>{DEMO_PASSWORD}</code>
      </p>
      <p className="muted">
        ¿No tenés cuenta? <Link to="/registro">Crear una</Link>
      </p>
    </section>
  );
}
