import { useState, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Building2, MapPin, Users, Clock, KeyRound, Loader2, AlertTriangle, RefreshCw, Undo2, ArrowLeftRight, Plus, Package } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { SearchableSelect } from '@/components/ui/SearchableSelect';
import { Textarea } from '@/components/ui/Textarea';

interface CampusClassroom {
  id: string;
  name: string;
  capacity: number;
  type: string;
  status: 'available' | 'occupied' | 'maintenance' | 'inactive';
  currentReservation?: { reservationId: string; title: string; userName: string; startTime: string; endTime: string } | null;
}

interface CampusFloor {
  floor: number;
  classrooms: CampusClassroom[];
}

interface CampusBuilding {
  id: string;
  name: string;
  floors: CampusFloor[];
}

interface CampusStatus {
  buildings: CampusBuilding[];
  timestamp: string;
}

interface Classroom { id: string; name: string; buildingName?: string; }

interface UserInfo { id: string; firstName: string; lastName: string; email: string; role: string; }

interface Accessory { id: string; name: string; description?: string; isActive: boolean; }

interface KeyDeliveryItem {
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
  nextReservation?: { reservationId: string; professorName: string; professorUserId: string; startTime: string; endTime: string };
  accessories?: Accessory[];
}

const statusConfig: Record<string, { bg: string; border: string; text: string; label: string; dot: string }> = {
  available: { bg: 'bg-green-50', border: 'border-green-400', text: 'text-green-700', label: 'Disponible', dot: 'bg-green-500' },
  occupied: { bg: 'bg-red-50', border: 'border-red-400', text: 'text-red-700', label: 'Ocupado', dot: 'bg-red-500' },
  maintenance: { bg: 'bg-yellow-50', border: 'border-yellow-400', text: 'text-yellow-700', label: 'Mantenimiento', dot: 'bg-yellow-500' },
  inactive: { bg: 'bg-gray-50', border: 'border-gray-300', text: 'text-gray-500', label: 'Inactivo', dot: 'bg-gray-400' },
};

const typeLabel: Record<string, string> = {
  Lecture: 'Aula', Laboratory: 'Lab', Workshop: 'Taller',
  Seminar: 'Seminario', Auditorium: 'Auditorio', Office: 'Oficina',
};

export default function BedeliaMap() {
  const queryClient = useQueryClient();
  const [selectedCr, setSelectedCr] = useState<{ classroom: CampusClassroom; buildingName: string; keyDelivery?: KeyDeliveryItem } | null>(null);
  const [deliverOpen, setDeliverOpen] = useState(false);
  const [returnConfirmId, setReturnConfirmId] = useState<string | null>(null);
  const [formClassroomId, setFormClassroomId] = useState('');
  const [formUserId, setFormUserId] = useState('');
  const [formOtherName, setFormOtherName] = useState('');
  const [formNotes, setFormNotes] = useState('');
  const [formAccessoryIds, setFormAccessoryIds] = useState<string[]>([]);

  const { data: campus, isLoading: campusLoading } = useQuery<CampusStatus>({
    queryKey: ['campus-status-bedelia'],
    queryFn: () => api.get<CampusStatus>('/campus/status'),
    refetchInterval: 30_000,
  });

  const { data: activeData } = useQuery<{ items: KeyDeliveryItem[] }>({
    queryKey: ['keydelivery-active-bedelia'],
    queryFn: () => api.get<{ items: KeyDeliveryItem[] }>('/keydelivery/active'),
    refetchInterval: 15_000,
  });

  const { data: classrooms } = useQuery<Classroom[]>({
    queryKey: ['classrooms-bedelia-map'],
    queryFn: () => api.get<Classroom[]>('/classrooms'),
  });

  const { data: users } = useQuery<UserInfo[]>({
    queryKey: ['users-bedelia-map'],
    queryFn: () => api.get<UserInfo[]>('/users'),
  });

  const { data: accessories } = useQuery<Accessory[]>({
    queryKey: ['accessories-bedelia-map'],
    queryFn: () => api.get<Accessory[]>('/accessories'),
  });

  const keyMap = useMemo(() => {
    const m = new Map<string, KeyDeliveryItem>();
    for (const item of activeData?.items || []) {
      m.set(item.classroomId, item);
    }
    return m;
  }, [activeData]);

  const deliverMutation = useMutation({
    mutationFn: (body: { classroomId: string; deliveredToUserId?: string; deliveredToName: string; notes?: string; accessoryIds?: string[] }) =>
      api.post('/keydelivery/deliver', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['keydelivery-active-bedelia'] });
      setDeliverOpen(false);
      setSelectedCr(null);
      resetDeliverForm();
    },
  });

  const returnMutation = useMutation({
    mutationFn: (id: string) => api.post(`/keydelivery/${id}/return`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['keydelivery-active-bedelia'] });
      queryClient.invalidateQueries({ queryKey: ['keydelivery-history'] });
      setReturnConfirmId(null);
      setSelectedCr(null);
    },
  });

  const transferMutation = useMutation({
    mutationFn: ({ id, userId, userName }: { id: string; userId: string; userName: string }) =>
      api.post(`/keydelivery/${id}/transfer`, { newUserId: userId, newUserName: userName }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['keydelivery-active-bedelia'] });
      setSelectedCr(null);
    },
  });

  const resetDeliverForm = () => {
    setFormClassroomId('');
    setFormUserId('');
    setFormOtherName('');
    setFormNotes('');
    setFormAccessoryIds([]);
  };

  const handleDeliverSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formClassroomId || !(formUserId || formOtherName.trim())) return;
    const u = formUserId ? users?.find((u) => u.id === formUserId) : null;
    const name = u ? `${u.firstName} ${u.lastName}`.trim() : formOtherName.trim();
    deliverMutation.mutate({
      classroomId: formClassroomId,
      deliveredToUserId: formUserId || undefined,
      deliveredToName: name,
      notes: formNotes || undefined,
      accessoryIds: formAccessoryIds.length > 0 ? formAccessoryIds : undefined,
    });
  };

  const handleClassroomClick = (cr: CampusClassroom, buildingName: string) => {
    const key = keyMap.get(cr.id);
    setSelectedCr({ classroom: cr, buildingName, keyDelivery: key });
  };

  const handleQuickTransfer = (delivery: KeyDeliveryItem) => {
    if (!delivery.nextReservation) return;
    transferMutation.mutate({
      id: delivery.id,
      userId: delivery.nextReservation.professorUserId,
      userName: delivery.nextReservation.professorName,
    });
  };

  if (campusLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <Loader2 className="h-8 w-8 animate-spin text-primary-300" />
      </div>
    );
  }

  if (!campus || campus.buildings.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <Building2 className="h-12 w-12 text-primary-200 mb-3" />
        <p className="text-primary-400 text-sm">No hay edificios disponibles</p>
      </div>
    );
  }

  const buildingsWithClassrooms = campus.buildings.filter(
    (b) => b.floors.some((f) => f.classrooms.length > 0)
  );

  if (buildingsWithClassrooms.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <Building2 className="h-12 w-12 text-primary-200 mb-3" />
        <p className="text-primary-400 text-sm">No hay aulas disponibles</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-3 text-xs">
            <span className="flex items-center gap-1.5"><span className="h-2.5 w-2.5 rounded-full bg-green-500" />Disponible</span>
            <span className="flex items-center gap-1.5"><span className="h-2.5 w-2.5 rounded-full bg-red-500" />Ocupado</span>
            <span className="flex items-center gap-1.5"><span className="h-2.5 w-2.5 rounded-full bg-yellow-500" />Mantenimiento</span>
            <span className="flex items-center gap-1.5"><KeyRound className="h-3.5 w-3.5 text-accent-500" />Con llave</span>
          </div>
        </div>
        <span className="text-xs text-primary-400 flex items-center gap-1">
          <RefreshCw className="h-3 w-3" />Actualizado cada 30s
        </span>
      </div>

      <div className="space-y-6">
        {buildingsWithClassrooms.map((building) => (
          <div key={building.id} className="bg-white rounded-xl border border-primary-100 overflow-hidden">
            <div className="bg-primary-50 px-5 py-3 border-b border-primary-100">
              <div className="flex items-center gap-2">
                <Building2 className="h-4 w-4 text-primary-500" />
                <h3 className="font-heading font-semibold text-primary-800">{building.name}</h3>
              </div>
            </div>
            <div className="p-4 space-y-4">
              {building.floors.length === 0 ? (
                <p className="text-sm text-primary-400 text-center py-4">Sin aulas en este edificio</p>
              ) : (
                building.floors.map((floor) => (
                  <div key={floor.floor}>
                    <div className="flex items-center gap-2 mb-2">
                      <MapPin className="h-3.5 w-3.5 text-primary-400" />
                      <span className="text-xs font-medium text-primary-500 uppercase tracking-wider">Piso {floor.floor}</span>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {[...floor.classrooms].sort((a, b) => a.name.localeCompare(b.name, 'es', { numeric: true })).map((cr) => {
                        const cfg = statusConfig[cr.status] || statusConfig.inactive;
                        const keyDelivery = keyMap.get(cr.id);
                        const hasKey = !!keyDelivery;
                        return (
                          <button
                            key={cr.id}
                            onClick={() => handleClassroomClick(cr, building.name)}
                            className={`${cfg.bg} ${hasKey ? 'border-accent-400' : cfg.border} border-2 rounded-lg px-3 py-2 text-left min-w-[130px] hover:shadow-md transition-shadow cursor-pointer relative`}
                          >
                            {hasKey && (
                              <span className="absolute -top-1.5 -right-1.5 bg-accent-500 text-white rounded-full p-0.5">
                                <KeyRound className="h-3 w-3" />
                              </span>
                            )}
                            <div className="flex items-center gap-1.5 mb-1">
                              <span className={`h-2 w-2 rounded-full ${cfg.dot}`} />
                              <span className="text-sm font-semibold text-primary-800">{cr.name}</span>
                            </div>
                            <div className="flex items-center gap-2 text-xs text-primary-500">
                              <span>{typeLabel[cr.type] || cr.type}</span>
                              <span>·</span>
                              <span className="flex items-center gap-0.5">
                                <Users className="h-3 w-3" />{cr.capacity}
                              </span>
                            </div>
                            {hasKey && (
                              <div className="mt-1 text-xs text-accent-700 font-medium truncate flex items-center gap-0.5">
                                <KeyRound className="h-3 w-3 flex-shrink-0" />
                                {keyDelivery!.deliveredToName}
                              </div>
                            )}
                            {hasKey && keyDelivery!.accessories?.length ? (
                              <div className="flex gap-0.5 mt-0.5">
                                {keyDelivery!.accessories.map((a) => (
                                  <span key={a.id} className="h-1.5 w-1.5 rounded-full bg-accent-400" title={a.name} />
                                ))}
                              </div>
                            ) : null}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        ))}
      </div>

      <Dialog open={!!selectedCr} onOpenChange={(o) => { if (!o) setSelectedCr(null); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{selectedCr?.classroom.name}</DialogTitle>
            <DialogDescription>{selectedCr?.buildingName}</DialogDescription>
          </DialogHeader>
          {selectedCr && (
            <DialogBody className="space-y-4">
              <div className="flex items-center gap-2">
                <span className={`h-3 w-3 rounded-full ${statusConfig[selectedCr.classroom.status].dot}`} />
                <span className={`text-sm font-medium ${statusConfig[selectedCr.classroom.status].text}`}>
                  {statusConfig[selectedCr.classroom.status].label}
                </span>
              </div>
              <div className="grid grid-cols-2 gap-3 text-sm">
                <div className="bg-primary-50 rounded-lg p-3">
                  <p className="text-primary-400 text-xs">Tipo</p>
                  <p className="font-medium text-primary-800">{typeLabel[selectedCr.classroom.type] || selectedCr.classroom.type}</p>
                </div>
                <div className="bg-primary-50 rounded-lg p-3">
                  <p className="text-primary-400 text-xs">Capacidad</p>
                  <p className="font-medium text-primary-800">{selectedCr.classroom.capacity} personas</p>
                </div>
              </div>

              {selectedCr.keyDelivery ? (
                <div className="bg-accent-50 border border-accent-200 rounded-lg p-3 space-y-2">
                  <p className="text-sm font-medium text-accent-800 flex items-center gap-1.5">
                    <KeyRound className="h-4 w-4" />Llave entregada
                  </p>
                  <p className="text-sm text-accent-700">A: <strong>{selectedCr.keyDelivery.deliveredToName}</strong></p>
                  <p className="text-xs text-accent-500">
                    {new Date(selectedCr.keyDelivery.deliveredAt).toLocaleString('es-ES', {
                      day: '2-digit', month: '2-digit', hour: '2-digit', minute: '2-digit',
                    })}
                  </p>
                  {selectedCr.keyDelivery.accessories?.length ? (
                    <div className="flex flex-wrap gap-1 pt-1 border-t border-accent-200">
                      {selectedCr.keyDelivery.accessories.map((a) => (
                        <span key={a.id} className="inline-flex items-center gap-0.5 text-xs bg-white text-accent-700 rounded-full px-2 py-0.5 border border-accent-300">
                          <Package className="h-3 w-3" />{a.name}
                        </span>
                      ))}
                    </div>
                  ) : null}
                  {selectedCr.keyDelivery.nextReservation && (
                    <div className="flex items-center gap-1.5 text-sm pt-1 border-t border-accent-200">
                      <Clock className="h-3.5 w-3.5 text-accent-500" />
                      <span className="text-accent-700">{selectedCr.keyDelivery.nextReservation.professorName}</span>
                      <span className="text-accent-400 text-xs">
                        {selectedCr.keyDelivery.nextReservation.startTime.slice(0, 5)}-{selectedCr.keyDelivery.nextReservation.endTime.slice(0, 5)}
                      </span>
                    </div>
                  )}
                </div>
              ) : (
                <div className="bg-primary-50 border border-primary-200 rounded-lg p-3">
                  <p className="text-sm text-primary-500 flex items-center gap-1.5">
                    <KeyRound className="h-4 w-4 text-primary-300" />Sin llave entregada
                  </p>
                </div>
              )}
            </DialogBody>
          )}
          <DialogFooter className="flex gap-2">
            {selectedCr?.keyDelivery ? (
              <>
                {selectedCr.keyDelivery.nextReservation && (
                  <Button variant="accent" size="sm" onClick={() => handleQuickTransfer(selectedCr.keyDelivery!)} disabled={transferMutation.isPending}>
                    <ArrowLeftRight className="h-4 w-4 mr-1" />Pasar a {selectedCr.keyDelivery.nextReservation.professorName}
                  </Button>
                )}
                <Button variant="outline" size="sm" onClick={() => setReturnConfirmId(selectedCr.keyDelivery!.id)} disabled={returnMutation.isPending}>
                  <Undo2 className="h-4 w-4 mr-1" />Devolver
                </Button>
              </>
            ) : (
              <Button variant="accent" size="sm" onClick={() => { setFormClassroomId(selectedCr!.classroom.id); setDeliverOpen(true); }}>
                <Plus className="h-4 w-4 mr-1" />Entregar llave
              </Button>
            )}
            <Button variant="outline" size="sm" onClick={() => setSelectedCr(null)}>Cerrar</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={deliverOpen} onOpenChange={(o) => { if (!o) { setDeliverOpen(false); resetDeliverForm(); } }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Entregar llave</DialogTitle>
            <DialogDescription>Registra la entrega de la llave</DialogDescription>
          </DialogHeader>
          <form onSubmit={handleDeliverSubmit}>
            <DialogBody className="space-y-4">
              <SearchableSelect label="Aula" placeholder="Buscar aula..." value={formClassroomId}
                onChange={(v) => setFormClassroomId(v)}
                options={(classrooms || []).map((c) => ({ value: c.id, label: `${c.name}${c.buildingName ? ` (${c.buildingName})` : ''}` }))} required />
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1">Entregar a</label>
                <SearchableSelect label="" placeholder="Buscar usuario..." value={formUserId}
                  onChange={(v) => { setFormUserId(v); if (v) setFormOtherName(''); }}
                  options={(users || []).filter((u) => u.role !== 'Student').map((u) => ({
                    value: u.id,
                    label: `${u.firstName} ${u.lastName} (${u.role === 'Admin' ? 'Admin' : u.role === 'Teacher' ? 'Profesor' : u.role})`,
                  }))} />
                <p className="text-xs text-primary-400 text-center my-2">— o —</p>
                <Input placeholder="Nombre de otra persona" value={formOtherName}
                  onChange={(e) => { setFormOtherName(e.target.value); if (e.target.value) setFormUserId(''); }} disabled={!!formUserId} />
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
              <Button type="button" variant="outline" onClick={() => { setDeliverOpen(false); resetDeliverForm(); }}>Cancelar</Button>
              <Button type="submit" loading={deliverMutation.isPending}>Entregar llave</Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={returnConfirmId !== null} onOpenChange={(o) => { if (!o) setReturnConfirmId(null); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Devolver llave</DialogTitle>
            <DialogDescription>¿Confirmás que la llave fue devuelta a bedelía?</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setReturnConfirmId(null)}>Cancelar</Button>
            <Button variant="destructive" loading={returnMutation.isPending}
              onClick={() => returnConfirmId && returnMutation.mutate(returnConfirmId)}>Confirmar devolución</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
