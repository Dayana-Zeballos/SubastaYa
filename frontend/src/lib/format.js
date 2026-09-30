const money = new Intl.NumberFormat("es-AR", {
  style: "currency",
  currency: "ARS",
  maximumFractionDigits: 0,
});

export function formatMoney(value) {
  return money.format(value ?? 0);
}

export function countdownUrgency(totalSeconds) {
  const seconds = Math.max(0, Number(totalSeconds) || 0);

  if (seconds <= 0) {
    return "";
  }

  if (seconds <= 60) {
    return "urgent";
  }

  if (seconds <= 300) {
    return "warn";
  }

  return "";
}

export function formatCountdown(totalSeconds) {
  const seconds = Math.max(0, Number(totalSeconds) || 0);

  if (seconds <= 0) {
    return "0s";
  }

  const days = Math.floor(seconds / 86400);
  const hours = Math.floor((seconds % 86400) / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const rest = seconds % 60;

  if (days > 0) {
    return `${days}d ${hours}h`;
  }

  if (hours > 0) {
    return `${hours}h ${String(minutes).padStart(2, "0")}m`;
  }

  if (minutes > 0) {
    return `${minutes}m ${String(rest).padStart(2, "0")}s`;
  }

  return `${rest}s`;
}

export function statusLabel(status) {
  switch (status) {
    case "active":
      return "Abierta";
    case "scheduled":
      return "Próxima";
    case "closing":
      return "Cerrando";
    case "finished":
      return "Cerrada";
    case "deserted":
      return "Nadie ofreció";
    case "cancelled":
      return "Cancelada";
    default:
      return status;
  }
}

export function formatDateTime(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleString("es-AR", {
    dateStyle: "short",
    timeStyle: "short",
  });
}
