import { useEffect, useRef, useState } from "react";

// Arranca desde secondsRemaining del servidor y cuenta hacia atrás con el
// reloj local, para no desfasar si la PC del usuario no está en UTC.
export function useSecondsRemaining(initialSeconds) {
  const deadline = useRef(Date.now() + Math.max(0, initialSeconds) * 1000);
  const [remaining, setRemaining] = useState(Math.max(0, initialSeconds));

  useEffect(() => {
    const start = Math.max(0, Number(initialSeconds) || 0);
    deadline.current = Date.now() + start * 1000;
    setRemaining(start);

    const id = setInterval(() => {
      setRemaining(Math.max(0, Math.ceil((deadline.current - Date.now()) / 1000)));
    }, 1000);

    return () => clearInterval(id);
  }, [initialSeconds]);

  return remaining;
}
