import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { KeyRound, Plus, ArrowLeftRight, Undo2, Search, Building2, User, Clock } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { Badge } from '@/components/ui/Badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';

interface Classroom {
  id: string;
  name: string;
  buildingName?: string;
}

interface UserInfo {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
}

interface NextReservation {
  reservationId: string;
  professorName: string;
  professorUserId: string;
  startTime: string;
  endTime: string;
}

interface KeyDelivery {
  id: string;
  classroomId: string;
  classroomName: string;
  buildingName?: string;
  deliveredToUserId?: string;
  deliveredToName: string;
  deliveredById: string;
  deliveredByName?: string;
  deliveredAt: string;
  returnedAt?: string;
  notes?: string;
  nextReservation?: NextReservation;
}

interface KeyDeliveryListResponse {
  items: KeyDelivery[];
  totalCount: number;
}

export default function AdminBedelia() {
  const [tab, setTab] = useState('active');
  const [deliverOpen, setDeliverOpen] = useState(false);
  const [returnConfirmId, setReturnConfirmId] = useState<string | null>(null);
  const [historySearch, setHistorySearch] = useState('');
  const [formClassroomId, setFormClassroomId] = useState('');
  const [formUserId, setFormUserId] = useState('');
  const [formOtherName, setFormOtherName] = useState('');
  const [formNotes, setFormNotes] = useState('');
  const queryClient = useQueryClient();

  const { data: classrooms } = useQuery({
    queryKey: ['classrooms'],
    queryFn: () => api.get<Classroom[]>('/classrooms'),
  });

  const { data: users } = useQuery({
    queryKey: ['users'],
    queryFn: () => api.get<UserInfo[]>('/users'),
  });

  const { data: activeData, isLoading: activeLoading } = useQuery({
    queryKey: ['keydelivery-active'],
    queryFn: () => api.get<KeyDeliveryListResponse>('/keydelivery/active'),
    refetchInterval: 15_000,
  });

  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ['keydelivery-history'],
    queryFn: () => api.get<KeyDeliveryListResponse>('/keydelivery/history'),
    enabled: tab === 'history',
  });

  const deliverMutation = useMutation({
    mutationFn: (body: { classroomId: string; deliveredToUserId?: string; deliveredToName: string; notes?: string }) =>
      api.post('/keydelivery/deliver', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['keydelivery-active'] });
      setDeliverOpen(false);
      resetForm();
    },
  });

  const returnMutation = useMutation({
    mutationFn: (id: string) => api.post(`/keydelivery/${id}/return`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['keydelivery-active'] });
      queryClient.invalidateQueries({ queryKey: ['keydelivery-history'] });
      setReturnConfirmId(null);
    },
  });

  const transferMutation = useMutation({
    mutationFn: ({ id, userId, userName }: { id: string; userId: string; userName: string }) =>
      api.post(`/keydelivery/${id}/transfer`, { newUserId: userId, newUserName: userName }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['keydelivery-active'] }),
  });

  const resetForm = () => {
    setFormClassroomId('');
    setFormUserId('');
    setFormOtherName('');
    setFormNotes('');
  };

  const handleSubmitDeliver = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formClassroomId || !(formUserId || formOtherName.trim())) return;
    const name = formUserId
      ? (users?.find((u) => u.id === formUserId).firstName + ' ' + users?.find((u) => u.id === formUserId).lastName)
      : formOtherName.trim();
    deliverMutation.mutate({
      classroomId: formClassroomId,
      deliveredToUserId: formUserId || undefined,
      deliveredToName: name,
      notes: formNotes || undefined,
    });
  };

  const handleTransfer = (delivery: KeyDelivery) => {
    if (!delivery.nextReservation) return;
    transferMutation.mutate({
      id: delivery.id,
      userId: delivery.nextReservation.professorUserId,
      userName: delivery.nextReservation.professorName,
    });
  };

  const activeItems = activeData?.items || [];
  const historyItems = historyData?.items || [];

  const filteredHistory = historySearch
    ? historyItems.filter(
        (d) =>
          d.classroomName.toLowerCase().includes(historySearch.toLowerCase()) ||
          d.deliveredToName.toLowerCase().includes(historySearch.toLowerCase())
      )
    : historyItems;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Bedelía</h1>
          <p className="text-primary-500 mt-1">Control de entrega y devolución de llaves de aulas</p>
        </div>
        <Button onClick={() => setDeliverOpen(true)}>
          <Plus className="h-4 w-4 mr-2" /> Entregar llave
        </Button>
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="active">
            <KeyRound className="h-4 w-4 mr-1.5" /> Activas ({activeItems.length})
          </TabsTrigger>
          <TabsTrigger value="history">
            <Undo2 className="h-4 w-4 mr-1.5" /> Devueltas
          </TabsTrigger>
        </TabsList>

        <TabsContent value="active">
          {activeLoading ? (
            <div className="text-center py-16 text-primary-400">Cargando...</div>
          ) : activeItems.length === 0 ? (
            <div className="text-center py-16 text-primary-400">
              <KeyRound className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p className="font-medium">No hay llaves entregadas</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableHead>Aula</TableHead>
                  <TableHead>Edificio</TableHead>
                  <TableHead>Entregado a</TableHead>
                  <TableHead>Desde</TableHead>
                  <TableHead className="hidden sm:table-cell">Próxima reserva</TableHead>
                  <TableHead className="text-right">Acciones</TableHead>
                </TableHeader>
                <TableBody>
                  {activeItems.map((d) => (
                    <TableRow key={d.id}>
                      <TableCell className="font-medium">{d.classroomName}</TableCell>
                      <TableCell className="text-primary-400 text-sm">{d.buildingName || '—'}</TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1.5">
                          <User className="h-3.5 w-3.5 text-primary-400" />
                          <span>{d.deliveredToName}</span>
                        </div>
                      </TableCell>
                      <TableCell className="text-primary-500 text-sm">
                        {new Date(d.deliveredAt).toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' })}
                      </TableCell>
                      <TableCell className="hidden sm:table-cell">
                        {d.nextReservation ? (
                          <div className="flex items-center gap-1.5 text-sm">
                            <Clock className="h-3.5 w-3.5 text-accent-500" />
                            <span className="font-medium text-accent-700">{d.nextReservation.professorName}</span>
                            <span className="text-primary-400">
                              {d.nextReservation.startTime.slice(0, 5)}-{d.nextReservation.endTime.slice(0, 5)}
                            </span>
                          </div>
                        ) : (
                          <span className="text-primary-300 text-sm">—</span>
                        )}
                      </TableCell>
                      <TableCell>
                        <div className="flex justify-end gap-1">
                          {d.nextReservation && (
                            <Button
                              variant="ghost"
                              size="sm"
                              onClick={() => handleTransfer(d)}
                              disabled={transferMutation.isPending}
                              title={`Pasar a ${d.nextReservation.professorName}`}
                            >
                              <ArrowLeftRight className="h-4 w-4 mr-1" />
                              Pasar
                            </Button>
                          )}
                          <Button
                            variant="outline"
                            size="sm"
                            onClick={() => setReturnConfirmId(d.id)}
                            disabled={returnMutation.isPending}
                          >
                            <Undo2 className="h-4 w-4 mr-1" />
                            Devolver
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </TabsContent>

        <TabsContent value="history">
          <div className="space-y-4">
            <div className="relative max-w-sm">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
              <input
                type="text"
                placeholder="Buscar por aula o persona..."
                value={historySearch}
                onChange={(e) => setHistorySearch(e.target.value)}
                className="w-full h-10 pl-9 pr-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
              />
            </div>
            {historyLoading ? (
              <div className="text-center py-16 text-primary-400">Cargando...</div>
            ) : filteredHistory.length === 0 ? (
              <div className="text-center py-16 text-primary-400">
                <KeyRound className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="font-medium">No hay devoluciones registradas</p>
              </div>
            ) : (
              <div className="overflow-x-auto">
                <Table>
                  <TableHeader>
                    <TableHead>Aula</TableHead>
                    <TableHead>Entregado a</TableHead>
                    <TableHead>Entregó</TableHead>
                    <TableHead>Entrega</TableHead>
                    <TableHead>Devolución</TableHead>
                  </TableHeader>
                  <TableBody>
                    {filteredHistory.map((d) => (
                      <TableRow key={d.id}>
                        <TableCell className="font-medium">{d.classroomName}</TableCell>
                        <TableCell>{d.deliveredToName}</TableCell>
                        <TableCell className="text-primary-500 text-sm">{d.deliveredByName || '—'}</TableCell>
                        <TableCell className="text-primary-400 text-sm">
                          {new Date(d.deliveredAt).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })}
                        </TableCell>
                        <TableCell className="text-primary-400 text-sm">
                          {d.returnedAt
                            ? new Date(d.returnedAt).toLocaleString('es-ES', { day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit' })
                            : '—'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </div>
            )}
          </div>
        </TabsContent>
      </Tabs>

      {/* Deliver dialog */}
      <Dialog open={deliverOpen} onOpenChange={(o) => { if (!o) { setDeliverOpen(false); resetForm(); } }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Entregar llave</DialogTitle>
            <DialogDescription>Registra la entrega de la llave de un aula</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleSubmitDeliver}>
            <DialogBody className="space-y-4">
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1">Aula</label>
                <select
                  className="w-full h-10 px-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
                  value={formClassroomId}
                  onChange={(e) => setFormClassroomId(e.target.value)}
                  required
                >
                  <option value="">Seleccionar aula...</option>
                  {(classrooms || []).map((c) => (
                    <option key={c.id} value={c.id}>{c.name}{c.buildingName ? ` (${c.buildingName})` : ''}</option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1">Entregar a</label>
                <select
                  className="w-full h-10 px-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500 mb-2"
                  value={formUserId}
                  onChange={(e) => { setFormUserId(e.target.value); if (e.target.value) setFormOtherName(''); }}
                >
                  <option value="">Seleccionar usuario...</option>
                  {(users || [])
                    .filter((u) => u.role !== 'Student')
                    .map((u) => (
                      <option key={u.id} value={u.id}>{u.firstName} {u.lastName} ({u.role})</option>
                    ))}
                </select>
                <p className="text-xs text-primary-400 text-center mb-1">— o —</p>
                <Input
                  placeholder="Nombre de otra persona"
                  value={formOtherName}
                  onChange={(e) => { setFormOtherName(e.target.value); if (e.target.value) setFormUserId(''); }}
                  disabled={!!formUserId}
                />
              </div>
              <Textarea label="Notas (opcional)" placeholder="Ej: Llave N° 3" value={formNotes} onChange={(e) => setFormNotes(e.target.value)} />
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => { setDeliverOpen(false); resetForm(); }}>Cancelar</Button>
              <Button type="submit" loading={deliverMutation.isPending}>Entregar llave</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Return confirm dialog */}
      <Dialog open={returnConfirmId !== null} onOpenChange={(o) => { if (!o) setReturnConfirmId(null); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Devolver llave</DialogTitle>
            <DialogDescription>¿Confirmás que la llave fue devuelta a bedelía?</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setReturnConfirmId(null)}>Cancelar</Button>
            <Button
              variant="destructive"
              loading={returnMutation.isPending}
              onClick={() => returnConfirmId && returnMutation.mutate(returnConfirmId)}
            >
              Confirmar devolución
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
