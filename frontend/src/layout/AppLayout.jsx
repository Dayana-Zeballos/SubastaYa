import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

const NAV = [
  { to: "/", label: "Catálogo", end: true },
  { to: "/publicar", label: "Publicar" },
  { to: "/billetera", label: "Billetera" },
  { to: "/actividad", label: "Mi actividad" },
];

export function AppLayout() {
  const { ready, user, logout } = useAuth();

  return (
    <div className="app-shell">
      <header className="topbar">
        <NavLink to="/" className="brand">
          SubastaYa
        </NavLink>
        <nav className="nav">
          {NAV.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) => (isActive ? "nav-link active" : "nav-link")}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
        <div className="session">
          {!ready ? null : user ? (
            <>
              <span className="session-name">{user.userName}</span>
              <button type="button" className="link-button" onClick={logout}>
                Salir
              </button>
            </>
          ) : (
            <NavLink to="/ingresar" className="nav-link">
              Ingresar
            </NavLink>
          )}
        </div>
      </header>
      <main className="page">
        <Outlet />
      </main>
    </div>
  );
}
