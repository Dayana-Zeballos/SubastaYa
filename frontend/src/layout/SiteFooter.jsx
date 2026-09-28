import { Link } from "react-router-dom";

export function SiteFooter() {
  return (
    <footer className="site-footer">
      <div className="footer-inner">
        <div>
          <p className="footer-brand">SubastaYa</p>
          <p>
            Subastas en vivo, con la plata retenida de verdad. Publicás, ofertás y el lote se
            cierra solo.
          </p>
          <Link className="footer-link" to="/como-funciona">
            Cómo funciona
          </Link>
        </div>

        <div>
          <p className="footer-title">Dónde estamos</p>
          <p>
            Av. Calchaquí 6200
            <br />
            Florencio Varela (CP 1888)
            <br />
            Buenos Aires, Argentina
          </p>
          <a
            className="footer-link"
            href="https://www.google.com/maps/search/?api=1&query=Av.+Calchaqu%C3%AD+6200+Florencio+Varela"
            target="_blank"
            rel="noreferrer"
          >
            Ver en el mapa
          </a>
        </div>

        <div>
          <p className="footer-title">Contacto</p>
          <p>
            <a className="footer-link" href="mailto:hola@subastaya.com">
              hola@subastaya.com
            </a>
          </p>
          <p>Lunes a viernes, 9 a 18 hs.</p>
        </div>

        <div>
          <p className="footer-title">Redes</p>
          <div className="footer-social">
            <a href="https://www.instagram.com/" target="_blank" rel="noreferrer">
              Instagram
            </a>
            <a href="https://www.facebook.com/" target="_blank" rel="noreferrer">
              Facebook
            </a>
            <a href="https://x.com/" target="_blank" rel="noreferrer">
              X
            </a>
          </div>
        </div>
      </div>
      <p className="footer-copy">© {new Date().getFullYear()} SubastaYa</p>
    </footer>
  );
}
