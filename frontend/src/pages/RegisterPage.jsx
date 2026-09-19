import { useState } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export function RegisterPage() {
  const { isAuthenticated, ready, register } = useAuth();
  const navigate = useNavigate();
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  if (ready && isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");
    setSubmitting(true);

    try {
      await register(userName, email, password);
      navigate("/", { replace: true });
    } catch (err) {
      setError(err.message || "No se pudo crear la cuenta.");
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <section className="auth-card">
      <p className="eyebrow">Sesión</p>
      <h1>Crear cuenta</h1>
      <form className="form" onSubmit={handleSubmit}>
        <label>
          Usuario
          <input
            value={userName}
            minLength={3}
            maxLength={64}
            autoComplete="username"
            onChange={(event) => setUserName(event.target.value)}
            required
          />
        </label>
        <label>
          Email
          <input
            type="email"
            value={email}
            autoComplete="email"
            onChange={(event) => setEmail(event.target.value)}
            required
          />
        </label>
        <label>
          Contraseña
          <input
            type="password"
            value={password}
            minLength={8}
            autoComplete="new-password"
            onChange={(event) => setPassword(event.target.value)}
            required
          />
        </label>
        {error ? <p className="error">{error}</p> : null}
        <button type="submit" className="primary" disabled={submitting}>
          {submitting ? "Creando…" : "Registrarme"}
        </button>
      </form>
      <p className="muted">
        ¿Ya tenés cuenta? <Link to="/ingresar">Ingresar</Link>
      </p>
    </section>
  );
}
