import { useEffect, useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Palette, ClipboardCheck, CheckCircle2 } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { api } from '@/lib/api';

interface AccessoryConfirmation {
  token: string;
  classroomName: string;
  date: string;
  startTime: string;
  endTime: string;
  title: string;
  alreadyConfirmed: boolean;
  accessories: { name: string; origin: string }[];
}

export default function ConfirmAccessoriesPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token') || '';

  const [data, setData] = useState<AccessoryConfirmation | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selected, setSelected] = useState<string[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [success, setSuccess] = useState(false);
  const [loaded, setLoaded] = useState(false);

  useEffect(() => {
    if (!token) return;
    api.get<AccessoryConfirmation>(`/accessory-confirmation?token=${encodeURIComponent(token)}`)
      .then((d) => {
        setData(d);
        setSelected([]);
        setSuccess(d.alreadyConfirmed);
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Enlace inválido o expirado.'))
      .finally(() => { setLoading(false); setLoaded(true); });
  }, [token]);

  const toggle = (name: string) => {
    setSelected((prev) => prev.includes(name) ? prev.filter((n) => n !== name) : [...prev, name]);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError('');
    try {
      await api.post('/accessory-confirmation', { token, requestedAccessories: selected });
      setSuccess(true);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Error al confirmar los accesorios');
    } finally {
      setSubmitting(false);
    }
  };

  const fmtDate = (iso: string) =>
    new Date(iso).toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' });

  return (
    <div className="min-h-screen flex items-center justify-center bg-canvas p-4">
      <div className="relative w-full max-w-lg animate-slide-up">
        <div className="bg-white rounded-2xl border border-primary-100 shadow-xl p-8">
          {loading ? (
            <div className="flex flex-col items-center py-8">
              <div className="h-8 w-8 border-2 border-primary-200 border-t-accent-500 rounded-full animate-spin mb-4" />
              <p className="text-primary-500 text-sm">Cargando... </p>
            </div>
          ) : error && !data ? (
            <div className="text-center">
              <div className="h-16 w-16 rounded-2xl bg-red-100 flex items-center justify-center mx-auto mb-4">
                <ClipboardCheck className="h-8 w-8 text-red-600" />
              </div>
              <h1 className="font-heading text-2xl font-bold text-primary-800 mb-2">Enlace inválido</h1>
              <p className="text-primary-500 mb-6">{error}</p>
              <Button onClick={() => navigate('/login')}>Volver a Lienzo</Button>
            </div>
          ) : success ? (
            <div className="text-center">
              <div className="h-16 w-16 rounded-2xl bg-green-100 flex items-center justify-center mx-auto mb-4">
                <CheckCircle2 className="h-8 w-8 text-green-600" />
              </div>
              <h1 className="font-heading text-2xl font-bold text-primary-800 mb-2">
                {data?.alreadyConfirmed ? 'Ya confirmado' : 'Accesorios confirmados'}
              </h1>
              <p className="text-primary-500 mb-6">
                {data?.alreadyConfirmed
                  ? 'Tu confirmación de accesorios ya fue registrada. La reserva quedó habilitada para su aprobación.'
                  : 'Tu confirmación de accesorios fue registrada. La reserva quedó habilitada para su aprobación.'}
              </p>
              <Button onClick={() => navigate('/login')}>Volver a Lienzo</Button>
            </div>
          ) : data ? (
            <form onSubmit={handleSubmit}>
              <div className="flex flex-col items-center mb-6">
                <div className="h-16 w-16 rounded-2xl bg-primary-800 flex items-center justify-center mb-4">
                  <Palette className="h-8 w-8 text-accent-500" />
                </div>
                <h1 className="font-heading text-2xl font-bold text-primary-800 text-center">Confirmación de accesorios</h1>
                <p className="text-primary-500 text-sm mt-1 text-center">
                  Reserva: <strong>{data.title}</strong>
                </p>
              </div>

              <div className="bg-primary-50 border border-primary-100 rounded-xl p-4 mb-6">
                <p className="text-sm text-primary-700">
                  <span className="font-medium">Aula:</span> {data.classroomName}
                </p>
                <p className="text-sm text-primary-700">
                  <span className="font-medium">Fecha:</span> {fmtDate(data.date)}
                </p>
                <p className="text-sm text-primary-700">
                  <span className="font-medium">Horario:</span> {data.startTime} - {data.endTime}
                </p>
              </div>

              <p className="text-sm text-primary-600 mb-3">
                Marcá los accesorios que vas a necesitar para tu actividad:
              </p>

              <div className="space-y-2 mb-6">
                {data.accessories.map((acc) => (
                  <label
                    key={acc.name}
                    className={`flex items-center gap-3 p-3 rounded-xl border cursor-pointer transition-colors ${
                      selected.includes(acc.name)
                        ? 'border-accent-500 bg-accent-50'
                        : 'border-primary-200 hover:bg-primary-50'
                    }`}
                  >
                    <input
                      type="checkbox"
                      className="h-4 w-4 text-accent-600 rounded border-primary-300"
                      checked={selected.includes(acc.name)}
                      onChange={() => toggle(acc.name)}
                    />
                    <span className="text-sm font-medium text-primary-800">{acc.name}</span>
                  </label>
                ))}
              </div>

              {error && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700 mb-4">
                  {error}
                </div>
              )}

              <Button type="submit" className="w-full h-11" loading={submitting}>
                <ClipboardCheck className="h-4 w-4 mr-2" />
                Confirmar accesorios
              </Button>
              <p className="text-xs text-primary-400 text-center mt-3">
                Si no necesitás ningún accesorio, confirmá igualmente para habilitar la reserva.
              </p>
            </form>
          ) : (
            <div className="text-center">
              <h1 className="font-heading text-2xl font-bold text-primary-800 mb-2">Enlace inválido</h1>
              <p className="text-primary-500 mb-6">Falta el token de confirmación.</p>
              <Button onClick={() => navigate('/login')}>Volver a Lienzo</Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
