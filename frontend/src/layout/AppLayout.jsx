import { NavLink, Outlet } from "react-router-dom";
import { useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { SiteFooter } from "./SiteFooter";

const NAV = [
  { to: "/", label: "Catálogo", end: true },
  { to: "/publicar", label: "Publicar" },
  { to: "/billetera", label: "Billetera" },
  { to: "/actividad", label: "Mi actividad" },
];

export function AppLayout() {
  const { ready, user, logout } = useAuth();
  const [menuOpen, setMenuOpen] = useState(false);

  function closeMenu() {
    setMenuOpen(false);
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <NavLink to="/" className="brand" onClick={closeMenu}>
          SubastaYa
        </NavLink>
        <button
          type="button"
          className="nav-toggle"
          aria-expanded={menuOpen}
          aria-label={menuOpen ? "Cerrar menú" : "Abrir menú"}
          onClick={() => setMenuOpen((open) => !open)}
        >
          <span />
          <span />
          <span />
        </button>
        <nav className={menuOpen ? "nav is-open" : "nav"}>
          {NAV.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              onClick={closeMenu}
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
            <NavLink to="/ingresar" className="nav-link" onClick={closeMenu}>
              Ingresar
            </NavLink>
          )}
        </div>
      </header>
      <main className="page">
        <Outlet />
      </main>
      <SiteFooter />
    </div>
  );
}
