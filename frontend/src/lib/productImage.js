const BY_TITLE = [
  { test: /campera|cuero|chaqueta|jacket/i, src: "https://images.unsplash.com/photo-1551028719-00167b16eac5?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /álbum|album|figurita|mundial/i, src: "https://images.unsplash.com/photo-1579952363873-27f3bade9f55?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /gundam|bandai|figura|muñeco/i, src: "https://images.unsplash.com/photo-1558060370-d644479cb6f7?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /thinkpad|notebook|laptop|lenovo/i, src: "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /fiat|cronos|auto|vehiculo|vehículo/i, src: "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /teclado|keychron|keyboard/i, src: "https://images.unsplash.com/photo-1511467687858-23d96c32e4ae?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /monitor|ultrasharp|dell/i, src: "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /bici|bicicleta|trek|domane/i, src: "https://images.unsplash.com/photo-1571068316344-75bc76f77890?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /auricular|sony|headphone/i, src: "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=900&h=600&q=80" },
  { test: /vinilo|disco|jazz/i, src: "https://images.unsplash.com/photo-1461360228754-6e81c478b882?auto=format&fit=crop&w=900&h=600&q=80" },
];

const BY_CATEGORY = {
  tecnologia: "https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=900&h=600&q=80",
  coleccionables: "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?auto=format&fit=crop&w=900&h=600&q=80",
  indumentaria: "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?auto=format&fit=crop&w=900&h=600&q=80",
  vehiculos: "https://images.unsplash.com/photo-1549317661-bd32c8ce0db2?auto=format&fit=crop&w=900&h=600&q=80",
};

const FALLBACK = BY_CATEGORY.tecnologia;

function looksGeneric(url) {
  return !url || /picsum\.photos/i.test(url);
}

export function categoryImage(categorySlug = "") {
  return BY_CATEGORY[categorySlug] || FALLBACK;
}

export function productImage({ title = "", categorySlug = "", imageUrl = "" } = {}) {
  const match = BY_TITLE.find((item) => item.test.test(title));
  if (match) {
    return match.src;
  }

  if (!looksGeneric(imageUrl)) {
    return imageUrl;
  }

  return categoryImage(categorySlug);
}
