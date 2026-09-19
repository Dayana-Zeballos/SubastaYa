import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

export function ProtectedRoute({ children }) {
  const { ready, isAuthenticated } = useAuth();
  const location = useLocation();

  if (!ready) {
    return <p className="muted">Cargando sesión…</p>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/ingresar" replace state={{ from: location.pathname }} />;
  }

  return children;
}
