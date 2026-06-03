import { useState, useMemo, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { QRCodeSVG } from 'qrcode.react';
import { ArrowLeft, CheckCircle2, XCircle, RefreshCw, Users, Clock, MapPin, QrCode, Upload, Lock, ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react';

interface AsistenciaAlumnoResponse {
  id: string;
  sgaAlumnoId: number;
  alumnoNombre: string;
  presente: boolean;
  sincronizado: boolean;
}

interface ClaseResponse {
  id: string;
  classroomName: string;
  actividadNombre: string;
  fecha: string;
  horaInicio: string;
  horaFin: string;
  estado: string;
  alumnos: AsistenciaAlumnoResponse[];
}

interface SyncResult {
  actualizados: number;
  errores: number;
  detalle: string[];
}

export default function AsistenciaDocente() {
  const { claseId } = useParams<{ claseId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [qrUrl, setQrUrl] = useState('');

  const { data: clase, isLoading } = useQuery<ClaseResponse>({
    queryKey: ['clase', claseId],
    queryFn: () => api.get<ClaseResponse>(`/asistencia/${claseId}`),
    enabled: !!claseId,
  });

  const { data: qrData } = useQuery<{ url: string }>({
    queryKey: ['qr-url', claseId],
    queryFn: () => api.get<{ url: string }>(`/asistencia/qr/${claseId}`),
    enabled: !!claseId,
  });

  type SortMode = 'nombre-asc' | 'nombre-desc' | 'presentes' | 'ausentes';
  const [sortMode, setSortMode] = useState<SortMode>('nombre-asc');

  const sortedAlumnos = useMemo(() => {
    if (!clase) return [];
    const sorted = [...clase.alumnos];
    switch (sortMode) {
      case 'nombre-asc':
        sorted.sort((a, b) => a.alumnoNombre.localeCompare(b.alumnoNombre));
        break;
      case 'nombre-desc':
        sorted.sort((a, b) => b.alumnoNombre.localeCompare(a.alumnoNombre));
        break;
      case 'presentes':
        sorted.sort((a, b) => (a.presente === b.presente ? 0 : a.presente ? -1 : 1));
        break;
      case 'ausentes':
        sorted.sort((a, b) => (a.presente === b.presente ? 0 : a.presente ? 1 : -1));
        break;
    }
    return sorted;
  }, [clase, sortMode]);

  const nextSort = () => {
    const order: SortMode[] = ['nombre-asc', 'nombre-desc', 'presentes', 'ausentes'];
    const idx = order.indexOf(sortMode);
    setSortMode(order[(idx + 1) % order.length]);
  };

  const sortLabels: Record<SortMode, string> = {
    'nombre-asc': 'A-Z',
    'nombre-desc': 'Z-A',
    'presentes': 'Presentes',
    'ausentes': 'Ausentes',
  };

  const toggleMutation = useMutation({
    mutationFn: (asistenciaId: string) =>
      api.post('/asistencia/toggle-alumno', { claseId, asistenciaId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['clase', claseId] }),
  });

  const syncMutation = useMutation<SyncResult>({
    mutationFn: () => api.post(`/asistencia/sync-sga/${claseId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['clase', claseId] }),
  });

  const cerrarMutation = useMutation({
    mutationFn: () => api.post(`/asistencia/cerrar/${claseId}`, {}),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['clase', claseId] }),
  });

  const [usuariosCreados, setUsuariosCreados] = useState(0);
  const [syncUsersDone, setSyncUsersDone] = useState(false);

  useEffect(() => {
    if (!claseId || syncUsersDone) return;
    setSyncUsersDone(true);
    api.post<number>(`/asistencia/${claseId}/sync-missing-users`, {})
      .then((created) => {
        if (created > 0) {
          setUsuariosCreados(created);
          queryClient.invalidateQueries({ queryKey: ['clase', claseId] });
        }
      })
      .catch(() => {});
  }, [claseId]);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <div className="animate-spin h-8 w-8 border-4 border-primary-200 border-t-accent-500 rounded-full" />
      </div>
    );
  }

  if (!clase) {
    return (
      <div className="text-center py-16">
        <p className="text-primary-500">Clase no encontrada</p>
        <Button variant="outline" className="mt-4" onClick={() => navigate(-1)}>Volver</Button>
      </div>
    );
  }

  const presentCount = clase.alumnos.filter(a => a.presente).length;
  const totalCount = clase.alumnos.length;

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Button variant="ghost" size="sm" onClick={() => navigate(-1)}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Tomar asistencia</h1>
          <p className="text-primary-500 text-sm">{clase.actividadNombre}</p>
          {usuariosCreados > 0 && (
            <p className="text-green-600 text-sm font-medium mt-1">
              Se crearon {usuariosCreados} usuario{usuariosCreados !== 1 ? 's' : ''} nuevo{usuariosCreados !== 1 ? 's' : ''} para que los alumnos puedan acceder
            </p>
          )}
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 space-y-4">
          <div className="bg-white rounded-xl border border-primary-100 p-4">
            <div className="flex items-center gap-4 text-sm text-primary-600 mb-4">
              <span className="flex items-center gap-1"><MapPin className="h-4 w-4" /> {clase.classroomName}</span>
              <span className="flex items-center gap-1"><Clock className="h-4 w-4" /> {clase.horaInicio.slice(0, 5)} - {clase.horaFin.slice(0, 5)}</span>
              <span className="flex items-center gap-1"><Users className="h-4 w-4" /> {presentCount}/{totalCount}</span>
            </div>

              <div className="flex items-center gap-4">
                <Badge variant={clase.estado === 'Abierta' ? 'approved' : 'default'}>
                  {clase.estado === 'Abierta' ? 'Clase abierta' : 'Cerrada'}
                </Badge>
                {clase.estado === 'Abierta' && (
                  <Button
                    variant="destructive"
                    size="sm"
                    onClick={() => cerrarMutation.mutate()}
                    disabled={cerrarMutation.isPending}
                  >
                    <Lock className="h-4 w-4 mr-1" />
                    Cerrar clase
                  </Button>
                )}
                {syncMutation.isPending && <span className="text-sm text-primary-400 animate-pulse">Sincronizando...</span>}
                {syncMutation.isSuccess && (
                  <span className="text-sm text-green-600">
                    Sincronizado: {syncMutation.data?.actualizados} actualizados
                    {syncMutation.data?.errores > 0 && `, ${syncMutation.data.errores} errores`}
                  </span>
                )}
              </div>
          </div>

          <div className="bg-white rounded-xl border border-primary-100 overflow-hidden">
            <div className="px-4 py-3 bg-primary-50/50 border-b border-primary-100 flex items-center justify-between">
              <h2 className="font-semibold text-primary-800">Alumnos ({totalCount})</h2>
              <div className="flex items-center gap-2">
                <Button variant="ghost" size="sm" onClick={nextSort} title={`Orden: ${sortLabels[sortMode]}`}>
                  <ArrowUpDown className="h-3.5 w-3.5 mr-1" />
                  {sortLabels[sortMode]}
                </Button>
                {clase.estado === 'Abierta' && (
                  <Button
                    variant="accent"
                    size="sm"
                    onClick={() => syncMutation.mutate()}
                    disabled={syncMutation.isPending}
                  >
                    <Upload className="h-4 w-4 mr-1" />
                    Sincronizar con SGA
                  </Button>
                )}
              </div>
            </div>

            <div className="divide-y divide-primary-50">
              {sortedAlumnos.map((alumno) => (
                <div key={alumno.id} className="flex items-center justify-between px-4 py-2.5 hover:bg-primary-50/30 transition-colors">
                  <span className="text-sm text-primary-700">{alumno.alumnoNombre}</span>
                  <div className="flex items-center gap-2">
                    {alumno.sincronizado && (
                      <span className="text-[10px] text-primary-400 bg-primary-50 px-1.5 py-0.5 rounded">SGA</span>
                    )}
                    {clase.estado === 'Abierta' ? (
                      <Button
                        variant={alumno.presente ? 'default' : 'outline'}
                        size="sm"
                        onClick={() => toggleMutation.mutate(alumno.id)}
                        disabled={toggleMutation.isPending}
                      >
                        {alumno.presente ? (
                          <><CheckCircle2 className="h-3.5 w-3.5 mr-1 text-green-600" /> Presente</>
                        ) : (
                          <><XCircle className="h-3.5 w-3.5 mr-1 text-primary-300" /> Ausente</>
                        )}
                      </Button>
                    ) : (
                      <Badge variant={alumno.presente ? 'success' : 'secondary'}>
                        {alumno.presente ? 'Presente' : 'Ausente'}
                      </Badge>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="space-y-4">
          <div className="bg-white rounded-xl border border-primary-100 p-4 text-center">
            <h3 className="font-semibold text-primary-800 mb-3 flex items-center justify-center gap-1.5">
              <QrCode className="h-4 w-4" /> Código QR
            </h3>
            {qrData?.url && (
              <div className="flex justify-center mb-3">
                <QRCodeSVG value={qrData.url} size={200} />
              </div>
            )}
            <p className="text-xs text-primary-400">
              Escaneá el QR con tu celular para marcar asistencia
            </p>
          </div>

          <div className="bg-white rounded-xl border border-primary-100 p-4">
            <h3 className="font-semibold text-primary-800 mb-2">Resumen</h3>
            <div className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-primary-500">Presentes</span>
                <span className="font-semibold text-green-600">{presentCount}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-primary-500">Ausentes</span>
                <span className="font-semibold text-red-500">{totalCount - presentCount}</span>
              </div>
              <div className="border-t pt-2 flex justify-between">
                <span className="text-primary-500">Total</span>
                <span className="font-semibold">{totalCount}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
