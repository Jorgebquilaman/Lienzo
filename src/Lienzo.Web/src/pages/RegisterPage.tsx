import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useNavigate } from 'react-router-dom';
import { Palette } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { useRegister } from '@/hooks/useAuth';
import type { UserRole } from '@/types';

const registerSchema = z.object({
  email: z.string().email('Correo electrónico inválido'),
  password: z.string().min(6, 'La contraseña debe tener al menos 6 caracteres'),
  confirmPassword: z.string(),
  firstName: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
  lastName: z.string().min(2, 'El apellido debe tener al menos 2 caracteres'),
  role: z.string().min(1, 'Selecciona un rol'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Las contraseñas no coinciden',
  path: ['confirmPassword'],
});

type RegisterFormData = z.infer<typeof registerSchema>;

export default function RegisterPage() {
  const navigate = useNavigate();
  const registerMutation = useRegister();

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      email: '',
      password: '',
      confirmPassword: '',
      firstName: '',
      lastName: '',
      role: 'Student',
    },
  });

  const onSubmit = async (data: RegisterFormData) => {
    try {
      await registerMutation.mutateAsync({
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
        role: data.role as UserRole,
      });
      navigate('/', { replace: true });
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
            <h1 className="font-heading text-3xl font-bold text-primary-800">Crear Cuenta</h1>
            <p className="text-primary-500 text-sm mt-1">Regístrate en el sistema Lienzo</p>
          </div>

          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <Input
                label="Nombre"
                placeholder="Tu nombre"
                error={errors.firstName?.message}
                {...register('firstName')}
              />
              <Input
                label="Apellido"
                placeholder="Tu apellido"
                error={errors.lastName?.message}
                {...register('lastName')}
              />
            </div>

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

            <Input
              type="password"
              label="Confirmar contraseña"
              placeholder="••••••••"
              error={errors.confirmPassword?.message}
              {...register('confirmPassword')}
            />

            <Select
              label="Rol"
              placeholder="Selecciona un rol"
              error={errors.role?.message}
              value={watch('role')}
              onValueChange={(v) => setValue('role', v)}
              options={[
                { value: 'Student', label: 'Estudiante' },
                { value: 'Teacher', label: 'Profesor' },
              ]}
            />

            {registerMutation.isError && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {registerMutation.error instanceof Error
                  ? registerMutation.error.message
                  : 'Error al registrar'}
              </div>
            )}

            <Button
              type="submit"
              className="w-full h-11"
              loading={registerMutation.isPending}
            >
              Crear cuenta
            </Button>
          </form>

          <p className="text-center text-sm text-primary-400 mt-6">
            ¿Ya tienes cuenta?{' '}
            <button
              type="button"
              className="text-accent-600 font-medium hover:text-accent-700"
              onClick={() => navigate('/login')}
            >
              Inicia sesión
            </button>
          </p>
        </div>
      </div>
    </div>
  );
}
