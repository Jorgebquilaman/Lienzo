import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import type { User, LoginRequest, RegisterRequest } from '@/types';

export function useLogin() {
  const loginStore = useAuthStore((s) => s.login);
  return useMutation({
    mutationFn: (data: LoginRequest) => loginStore(data.email, data.password),
  });
}

export function useRegister() {
  const registerStore = useAuthStore((s) => s.register);
  return useMutation({
    mutationFn: (data: RegisterRequest) => registerStore(data),
  });
}

export function useLogout() {
  const logout = useAuthStore((s) => s.logout);
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async () => {
      try {
        await api.post('/auth/logout');
      } catch {
        // ignore
      }
    },
    onSuccess: () => {
      logout();
      queryClient.clear();
    },
    onError: () => {
      logout();
      queryClient.clear();
    },
  });
}

export function useCurrentUser() {
  const token = useAuthStore((s) => s.token);
  return useQuery({
    queryKey: ['currentUser'],
    queryFn: () => api.get<User>('/auth/me'),
    enabled: !!token,
    retry: false,
    staleTime: 5 * 60 * 1000,
  });
}
