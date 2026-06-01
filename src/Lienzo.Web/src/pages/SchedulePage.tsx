import { useState, useMemo } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import {
  ChevronLeft, ChevronRight, Building2, Repeat, CalendarDays, User, Clock, MapPin, Tag, Calendar, Plus,
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
import type { Building, Classroom, Reservation } from '@/types';

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

function toMinutes(time: string): number {
  const [h, m] = time.split(':').map(Number);
  return h * 60 + m;
}

export default function SchedulePage() {
  const queryClient = useQueryClient();
  const today = new Date();
  const [currentDate, setCurrentDate] = useState(today);
  const [buildingId, setBuildingId] = useState('');
  const [classroomId, setClassroomId] = useState('');
  const [colorMode, setColorMode] = useState<'status' | 'period'>('status');

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
    if (classroomId) return classrooms.filter((c) => c.id === classroomId);
    return classrooms;
  }, [classrooms, classroomId]);

  const { data: reservations, isLoading } = useQuery({
    queryKey: ['schedule', dateStr, buildingId, classroomId],
    queryFn: () =>
      api.get<Reservation[]>(
        `/reservations/schedule?fromDate=${dateStr}&toDate=${dateStr}${buildingId ? `&buildingId=${buildingId}` : ''}${classroomId ? `&classroomId=${classroomId}` : ''}`
      ),
    enabled: !!buildingId,
  });

  const { data: holidaysResponse } = useQuery({
    queryKey: ['holidays'],
    queryFn: () => api.get<{ id: string; date: string; description: string }[]>('/holidays'),
  });
  const holidays = holidaysResponse || [];
  const isUserHoliday = holidays.find((h) => h.date === dateStr);

  const [selectedReservation, setSelectedReservation] = useState<Reservation | null>(null);

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
        </div>
      </div>

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
          ) : (
            <div className="overflow-x-auto">
              <div className="min-w-[900px]">
                <div className="flex border-b border-primary-200 bg-primary-50/50">
                  <div className="w-44 flex-shrink-0 p-2 text-xs font-medium text-primary-500 uppercase tracking-wider">
                    Aula
                  </div>
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
                      <div key={classroom.id} className="flex border-b border-primary-100 last:border-b-0">
                        <div className="w-44 flex-shrink-0 p-2 flex items-center text-sm font-medium text-primary-700 border-r border-primary-100" title={`${classroom.name} (Piso ${classroom.floor})`}>
                          {classroom.name}
                          <span className="ml-1 text-xs text-primary-400 font-normal">
                            (P{classroom.floor})
                          </span>
                        </div>
                        <div
                          className="flex-1 relative min-h-[48px] cursor-pointer"
                          onClick={(e) => handleTimelineClick(e, classroom)}
                        >
                          {visibleHours.map((h) => (
                              <div
                                key={h}
                                className="absolute top-0 bottom-0 border-l border-primary-100 pointer-events-none"
                                style={{ left: `${((h - 7) / 15) * 100}%`, width: `${(1 / 15) * 100}%` }}
                              />
                          ))}
                          {/* Empty state hint when no reservations */}
                          {classroomReservations.length === 0 && (
                            <div className="absolute inset-0 flex items-center justify-center opacity-0 hover:opacity-100 transition-opacity">
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
                      </div>
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

      <Dialog open={!!selectedReservation} onOpenChange={(open) => { if (!open) setSelectedReservation(null); }}>
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
            </DialogBody>
          </DialogContent>
        )}
      </Dialog>
    </div>
  );
}
