import { useState, useMemo } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  MapPin,
  Users,
  Layers,
  ArrowLeft,
  Monitor,
  Wifi,
  Projector,
  Thermometer,
  ArmchairIcon as Chair,
  School,
  Clock,
  CalendarDays,
  Repeat,
  AlertTriangle,
} from 'lucide-react';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { ReservationModal } from '@/components/classrooms/ReservationModal';
import { getClassroomTypeLabel, formatDate } from '@/lib/utils';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';
import type { Classroom, Reservation } from '@/types';

interface MaintenanceBlockDto {
  id: string;
  classroomId: string;
  classroomName: string;
  buildingName?: string;
  startTime: string;
  endTime: string;
  reason: string;
  createdBy: string;
  createdAt: string;
}

const featureIcons: Record<string, React.ReactNode> = {
  projector: <Projector className="h-4 w-4" />,
  wifi: <Wifi className="h-4 w-4" />,
  airconditioning: <Thermometer className="h-4 w-4" />,
  computer: <Monitor className="h-4 w-4" />,
  whiteboard: <School className="h-4 w-4" />,
  chairs: <Chair className="h-4 w-4" />,
};

const featureLabels: Record<string, string> = {
  projector: 'Proyector',
  wifi: 'WiFi',
  airconditioning: 'Aire acondicionado',
  computer: 'Computadoras',
  whiteboard: 'Pizarrón',
  chairs: 'Mesabancos',
};

export default function ClassroomDetail() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const [showReservationModal, setShowReservationModal] = useState(false);

  const { data: classroom, isLoading } = useQuery({
    queryKey: ['classroom', id],
    queryFn: () => api.get<Classroom>(`/classrooms/${id}`),
    enabled: !!id,
  });

  const { data: weeklySchedule } = useQuery({
    queryKey: ['classroomSchedule', id],
    queryFn: () => api.get<Reservation[]>(`/classrooms/${id}/schedule?days=7`),
    enabled: !!id,
    retry: false,
  });

  const { data: activeMaintenance } = useQuery({
    queryKey: ['classroomMaintenance', id],
    queryFn: () => api.get<{ items: MaintenanceBlockDto[]; totalCount: number }>(`/maintenance?classroomId=${id}&activeOnly=true`),
    enabled: !!id,
    retry: false,
  });

  const isInMaintenance = (activeMaintenance?.items?.length ?? 0) > 0;

  const scheduleData = weeklySchedule?.reduce<Record<string, number>>((acc, r) => {
    const day = formatDate(r.date).split(',')[0];
    acc[day] = (acc[day] || 0) + 1;
    return acc;
  }, {});

  const chartData = scheduleData
    ? Object.entries(scheduleData).map(([date, count]) => ({ date, count }))
    : [];

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton variant="rectangular" className="h-48 w-full" />
        <div className="space-y-3">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-4 w-96" />
        </div>
      </div>
    );
  }

  if (!classroom) {
    return (
      <div className="flex flex-col items-center justify-center py-20">
        <School className="h-16 w-16 text-primary-200 mb-4" />
        <h2 className="font-heading text-xl font-semibold text-primary-600">Aula no encontrada</h2>
        <Button variant="outline" className="mt-4" onClick={() => navigate('/classrooms')}>
          Volver a aulas
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <button
        onClick={() => navigate('/classrooms')}
        className="flex items-center gap-1 text-sm text-primary-500 hover:text-primary-700"
      >
        <ArrowLeft className="h-4 w-4" />
        Volver a aulas
      </button>

      <div
        className="h-48 sm:h-64 rounded-xl bg-cover bg-center flex items-center justify-center"
        style={classroom.imageUrl ? { backgroundImage: `url(${classroom.imageUrl})` } : undefined}
      >
        {!classroom.imageUrl ? (
          <div className="w-full h-full rounded-xl bg-gradient-to-br from-primary-100 via-primary-200 to-primary-300 flex items-center justify-center">
            <div className="text-center">
              <p className="font-heading text-5xl font-bold text-primary-400/60">{classroom.code}</p>
              <p className="text-primary-400/80 mt-2">{classroom.buildingName}</p>
            </div>
          </div>
        ) : (
          <div className="w-full h-full rounded-xl bg-gradient-to-t from-black/40 to-transparent flex items-end p-6">
            <p className="font-heading text-2xl font-bold text-white drop-shadow-lg">{classroom.name}</p>
          </div>
        )}
      </div>

      <div className="flex flex-col sm:flex-row sm:items-start sm:justify-between gap-4">
        <div>
          <div className="flex items-center gap-3 mb-1">
            <h1 className="font-heading text-2xl sm:text-3xl font-bold text-primary-800">
              {classroom.name}
            </h1>
            <Badge>{getClassroomTypeLabel(classroom.type)}</Badge>
            {isInMaintenance && (
              <Badge variant="pending" className="flex items-center gap-1">
                <AlertTriangle className="h-3 w-3" />
                En mantenimiento
              </Badge>
            )}
          </div>
          <div className="flex flex-wrap items-center gap-3 text-sm text-primary-500 mt-2">
            <span className="flex items-center gap-1">
              <MapPin className="h-4 w-4" />
              {classroom.buildingName}
            </span>
            <span className="flex items-center gap-1">
              <Layers className="h-4 w-4" />
              Piso {classroom.floor}
            </span>
            <span className="flex items-center gap-1">
              <Users className="h-4 w-4" />
              Capacidad: {classroom.capacity}
            </span>
          </div>
        </div>
        {user?.role !== 'Student' && !isInMaintenance && (
          <Button
            variant="accent"
            size="lg"
            onClick={() => setShowReservationModal(true)}
          >
            Reservar ahora
          </Button>
        )}
        {isInMaintenance && (
          <Button variant="outline" size="lg" disabled className="opacity-60 cursor-not-allowed">
            <AlertTriangle className="h-4 w-4 mr-2" />
            En mantenimiento
          </Button>
        )}
      </div>

      {classroom.description && (
        <Card>
          <CardContent className="p-4">
            <p className="text-sm text-primary-600">{classroom.description}</p>
          </CardContent>
        </Card>
      )}

      <div className="grid sm:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-4">
            <h3 className="font-medium text-primary-800 mb-3">Características</h3>
            {classroom.features && classroom.features.length > 0 ? (
              <div className="grid grid-cols-2 gap-2">
                {classroom.features.map((feature) => (
                  <div
                    key={feature}
                    className="flex items-center gap-2 p-2 rounded-lg bg-primary-50 text-sm text-primary-700"
                  >
                    {featureIcons[feature.toLowerCase()] || <div className="h-4 w-4" />}
                    {featureLabels[feature.toLowerCase()] || feature}
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-primary-400">Sin características registradas</p>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-4">
            <h3 className="font-medium text-primary-800 mb-3">Ocupación Semanal</h3>
            {chartData.length > 0 ? (
              <ResponsiveContainer width="100%" height={180}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e4e1d8" />
                  <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 11 }} />
                  <Tooltip />
                  <Bar dataKey="count" fill="#f5a623" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="flex flex-col items-center justify-center h-[180px] text-primary-400">
                <p className="text-sm">Sin reservaciones esta semana</p>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {weeklySchedule && weeklySchedule.length > 0 && (
        <Card>
          <CardContent className="p-4">
            <h3 className="font-medium text-primary-800 mb-4 flex items-center gap-2">
              <CalendarDays className="h-4 w-4 text-accent-500" />
              Horario Semanal
            </h3>

            <div className="overflow-x-auto">
              <div className="min-w-[700px]">
                <div className="grid grid-cols-[80px_repeat(7,1fr)] border-b border-primary-200 bg-primary-50/50 rounded-t-lg">
                  <div className="p-2 text-xs font-medium text-primary-500" />
                  {[0, 1, 2, 3, 4, 5, 6].map((i) => {
                    const d = new Date();
                    d.setDate(d.getDate() + i);
                    const labels = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
                    return (
                      <div key={i} className="p-2 text-center text-xs font-medium text-primary-600 border-l border-primary-200">
                        {i === 0 ? 'Hoy' : labels[d.getDay()]}
                        <span className="block text-[10px] text-primary-400 font-normal">{d.getDate()}</span>
                      </div>
                    );
                  })}
                </div>
                {[7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21].map((hour) => (
                  <div key={hour} className="grid grid-cols-[80px_repeat(7,1fr)] border-b border-primary-100">
                    <div className="p-2 text-xs text-primary-400 flex items-center">{hour}:00</div>
                    {[0, 1, 2, 3, 4, 5, 6].map((dayOffset) => {
                      const d = new Date();
                      d.setDate(d.getDate() + dayOffset);
                      const dateStr = d.toISOString().slice(0, 10);
                      const r = weeklySchedule.find((res) => {
                        const resDate = res.date.slice(0, 10);
                        const rs = parseInt(res.startTime);
                        const re = parseInt(res.endTime);
                        return resDate === dateStr && rs <= hour && re > hour;
                      });
                      return (
                        <div key={dayOffset} className="p-1 border-l border-primary-100 min-h-[32px]">
                          {r && (
                            <div className="h-full w-full rounded bg-accent-100 border border-accent-200 px-1 py-0.5 text-[10px] text-accent-800 leading-tight truncate">
                              <span className="font-medium">{r.title}</span>
                            </div>
                          )}
                        </div>
                      );
                    })}
                  </div>
                ))}
              </div>
            </div>

            <h4 className="font-medium text-primary-700 mt-6 mb-3 flex items-center gap-2">
              <Clock className="h-4 w-4 text-accent-500" />
              Detalle de Reservaciones
            </h4>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="border-b border-primary-200">
                    <th className="text-left p-2 text-primary-500 font-medium">Día</th>
                    <th className="text-left p-2 text-primary-500 font-medium">Fecha</th>
                    <th className="text-left p-2 text-primary-500 font-medium">Horario</th>
                    <th className="text-left p-2 text-primary-500 font-medium">Título</th>
                    <th className="text-left p-2 text-primary-500 font-medium">Usuario</th>
                    <th className="text-left p-2 text-primary-500 font-medium">Estado</th>
                  </tr>
                </thead>
                <tbody>
                  {weeklySchedule
                    .slice()
                    .sort((a, b) => a.date.localeCompare(b.date) || a.startTime.localeCompare(b.startTime))
                    .map((r) => {
                      const d = new Date(r.date);
                      const dayNames = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];
                      return (
                        <tr key={r.id} className="border-b border-primary-100 hover:bg-primary-50/50">
                          <td className="p-2 text-primary-700">{dayNames[d.getDay()]}</td>
                          <td className="p-2 text-primary-500">{r.date.slice(0, 10)}</td>
                          <td className="p-2 text-primary-700">{r.startTime}-{r.endTime}</td>
                          <td className="p-2 font-medium text-primary-800">
                            <span className="flex items-center gap-1">
                              {r.recurringGroupId && <Repeat className="h-3 w-3 text-accent-500" />}
                              {r.title}
                            </span>
                          </td>
                          <td className="p-2 text-primary-500">{r.userName || '—'}</td>
                          <td className="p-2">
                            <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                              r.status === 'Approved' ? 'bg-green-100 text-green-700' :
                              r.status === 'Pending' ? 'bg-yellow-100 text-yellow-700' :
                              r.status === 'Rejected' ? 'bg-red-100 text-red-700' :
                              r.status === 'Cancelled' ? 'bg-gray-100 text-gray-500' :
                              r.status === 'Completed' ? 'bg-blue-100 text-blue-700' : ''
                            }`}>
                              {r.status === 'Approved' ? 'Aprobada' :
                               r.status === 'Pending' ? 'Pendiente' :
                               r.status === 'Rejected' ? 'Rechazada' :
                               r.status === 'Cancelled' ? 'Cancelada' :
                               r.status === 'Completed' ? 'Completada' : r.status}
                            </span>
                          </td>
                        </tr>
                      );
                    })}
                </tbody>
              </table>
            </div>
          </CardContent>
        </Card>
      )}

      <ReservationModal
        open={showReservationModal}
        onOpenChange={setShowReservationModal}
        classroom={classroom}
        onSuccess={() => navigate('/reservations')}
      />
    </div>
  );
}
