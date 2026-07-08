import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate, useLocation } from 'react-router-dom';
import { Palette } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { useLogin } from '@/hooks/useAuth';

const loginSchema = z.object({
  email: z.string().email('Correo electrónico inválido'),
  password: z.string().min(6, 'La contraseña debe tener al menos 6 caracteres'),
});

type LoginFormData = z.infer<typeof loginSchema>;

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const loginMutation = useLogin();
  const from = (location.state as { from?: { pathname: string } })?.from?.pathname || '/';

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      await loginMutation.mutateAsync(data);
      navigate(from, { replace: true });
    } catch {
      // error handled via mutation state
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-canvas p-4">
      <div
        className="absolute inset-0 opacity-[0.03] pointer-events-none"
        style={{
          backgroundImage: `url("data:image/svg+xml,%3Csvg width='60' height='60' viewBox='0 0 60 60' xmlns='http://www.w3.org/2000/svg'%3E%3Cg fill='none' fill-rule='evenodd'%3E%3Cg fill='%231e2d4a' fill-opacity='1'%3E%3Cpath d='M36 34v-4h-2v4h-4v2h4v4h2v-4h4v-2h-4zm0-30V0h-2v4h-4v2h4v4h2V6h4V4h-4zM6 34v-4H4v4H0v2h4v4h2v-4h4v-2H6zM6 4V0H4v4H0v2h4v4h2V6h4V4H6z'/%3E%3C/g%3E%3C/g%3E%3C/svg%3E")`,
        }}
      />

      <div className="relative w-full max-w-md animate-slide-up">
        <div className="bg-white rounded-2xl border border-primary-100 shadow-xl p-8">
          <div className="flex flex-col items-center mb-8">
            <div className="h-16 w-16 rounded-2xl bg-primary-800 flex items-center justify-center mb-4">
              <Palette className="h-8 w-8 text-accent-500" />
            </div>
            <h1 className="font-heading text-3xl font-bold text-primary-800">Lienzo</h1>
            <p className="text-primary-500 text-sm mt-1">Sistema de Reservación de Aulas</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              type="email"
              label="Correo electrónico"
              placeholder="tu@correo.com"
              error={errors.email?.message}
              {...register('email')}
            />

            <Input
              type="password"
              label="Contraseña"
              placeholder="••••••••"
              error={errors.password?.message}
              {...register('password')}
            />

            {loginMutation.isError && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {loginMutation.error instanceof Error
                  ? loginMutation.error.message
                  : 'Credenciales inválidas'}
              </div>
            )}

            <Button
              type="submit"
              className="w-full h-11"
              loading={loginMutation.isPending}
            >
              Iniciar sesión
            </Button>
          </form>

          <div className="text-center text-sm mt-4">
            <button
              type="button"
              className="text-primary-400 hover:text-accent-600"
              onClick={() => navigate('/forgot-password')}
            >
              ¿Olvidaste tu contraseña? o eres usuario nuevo
            </button>
          </div>


        </div>
      </div>
    </div>
  );
}
