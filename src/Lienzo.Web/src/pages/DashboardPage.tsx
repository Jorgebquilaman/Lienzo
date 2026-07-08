import { useQuery, useMutation } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  DoorOpen,
  CalendarCheck,
  Clock,
  Percent,
  Megaphone,
  ArrowRight,
  Bell,
  AlertTriangle,
  CheckCircle,
  XCircle,
  ClipboardCheck,
} from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Skeleton } from '@/components/ui/Skeleton';
import { useAuthStore } from '@/stores/authStore';
import { getStatusLabel } from '@/lib/utils';
import type { DashboardStats, Reservation, Announcement, PaginatedResponse } from '@/types';

interface MisPendiente {
  claseId: string;
  actividadNombre: string;
  classroomName: string;
  fecha: string;
  horaInicio: string;
  horaFin: string;
  docenteNombre: string;
}

export default function DashboardPage() {
  const user = useAuthStore((s) => s.user);
  const navigate = useNavigate();

  const { data: stats, isLoading: statsLoading } = useQuery({
    queryKey: ['dashboardStats'],
    queryFn: () => api.get<DashboardStats>('/dashboard/stats'),
    enabled: user?.role === 'Admin',
  });

  const { data: pendingResponse } = useQuery({
    queryKey: ['pendingReservations'],
    queryFn: () => api.get<PaginatedResponse<Reservation>>('/reservations?status=Pending&pageSize=5'),
    enabled: user?.role === 'Admin',
  });
  const pendingReservations = pendingResponse?.value;

  const { data: upcomingResponse } = useQuery({
    queryKey: ['upcomingReservations'],
    queryFn: () => api.get<PaginatedResponse<Reservation>>('/reservations?filter=upcoming&pageSize=10'),
    enabled: user?.role === 'Teacher' || user?.role === 'Student',
  });
  const upcomingReservations = upcomingResponse?.value;

  const { data: pendientes } = useQuery({
    queryKey: ['misPendientes'],
    queryFn: () => api.get<MisPendiente[]>('/asistencia/mis-pendientes'),
    enabled: user?.role === 'Student',
  });

  const { data: announcements } = useQuery({
    queryKey: ['recentAnnouncements'],
    queryFn: () => api.get<Announcement[]>('/announcements?pageSize=3'),
  });

  if (user?.role === 'Admin') {
    return <AdminDashboard stats={stats} statsLoading={statsLoading} pendingReservations={pendingReservations} />;
  }

  if (user?.role === 'Teacher') {
    return <TeacherDashboard upcoming={upcomingReservations} announcements={announcements} />;
  }

  return <StudentDashboard upcoming={upcomingReservations} announcements={announcements} pendientes={pendientes} />;
}

function AdminDashboard({
  stats,
  statsLoading,
  pendingReservations,
}: {
  stats?: DashboardStats;
  statsLoading: boolean;
  pendingReservations?: Reservation[];
}) {
  const navigate = useNavigate();

  const statCards = [
    { label: 'Total Aulas', value: stats?.totalClassrooms, icon: DoorOpen, color: 'text-blue-600 bg-blue-50', border: 'border-blue-500' },
    { label: 'Reservaciones Hoy', value: stats?.reservationsToday, icon: CalendarCheck, color: 'text-green-600 bg-green-50', border: 'border-green-500' },
    { label: 'Pendientes de Aprobación', value: stats?.pendingApprovals, icon: Clock, color: 'text-yellow-600 bg-yellow-50', border: 'border-yellow-500' },
    { label: 'Ocupación', value: stats ? `${Math.round(stats.occupancyRate)}%` : '--', icon: Percent, color: 'text-accent-600 bg-accent-50', border: 'border-accent-500' },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Panel de Administración</h1>
        <p className="text-primary-500 mt-1">Bienvenido, administrador</p>
      </div>

      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {statCards.map((stat) => (
          <Card key={stat.label} className={`border-t-4 ${stat.border}`}>
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <div className={`p-2 rounded-lg ${stat.color}`}>
                  <stat.icon className="h-5 w-5" />
                </div>
                <div>
                  <div className="text-2xl font-bold text-primary-800">
                    {statsLoading ? <Skeleton className="h-7 w-12" /> : stat.value ?? '--'}
                  </div>
                  <p className="text-xs text-primary-500">{stat.label}</p>
                </div>
              </div>
            </CardContent>
          </Card>
        ))}
      </div>

      <div className="grid lg:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Resumen General</CardTitle>
          </CardHeader>
          <CardContent>
            {statsLoading ? (
              <Skeleton className="h-[100px] w-full" />
            ) : (
              <div className="space-y-3">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-primary-500">Aulas activas</span>
                  <span className="text-lg font-bold text-primary-800">{stats?.activeClassrooms ?? '--'}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-primary-500">Usuarios registrados</span>
                  <span className="text-lg font-bold text-primary-800">{stats?.totalUsers ?? '--'}</span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-primary-500">Reservaciones hoy</span>
                  <span className="text-lg font-bold text-primary-800">{stats?.reservationsToday ?? '--'}</span>
                </div>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg">Reservaciones Pendientes</CardTitle>
            <Button variant="ghost" size="sm" onClick={() => navigate('/admin/reservations')}>
              Ver todas
            </Button>
          </CardHeader>
          <CardContent>
            {!pendingReservations ? (
              <div className="space-y-2">
                {[1, 2, 3].map((i) => <Skeleton key={i} className="h-14 w-full" />)}
              </div>
            ) : pendingReservations.length === 0 ? (
              <div className="text-center py-8 text-primary-400 text-sm">
                No hay reservaciones pendientes
              </div>
            ) : (
              <div className="space-y-2">
                {pendingReservations.map((r) => (
                  <div key={r.id} className="flex items-center justify-between p-2 rounded-lg hover:bg-primary-50">
                    <div className="min-w-0">
                      <p className="text-sm font-medium text-primary-800 truncate">{r.title}</p>
                      <p className="text-xs text-primary-400">{r.classroomName} · {r.date}</p>
                    </div>
                    <div className="flex gap-1 flex-shrink-0">
                      <button
                        className="p-1.5 rounded-md text-green-600 hover:bg-green-50"
                        onClick={() => api.patch(`/reservations/${r.id}/approve`)}
                      >
                        <CheckCircle className="h-4 w-4" />
                      </button>
                      <button
                        className="p-1.5 rounded-md text-red-600 hover:bg-red-50"
                        onClick={() => api.patch(`/reservations/${r.id}/reject`)}
                      >
                        <XCircle className="h-4 w-4" />
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function CheckInButton({ reservationId }: { reservationId: string }) {
  const navigate = useNavigate();
  const checkinMutation = useMutation({
    mutationFn: (id: string) => api.post('/asistencia/checkin', { reservationId: id }),
    onSuccess: (data: any) => {
      navigate(`/asistencia/${data.claseId}`);
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al iniciar check-in');
    },
  });

  return (
    <Button
      variant="accent"
      size="sm"
      className="w-full"
      onClick={() => checkinMutation.mutate(reservationId)}
      disabled={checkinMutation.isPending}
    >
      <ClipboardCheck className="h-3.5 w-3.5 mr-1" />
      {checkinMutation.isPending ? 'Iniciando...' : 'Habilitar Asistencia'}
    </Button>
  );
}

function TeacherDashboard({
  upcoming,
  announcements,
}: {
  upcoming?: Reservation[];
  announcements?: Announcement[];
}) {
  const navigate = useNavigate();

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Bienvenido, {useAuthStore.getState().user?.firstName}</h1>
        <p className="text-primary-500 mt-1">Gestiona tus clases y aulas</p>
      </div>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg">Próximas Clases</CardTitle>
          <Button variant="ghost" size="sm" onClick={() => navigate('/schedule')}>
            Ver horario
          </Button>
        </CardHeader>
        <CardContent>
          {!upcoming ? (
            <div className="space-y-2">{[1, 2].map((i) => <Skeleton key={i} className="h-14 w-full" />)}</div>
          ) : upcoming.length === 0 ? (
            <div className="text-center py-8">
              <CalendarCheck className="h-10 w-10 text-primary-200 mx-auto mb-2" />
              <p className="text-sm text-primary-400">No tienes clases próximas</p>
              <Button variant="outline" size="sm" className="mt-3" onClick={() => navigate('/schedule')}>
                Ir al horario
              </Button>
            </div>
          ) : (
            <div className="space-y-2">
              {upcoming.slice(0, 5).map((r) => (
                <div key={r.id} className="p-2 rounded-lg hover:bg-primary-50 space-y-2">
                  <div className="flex items-center justify-between gap-2">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-primary-800 break-words">{r.actividadNombre || r.title}</p>
                      <p className="text-xs text-primary-400">
                        {r.classroomName} · {r.date.split('T')[0].split('-').reverse().join('/')} {r.startTime.slice(0, 5)}-{r.endTime.slice(0, 5)}
                      </p>
                      {r.actividadDocentes && (
                        <p className="text-xs text-primary-400 truncate">{r.actividadDocentes}</p>
                      )}
                    </div>
                    {r.status !== 'Approved' && (
                      <Badge variant={r.status.toLowerCase() as any}>{getStatusLabel(r.status)}</Badge>
                    )}
                  </div>
                  {r.status === 'Approved' && (
                    <CheckInButton reservationId={r.id} />
                  )}
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Acciones Rápidas</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <p className="text-xs font-semibold text-primary-500 uppercase tracking-wider">Tus Reservaciones</p>
            {!upcoming ? (
              <div className="space-y-2">{[1, 2].map((i) => <Skeleton key={i} className="h-10 w-full" />)}</div>
            ) : upcoming.length === 0 ? (
              <p className="text-sm text-primary-400">Sin reservaciones</p>
            ) : (
              <div className="space-y-1">
                {upcoming.slice(0, 5).map((r) => (
                  <div key={r.id} className="flex items-center justify-between gap-2 py-1.5">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm text-primary-700 truncate">{r.actividadNombre || r.title}</p>
                      <p className="text-xs text-primary-400">{r.date.split('T')[0].split('-').reverse().join('/')} {r.startTime.slice(0, 5)}-{r.endTime.slice(0, 5)}</p>
                    </div>
                    <Badge variant={r.status.toLowerCase() as any} className="shrink-0">{getStatusLabel(r.status)}</Badge>
                  </div>
                ))}
              </div>
            )}
          </div>
          <div className="border-t border-primary-100 pt-3 space-y-3">
            <Button variant="accent" className="w-full justify-start" onClick={() => navigate('/announcements')}>
              <Megaphone className="h-4 w-4 mr-2" />
              Enviar Anuncio
            </Button>
            <Button variant="outline" className="w-full justify-start" onClick={() => navigate('/classrooms')}>
              <DoorOpen className="h-4 w-4 mr-2" />
              Explorar Aulas
            </Button>
          </div>
        </CardContent>
      </Card>

      {announcements && announcements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Últimos Anuncios</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {announcements.map((a) => (
              <div key={a.id} className="p-3 rounded-lg bg-primary-50">
                <p className="text-sm font-medium text-primary-800">{a.title}</p>
                <p className="text-xs text-primary-500 mt-0.5">{a.body.slice(0, 100)}...</p>
              </div>
            ))}
          </CardContent>
        </Card>
      )}
    </div>
  );
}

function StudentDashboard({
  upcoming,
  announcements,
  pendientes,
}: {
  upcoming?: Reservation[];
  announcements?: Announcement[];
  pendientes?: MisPendiente[];
}) {
  const navigate = useNavigate();
  const unreadAnnouncements = announcements?.filter((a) => !a.isRead) || [];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">
          Hola, {useAuthStore.getState().user?.firstName}
        </h1>
        <p className="text-primary-500 mt-1">Tus reservaciones y anuncios</p>
      </div>

      {unreadAnnouncements.length > 0 && (
        <div className="bg-accent-50 border border-accent-200 rounded-xl p-4 flex items-start gap-3">
          <Megaphone className="h-5 w-5 text-accent-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="text-sm font-medium text-accent-800">
              {unreadAnnouncements.length} anuncio(s) sin leer
            </p>
            <button
              className="text-xs text-accent-600 underline mt-0.5"
              onClick={() => navigate('/announcements')}
            >
              Ver anuncios
            </button>
          </div>
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle className="text-lg flex items-center gap-2">
            <ClipboardCheck className="h-5 w-5" />
            Asistencias Pendientes
          </CardTitle>
        </CardHeader>
        <CardContent>
          {!pendientes ? (
            <div className="space-y-2">{[1, 2].map((i) => <Skeleton key={i} className="h-14 w-full" />)}</div>
          ) : pendientes.length === 0 ? (
            <div className="text-center py-6">
              <CheckCircle className="h-10 w-10 text-green-200 mx-auto mb-2" />
              <p className="text-sm text-primary-400">No tienes asistencias pendientes</p>
            </div>
          ) : (
            <div className="space-y-2">
              {pendientes.map((p) => (
                <div key={p.claseId} className="p-2 rounded-lg hover:bg-primary-50 space-y-2">
                  <div className="flex items-center justify-between gap-2">
                    <div className="min-w-0 flex-1">
                      <p className="text-sm font-medium text-primary-800 break-words">{p.actividadNombre}</p>
                      <p className="text-xs text-primary-400">
                        {p.classroomName} · {p.fecha.split('-').reverse().join('/')} {p.horaInicio.slice(0, 5)}-{p.horaFin.slice(0, 5)}
                      </p>
                      <p className="text-xs text-primary-400">{p.docenteNombre}</p>
                    </div>
                  </div>
                  <Button
                    variant="accent"
                    size="sm"
                    className="w-full"
                    onClick={() => navigate(`/asistencia/marcar?claseId=${p.claseId}`)}
                  >
                    <ClipboardCheck className="h-3.5 w-3.5 mr-1" />
                    Marcar Asistencia
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg">Mis Reservaciones</CardTitle>
          <Button variant="ghost" size="sm" onClick={() => navigate('/reservations')}>
            Ver todas
          </Button>
        </CardHeader>
        <CardContent>
          {!upcoming ? (
            <div className="space-y-2">{[1, 2].map((i) => <Skeleton key={i} className="h-14 w-full" />)}</div>
          ) : upcoming.length === 0 ? (
            <div className="text-center py-8">
              <CalendarCheck className="h-10 w-10 text-primary-200 mx-auto mb-2" />
              <p className="text-sm text-primary-400 mb-3">No tienes reservaciones activas</p>
              <Button variant="accent" onClick={() => navigate('/classrooms')}>
                Explorar Aulas
                <ArrowRight className="h-4 w-4 ml-2" />
              </Button>
            </div>
          ) : (
            <div className="space-y-2">
              {upcoming.slice(0, 5).map((r) => (
                <div key={r.id} className="flex items-center justify-between p-2 rounded-lg hover:bg-primary-50">
                  <div>
                    <p className="text-sm font-medium text-primary-800">{r.title}</p>
                    <p className="text-xs text-primary-400">{r.classroomName} · {r.date} {r.startTime}-{r.endTime}</p>
                  </div>
                  <Badge variant={r.status.toLowerCase() as any}>{getStatusLabel(r.status)}</Badge>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {announcements && announcements.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Anuncios Recientes</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {announcements.slice(0, 3).map((a) => (
              <div key={a.id} className="flex items-start gap-3 p-3 rounded-lg hover:bg-primary-50">
                <Bell className="h-4 w-4 text-primary-400 mt-0.5" />
                <div>
                  <p className="text-sm font-medium text-primary-800">{a.title}</p>
                  <p className="text-xs text-primary-400">{a.body.slice(0, 80)}...</p>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>
      )}
    </div>
  );
}
