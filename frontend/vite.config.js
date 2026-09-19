import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// En Development el proxy manda /api y /hubs a la API, así el front no hardcodea
// el origen y SignalR usa el mismo host que Vite.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: "http://localhost:5240",
        changeOrigin: true,
      },
      "/hubs": {
        target: "http://localhost:5240",
        changeOrigin: true,
        ws: true,
      },
    },
  },
});
