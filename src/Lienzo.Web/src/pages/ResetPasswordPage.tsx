import { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Palette, ArrowLeft, Lock } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { api } from '@/lib/api';

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const emailParam = searchParams.get('email') || '';
  const codeParam = searchParams.get('code') || '';

  const [email] = useState(emailParam);
  const [code] = useState(codeParam);
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (newPassword !== confirmPassword) {
      setError('Las contraseñas no coinciden');
      return;
    }
    setLoading(true);
    setError('');
    try {
      await api.post('/auth/reset-password', { email, code, newPassword, confirmNewPassword: confirmPassword });
      setSuccess(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al restablecer la contraseña');
    } finally {
      setLoading(false);
    }
  };

  if (!email || !code) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-canvas p-4">
        <div className="bg-white rounded-2xl border border-primary-100 shadow-xl p-8 max-w-md w-full text-center">
          <Palette className="h-12 w-12 text-accent-500 mx-auto mb-4" />
          <h1 className="font-heading text-2xl font-bold text-primary-800 mb-2">Enlace inválido</h1>
          <p className="text-primary-500 mb-6">Faltan datos para restablecer la contraseña.</p>
          <Button onClick={() => navigate('/forgot-password')}>Solicitar nuevo código</Button>
        </div>
      </div>
    );
  }

  if (success) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-canvas p-4">
        <div className="bg-white rounded-2xl border border-primary-100 shadow-xl p-8 max-w-md w-full text-center">
          <div className="h-16 w-16 rounded-2xl bg-green-100 flex items-center justify-center mx-auto mb-4">
            <Lock className="h-8 w-8 text-green-600" />
          </div>
          <h1 className="font-heading text-2xl font-bold text-primary-800 mb-2">Contraseña restablecida</h1>
          <p className="text-primary-500 mb-6">Tu contraseña se ha actualizado correctamente.</p>
          <Button onClick={() => navigate('/login')}>Iniciar sesión</Button>
        </div>
      </div>
    );
  }

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
            <h1 className="font-heading text-3xl font-bold text-primary-800">Nueva Contraseña</h1>
            <p className="text-primary-500 text-sm mt-1">Ingresa tu nueva contraseña</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <Input
              label="Correo"
              value={email}
              disabled
            />

            <Input
              label="Código de recuperación"
              value={code}
              disabled
            />

            <Input
              type="password"
              label="Nueva contraseña"
              placeholder="••••••••"
              value={newPassword}
              onChange={(e) => setNewPassword(e.target.value)}
              required
              minLength={6}
            />

            <Input
              type="password"
              label="Confirmar contraseña"
              placeholder="••••••••"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
              minLength={6}
            />

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <Button type="submit" className="w-full h-11" loading={loading}>
              <Lock className="h-4 w-4 mr-2" />
              Restablecer contraseña
            </Button>
          </form>

          <p className="text-center text-sm text-primary-400 mt-6">
            <button
              type="button"
              className="text-accent-600 font-medium hover:text-accent-700 inline-flex items-center gap-1"
              onClick={() => navigate('/login')}
            >
              <ArrowLeft className="h-3 w-3" />
              Volver al inicio de sesión
            </button>
          </p>
        </div>
      </div>
    </div>
  );
}
