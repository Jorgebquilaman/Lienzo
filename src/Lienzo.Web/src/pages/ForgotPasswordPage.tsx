import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Palette, ArrowLeft, Mail } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { api } from '@/lib/api';

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [sent, setSent] = useState(false);
  const [code, setCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      const result = await api.post<string>('/auth/forgot-password', { email });
      setCode(result);
      setSent(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al enviar solicitud');
    } finally {
      setLoading(false);
    }
  };

  const handleResetNavigate = () => {
    navigate(`/reset-password?email=${encodeURIComponent(email)}&code=${code}`);
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
            <h1 className="font-heading text-3xl font-bold text-primary-800">Recuperar Contraseña</h1>
            <p className="text-primary-500 text-sm mt-1">Ingresa tu correo para recibir un código</p>
          </div>

          <form onSubmit={handleSubmit} className="space-y-4">
            <Input
              type="email"
              label="Correo electrónico"
              placeholder="tu@correo.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />

            {error && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {error}
              </div>
            )}

            {sent && (
              <div className="bg-green-50 border border-green-200 rounded-lg p-4 text-sm text-green-700 space-y-2">
                <p className="font-medium">Código de recuperación generado:</p>
                <p className="text-2xl font-bold text-center tracking-widest">{code}</p>
                <p className="text-xs text-green-600">Este código expira en 15 minutos. En producción se enviaría por correo.</p>
              </div>
            )}

            {sent ? (
              <Button type="button" className="w-full h-11" onClick={handleResetNavigate}>
                Restablecer contraseña
              </Button>
            ) : (
              <Button type="submit" className="w-full h-11" loading={loading}>
                <Mail className="h-4 w-4 mr-2" />
                Enviar código
              </Button>
            )}
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
