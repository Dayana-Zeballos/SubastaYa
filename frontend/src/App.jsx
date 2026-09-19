import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "./auth/ProtectedRoute";
import { AppLayout } from "./layout/AppLayout";
import { AuctionRoomPage } from "./pages/AuctionRoomPage";
import { CatalogPage } from "./pages/CatalogPage";
import { LoginPage } from "./pages/LoginPage";
import { MyActivityPage } from "./pages/MyActivityPage";
import { PublishPage } from "./pages/PublishPage";
import { RegisterPage } from "./pages/RegisterPage";
import { WalletPage } from "./pages/WalletPage";

export default function App() {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route path="/" element={<CatalogPage />} />
        <Route path="/subastas/:id" element={<AuctionRoomPage />} />
        <Route path="/ingresar" element={<LoginPage />} />
        <Route path="/registro" element={<RegisterPage />} />
        <Route
          path="/publicar"
          element={
            <ProtectedRoute>
              <PublishPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/billetera"
          element={
            <ProtectedRoute>
              <WalletPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/actividad"
          element={
            <ProtectedRoute>
              <MyActivityPage />
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
