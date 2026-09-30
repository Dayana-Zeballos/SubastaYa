import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { createAuction } from "../api/auctions";
import { getCategories } from "../api/categories";
import { useToast } from "../components/ToastContext";
import { productImage } from "../lib/productImage";

function toLocalInputValue(date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60000);
  return local.toISOString().slice(0, 16);
}

function toIsoUtc(value) {
  return value ? new Date(value).toISOString() : null;
}

const defaultEndsAt = toLocalInputValue(new Date(Date.now() + 24 * 60 * 60 * 1000));

export function PublishPage() {
  const navigate = useNavigate();
  const { showToast } = useToast();
  const [categories, setCategories] = useState([]);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [categoryId, setCategoryId] = useState("");
  const [startingPrice, setStartingPrice] = useState("1000");
  const [minimumIncrement, setMinimumIncrement] = useState("100");
  const [endsAt, setEndsAt] = useState(defaultEndsAt);
  const [scheduleStart, setScheduleStart] = useState(false);
  const [startsAt, setStartsAt] = useState("");
  const [imageUrl, setImageUrl] = useState("");
  const [error, setError] = useState("");
  const [submitting, setSubmitting] = useState(false);

  const selectedCategory = categories.find((item) => item.id === categoryId);
  const suggestedImage = useMemo(
    () => productImage({ title, categorySlug: selectedCategory?.slug ?? "", imageUrl: "" }),
    [title, selectedCategory],
  );

  useEffect(() => {
    let cancelled = false;

    getCategories()
      .then((items) => {
        if (cancelled) {
          return;
        }

        setCategories(items);
        setCategoryId((current) => current || items[0]?.id || "");
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err.message || "No se pudieron cargar las categorías.");
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");

    const base = Number(startingPrice);
    const increment = Number(minimumIncrement);
    if (!(base > 0) || !(increment > 0)) {
      const message = "El precio base y el incremento tienen que ser mayores a cero.";
      setError(message);
      showToast(message, "error");
      return;
    }

    const start = scheduleStart && startsAt ? new Date(startsAt) : new Date();
    const end = endsAt ? new Date(endsAt) : null;
    if (!end || Number.isNaN(end.getTime())) {
      const message = "Poné una fecha de cierre.";
      setError(message);
      showToast(message, "error");
      return;
    }

    if (end <= start) {
      const message = scheduleStart
        ? "La fecha de cierre tiene que ser posterior a la de inicio."
        : "La fecha de cierre tiene que ser posterior a ahora.";
      setError(message);
      showToast(message, "error");
      return;
    }

    setSubmitting(true);

    const payload = {
      title: title.trim(),
      description: description.trim(),
      categoryId,
      startingPrice: Number(startingPrice),
      minimumIncrement: Number(minimumIncrement),
      endsAt: toIsoUtc(endsAt),
      imageUrl: (imageUrl || suggestedImage).trim(),
    };

    if (scheduleStart && startsAt) {
      payload.startsAt = toIsoUtc(startsAt);
    }

    try {
      const auction = await createAuction(payload);
      navigate(`/subastas/${auction.id}`, { replace: true });
    } catch (err) {
      const message = err.message || "No se pudo publicar.";
      setError(message);
      showToast(message, "error");
    } finally {
      setSubmitting(false);
    }
  }

  const preview = imageUrl || suggestedImage;

  return (
    <section className="publish stack">
      <h1>Publicar un lote</h1>
      <p className="lede">
        Si no marcás un inicio, arranca ahora. Dejale al menos cinco minutos y no más de un mes.
      </p>

      <form className="panel form" onSubmit={handleSubmit}>
        <label>
          Título
          <input
            value={title}
            minLength={5}
            maxLength={200}
            placeholder="Campera de cuero vintage"
            onChange={(event) => setTitle(event.target.value)}
            required
          />
        </label>
        <label>
          Descripción
          <textarea
            value={description}
            minLength={20}
            maxLength={2000}
            rows={5}
            onChange={(event) => setDescription(event.target.value)}
            required
          />
        </label>
        <label>
          Categoría
          <select value={categoryId} onChange={(event) => setCategoryId(event.target.value)} required>
            {categories.length === 0 ? <option value="">Cargando…</option> : null}
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </label>
        <div className="form-row">
          <label>
            Precio base
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={startingPrice}
              onChange={(event) => setStartingPrice(event.target.value)}
              required
            />
          </label>
          <label>
            De a cuánto sube
            <input
              type="number"
              min="0.01"
              step="0.01"
              value={minimumIncrement}
              onChange={(event) => setMinimumIncrement(event.target.value)}
              required
            />
          </label>
        </div>
        <label>
          Cierra
          <input
            type="datetime-local"
            value={endsAt}
            onChange={(event) => setEndsAt(event.target.value)}
            required
          />
        </label>
        <label className="check-label">
          <input
            type="checkbox"
            checked={scheduleStart}
            onChange={(event) => setScheduleStart(event.target.checked)}
          />
          Quiero que empiece más tarde
        </label>
        {scheduleStart ? (
          <label>
            Empieza
            <input
              type="datetime-local"
              value={startsAt}
              onChange={(event) => setStartsAt(event.target.value)}
              required
            />
          </label>
        ) : null}
        <label>
          Foto
          <input
            type="url"
            placeholder="Link de la foto, si tenés"
            value={imageUrl}
            onChange={(event) => setImageUrl(event.target.value)}
          />
        </label>
        {preview ? (
          <img className="publish-preview" src={preview} alt="" />
        ) : null}
        {error ? <p className="error">{error}</p> : null}
        <button type="submit" className="primary" disabled={submitting || !categoryId}>
          {submitting ? "Publicando…" : "Publicar"}
        </button>
      </form>
    </section>
  );
}
