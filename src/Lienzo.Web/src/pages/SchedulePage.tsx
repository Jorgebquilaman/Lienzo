import { useState, useMemo, Fragment } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  ChevronLeft, ChevronRight, Building2, Repeat, CalendarDays, User, Clock, MapPin, Tag, Calendar, Plus, BookOpen, ListOrdered, Search, Move, ClipboardCheck, AlertTriangle,
} from 'lucide-react';
import { addDays, subDays, format, parseISO } from 'date-fns';
import { es } from 'date-fns/locale';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
import { Card, CardContent } from '@/components/ui/Card';
import { Skeleton } from '@/components/ui/Skeleton';
import { Badge } from '@/components/ui/Badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogBody } from '@/components/ui/Dialog';
import { getStatusLabel, getStatusColor } from '@/lib/utils';
import { ReservationModal } from '@/components/classrooms/ReservationModal';
import { useAuthStore } from '@/stores/authStore';
import type { Building, Classroom, Reservation, UserRole } from '@/types';

const DAY_LABELS: Record<string, string> = {
  Monday: 'Lun', Tuesday: 'Mar', Wednesday: 'Mié', Thursday: 'Jue',
  Friday: 'Vie', Saturday: 'Sáb', Sunday: 'Dom',
};

function formatRecurrenceRule(rule: string): string {
  try {
    const parsed = JSON.parse(rule);
    const days = (parsed.daysOfWeek as string[] || []).map((d) => DAY_LABELS[d] || d).join(', ');
    const endDate = parsed.endDate ? format(parseISO(parsed.endDate), "d 'de' MMMM", { locale: es }) : '';
    return `Semanal: ${days}${endDate ? ` — hasta ${endDate}` : ''}`;
  } catch {
    return rule;
  }
}

const HOURS = Array.from({ length: 15 }, (_, i) => i + 7);
const MIN_TIME = 7 * 60;
const MAX_TIME = 22 * 60;
const TOTAL_MINUTES = MAX_TIME - MIN_TIME;

const STATUS_COLORS: Record<string, string> = {
  Approved: 'bg-green-500',
  Pending: 'bg-yellow-500',
  Rejected: 'bg-red-300',
  Cancelled: 'bg-gray-400',
  Completed: 'bg-blue-500',
};

const PERIOD_PALETTE = [
  'bg-purple-500', 'bg-orange-500', 'bg-teal-500', 'bg-pink-500',
  'bg-indigo-500', 'bg-rose-500', 'bg-cyan-500', 'bg-amber-500',
  'bg-fuchsia-500', 'bg-emerald-500', 'bg-violet-500', 'bg-lime-600',
];

function ExistingClaseButton({ reservationId }: { reservationId: string }) {
  const navigate = useNavigate();
  const { data: claseId } = useQuery({
    queryKey: ['clase-por-reserva', reservationId],
    queryFn: () => api.get<string | null>(`/asistencia/por-reserva/${reservationId}`),
  });

  if (claseId) {
    return (
      <Button variant="accent" size="sm" className="w-full mt-2" onClick={() => navigate(`/asistencia/${claseId}`)}>
        Ver Asistencia
      </Button>
    );
  }

  return <CheckInButton reservationId={reservationId} />;
}

function CheckInButton({ reservationId }: { reservationId: string }) {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
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
      className="w-full mt-2"
      onClick={() => checkinMutation.mutate(reservationId)}
      disabled={checkinMutation.isPending}
    >
      <ClipboardCheck className="h-4 w-4 mr-1.5" />
      {checkinMutation.isPending ? 'Iniciando...' : 'Habilitar Asistencia'}
    </Button>
  );
}

function toMinutes(time: string): number {
  const [h, m] = time.split(':').map(Number);
  return h * 60 + m;
}

export default function SchedulePage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const today = new Date();
  const [currentDate, setCurrentDate] = useState(today);
  const [buildingId, setBuildingId] = useState('');
  const [classroomId, setClassroomId] = useState('');
  const [colorMode, setColorMode] = useState<'status' | 'period'>('status');
  const [viewMode, setViewMode] = useState<'classroom' | 'time'>('classroom');

  const dateStr = format(currentDate, 'yyyy-MM-dd');
  const dayName = format(currentDate, 'EEEE', { locale: es });
  const dayLabel = format(currentDate, "d 'de' MMMM, yyyy", { locale: es });
  const isSaturday = currentDate.getDay() === 6;
  const visibleHours = isSaturday ? HOURS.filter((h) => h < 16) : HOURS;

  const { data: buildings } = useQuery({
    queryKey: ['buildings'],
    queryFn: () => api.get<Building[]>('/buildings'),
  });

  const { data: classrooms } = useQuery({
    queryKey: ['classrooms', buildingId],
    queryFn: () => {
      const params: Record<string, string> = {};
      if (buildingId) params.buildingId = buildingId;
      return api.get<Classroom[]>('/classrooms', params);
    },
    enabled: !!buildingId,
  });

  const filteredClassrooms = useMemo(() => {
    if (!classrooms) return [];
    let list = classrooms;
    if (classroomId) list = list.filter((c) => c.id === classroomId);
    return [...list].sort((a, b) => a.name.localeCompare(b.name, 'es', { numeric: true }));
  }, [classrooms, classroomId]);

  const { data: reservations, isLoading } = useQuery({
    queryKey: ['schedule', dateStr, buildingId, classroomId],
    queryFn: () =>
      api.get<Reservation[]>(
        `/reservations/schedule?fromDate=${dateStr}&toDate=${dateStr}${buildingId ? `&buildingId=${buildingId}` : ''}${classroomId ? `&classroomId=${classroomId}` : ''}`
      ),
    enabled: !!buildingId,
    refetchInterval: 15_000,
  });

  const { data: holidaysResponse } = useQuery({
    queryKey: ['holidays'],
    queryFn: () => api.get<{ id: string; date: string; description: string }[]>('/holidays'),
  });
  const holidays = holidaysResponse || [];
  const isUserHoliday = holidays.find((h) => h.date === dateStr);

  const [selectedReservation, setSelectedReservation] = useState<Reservation | null>(null);
  const user = useAuthStore((s) => s.user);
  const [movingReservation, setMovingReservation] = useState(false);
  const [moveBuildingId, setMoveBuildingId] = useState('');
  const [moveClassroomId, setMoveClassroomId] = useState('');
  const [moveApplyFuture, setMoveApplyFuture] = useState(false);

  const { data: moveBuildings } = useQuery({
    queryKey: ['buildings'],
    queryFn: () => api.get<Building[]>('/buildings'),
    enabled: movingReservation,
  });

  const { data: moveClassrooms } = useQuery({
    queryKey: ['classrooms', moveBuildingId],
    queryFn: () => {
      const params: Record<string, string> = {};
      if (moveBuildingId) params.buildingId = moveBuildingId;
      return api.get<Classroom[]>('/classrooms', params);
    },
    enabled: movingReservation && !!moveBuildingId,
  });

  const moveMutation = useMutation({
    mutationFn: ({ reservationId, newClassroomId, applyToFuture }: { reservationId: string; newClassroomId: string; applyToFuture: boolean }) =>
      api.patch(`/reservations/${reservationId}/move`, { newClassroomId, applyToFuture }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedule'] });
      setMovingReservation(false);
      setSelectedReservation(null);
      setMoveClassroomId('');
      setMoveBuildingId('');
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al mover la reserva');
    },
  });

  const cancelFutureMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/reservations/${id}/cancel-future`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['schedule'] });
      setSelectedReservation(null);
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al cancelar futuras reservas');
    },
  });

  interface Actividad { id: string; nombre: string; codigoMateria: string; periodoNombre?: string; carreraNombre?: string; aulaId?: string; aulaNombre?: string; diaSemana?: string; horaInicio?: string; horaFin?: string; docenteIds: string[]; docentesNombres?: string; }

  const { data: actividades } = useQuery({
    queryKey: ['actividades'],
    queryFn: () => api.get<Actividad[]>('/actividades'),
  });

  const unassignedActividades = useMemo(() => {
    if (!actividades) return [];
    const reservedActividadIds = new Set(
      (reservations || []).filter((r) => r.actividadId).map((r) => r.actividadId)
    );
    return actividades.filter(
      (a) => a.diaSemana && !a.aulaId && !reservedActividadIds.has(a.id)
    );
  }, [actividades, reservations]);

  const [showUnassigned, setShowUnassigned] = useState(false);
  const [unassignedAct, setUnassignedAct] = useState<Actividad | null>(null);
  const [unassignedSearch, setUnassignedSearch] = useState('');

  const DAY_LABEL_MAP: Record<string, string> = { Monday: 'Lun', Tuesday: 'Mar', Wednesday: 'Mié', Thursday: 'Jue', Friday: 'Vie', Saturday: 'Sáb' };

  function getNextDateForDay(day: string): string {
    const dayMap: Record<string, number> = { Monday: 1, Tuesday: 2, Wednesday: 3, Thursday: 4, Friday: 5, Saturday: 6, Sunday: 0 };
    const targetDay = dayMap[day] ?? 0;
    const now = new Date();
    const diff = (targetDay - now.getDay() + 7) % 7;
    const d = new Date(now);
    d.setDate(d.getDate() + (diff === 0 ? 7 : diff));
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const dd = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${dd}`;
  }

  const reservationsByClassroom = useMemo(() => {
    const map: Record<string, Reservation[]> = {};
    for (const r of reservations || []) {
      if (!map[r.classroomId]) map[r.classroomId] = [];
      map[r.classroomId].push(r);
    }
    return map;
  }, [reservations]);

  const prevDay = () => setCurrentDate((d) => subDays(d, 1));
  const nextDay = () => setCurrentDate((d) => addDays(d, 1));
  const goToday = () => setCurrentDate(today);

  const periodColorMap = useMemo(() => {
    const map: Record<string, string> = {};
    let i = 0;
    for (const r of reservations || []) {
      if (r.actividadPeriodo && !map[r.actividadPeriodo]) {
        map[r.actividadPeriodo] = PERIOD_PALETTE[i % PERIOD_PALETTE.length];
        i++;
      }
    }
    return map;
  }, [reservations]);

  const getBarColor = (r: Reservation) =>
    colorMode === 'period' && r.actividadPeriodo
      ? periodColorMap[r.actividadPeriodo] || 'bg-primary-400'
      : STATUS_COLORS[r.status] || 'bg-primary-400';

  const [createSlot, setCreateSlot] = useState<{ classroom: Classroom; startTime: string; endTime: string } | null>(null);

  const handleTimelineClick = (e: React.MouseEvent<HTMLDivElement>, classroom: Classroom) => {
    if ((e.target as HTMLElement).closest('[data-reservation-id]')) return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const pct = x / rect.width;
    const minutesFromStart = pct * TOTAL_MINUTES;
    const totalMinutes = MIN_TIME + minutesFromStart;
    const snappedHour = Math.floor(totalMinutes / 60);
    const maxHour = isSaturday ? 15 : 21;
    if (snappedHour < 7 || snappedHour >= maxHour) return;
    const startStr = `${String(snappedHour).padStart(2, '0')}:00`;
    const endStr = `${String(snappedHour + 1).padStart(2, '0')}:00`;
    setCreateSlot({ classroom, startTime: startStr, endTime: endStr });
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Horario</h1>
          <p className="text-primary-500 mt-1">Vista general de ocupación de aulas</p>
        </div>
      </div>

      <div className="flex flex-wrap items-end gap-3">
        <div className="w-56">
          <Select
            label="Edificio"
            placeholder="Seleccionar edificio"
            value={buildingId}
            onValueChange={(v) => { setBuildingId(v); setClassroomId(''); }}
            options={(buildings || []).map((b) => ({ value: b.id, label: b.name }))}
          />
        </div>

        <div className="w-56">
          <Select
            label="Aula"
            placeholder="Todas las aulas"
            value={classroomId}
            onValueChange={(v) => setClassroomId(v)}
            options={(classrooms || []).map((c) => ({ value: c.id, label: `${c.name} (Piso ${c.floor})` }))}
          />
        </div>

        <div className="w-40">
          <Select
            label="Color por"
            value={colorMode}
            onValueChange={(v) => setColorMode(v as 'status' | 'period')}
            options={[
              { value: 'status', label: 'Estado' },
              { value: 'period', label: 'Periodo' },
            ]}
          />
        </div>

        <Button
          variant="outline"
          size="sm"
          onClick={() => setViewMode(prev => prev === 'classroom' ? 'time' : 'classroom')}
          className={`${viewMode === 'time' ? 'bg-accent-50 border-accent-300 text-accent-700' : ''}`}
        >
          {viewMode === 'classroom' ? 'Por horarios' : 'Por aulas'}
        </Button>

        <div className="flex items-center gap-1">
          <Button variant="outline" size="sm" onClick={prevDay}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <div className="px-3 text-center min-w-[180px]">
            <p className="text-sm font-medium text-primary-700 capitalize">{dayName}</p>
            <p className="text-xs text-primary-400">{dayLabel}</p>
          </div>
          <Button variant="outline" size="sm" onClick={nextDay}>
            <ChevronRight className="h-4 w-4" />
          </Button>
          <Button variant="ghost" size="sm" onClick={goToday} className="text-xs ml-1">
            Hoy
          </Button>
          <div className="relative ml-2">
            <Calendar className="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400 pointer-events-none" />
            <input type="date" value={dateStr} onChange={e => setCurrentDate(new Date(e.target.value + 'T12:00:00'))}
              className="pl-8 pr-3 py-1.5 text-sm border border-primary-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-accent-300" />
          </div>
          {unassignedActividades.length > 0 && (
            <Button variant="outline" size="sm" onClick={() => setShowUnassigned(!showUnassigned)} className={`ml-2 ${showUnassigned ? 'bg-accent-50 border-accent-300 text-accent-700' : ''}`}>
              <ListOrdered className="h-4 w-4 mr-1" />
              No asignados ({unassignedActividades.length})
            </Button>
          )}
        </div>
      </div>

      {showUnassigned && unassignedActividades.length > 0 && (
        <Card>
          <CardContent className="p-4">
            <h3 className="text-sm font-semibold text-primary-700 mb-3 flex items-center gap-2">
              <BookOpen className="h-4 w-4 text-accent-500" />
              Actividades sin aula asignada
            </h3>
            <div className="relative mb-3">
              <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400 pointer-events-none" />
              <input
                type="text"
                value={unassignedSearch}
                onChange={(e) => setUnassignedSearch(e.target.value)}
                placeholder="Filtrar por nombre de actividad..."
                className="w-full pl-8 pr-3 py-1.5 text-sm border border-primary-200 rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-accent-300"
              />
            </div>
            <div className="space-y-2">
              {unassignedActividades.filter((a) => !unassignedSearch || a.nombre.toLowerCase().includes(unassignedSearch.toLowerCase())).map((act) => (
                <div key={act.id} className="flex items-center justify-between p-2.5 rounded-lg border border-primary-100 hover:border-accent-200 transition-colors">
                  <div className="min-w-0 flex-1">
                    <p className="text-sm font-medium text-primary-700 truncate">{act.nombre}</p>
                    <p className="text-xs text-primary-400 mt-0.5">
                      <span className="inline-flex items-center gap-1">
                        <Clock className="h-3 w-3" />
                        {DAY_LABEL_MAP[act.diaSemana!] || act.diaSemana} {act.horaInicio}-{act.horaFin}
                      </span>
                      {act.docentesNombres && (
                        <span className="inline-flex items-center gap-1 ml-3">
                          <User className="h-3 w-3" />
                          {act.docentesNombres}
                        </span>
                      )}
                    </p>
                  </div>
                  <Button variant="accent" size="sm" className="ml-3 flex-shrink-0" onClick={() => {
                    setUnassignedAct(act);
                  }}>
                    <MapPin className="h-3.5 w-3.5 mr-1" />
                    Asignar aula
                  </Button>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}

      <Card>
        <CardContent className="p-0">
          {!buildingId ? (
            <div className="flex flex-col items-center justify-center py-20 text-primary-400">
              <Building2 className="h-16 w-16 mb-4" />
              <p className="font-medium">Selecciona un edificio para ver el horario</p>
            </div>
          ) : currentDate.getDay() === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-primary-400">
              <CalendarDays className="h-12 w-12 mb-3" />
              <p className="font-medium">Domingo — Feriado</p>
              <p className="text-sm mt-1">No se realizan reservas los domingos</p>
            </div>
          ) : isUserHoliday ? (
            <div className="flex flex-col items-center justify-center py-16 text-primary-400">
              <CalendarDays className="h-12 w-12 mb-3" />
              <p className="font-medium">{isUserHoliday.description} — Feriado</p>
              <p className="text-sm mt-1">No se realizan reservas en días feriados</p>
            </div>
          ) : isLoading ? (
            <div className="p-4 space-y-2">
              {Array.from({ length: 8 }).map((_, i) => (
                <Skeleton key={i} variant="rectangular" className="h-10 w-full" />
              ))}
            </div>
          ) : filteredClassrooms.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16 text-primary-400">
              <CalendarDays className="h-12 w-12 mb-3" />
              <p className="font-medium">No hay aulas en este edificio</p>
            </div>
          ) : viewMode === 'time' ? (
            <div className="overflow-x-auto">
              <div className="min-w-[900px]" style={{ display: 'grid', gridTemplateColumns: '80px 1fr' }}>
                <div className="sticky left-0 z-10 p-2 text-xs font-medium text-primary-500 uppercase tracking-wider min-h-[36px] flex items-center border-r border-primary-200 bg-white">
                  Horario
                </div>
                <div className="flex border-b border-primary-200 bg-primary-50/50">
                  {filteredClassrooms.map((c) => (
                    <div key={c.id} className="w-36 shrink-0 p-2 text-center text-xs font-medium text-primary-500 border-l border-primary-200">
                      {c.name}
                    </div>
                  ))}
                </div>
                {visibleHours.map((hour) => (
                  <Fragment key={hour}>
                    <div className="sticky left-0 z-10 p-2 flex items-center text-sm font-medium text-primary-700 border-b border-primary-100 min-h-[36px] border-r border-primary-200 bg-white">
                      {hour.toString().padStart(2, '0')}:00
                    </div>
                    <div className="flex border-b border-primary-100 last:border-b-0">
                      {filteredClassrooms.map((classroom) => {
                        const classroomReservations = reservationsByClassroom[classroom.id] || [];
                        const cellStart = hour * 60;
                        const cellEnd = (hour + 1) * 60;
                        const reservation = classroomReservations.find((r) => {
                          const rs = toMinutes(r.startTime);
                          const re = toMinutes(r.endTime);
                          return rs < cellEnd && re > cellStart;
                        });
                        return (
                          <div key={classroom.id}
                            className="w-36 shrink-0 relative min-h-[36px] border-l border-primary-100 group cursor-pointer"
                            onClick={() => {
                              if (!reservation) {
                                const startStr = `${hour.toString().padStart(2, '0')}:00`;
                                const endStr = `${(hour + 1).toString().padStart(2, '0')}:00`;
                                setCreateSlot({ classroom, startTime: startStr, endTime: endStr });
                              }
                            }}
                          >
                            {reservation && (
                              <div
                                className={`absolute inset-1 rounded px-1.5 py-1 text-xs text-white overflow-hidden cursor-pointer hover:opacity-90 flex items-center ${getBarColor(reservation)}`}
                                onClick={(e) => { e.stopPropagation(); setSelectedReservation(reservation); }}
                                title={`${reservation.title} (${reservation.startTime}-${reservation.endTime})`}
                              >
                                <span className="truncate leading-tight font-medium">
                                  {reservation.startTime !== `${hour.toString().padStart(2, '0')}:00`
                                    ? `${reservation.startTime} ${reservation.title}`
                                    : reservation.title}
                                </span>
                              </div>
                            )}
                            {!reservation && (
                              <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                                <div className="flex items-center gap-1 text-xs text-accent-500 bg-white/80 rounded-full px-2 py-0.5 shadow-sm border border-accent-200">
                                  <Plus className="h-3 w-3" /> Reservar
                                </div>
                              </div>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  </Fragment>
                ))}
              </div>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <div className="min-w-[900px]" style={{ display: 'grid', gridTemplateColumns: '176px 1fr' }}>
                <div className="sticky left-0 z-10 p-2 text-xs font-medium text-primary-500 uppercase tracking-wider min-h-[48px] flex items-center border-r border-primary-200 bg-white">
                  Aula
                </div>
                <div className="flex border-b border-primary-200 bg-primary-50/50">
                  <div className="flex-1 flex">
                    {visibleHours.map((h) => (
                      <div
                        key={h}
                        className="flex-1 p-2 text-center text-xs font-medium text-primary-500 border-l border-primary-200"
                      >
                        {h.toString().padStart(2, '0')}:00
                      </div>
                    ))}
                  </div>
                </div>
                {filteredClassrooms.map((classroom) => {
                  const classroomReservations = reservationsByClassroom[classroom.id] || [];
                  return (
                    <Fragment key={classroom.id}>
                      <div className="sticky left-0 z-10 p-2 flex items-center text-sm font-medium text-primary-700 border-b border-primary-100 min-h-[48px] border-r border-primary-200 bg-white" title={`${classroom.name} (Piso ${classroom.floor})`}>
                        {classroom.name}
                        <span className="ml-1 text-xs text-primary-400 font-normal">
                          (P{classroom.floor})
                        </span>
                      </div>
                      <div
                        className="relative min-h-[48px] cursor-pointer border-b border-primary-100 last:border-b-0 group"
                        onClick={(e) => handleTimelineClick(e, classroom)}
                      >
                          {visibleHours.map((h) => (
                              <div
                                key={h}
                                className="absolute top-0 bottom-0 border-l border-primary-100 pointer-events-none"
                                style={{ left: `${((h - 7) / 15) * 100}%`, width: `${(1 / 15) * 100}%` }}
                              />
                          ))}
                          {classroomReservations.length === 0 && (
                            <div className="absolute inset-0 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
                              <div className="flex items-center gap-1 text-xs text-accent-500 bg-white/80 rounded-full px-2 py-0.5 shadow-sm border border-accent-200">
                                <Plus className="h-3 w-3" /> Reservar
                              </div>
                            </div>
                          )}
                          {classroomReservations.map((r) => {
                            const rs = toMinutes(r.startTime);
                            const re = toMinutes(r.endTime);
                            const left = ((rs - MIN_TIME) / TOTAL_MINUTES) * 100;
                            const width = ((re - rs) / TOTAL_MINUTES) * 100;
                            return (
                              <div
                                key={r.id}
                                data-reservation-id={r.id}
                                className={`absolute top-1 bottom-1 rounded px-1.5 py-1 text-xs text-white overflow-hidden cursor-pointer hover:opacity-90 ${getBarColor(r)}`}
                                style={{ left: `${left}%`, width: `${width}%` }}
                                onClick={(e) => { e.stopPropagation(); setSelectedReservation(r); }}
                                title={`${r.title} · ${r.classroomName} (${r.startTime}-${r.endTime})`}
                              >
                                <span className="truncate block leading-tight font-medium">
                                  {r.recurringGroupId && <Repeat className="h-3 w-3 inline mr-0.5" />}
                                  {r.title}
                                </span>
                                <span className="text-[10px] opacity-80">
                                  {r.startTime}-{r.endTime}
                                </span>
                              </div>
                            );
                          })}
                      </div>
                    </Fragment>
                  );
                })}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {isSaturday && (
        <p className="text-xs text-amber-600">Los sábados el horario de reserva es hasta las 16:00</p>
      )}

      <div className="flex items-center gap-4 text-xs text-primary-400 flex-wrap">
        {colorMode === 'status' ? (
          <>
            <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-green-500 inline-block" /> Aprobada</span>
            <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-yellow-500 inline-block" /> Pendiente</span>
            <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-blue-500 inline-block" /> Completada</span>
          </>
        ) : (
          Object.entries(periodColorMap).map(([period, color]) => (
            <span key={period} className="flex items-center gap-1">
              <span className={`w-3 h-3 rounded ${color} inline-block`} /> {period}
            </span>
          ))
        )}
      </div>

      <ReservationModal
        open={!!createSlot}
        onOpenChange={(open) => { if (!open) setCreateSlot(null); }}
        classroom={createSlot?.classroom}
        defaultDate={dateStr}
        defaultStartTime={createSlot?.startTime}
        defaultEndTime={createSlot?.endTime}
        onSuccess={() => {
          setCreateSlot(null);
          queryClient.invalidateQueries({ queryKey: ['schedule', dateStr, buildingId, classroomId] });
        }}
      />

      <ReservationModal
        open={!!unassignedAct}
        onOpenChange={(open) => { if (!open) setUnassignedAct(null); }}
        defaultDate={dateStr}
        defaultStartTime={unassignedAct?.horaInicio}
        defaultEndTime={unassignedAct?.horaFin}
        defaultActividadId={unassignedAct?.id}
        defaultBuildingId={buildingId || undefined}
        defaultTitle={unassignedAct?.nombre}
        onSuccess={() => {
          setUnassignedAct(null);
          setShowUnassigned(false);
          queryClient.invalidateQueries({ queryKey: ['schedule', dateStr, buildingId, classroomId] });
        }}
      />

      <Dialog open={!!selectedReservation} onOpenChange={(open) => { if (!open) { setSelectedReservation(null); setMovingReservation(false); setMoveClassroomId(''); setMoveBuildingId(''); } }}>
        {selectedReservation && (
          <DialogContent className="max-w-md">
            <DialogHeader>
              <DialogTitle className="flex items-center gap-2">
                {selectedReservation.recurringGroupId && <Repeat className="h-4 w-4 text-accent-500" />}
                {selectedReservation.title}
              </DialogTitle>
            </DialogHeader>
            <DialogBody className="space-y-3">
              <div className="flex items-center gap-2 text-sm text-primary-600">
                <MapPin className="h-4 w-4 text-primary-400 flex-shrink-0" />
                <span>{selectedReservation.classroomName || '—'}{selectedReservation.buildingName ? ` (${selectedReservation.buildingName})` : ''}</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-primary-600">
                <CalendarDays className="h-4 w-4 text-primary-400 flex-shrink-0" />
                <span>{format(parseISO(selectedReservation.date), "EEEE d 'de' MMMM, yyyy", { locale: es })}</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-primary-600">
                <Clock className="h-4 w-4 text-primary-400 flex-shrink-0" />
                <span>{selectedReservation.startTime} — {selectedReservation.endTime}</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-primary-600">
                <User className="h-4 w-4 text-primary-400 flex-shrink-0" />
                <span>{selectedReservation.userName || '—'}</span>
              </div>
              <div className="flex items-center gap-2">
                <Tag className="h-4 w-4 text-primary-400 flex-shrink-0" />
                <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${getStatusColor(selectedReservation.status)}`}>
                  {getStatusLabel(selectedReservation.status)}
                </span>
              </div>
              {selectedReservation.actividadId && (
                <div className="pt-2 border-t border-primary-100 space-y-1">
                  <p className="text-xs text-primary-400 mb-1 font-medium">Actividad</p>
                  <p className="text-sm text-primary-600 font-medium">{selectedReservation.actividadNombre || '—'}</p>
                  {selectedReservation.actividadPeriodo && <p className="text-xs text-primary-500">Periodo: {selectedReservation.actividadPeriodo}</p>}
                  {selectedReservation.actividadCarrera && <p className="text-xs text-primary-500">Carrera: {selectedReservation.actividadCarrera}</p>}
                  {selectedReservation.actividadDocentes && <p className="text-xs text-primary-500">Docentes: {selectedReservation.actividadDocentes}</p>}
                  {selectedReservation.status === 'Approved' && <ExistingClaseButton reservationId={selectedReservation.id} />}
                </div>
              )}
              {selectedReservation.status === 'Approved' && user?.role === 'Admin' && !movingReservation && (
                <Button variant="outline" size="sm" className="w-full" onClick={() => setMovingReservation(true)}>
                  <Move className="h-4 w-4 mr-1" /> Mover aula
                </Button>
              )}
              {movingReservation && (
                <div className="pt-2 border-t border-primary-100 space-y-3">
                  <p className="text-xs text-primary-400 font-medium">Mover a otra aula</p>
                  <Select
                    label="Edificio"
                    placeholder="Seleccionar edificio"
                    value={moveBuildingId}
                    onValueChange={(v) => { setMoveBuildingId(v); setMoveClassroomId(''); }}
                    options={(moveBuildings || []).map((b) => ({ value: b.id, label: b.name }))}
                  />
                  <Select
                    label="Aula"
                    placeholder="Seleccionar aula"
                    value={moveClassroomId}
                    onValueChange={(v) => setMoveClassroomId(v)}
                    options={(moveClassrooms || []).map((c) => ({ value: c.id, label: `${c.name} (Piso ${c.floor})` }))}
                  />
                  {selectedReservation.recurringGroupId && (
                    <label className="flex items-center gap-2 text-sm text-primary-600 cursor-pointer">
                      <input type="checkbox" checked={moveApplyFuture} onChange={(e) => setMoveApplyFuture(e.target.checked)}
                        className="rounded border-primary-300 text-accent-600 focus:ring-accent-500" />
                      Aplicar a todas las futuras
                    </label>
                  )}
                  <div className="flex gap-2">
                    <Button variant="ghost" size="sm" className="flex-1" onClick={() => { setMovingReservation(false); setMoveClassroomId(''); setMoveBuildingId(''); }}>
                      Cancelar
                    </Button>
                    <Button variant="accent" size="sm" className="flex-1" disabled={!moveClassroomId || moveMutation.isPending}
                      onClick={() => moveMutation.mutate({
                        reservationId: selectedReservation.id,
                        newClassroomId: moveClassroomId,
                        applyToFuture: moveApplyFuture,
                      })}>
                      {moveMutation.isPending ? 'Moviendo...' : 'Confirmar'}
                    </Button>
                  </div>
                </div>
              )}
              {selectedReservation.description && (
                <div className="pt-2 border-t border-primary-100">
                  <p className="text-xs text-primary-400 mb-1 font-medium">Descripción</p>
                  <p className="text-sm text-primary-600">{selectedReservation.description}</p>
                </div>
              )}
              {selectedReservation.recurrenceRule && (
                <div className="pt-2 border-t border-primary-100">
                  <p className="text-xs text-primary-400 mb-1 font-medium">Reservación periódica</p>
                  <p className="text-sm text-primary-600">{formatRecurrenceRule(selectedReservation.recurrenceRule)}</p>
                </div>
              )}
              {selectedReservation.recurrenceRule && (selectedReservation.status === 'Approved' || selectedReservation.status === 'Pending') && !movingReservation && (
                <div className="pt-2 border-t border-primary-100">
                  <div className="flex items-start gap-2 p-2 rounded-lg bg-red-50 border border-red-200 mb-2">
                    <AlertTriangle className="h-4 w-4 text-red-500 mt-0.5 shrink-0" />
                    <p className="text-xs text-red-700">
                      Se cancelarán esta clase y todas las futuras de la reservación periódica. El aula quedará libre para nuevas reservas.
                    </p>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    className="w-full border-red-300 text-red-600 hover:bg-red-50"
                    disabled={cancelFutureMutation.isPending}
                    onClick={() => {
                      if (window.confirm('¿Cancelar esta clase y todas las futuras?')) {
                        cancelFutureMutation.mutate(selectedReservation.id);
                      }
                    }}
                  >
                    {cancelFutureMutation.isPending ? 'Cancelando...' : 'Cancelar desde próxima clase'}
                  </Button>
                </div>
              )}
              <div className="pt-2 border-t border-primary-100">
                <Button variant="ghost" size="sm" className="w-full" onClick={() => setSelectedReservation(null)}>
                  Salir
                </Button>
              </div>
            </DialogBody>
          </DialogContent>
        )}
      </Dialog>
    </div>
  );
}
