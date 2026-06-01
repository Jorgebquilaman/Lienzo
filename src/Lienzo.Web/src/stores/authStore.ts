import { create } from 'zustand';
import type { User, LoginRequest, RegisterRequest, AuthResponse } from '@/types';
import { api } from '@/lib/api';

interface AuthState {
  user: User | null;
  token: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;

  login: (email: string, password: string) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;
  checkAuth: () => Promise<void>;
  setUser: (user: User) => void;
  refreshAuthToken: () => Promise<void>;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  token: localStorage.getItem('lienzo_token'),
  refreshToken: localStorage.getItem('lienzo_refresh'),
  isAuthenticated: !!localStorage.getItem('lienzo_token'),
  isLoading: false,

  login: async (email: string, password: string) => {
    set({ isLoading: true });
    try {
      const data = await api.post<AuthResponse>('/auth/login', { email, password });
      localStorage.setItem('lienzo_token', data.token);
      localStorage.setItem('lienzo_refresh', data.refreshToken);
      set({
        user: data.user,
        token: data.token,
        refreshToken: data.refreshToken,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error) {
      set({ isLoading: false });
      throw error;
    }
  },

  register: async (registerData: RegisterRequest) => {
    set({ isLoading: true });
    try {
      const data = await api.post<AuthResponse>('/auth/register', registerData);
      localStorage.setItem('lienzo_token', data.token);
      localStorage.setItem('lienzo_refresh', data.refreshToken);
      set({
        user: data.user,
        token: data.token,
        refreshToken: data.refreshToken,
        isAuthenticated: true,
        isLoading: false,
      });
    } catch (error) {
      set({ isLoading: false });
      throw error;
    }
  },

  logout: () => {
    localStorage.removeItem('lienzo_token');
    localStorage.removeItem('lienzo_refresh');
    set({
      user: null,
      token: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,
    });
  },

  checkAuth: async () => {
    const token = get().token;
    if (!token) {
      get().logout();
      return;
    }
    set({ isLoading: true });
    try {
      const user = await api.get<User>('/auth/me');
      set({ user, isAuthenticated: true, isLoading: false });
    } catch {
      get().logout();
      set({ isLoading: false });
    }
  },

  setUser: (user: User) => {
    set({ user });
  },

  refreshAuthToken: async () => {
    const currentRefresh = get().refreshToken;
    if (!currentRefresh) {
      get().logout();
      return;
    }
    try {
      const data = await api.post<AuthResponse>('/auth/refresh', {
        refreshToken: currentRefresh,
      });
      localStorage.setItem('lienzo_token', data.token);
      localStorage.setItem('lienzo_refresh', data.refreshToken);
      set({
        token: data.token,
        refreshToken: data.refreshToken,
        user: data.user,
        isAuthenticated: true,
      });
    } catch {
      get().logout();
    }
  },
}));
