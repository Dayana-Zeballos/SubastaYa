import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { getMe, login as loginRequest, register as registerRequest } from "../api/auth";
import { getToken, setToken } from "../api/client";

const AuthContext = createContext(null);

function userFromAuth(response) {
  return {
    userId: response.userId,
    userName: response.userName,
    email: response.email,
  };
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [ready, setReady] = useState(false);

  useEffect(() => {
    if (!getToken()) {
      setReady(true);
      return;
    }

    getMe()
      .then((me) => setUser(me))
      .catch(() => {
        setToken(null);
        setUser(null);
      })
      .finally(() => setReady(true));
  }, []);

  const value = useMemo(
    () => ({
      user,
      ready,
      isAuthenticated: Boolean(user),
      async login(email, password) {
        const response = await loginRequest(email, password);
        setToken(response.token);
        setUser(userFromAuth(response));
        return response;
      },
      async register(userName, email, password) {
        const response = await registerRequest(userName, email, password);
        setToken(response.token);
        setUser(userFromAuth(response));
        return response;
      },
      logout() {
        setToken(null);
        setUser(null);
      },
    }),
    [user, ready],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth tiene que usarse dentro de AuthProvider.");
  }

  return context;
}
