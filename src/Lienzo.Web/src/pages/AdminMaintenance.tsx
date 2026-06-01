import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, Wrench, AlertTriangle } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { Badge } from '@/components/ui/Badge';

interface MaintenanceBlock {
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

interface MaintenanceListResponse {
  items: MaintenanceBlock[];
  totalCount: number;
}

export default function AdminMaintenance() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [formClassroomId, setFormClassroomId] = useState('');
  const [formStart, setFormStart] = useState('');
  const [formEnd, setFormEnd] = useState('');
  const [formReason, setFormReason] = useState('');
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['maintenance'],
    queryFn: () => api.get<MaintenanceListResponse>('/maintenance', { activeOnly: 'true' }),
  });

  const { data: classrooms } = useQuery({
    queryKey: ['classrooms'],
    queryFn: () => api.get<{ id: string; name: string }[]>('/classrooms'),
  });

  const createMutation = useMutation({
    mutationFn: (body: { classroomId: string; startTime: string; endTime: string; reason: string }) =>
      api.post('/maintenance', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['maintenance'] });
      setDialogOpen(false);
      setFormClassroomId('');
      setFormStart('');
      setFormEnd('');
      setFormReason('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/maintenance/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['maintenance'] }),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formClassroomId || !formStart || !formEnd || !formReason.trim()) return;
    createMutation.mutate({
      classroomId: formClassroomId,
      startTime: new Date(formStart).toISOString(),
      endTime: new Date(formEnd).toISOString(),
      reason: formReason.trim(),
    });
  };

  const items = data?.items || [];

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Mantenimiento</h1>
          <p className="text-primary-500 mt-1">Bloquea aulas por mantenimiento y gestiona las reservas afectadas</p>
        </div>
        <Button onClick={() => setDialogOpen(true)}>
          <Plus className="h-4 w-4 mr-2" />
          Nuevo bloque
        </Button>
      </div>

      {isLoading ? (
        <div className="text-center py-16 text-primary-400">Cargando...</div>
      ) : items.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <Wrench className="h-12 w-12 mx-auto mb-3 opacity-50" />
          <p className="font-medium">No hay bloques de mantenimiento activos</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Aula</TableHead>
                <TableHead>Edificio</TableHead>
                <TableHead>Inicio</TableHead>
                <TableHead>Fin</TableHead>
                <TableHead className="hidden sm:table-cell">Motivo</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead className="w-20 text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.map((b) => (
                <TableRow key={b.id}>
                  <TableCell className="font-medium">{b.classroomName}</TableCell>
                  <TableCell className="text-primary-400 text-sm">{b.buildingName || '—'}</TableCell>
                  <TableCell className="text-primary-500 text-sm">
                    {new Date(b.startTime).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })}
                  </TableCell>
                  <TableCell className="text-primary-500 text-sm">
                    {new Date(b.endTime).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })}
                  </TableCell>
                  <TableCell className="hidden sm:table-cell text-primary-400 text-sm max-w-[200px] truncate">{b.reason}</TableCell>
                  <TableCell>
                    <Badge variant="pending">
                      <AlertTriangle className="h-3 w-3 mr-1" />
                      Activo
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    <button
                      className="p-1.5 rounded-md text-red-500 hover:bg-red-50"
                      onClick={() => deleteMutation.mutate(b.id)}
                      title="Finalizar bloque"
                    >
                      <Trash2 className="h-4 w-4" />
                    </button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Nuevo bloque de mantenimiento</DialogTitle>
            <DialogDescription>Las reservas que se superpongan serán canceladas automáticamente y los usuarios notificados.</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmit}>
            <DialogBody className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1">Aula</label>
                <select
                  className="w-full h-10 px-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
                  value={formClassroomId}
                  onChange={e => setFormClassroomId(e.target.value)}
                  required
                >
                  <option value="">Seleccionar aula...</option>
                  {(classrooms || []).map(c => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>
              <Input label="Inicio" type="datetime-local" value={formStart} onChange={e => setFormStart(e.target.value)} required />
              <Input label="Fin" type="datetime-local" value={formEnd} onChange={e => setFormEnd(e.target.value)} required />
              <Textarea label="Motivo" placeholder="Ej: Mantenimiento de aire acondicionado" value={formReason} onChange={e => setFormReason(e.target.value)} required />
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>Cancelar</Button>
              <Button type="submit" variant="destructive" loading={createMutation.isPending}>
                Bloquear aula
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
