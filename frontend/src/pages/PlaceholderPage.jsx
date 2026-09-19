export function PlaceholderPage({ eyebrow, title, summary, endpoints }) {
  return (
    <section className="stack">
      <p className="eyebrow">{eyebrow}</p>
      <h1>{title}</h1>
      <p className="lede">{summary}</p>
      <div className="panel">
        <h2>Endpoints listos</h2>
        <ul className="endpoint-list">
          {endpoints.map((item) => (
            <li key={item}>
              <code>{item}</code>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
