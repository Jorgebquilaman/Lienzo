import { useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { CheckCircle2, Clock, MapPin, Loader2, AlertTriangle } from 'lucide-react';

interface ClaseInfo {
  id: string;
  classroomName: string;
  actividadNombre: string;
  fecha: string;
  horaInicio: string;
  horaFin: string;
  estado: string;
  alumnoNombre?: string;
}

interface MarcarResult {
  success: boolean;
}

export default function AsistenciaAlumno() {
  const [searchParams] = useSearchParams();
  const claseId = searchParams.get('claseId');
  const [marcado, setMarcado] = useState(false);

  const { data: clase, isLoading, isError } = useQuery<ClaseInfo>({
    queryKey: ['clase', claseId],
    queryFn: () => api.get<ClaseInfo>(`/asistencia/${claseId}`),
    enabled: !!claseId,
  });

  const marcarMutation = useMutation<MarcarResult>({
    mutationFn: () => api.post('/asistencia/marcar', { claseId }),
    onSuccess: () => setMarcado(true),
  });

  if (!claseId) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-primary-50/50 p-4">
        <div className="bg-white rounded-xl shadow-sm border border-primary-100 p-8 max-w-md text-center">
          <AlertTriangle className="h-12 w-12 text-yellow-500 mx-auto mb-3" />
          <h2 className="font-heading text-xl font-bold text-primary-800 mb-2">Enlace inválido</h2>
          <p className="text-primary-500">El enlace no contiene un código de clase válido.</p>
        </div>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-primary-50/50">
        <Loader2 className="h-8 w-8 animate-spin text-primary-300" />
      </div>
    );
  }

  if (isError || !clase) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-primary-50/50 p-4">
        <div className="bg-white rounded-xl shadow-sm border border-primary-100 p-8 max-w-md text-center">
          <AlertTriangle className="h-12 w-12 text-red-300 mx-auto mb-3" />
          <h2 className="font-heading text-xl font-bold text-primary-800 mb-2">Error</h2>
          <p className="text-primary-500">No se pudo cargar la información de la clase.</p>
        </div>
      </div>
    );
  }

  if (clase.estado !== 'Abierta') {
    return (
      <div className="min-h-screen flex items-center justify-center bg-primary-50/50 p-4">
        <div className="bg-white rounded-xl shadow-sm border border-primary-100 p-8 max-w-md text-center">
          <AlertTriangle className="h-12 w-12 text-yellow-500 mx-auto mb-3" />
          <h2 className="font-heading text-xl font-bold text-primary-800 mb-2">Clase cerrada</h2>
          <p className="text-primary-500">La clase ya ha sido cerrada por el docente.</p>
        </div>
      </div>
    );
  }

  if (marcado) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-primary-50/50 p-4">
        <div className="bg-white rounded-xl shadow-sm border border-green-200 p-8 max-w-md text-center">
          <CheckCircle2 className="h-16 w-16 text-green-500 mx-auto mb-4" />
          <h2 className="font-heading text-2xl font-bold text-green-700 mb-2">¡Asistencia registrada!</h2>
          {clase.alumnoNombre && (
            <p className="font-semibold text-primary-800 text-sm mb-3">{clase.alumnoNombre}</p>
          )}
          <p className="text-primary-500 mb-1">{clase.actividadNombre}</p>
          <p className="text-sm text-primary-400 flex items-center justify-center gap-1">
            <MapPin className="h-3.5 w-3.5" /> {clase.classroomName}
          </p>
          <p className="text-sm text-primary-400 flex items-center justify-center gap-1 mt-1">
            <Clock className="h-3.5 w-3.5" /> {clase.horaInicio.slice(0, 5)} - {clase.horaFin.slice(0, 5)}
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-primary-50/50 p-4">
      <div className="bg-white rounded-xl shadow-sm border border-primary-100 p-8 max-w-md text-center">
        <h2 className="font-heading text-xl font-bold text-primary-800 mb-1">Confirmar asistencia</h2>
        <p className="text-primary-500 text-sm mb-6">¿Estás presente en esta clase?</p>

        <div className="bg-primary-50/50 rounded-lg p-4 mb-6 space-y-2 text-left">
          {clase.alumnoNombre && (
            <p className="font-semibold text-primary-800 text-sm text-center mb-2">{clase.alumnoNombre}</p>
          )}
          <p className="font-semibold text-primary-800">{clase.actividadNombre}</p>
          <p className="text-sm text-primary-500 flex items-center gap-1.5">
            <MapPin className="h-4 w-4" /> {clase.classroomName}
          </p>
          <p className="text-sm text-primary-500 flex items-center gap-1.5">
            <Clock className="h-4 w-4" /> {clase.horaInicio.slice(0, 5)} - {clase.horaFin.slice(0, 5)}
          </p>
        </div>

        {marcarMutation.isError && (
          <p className="text-sm text-red-500 mb-3">
            {marcarMutation.error?.message || 'Error al registrar asistencia'}
          </p>
        )}

        <div className="space-y-2">
          <Button
            variant="accent"
            className="w-full"
            size="lg"
            onClick={() => marcarMutation.mutate()}
            disabled={marcarMutation.isPending}
          >
            {marcarMutation.isPending ? (
              <><Loader2 className="h-4 w-4 mr-2 animate-spin" /> Registrando...</>
            ) : (
              'Sí, estoy presente'
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
