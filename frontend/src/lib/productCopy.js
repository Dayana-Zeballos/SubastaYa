// Saca muletillas de prueba para que en pantalla se lea el producto, no el test.
export function productName(title = "") {
  const cleaned = title
    .replace(/\s+de prueba\b.*$/i, "")
    .replace(/\s+para el cat[aá]logo\b.*$/i, "")
    .replace(/\s+para defensa oral\b.*$/i, "")
    .trim();

  return cleaned || title;
}

export function productBlurb(description = "") {
  const cleaned = description
    .replace(/,?\s*lista para subastar[^.]*/gi, "")
    .replace(/,?\s*desde el formulario[^.]*/gi, "")
    .replace(/Publicada para[^.]*\.?/gi, "")
    .replace(/\s{2,}/g, " ")
    .replace(/\s+\./g, ".")
    .trim();

  if (!cleaned || /prueba|cat[aá]logo|contador|formulario de publicar|recorrer el flujo/i.test(cleaned)) {
    return "";
  }

  return cleaned;
}
