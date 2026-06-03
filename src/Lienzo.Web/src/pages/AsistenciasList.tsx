import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Card, CardContent } from '@/components/ui/Card';
import { useAuthStore } from '@/stores/authStore';
import { CalendarDays, Clock, MapPin, Users, BookOpen, Search, ChevronLeft, ChevronRight } from 'lucide-react';

interface ClaseListItem {
  id: string;
  actividadNombre: string;
  classroomName: string;
  fecha: string;
  horaInicio: string;
  horaFin: string;
  estado: string;
  totalAlumnos: number;
  presentes: number;
  docenteNombre: string;
  createdAt: string;
}

interface PaginatedResponse<T> {
  value: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

function getStatusBadge(estado: string) {
  switch (estado) {
    case 'Abierta': return <Badge>Abierta</Badge>;
    case 'Cerrada': return <Badge variant="default">Cerrada</Badge>;
    default: return <Badge>{estado}</Badge>;
  }
}

export default function AsistenciasList() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const [page, setPage] = useState(1);
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const pageSize = 20;

  const { data, isLoading } = useQuery<PaginatedResponse<ClaseListItem>>({
    queryKey: ['asistencias', page, desde, hasta],
    queryFn: () => {
      const params: Record<string, string> = { page: String(page), pageSize: String(pageSize) };
      if (desde) params.desde = desde;
      if (hasta) params.hasta = hasta;
      return api.get<PaginatedResponse<ClaseListItem>>('/asistencia', params);
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl font-bold text-primary-800">Asistencias</h1>
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-3 mb-6">
            <div className="w-40">
              <Input label="Desde" type="date" value={desde} onChange={(e) => { setDesde(e.target.value); setPage(1); }} />
            </div>
            <div className="w-40">
              <Input label="Hasta" type="date" value={hasta} onChange={(e) => { setHasta(e.target.value); setPage(1); }} />
            </div>
            {(desde || hasta) && (
              <Button variant="ghost" size="sm" onClick={() => { setDesde(''); setHasta(''); setPage(1); }}>
                Limpiar
              </Button>
            )}
          </div>

          {isLoading ? (
            <p className="text-primary-400 text-sm">Cargando...</p>
          ) : !data?.value?.length ? (
            <div className="text-center py-12 text-primary-400">
              <BookOpen className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>No hay asistencias registradas</p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-primary-100">
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Actividad</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Aula</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Fecha</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Horario</th>
                      <th className="text-center py-3 px-2 font-medium text-primary-500">Alumnos</th>
                      <th className="text-center py-3 px-2 font-medium text-primary-500">Estado</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Docente</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.value.map((clase) => (
                      <tr key={clase.id}
                        className="border-b border-primary-50 hover:bg-primary-50 cursor-pointer transition-colors"
                        onClick={() => navigate(`/asistencia/${clase.id}`)}
                      >
                        <td className="py-3 px-2 font-medium text-primary-800">{clase.actividadNombre}</td>
                        <td className="py-3 px-2 text-primary-600">
                          <div className="flex items-center gap-1">
                            <MapPin className="h-3.5 w-3.5 text-primary-400" />
                            {clase.classroomName}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-primary-600">
                          <div className="flex items-center gap-1">
                            <CalendarDays className="h-3.5 w-3.5 text-primary-400" />
                            {clase.fecha}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-primary-600">
                          <div className="flex items-center gap-1">
                            <Clock className="h-3.5 w-3.5 text-primary-400" />
                            {clase.horaInicio} — {clase.horaFin}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-center">
                          <div className="flex items-center justify-center gap-1">
                            <Users className="h-3.5 w-3.5 text-primary-400" />
                            <span className={clase.presentes === clase.totalAlumnos ? 'text-green-600 font-medium' : 'text-primary-600'}>
                              {clase.presentes}/{clase.totalAlumnos}
                            </span>
                          </div>
                        </td>
                        <td className="py-3 px-2 text-center">{getStatusBadge(clase.estado)}</td>
                        <td className="py-3 px-2 text-primary-600 text-xs">{clase.docenteNombre}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {data.totalPages > 1 && (
                <div className="flex items-center justify-between pt-4 border-t border-primary-100 mt-4">
                  <p className="text-sm text-primary-500">
                    Página {data.page} de {data.totalPages} ({data.totalCount} registros)
                  </p>
                  <div className="flex gap-2">
                    <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                      <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage((p) => p + 1)}>
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
