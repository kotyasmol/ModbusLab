import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { useQueryClient } from "@tanstack/react-query";
import { getMe, login as loginRequest, register as registerRequest } from "../../features/auth/authApi";
import type { AuthUser, LoginRequest, RegisterRequest } from "../../features/auth/types";
import {
  clearAccessToken,
  getAccessToken,
  setAccessToken,
  setUnauthorizedHandler,
} from "../api/apiClient";

type AuthContextValue = {
  user: AuthUser | null;
  accessToken: string | null;
  isLoading: boolean;
  login: (request: LoginRequest) => Promise<void>;
  register: (request: RegisterRequest) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient();
  const [user, setUser] = useState<AuthUser | null>(null);
  const [accessToken, setTokenState] = useState<string | null>(() => getAccessToken());
  const [isLoading, setIsLoading] = useState(() => getAccessToken() !== null);

  const logout = useCallback(() => {
    clearAccessToken();
    setTokenState(null);
    setUser(null);
    queryClient.clear();
  }, [queryClient]);

  useEffect(() => {
    setUnauthorizedHandler(logout);
    return () => setUnauthorizedHandler(null);
  }, [logout]);

  useEffect(() => {
    if (!accessToken) {
      return;
    }

    let isMounted = true;

    getMe()
      .then((currentUser) => {
        if (isMounted) {
          setUser(currentUser);
        }
      })
      .catch(() => {
        if (isMounted) {
          logout();
        }
      })
      .finally(() => {
        if (isMounted) {
          setIsLoading(false);
        }
      });

    return () => {
      isMounted = false;
    };
  }, [accessToken, logout]);

  const login = useCallback(async (request: LoginRequest) => {
    const response = await loginRequest(request);
    setAccessToken(response.accessToken);
    setTokenState(response.accessToken);
    setUser(response.user);
  }, []);

  const register = useCallback(async (request: RegisterRequest) => {
    const response = await registerRequest(request);
    setAccessToken(response.accessToken);
    setTokenState(response.accessToken);
    setUser(response.user);
  }, []);

  const value = useMemo(
    () => ({ user, accessToken, isLoading, login, register, logout }),
    [user, accessToken, isLoading, login, register, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);

  if (value === null) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return value;
}
