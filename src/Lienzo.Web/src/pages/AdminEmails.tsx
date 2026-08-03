import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Mail, MailOpen, Inbox, Download, CalendarPlus, Paperclip, ClipboardCheck, ChevronLeft, ChevronRight } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Select } from '@/components/ui/Select';
import { Skeleton } from '@/components/ui/Skeleton';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import type { PaginatedResponse, Classroom } from '@/types';

interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  avatarUrl: string | null;
  isActive: boolean;
  createdAt: string;
}

interface EmailSummary {
  uid: string;
  from: string;
  fromName: string;
  subject: string;
  date: string;
  hasAttachment: boolean;
  isRead: boolean;
  isProcessed: boolean;
  snippet?: string;
}

interface EmailDetail {
  uid: string;
  from: string;
  fromName: string;
  subject: string;
  date: string;
  hasAttachment: boolean;
  isRead: boolean;
  isProcessed: boolean;
  bodyText?: string;
  bodyHtml?: string;
  attachments: { name: string; contentType: string; size: number }[];
  reservationId?: string | null;
  requiresAccessoryConfirmation?: boolean;
  accessoriesConfirmed?: boolean;
}

export default function AdminEmails() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const pageSize = 20;
  const [selected, setSelected] = useState<EmailDetail | null>(null);
  const [showCreate, setShowCreate] = useState(false);
  const [createMode, setCreateMode] = useState<'create' | 'pending' | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [resending, setResending] = useState(false);

  const { data: response, isLoading } = useQuery({
    queryKey: ['emails', page],
    queryFn: () => api.get<PaginatedResponse<EmailSummary>>(`/emails?page=${page}&pageSize=${pageSize}`),
  });
  const emails = response?.value ?? [];
  const totalPages = response?.totalPages ?? 1;

  const openDetail = async (uid: string) => {
    setDetailLoading(true);
    setSelected(null);
    try {
      const data = await api.get<EmailDetail>(`/emails/${uid}`);
      setSelected(data);
    } catch (err: any) {
      alert(err?.message || 'Error al abrir el correo');
    } finally {
      setDetailLoading(false);
    }
  };

  const downloadRaw = async (uid: string) => {
    try {
      const token = localStorage.getItem('lienzo_token');
      const response = await fetch(`/api/emails/${uid}/download`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${uid}.eml`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err: any) {
      alert(err?.message || 'Error al descargar el correo');
    }
  };

  const resendAccessoryConfirmation = async (reservationId: string) => {
    setResending(true);
    try {
      await api.post(`/reservations/${reservationId}/accessories/resend`);
      alert('Se envió el correo de confirmación de accesorios al solicitante.');
    } catch (err: any) {
      alert(err?.message || 'Error al enviar el correo de confirmación');
    } finally {
      setResending(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Bandeja de Correo</h1>
        <p className="text-primary-500 mt-1">Correos recibidos para gestionar reservas con evidencia legal</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-0">
            <div className="flex items-center justify-between px-4 py-3 border-b border-primary-100">
              <div className="flex items-center gap-2 text-primary-700">
                <Inbox className="h-4 w-4" />
                <span className="text-sm font-medium">Recibidos</span>
              </div>
            </div>
            {isLoading ? (
              <div className="space-y-2 p-4">
                {[1, 2, 3, 4, 5].map((i) => (
                  <Skeleton key={i} variant="rectangular" className="h-16 w-full" />
                ))}
              </div>
            ) : emails.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-16 text-center">
                <Mail className="h-12 w-12 text-primary-200 mb-3" />
                <p className="text-sm text-primary-500">No hay correos disponibles</p>
              </div>
            ) : (
              <div className="divide-y divide-primary-100 max-h-[70vh] overflow-y-auto">
                {emails.map((email) => (
                  <button
                    key={email.uid}
                    className={`w-full text-left px-4 py-3 hover:bg-primary-50 transition-colors ${selected?.uid === email.uid ? 'bg-accent-50' : ''} ${!email.isRead ? 'bg-white' : ''}`}
                    onClick={() => openDetail(email.uid)}
                  >
                    <div className="flex items-center gap-2">
                      {!email.isRead ? (
                        <Mail className="h-4 w-4 text-accent-500 flex-shrink-0" />
                      ) : (
                        <MailOpen className="h-4 w-4 text-primary-300 flex-shrink-0" />
                      )}
                      <span className="flex-1 text-sm font-medium text-primary-800 truncate">
                        {email.subject || '(sin asunto)'}
                      </span>
                      {email.hasAttachment && <Paperclip className="h-3.5 w-3.5 text-primary-400 flex-shrink-0" />}
                      {email.isProcessed && (
                        <Badge variant="approved" className="flex-shrink-0">Procesado</Badge>
                      )}
                    </div>
                    <div className="flex items-center gap-2 mt-0.5 ml-6">
                      <span className="text-xs text-primary-500 truncate">{email.fromName || email.from}</span>
                      <span className="text-xs text-primary-300 flex-shrink-0">
                        {new Date(email.date).toLocaleDateString('es-AR', { day: '2-digit', month: '2-digit', year: 'numeric' })}
                      </span>
                    </div>
                    {email.snippet && (
                      <p className="text-xs text-primary-400 truncate ml-6 mt-0.5">{email.snippet}</p>
                    )}
                  </button>
                ))}
              </div>
            )}
            {totalPages > 1 && (
              <div className="flex items-center justify-center gap-2 py-3 border-t border-primary-100">
                <button
                  className="p-2 rounded-lg text-primary-500 hover:bg-primary-50 disabled:opacity-30"
                  disabled={page <= 1}
                  onClick={() => setPage((p) => Math.max(1, p - 1))}
                >
                  <ChevronLeft className="h-4 w-4" />
                </button>
                <span className="text-sm text-primary-500">Página {page} de {totalPages}</span>
                <button
                  className="p-2 rounded-lg text-primary-500 hover:bg-primary-50 disabled:opacity-30"
                  disabled={page >= totalPages}
                  onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                >
                  <ChevronRight className="h-4 w-4" />
                </button>
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-0">
            <div className="px-4 py-3 border-b border-primary-100 flex items-center justify-between">
              <span className="text-sm font-medium text-primary-700">Detalle</span>
              {selected && (
                <div className="flex items-center gap-2">
                  {!selected.isProcessed && (
                    <>
                      <Button
                        variant="accent"
                        size="sm"
                        onClick={() => { setCreateMode('create'); setShowCreate(true); }}
                      >
                        <CalendarPlus className="h-4 w-4 mr-1" />
                        Crear Reserva
                      </Button>
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => { setCreateMode('pending'); setShowCreate(true); }}
                      >
                        <ClipboardCheck className="h-4 w-4 mr-1" />
                        Crear y pedir accesorios
                      </Button>
                    </>
                  )}
                  {selected.isProcessed && selected.reservationId && !selected.accessoriesConfirmed && (
                    <Button
                      variant="accent"
                      size="sm"
                      loading={resending}
                      onClick={() => resendAccessoryConfirmation(selected.reservationId!)}
                    >
                      <ClipboardCheck className="h-4 w-4 mr-1" />
                      Solicitar confirmación de accesorios
                    </Button>
                  )}
                  <Button variant="ghost" size="sm" onClick={() => downloadRaw(selected.uid)}>
                    <Download className="h-4 w-4 mr-1" />
                    .eml
                  </Button>
                </div>
              )}
            </div>
            <div className="max-h-[70vh] overflow-y-auto">
              {detailLoading ? (
                <div className="space-y-3 p-4">
                  <Skeleton variant="rectangular" className="h-6 w-3/4" />
                  <Skeleton variant="rectangular" className="h-4 w-1/2" />
                  <Skeleton variant="rectangular" className="h-40 w-full" />
                </div>
              ) : selected ? (
                <div className="p-4 space-y-3">
                  <h3 className="font-medium text-primary-800">{selected.subject || '(sin asunto)'}</h3>
                  <div className="text-sm text-primary-500 space-y-0.5">
                    <p><span className="font-medium text-primary-700">De:</span> {selected.fromName || selected.from} ({selected.from})</p>
                    <p className="text-xs text-primary-400">
                      {new Date(selected.date).toLocaleString('es-AR', { dateStyle: 'long', timeStyle: 'short' })}
                    </p>
                  </div>
                  {selected.isProcessed && (
                    <Badge variant="approved">Ya procesado para una reserva</Badge>
                  )}
                  {selected.requiresAccessoryConfirmation && (
                    <Badge variant={selected.accessoriesConfirmed ? 'approved' : 'pending'}>
                      {selected.accessoriesConfirmed ? 'Accesorios confirmados' : 'Pendiente confirmación de accesorios'}
                    </Badge>
                  )}
                  {selected.attachments.length > 0 && (
                    <div className="text-xs text-primary-500">
                      Adjuntos: {selected.attachments.map((a) => `${a.name} (${(a.size / 1024).toFixed(1)} KB)`).join(', ')}
                    </div>
                  )}
                  <div className="text-sm text-primary-700 whitespace-pre-wrap bg-primary-50 rounded-lg p-3 border border-primary-100">
                    {selected.bodyText || '(sin contenido de texto)'}
                  </div>
                </div>
              ) : (
                <div className="flex flex-col items-center justify-center py-16 text-center">
                  <Mail className="h-12 w-12 text-primary-200 mb-3" />
                  <p className="text-sm text-primary-500">Selecciona un correo para ver su detalle</p>
                </div>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      {selected && createMode && (
        <CreateReservationDialog
          open={showCreate}
          onOpenChange={setShowCreate}
          email={selected}
          requestAccessoryConfirmation={createMode === 'pending'}
          onSuccess={() => {
            setShowCreate(false);
            setCreateMode(null);
            setSelected(null);
            queryClient.invalidateQueries({ queryKey: ['emails'] });
          }}
        />
      )}
    </div>
  );
}

function CreateReservationDialog({ open, onOpenChange, email, requestAccessoryConfirmation, onSuccess }: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  email: EmailDetail;
  requestAccessoryConfirmation: boolean;
  onSuccess: () => void;
}) {
  const [assignedUserId, setAssignedUserId] = useState('');
  const [classroomId, setClassroomId] = useState('');
  const [title, setTitle] = useState(email.subject || '');
  const [description, setDescription] = useState(email.bodyText || '');
  const [date, setDate] = useState('');
  const [startTime, setStartTime] = useState('08:00');
  const [endTime, setEndTime] = useState('10:00');

  const { data: users } = useQuery({
    queryKey: ['users', 'all'],
    queryFn: () => api.get<AdminUser[]>('/users'),
  });

  const { data: classrooms } = useQuery({
    queryKey: ['classrooms', 'active'],
    queryFn: () => api.get<Classroom[]>('/classrooms'),
  });

  const mutation = useMutation({
    mutationFn: () =>
      api.post(`/emails/${email.uid}/reservation`, {
        emailUid: email.uid,
        assignedUserId,
        classroomId,
        title,
        description,
        date,
        startTime,
        endTime,
        requestAccessoryConfirmation,
      }),
    onSuccess: () => {
      alert(requestAccessoryConfirmation
        ? 'Se creó la reserva como pendiente y se envió el correo al solicitante para confirmar los accesorios.'
        : 'Reserva creada correctamente con evidencia del correo');
      onSuccess();
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al crear la reserva');
    },
  });

  const canSubmit = assignedUserId && classroomId && title && date && startTime && endTime && !mutation.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>{requestAccessoryConfirmation ? 'Crear Reserva y Confirmar Accesorios' : 'Crear Reserva desde Correo'}</DialogTitle>
          <DialogDescription>
            {requestAccessoryConfirmation
              ? 'La reserva quedará pendiente hasta que el solicitante confirme los accesorios'
              : 'La reserva quedará vinculada al correo como evidencia legal'}
          </DialogDescription>
        </DialogHeader>
        <DialogBody className="space-y-4">
          <div className="bg-primary-50 rounded-lg p-3 border border-primary-100 text-sm text-primary-600 space-y-1">
            <p className="font-medium text-primary-800">{email.subject}</p>
            <p className="text-xs">De: {email.fromName || email.from} · {new Date(email.date).toLocaleString('es-AR')}</p>
          </div>
          <Select
            label="Usuario asignado (dueño de la reserva)"
            placeholder="Seleccionar usuario"
            value={assignedUserId}
            onValueChange={setAssignedUserId}
            options={(users || []).map((u) => ({
              value: u.id,
              label: `${u.firstName} ${u.lastName} (${u.role === 'Admin' ? 'Administrador' : u.role === 'Teacher' ? 'Profesor' : 'Estudiante'})`,
            }))}
          />
          <Select
            label="Aula"
            placeholder="Seleccionar aula"
            value={classroomId}
            onValueChange={setClassroomId}
            options={(classrooms || [])
              .sort((a, b) => a.name.localeCompare(b.name, 'es', { numeric: true }))
              .map((c) => ({ value: c.id, label: `${c.name} (Piso ${c.floor})` }))}
          />
          <Input
            label="Título"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <Textarea
            label="Descripción"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={4}
          />
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <Input
              label="Fecha"
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
            />
            <Input
              label="Inicio"
              type="time"
              value={startTime}
              onChange={(e) => setStartTime(e.target.value)}
            />
            <Input
              label="Fin"
              type="time"
              value={endTime}
              onChange={(e) => setEndTime(e.target.value)}
            />
          </div>
        </DialogBody>
        <DialogFooter>
          <Button variant="ghost" onClick={() => onOpenChange(false)}>
            Cancelar
          </Button>
          <Button
            variant="accent"
            onClick={() => mutation.mutate()}
            disabled={!canSubmit}
            loading={mutation.isPending}
          >
            {requestAccessoryConfirmation ? 'Crear y Pedir Accesorios' : 'Crear Reserva'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
