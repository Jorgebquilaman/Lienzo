import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, Search, RefreshCw, Map, Maximize2, Minimize2 } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { TableSkeleton } from '@/components/ui/Skeleton';
import BuildingFloorPlanTab from '@/components/admin/BuildingFloorPlanTab';
import type { Building } from '@/types';

const buildingSchema = z.object({
  name: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
  code: z.string().min(1, 'El código es requerido'),
  address: z.string().optional(),
  floors: z.coerce.number().min(1, 'Debe tener al menos 1 piso'),
});

type BuildingFormData = z.infer<typeof buildingSchema>;

export default function AdminBuildings() {
  const [search, setSearch] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [floorPlanOpen, setFloorPlanOpen] = useState(false);
  const [floorPlanFullScreen, setFloorPlanFullScreen] = useState(false);
  const [floorPlanBuilding, setFloorPlanBuilding] = useState<Building | null>(null);
  const [editing, setEditing] = useState<Building | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const [syncResult, setSyncResult] = useState<{ creados: number; existentes: number } | null>(null);
  const queryClient = useQueryClient();

  const { data, isLoading } = useQuery({
    queryKey: ['adminBuildings', search],
    queryFn: () => {
      const params: Record<string, string> = {};
      if (search) params.search = search;
      return api.get<Building[]>('/buildings', params);
    },
  });

  const syncMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; totalExterno: number }>('/buildings/sync'),
    onSuccess: (data) => {
      setSyncResult({ creados: data.creados, existentes: data.existentes });
      queryClient.invalidateQueries({ queryKey: ['adminBuildings'] });
      queryClient.invalidateQueries({ queryKey: ['buildings'] });
    },
  });

  const form = useForm<BuildingFormData>({
    resolver: zodResolver(buildingSchema),
    defaultValues: { name: '', code: '', address: '', floors: 1 },
  });

  const openDialog = (building?: Building) => {
    setEditing(building || null);
    form.reset(building ? { name: building.name, code: building.code, address: building.address || '', floors: building.floors } : { name: '', code: '', address: '', floors: 1 });
    setDialogOpen(true);
  };

  const saveMutation = useMutation({
    mutationFn: (data: BuildingFormData) => {
      if (editing) return api.put(`/buildings/${editing.id}`, data);
      return api.post('/buildings', data);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminBuildings'] });
      queryClient.invalidateQueries({ queryKey: ['buildings'] });
      setDialogOpen(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/buildings/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminBuildings'] });
      queryClient.invalidateQueries({ queryKey: ['buildings'] });
      setDeleteConfirm(null);
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Administrar Edificios</h1>
          <p className="text-primary-500 mt-1">Gestiona los edificios del campus</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => syncMutation.mutate()} loading={syncMutation.isPending}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Sincronizar
          </Button>
          <Button onClick={() => openDialog()}>
            <Plus className="h-4 w-4 mr-2" />
            Nuevo Edificio
          </Button>
        </div>
      </div>

      {syncResult && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3 flex items-center justify-between">
          <p className="text-sm text-green-800">
            Sincronización completada: <strong>{syncResult.creados}</strong> edificios nuevos creados, <strong>{syncResult.existentes}</strong> ya existían.
          </p>
          <button className="text-green-600 hover:text-green-800 text-sm font-medium" onClick={() => setSyncResult(null)}>
            Cerrar
          </button>
        </div>
      )}

      <div className="relative">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
        <input
          placeholder="Buscar edificios..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="w-full h-10 pl-9 pr-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
        />
      </div>

      {isLoading ? (
        <TableSkeleton rows={5} />
      ) : !data || data.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <p className="font-medium">No hay edificios registrados</p>
          <Button variant="outline" className="mt-3" onClick={() => openDialog()}>
            Crear primer edificio
          </Button>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Código</TableHead>
                <TableHead className="hidden sm:table-cell">Dirección</TableHead>
                <TableHead>Pisos</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.map((b) => (
                <TableRow key={b.id}>
                  <TableCell className="font-medium">{b.name}</TableCell>
                  <TableCell className="text-primary-500">{b.code}</TableCell>
                  <TableCell className="hidden sm:table-cell text-primary-400 text-sm">{b.address || '—'}</TableCell>
                  <TableCell className="text-primary-500">{b.floors}</TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-1">
                      <button
                        className="p-1.5 rounded-md text-primary-500 hover:bg-primary-50"
                        title="Plano del edificio"
                        onClick={() => {
                          setFloorPlanBuilding(b);
                          setFloorPlanOpen(true);
                        }}
                      >
                        <Map className="h-4 w-4" />
                      </button>
                      <button className="p-1.5 rounded-md text-primary-500 hover:bg-primary-50" onClick={() => openDialog(b)}>
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button className="p-1.5 rounded-md text-red-500 hover:bg-red-50" onClick={() => setDeleteConfirm(b.id)}>
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {/* Edit dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar Edificio' : 'Nuevo Edificio'}</DialogTitle>
            <DialogDescription>{editing ? 'Modifica los datos del edificio' : 'Ingresa los datos del nuevo edificio'}</DialogDescription>
          </DialogHeader>
          <form onSubmit={form.handleSubmit((data) => saveMutation.mutate(data))}>
            <DialogBody className="space-y-4">
              <Input label="Nombre" error={form.formState.errors.name?.message} {...form.register('name')} />
              <Input label="Código" error={form.formState.errors.code?.message} {...form.register('code')} />
              <Input label="Dirección" {...form.register('address')} />
              <Input label="Número de pisos" type="number" error={form.formState.errors.floors?.message} {...form.register('floors')} />
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>Cancelar</Button>
              <Button type="submit" loading={saveMutation.isPending}>
                {editing ? 'Guardar cambios' : 'Crear edificio'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      {/* Floor plan dialog */}
      <Dialog
        open={floorPlanOpen}
        onOpenChange={(open) => { setFloorPlanOpen(open); if (!open) setFloorPlanFullScreen(false); }}
        containerClassName={floorPlanFullScreen ? '!max-w-none !mx-0' : ''}
      >
        <DialogContent
          className={
            floorPlanFullScreen
              ? '!max-w-none !w-screen !h-screen !max-h-screen !m-0 !rounded-none'
              : 'max-w-6xl xl:max-w-[95vw] max-h-[90vh] overflow-y-auto'
          }
        >
          <DialogHeader>
            <div className="flex items-center justify-between">
              <div>
                <DialogTitle>Plano - {floorPlanBuilding?.name}</DialogTitle>
                <DialogDescription>
                  Subí el plano del edificio y colocá las aulas sobre él
                </DialogDescription>
              </div>
              <button
                className="p-2 rounded-lg text-primary-500 hover:bg-primary-50 transition-colors"
                title={floorPlanFullScreen ? 'Salir de pantalla completa' : 'Pantalla completa'}
                onClick={() => setFloorPlanFullScreen(!floorPlanFullScreen)}
              >
                {floorPlanFullScreen ? <Minimize2 className="h-5 w-5" /> : <Maximize2 className="h-5 w-5" />}
              </button>
            </div>
          </DialogHeader>
          <DialogBody>
            {floorPlanBuilding && <BuildingFloorPlanTab building={floorPlanBuilding} />}
          </DialogBody>
        </DialogContent>
      </Dialog>

      {/* Delete confirmation */}
      <Dialog open={!!deleteConfirm} onOpenChange={() => setDeleteConfirm(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar eliminación</DialogTitle>
            <DialogDescription>¿Estás seguro de eliminar este edificio? Esta acción no se puede deshacer.</DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteConfirm(null)}>Cancelar</Button>
            <Button variant="destructive" onClick={() => deleteConfirm && deleteMutation.mutate(deleteConfirm)} loading={deleteMutation.isPending}>
              Eliminar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
