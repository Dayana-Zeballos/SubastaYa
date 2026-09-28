import { Link } from "react-router-dom";

export function HowItWorksPage() {
  return (
    <section className="how stack">
      <h1>Cómo funciona</h1>
      <p className="lede">
        Publicás un lote, alguien ofrece más y la plata queda apartada hasta que cierra. Sin vueltas.
      </p>

      <div className="how-steps">
        <article className="panel">
          <p className="how-num">01</p>
          <h2>Publicás</h2>
          <p className="muted">
            Ponés qué es, hasta cuándo y de a cuánto suben las ofertas. Si no marcás un inicio,
            arranca ahora. El lote se ve en el catálogo y cualquiera puede entrar a mirarlo.
          </p>
        </article>
        <article className="panel">
          <p className="how-num">02</p>
          <h2>Ofertás</h2>
          <p className="muted">
            Entrás a la sala, ves cuánto va y ofrecés un poco más. No se puede ofertar en un lote
            propio. Si te superan, te avisamos ahí mismo.
          </p>
        </article>
        <article className="panel">
          <p className="how-num">03</p>
          <h2>La plata queda apartada</h2>
          <p className="muted">
            Cuando vas ganando, ese monto se retiene en tu billetera. Si alguien te pasa, se te
            libera. Cuando el reloj llega a cero, le llega al que publicó.
          </p>
        </article>
      </div>

      <div className="panel">
        <h2>Si ofertás al final</h2>
        <p className="muted">
          Si entra una oferta cuando está por cerrar, el lote se estira un par de minutos. Así el
          resto puede responder y no se lo lleva el último segundo.
        </p>
      </div>

      <div className="panel">
        <h2>Si nadie ofrece</h2>
        <p className="muted">
          El lote cierra sin ganador. No se mueve un peso. El que publicó puede volver a intentarlo
          cuando quiera.
        </p>
      </div>

      <div className="how-actions">
        <Link to="/" className="primary">
          Ver el catálogo
        </Link>
        <Link to="/publicar" className="chip">
          Publicar un lote
        </Link>
      </div>
    </section>
  );
}
