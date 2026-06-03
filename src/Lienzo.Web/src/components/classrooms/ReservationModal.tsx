import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Textarea } from '@/components/ui/Textarea';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { AlertTriangle, Loader2, Repeat, BookOpen } from 'lucide-react';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import type { Reservation, Classroom } from '@/types';

function parseDateLocal(dateStr: string): Date {
  const [y, m, d] = dateStr.split('-').map(Number);
  return new Date(y, m - 1, d);
}

const DAYS_OF_WEEK = [
  { value: 'Monday', label: 'Lun' },
  { value: 'Tuesday', label: 'Mar' },
  { value: 'Wednesday', label: 'Mié' },
  { value: 'Thursday', label: 'Jue' },
  { value: 'Friday', label: 'Vie' },
  { value: 'Saturday', label: 'Sáb' },
] as const;

const reservationSchema = z.object({
  date: z.string().min(1, 'La fecha es requerida'),
  startTime: z.string().min(1, 'La hora de inicio es requerida'),
  endTime: z.string().min(1, 'La hora de fin es requerida'),
  title: z.string().min(3, 'El título debe tener al menos 3 caracteres').max(100),
  description: z.string().max(500).optional(),
  isRecurring: z.boolean().optional(),
  endDate: z.string().optional(),
}).refine(
  (data) => {
    if (!data.date) return true;
    const d = parseDateLocal(data.date);
    return d.getDay() !== 0;
  },
  { message: 'No se permiten reservas los domingos', path: ['date'] }
).refine(
  (data) => {
    if (!data.date || !data.startTime || !data.endTime) return true;
    const d = parseDateLocal(data.date);
    if (d.getDay() !== 6) return true;
    return data.startTime < '16:00' && data.endTime <= '16:00';
  },
  { message: 'Los sábados solo se permite reservar hasta las 16:00', path: ['endTime'] }
).refine(
  (data) => {
    if (!data.startTime || !data.endTime) return true;
    return data.startTime < data.endTime;
  },
  { message: 'La hora de fin debe ser posterior a la de inicio', path: ['endTime'] }
).refine(
  (data) => {
    if (!data.isRecurring || !data.endDate) return true;
    return data.endDate >= data.date;
  },
  { message: 'La fecha fin debe ser posterior o igual a la fecha inicio', path: ['endDate'] }
);

type ReservationFormData = z.infer<typeof reservationSchema>;

interface ReservationModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  classroom?: Classroom;
  reservation?: Reservation;
  onSuccess?: () => void;
  defaultDate?: string;
  defaultStartTime?: string;
  defaultEndTime?: string;
  defaultActividadId?: string;
}

export function ReservationModal({ open, onOpenChange, classroom, reservation, onSuccess, defaultDate, defaultStartTime, defaultEndTime, defaultActividadId }: ReservationModalProps) {
  const [conflict, setConflict] = useState<{ hasConflict: boolean } | null>(null);
  const [checkingConflict, setCheckingConflict] = useState(false);
  const [selectedDays, setSelectedDays] = useState<string[]>([]);
  const [selectedActividadId, setSelectedActividadId] = useState('');
  const [selectedBuildingId, setSelectedBuildingId] = useState('');
  const [selectedClassroomId, setSelectedClassroomId] = useState('');
  const isEditing = !!reservation;
  const needsClassroomSelection = !classroom && !reservation;
  const classroomId = classroom?.id || reservation?.classroomId || (needsClassroomSelection ? selectedClassroomId : '');
  const classroomName = classroom?.name || reservation?.classroomName || '';

  const { register, handleSubmit, formState: { errors }, watch, reset, setValue } = useForm<ReservationFormData>({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      date: '',
      startTime: '08:00',
      endTime: '10:00',
      title: '',
      description: '',
      isRecurring: false,
      endDate: '',
    },
  });

  useEffect(() => {
    if (open) {
      reset({
        date: reservation ? reservation.date.split('T')[0] : defaultDate || (() => {
          const now = new Date();
          const y = now.getFullYear();
          const m = String(now.getMonth() + 1).padStart(2, '0');
          const d = String(now.getDate()).padStart(2, '0');
          return `${y}-${m}-${d}`;
        })(),
        startTime: reservation ? reservation.startTime.slice(0, 5) : defaultStartTime || '08:00',
        endTime: reservation ? reservation.endTime.slice(0, 5) : defaultEndTime || '10:00',
        title: reservation ? reservation.title : '',
        description: reservation ? reservation.description || '' : '',
        isRecurring: false,
        endDate: '',
      });
      setSelectedDays([]);
      setSelectedActividadId(reservation ? (reservation as any).actividadId || '' : defaultActividadId || '');
      setConflict(null);
      setSelectedBuildingId('');
      setSelectedClassroomId('');
    }
  }, [open, reservation, defaultDate, defaultStartTime, defaultEndTime, defaultActividadId, reset]);

  const { data: holidaysResponse } = useQuery({
    queryKey: ['holidays'],
    queryFn: () => api.get<{ id: string; date: string; description: string }[]>('/holidays'),
    enabled: open,
  });
  const holidays = holidaysResponse || [];

  const { data: actividadesRes } = useQuery({
    queryKey: ['actividades'],
    queryFn: () => api.get<{ id: string; nombre: string; codigoMateria: string; periodoNombre?: string; carreraNombre?: string }[]>('/actividades'),
    enabled: open,
  });
  const actividades = actividadesRes || [];

  const { data: buildings } = useQuery({
    queryKey: ['buildings'],
    queryFn: () => api.get<{ id: string; name: string }[]>('/buildings'),
    enabled: open && needsClassroomSelection,
  });

  const { data: classrooms } = useQuery({
    queryKey: ['classrooms', selectedBuildingId],
    queryFn: () => {
      const params: Record<string, string> = {};
      if (selectedBuildingId) params.buildingId = selectedBuildingId;
      return api.get<{ id: string; name: string; floor: number; capacity: number }[]>('/classrooms', params);
    },
    enabled: open && needsClassroomSelection,
  });

  const handleActividadChange = (id: string) => {
    setSelectedActividadId(id);
    if (!isEditing) {
      const act = actividades.find((a) => a.id === id);
      if (act) setValue('title', act.nombre);
    }
  };

  const watchDate = watch('date');
  const watchStart = watch('startTime');
  const watchEnd = watch('endTime');
  const isRecurring = watch('isRecurring');

  const toggleDay = (day: string) => {
    setSelectedDays((prev) =>
      prev.includes(day) ? prev.filter((d) => d !== day) : [...prev, day]
    );
  };

  const checkConflict = async () => {
    if (!watchDate || !watchStart || !watchEnd || !classroomId) return;
    setCheckingConflict(true);
    try {
      const data = await api.get<{ isAvailable: boolean }>(
        `/classrooms/${classroomId}/availability?date=${watchDate}&startTime=${watchStart}&endTime=${watchEnd}`
      );
      setConflict({ hasConflict: !data.isAvailable });
    } catch {
      setConflict(null);
    } finally {
      setCheckingConflict(false);
    }
  };

  const mutation = useMutation({
    mutationFn: (formData: ReservationFormData) => {
      if (isEditing) {
        return api.put(`/reservations/${reservation.id}`, { title: formData.title, description: formData.description });
      }

      const body: Record<string, unknown> = {
        classroomId,
        title: formData.title,
        description: formData.description || '',
        date: formData.date,
        startTime: formData.startTime,
        endTime: formData.endTime,
      };

      if (formData.isRecurring && selectedDays.length > 0 && formData.endDate) {
        body.daysOfWeek = selectedDays.join(',');
        body.endDate = formData.endDate;
      }

      if (selectedActividadId) {
        body.actividadId = selectedActividadId;
      }

      return api.post('/reservations', body);
    },
    onSuccess: () => {
      reset();
      setConflict(null);
      setSelectedDays([]);
      onSuccess?.();
      onOpenChange(false);
    },
  });

  const onSubmit = async (formData: ReservationFormData) => {
    if (!isEditing) {
      if (holidays.find((h) => h.date === formData.date)) {
        setConflict({ hasConflict: true });
        return;
      }
      if (!conflict) {
        await checkConflict();
      }
      if (conflict?.hasConflict) return;
    }
    mutation.mutate(formData);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{isEditing ? 'Editar Reservación' : needsClassroomSelection ? 'Nueva Reservación' : `Reservar ${classroomName}`}</DialogTitle>
          <DialogDescription>
            {classroomName ? `${classroomName}${classroom ? ` - Piso ${classroom.floor} · Capacidad: ${classroom.capacity}` : ''}` : 'Selecciona un aula y completa los datos'}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)}>
          <DialogBody className="space-y-4">
            {needsClassroomSelection && (
              <>
                <div className="w-full">
                  <Select
                    label="Edificio"
                    placeholder="Seleccionar edificio"
                    value={selectedBuildingId}
                    onValueChange={(v) => { setSelectedBuildingId(v); setSelectedClassroomId(''); }}
                    options={(buildings || []).map((b: any) => ({ value: b.id, label: b.name }))}
                  />
                </div>
                <div className="w-full">
                  <Select
                    label="Aula"
                    placeholder="Seleccionar aula"
                    value={selectedClassroomId}
                    onValueChange={(v) => setSelectedClassroomId(v)}
                    options={(classrooms || []).map((c: any) => ({ value: c.id, label: `${c.name} (Piso ${c.floor})` }))}
                  />
                </div>
              </>
            )}

            <Input
              type="date"
              label="Fecha"
              error={errors.date?.message}
              disabled={isEditing}
              {...register('date')}
            />
            {watchDate && parseDateLocal(watchDate).getDay() === 0 && (
              <p className="text-xs text-red-500 -mt-2">No se permiten reservas los domingos</p>
            )}
            {watchDate && parseDateLocal(watchDate).getDay() === 6 && (
              <p className="text-xs text-amber-600 -mt-2">Los sábados el horario máximo es hasta las 16:00</p>
            )}
            {watchDate && holidays.find((h) => h.date === watchDate) && (
              <p className="text-xs text-red-500 -mt-2">No se permiten reservas en días feriados</p>
            )}

            <div className="grid grid-cols-2 gap-3">
              <Input
                type="time"
                label="Hora inicio"
                error={errors.startTime?.message}
                disabled={isEditing}
                {...register('startTime')}
                onBlur={(e) => { register('startTime').onBlur(e); checkConflict(); }}
              />
              <Input
                type="time"
                label="Hora fin"
                error={errors.endTime?.message}
                disabled={isEditing}
                {...register('endTime')}
                onBlur={(e) => { register('endTime').onBlur(e); checkConflict(); }}
              />
            </div>

            <div>
              <SearchableSelect
                label="Actividad (opcional)"
                placeholder="Buscar actividad..."
                value={selectedActividadId}
                onChange={handleActividadChange}
                options={actividades.map((a) => ({
                  value: a.id,
                  label: `${a.nombre} (${a.codigoMateria})${a.periodoNombre ? ` - ${a.periodoNombre}` : ''}`,
                }))}
              />
            </div>

            <Input
              label="Título"
              placeholder="Ej: Clase de Matemáticas"
              error={errors.title?.message}
              {...register('title')}
            />

            <Textarea
              label="Descripción (opcional)"
              placeholder="Notas adicionales..."
              {...register('description')}
            />

            {!isEditing && (
              <>
                <label className="flex items-center gap-2 cursor-pointer">
                  <input
                    type="checkbox"
                    className="rounded border-primary-300 text-accent-500 focus:ring-accent-500"
                    {...register('isRecurring')}
                  />
                  <span className="flex items-center gap-1.5 text-sm font-medium text-primary-700">
                    <Repeat className="h-4 w-4" />
                    Reservación periódica
                  </span>
                </label>

                {isRecurring && (
                  <div className="space-y-3 pl-6 border-l-2 border-accent-200">
                    <div>
                      <p className="text-sm font-medium text-primary-700 mb-2">Días de la semana</p>
                      <div className="flex flex-wrap gap-2">
                        {DAYS_OF_WEEK.map((day) => (
                          <button
                            key={day.value}
                            type="button"
                            onClick={() => toggleDay(day.value)}
                            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition-colors ${
                              selectedDays.includes(day.value)
                                ? 'bg-accent-500 text-white'
                                : 'bg-primary-100 text-primary-600 hover:bg-primary-200'
                            }`}
                          >
                            {day.label}
                          </button>
                        ))}
                      </div>
                      {selectedDays.length === 0 && (
                        <p className="text-xs text-red-500 mt-1">Selecciona al menos un día</p>
                      )}
                    </div>

                    <Input
                      type="date"
                      label="Fecha fin"
                      error={errors.endDate?.message}
                      {...register('endDate')}
                    />
                    {selectedDays.includes('Saturday') && (
                      <p className="text-xs text-amber-600">Las reservas de sábado terminan a las 16:00</p>
                    )}
                  </div>
                )}
              </>
            )}

            {checkingConflict && (
              <div className="flex items-center gap-2 text-sm text-primary-500">
                <Loader2 className="h-4 w-4 animate-spin" />
                Verificando disponibilidad...
              </div>
            )}

            {conflict?.hasConflict && !isEditing && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3">
                <div className="flex items-start gap-2">
                  <AlertTriangle className="h-5 w-5 text-red-500 flex-shrink-0 mt-0.5" />
                  <div>
                    <p className="text-sm font-medium text-red-800">Conflicto de horario</p>
                    <p className="text-xs text-red-600 mt-1">
                      El aula ya está reservada en este horario.
                    </p>
                  </div>
                </div>
              </div>
            )}

            {mutation.isError && (
              <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
                {mutation.error instanceof Error ? mutation.error.message : `Error al ${isEditing ? 'actualizar' : 'crear'} la reservación`}
              </div>
            )}
          </DialogBody>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button
              type="submit"
              variant="accent"
              loading={mutation.isPending}
              disabled={(!isEditing && (conflict?.hasConflict || checkingConflict)) || (!isEditing && isRecurring && selectedDays.length === 0) || (needsClassroomSelection && !classroomId)}
            >
              {isEditing ? 'Guardar cambios' : conflict?.hasConflict ? 'Conflicto detectado' : isRecurring ? 'Crear reservaciones' : 'Reservar'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
