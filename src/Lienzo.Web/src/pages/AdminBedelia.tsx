import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { KeyRound, Plus, ArrowLeftRight, Undo2, Search, Building2, User, Clock, Map, Package } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { Badge } from '@/components/ui/Badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import BedeliaMap from '@/components/bedelia/BedeliaMap';

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

interface Accessory {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
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
  accessories?: Accessory[];
}

interface KeyDeliveryListResponse {
  items: KeyDelivery[];
  totalCount: number;
}

export default function AdminBedelia() {
  const [tab, setTab] = useState('map');
  const [deliverOpen, setDeliverOpen] = useState(false);
  const [returnConfirmId, setReturnConfirmId] = useState<string | null>(null);
  const [historySearch, setHistorySearch] = useState('');
  const [formClassroomId, setFormClassroomId] = useState('');
  const [formUserId, setFormUserId] = useState('');
  const [formOtherName, setFormOtherName] = useState('');
  const [formNotes, setFormNotes] = useState('');
  const [formAccessoryIds, setFormAccessoryIds] = useState<string[]>([]);
  const queryClient = useQueryClient();

  const { data: classrooms } = useQuery({
    queryKey: ['classrooms'],
    queryFn: () => api.get<Classroom[]>('/classrooms'),
  });

  const { data: users } = useQuery({
    queryKey: ['users'],
    queryFn: () => api.get<UserInfo[]>('/users'),
  });

  const { data: accessories } = useQuery({
    queryKey: ['accessories'],
    queryFn: () => api.get<Accessory[]>('/accessories'),
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
    mutationFn: (body: { classroomId: string; deliveredToUserId?: string; deliveredToName: string; notes?: string; accessoryIds?: string[] }) =>
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
    setFormAccessoryIds([]);
  };

  const handleSubmitDeliver = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formClassroomId || !(formUserId || formOtherName.trim())) return;
    const user = formUserId ? users?.find((u) => u.id === formUserId) : null;
    const name = user ? `${user.firstName} ${user.lastName}`.trim() : formOtherName.trim();
    deliverMutation.mutate({
      classroomId: formClassroomId,
      deliveredToUserId: formUserId || undefined,
      deliveredToName: name,
      notes: formNotes || undefined,
      accessoryIds: formAccessoryIds.length > 0 ? formAccessoryIds : undefined,
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

  const activeItems = (activeData?.items || []).sort((a, b) =>
    a.classroomName.localeCompare(b.classroomName, 'es', { numeric: true })
  );
  const historyItems = (historyData?.items || []).sort((a, b) =>
    a.classroomName.localeCompare(b.classroomName, 'es', { numeric: true })
  );

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
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="map">
            <Map className="h-4 w-4 mr-1.5" /> Mapa
          </TabsTrigger>
          <TabsTrigger value="active">
            <KeyRound className="h-4 w-4 mr-1.5" /> Activas ({activeItems.length})
          </TabsTrigger>
          <TabsTrigger value="history">
            <Undo2 className="h-4 w-4 mr-1.5" /> Devueltas
          </TabsTrigger>
        </TabsList>

        <TabsContent value="map">
          <BedeliaMap />
        </TabsContent>

        <TabsContent value="active">
          <div className="flex justify-end mb-4">
            <Button onClick={() => setDeliverOpen(true)}>
              <Plus className="h-4 w-4 mr-2" /> Entregar llave
            </Button>
          </div>
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
                    <TableHead className="hidden sm:table-cell">Accesorios</TableHead>
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
                          {d.accessories?.length ? (
                            <div className="flex flex-wrap gap-1">
                              {d.accessories.map((a) => (
                                <span key={a.id} className="inline-flex items-center gap-0.5 text-xs bg-accent-50 text-accent-700 rounded-full px-2 py-0.5">
                                  <Package className="h-3 w-3" />{a.name}
                                </span>
                              ))}
                            </div>
                          ) : (
                            <span className="text-primary-300 text-sm">—</span>
                          )}
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
                    <TableHead className="hidden sm:table-cell">Accesorios</TableHead>
                    <TableHead>Entrega</TableHead>
                    <TableHead>Devolución</TableHead>
                  </TableHeader>
                  <TableBody>
                    {filteredHistory.map((d) => (
                      <TableRow key={d.id}>
                        <TableCell className="font-medium">{d.classroomName}</TableCell>
                        <TableCell>{d.deliveredToName}</TableCell>
                        <TableCell className="text-primary-500 text-sm">{d.deliveredByName || '—'}</TableCell>
                        <TableCell className="hidden sm:table-cell">
                          {d.accessories?.length ? (
                            <div className="flex flex-wrap gap-1">
                              {d.accessories.map((a) => (
                                <span key={a.id} className="inline-flex items-center gap-0.5 text-xs bg-accent-50 text-accent-700 rounded-full px-2 py-0.5">
                                  <Package className="h-3 w-3" />{a.name}
                                </span>
                              ))}
                            </div>
                          ) : (
                            <span className="text-primary-300 text-sm">—</span>
                          )}
                        </TableCell>
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
              <SearchableSelect
                label="Aula"
                placeholder="Buscar aula..."
                value={formClassroomId}
                onChange={(v) => setFormClassroomId(v)}
                options={(classrooms || []).map((c) => ({
                  value: c.id,
                  label: `${c.name}${c.buildingName ? ` (${c.buildingName})` : ''}`,
                }))}
                required
              />
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1">Entregar a</label>
                <SearchableSelect
                  label=""
                  placeholder="Buscar usuario..."
                  value={formUserId}
                  onChange={(v) => { setFormUserId(v); if (v) setFormOtherName(''); }}
                  options={(users || [])
                    .filter((u) => u.role !== 'Student')
                    .map((u) => ({
                      value: u.id,
                      label: `${u.firstName} ${u.lastName} (${u.role === 'Admin' ? 'Admin' : u.role === 'Teacher' ? 'Profesor' : u.role})`,
                    }))}
                />
                <p className="text-xs text-primary-400 text-center my-2">— o —</p>
                <Input
                  placeholder="Nombre de otra persona"
                  value={formOtherName}
                  onChange={(e) => { setFormOtherName(e.target.value); if (e.target.value) setFormUserId(''); }}
                  disabled={!!formUserId}
                />
              </div>
              <Textarea label="Notas (opcional)" placeholder="Ej: Llave N° 3" value={formNotes} onChange={(e) => setFormNotes(e.target.value)} />
              {(accessories?.length ?? 0) > 0 && (
                <div>
                  <label className="block text-sm font-medium text-primary-700 mb-1">Accesorios</label>
                  <div className="space-y-1.5 max-h-40 overflow-y-auto border border-primary-200 rounded-lg p-2">
                    {accessories!.map((a) => (
                      <label key={a.id} className="flex items-center gap-2 px-2 py-1 rounded hover:bg-primary-50 cursor-pointer">
                        <input type="checkbox" checked={formAccessoryIds.includes(a.id)}
                          onChange={(e) => {
                            setFormAccessoryIds(e.target.checked ? [...formAccessoryIds, a.id] : formAccessoryIds.filter((id) => id !== a.id));
                          }}
                          className="rounded border-primary-300 text-accent-600 focus:ring-accent-500" />
                        <span className="text-sm text-primary-700">{a.name}</span>
                        {a.description && <span className="text-xs text-primary-400">({a.description})</span>}
                      </label>
                    ))}
                  </div>
                </div>
              )}
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
