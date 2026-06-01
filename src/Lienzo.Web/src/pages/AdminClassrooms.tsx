import { useState, useRef, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, Search, Image, X, RefreshCw, ArrowUpDown, ArrowUp, ArrowDown } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Textarea } from '@/components/ui/Textarea';
import { Badge } from '@/components/ui/Badge';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { TableSkeleton } from '@/components/ui/Skeleton';
import { getClassroomTypeLabel } from '@/lib/utils';
import { useAuthStore } from '@/stores/authStore';
import type { Classroom, Building } from '@/types';

const classroomSchema = z.object({
  name: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
  code: z.string().min(2, 'El código debe tener al menos 2 caracteres'),
  buildingId: z.string().min(1, 'Selecciona un edificio'),
  floor: z.coerce.number().min(0, 'El piso no puede ser negativo'),
  capacity: z.coerce.number().min(1, 'La capacidad debe ser al menos 1'),
  type: z.string().min(1, 'Selecciona un tipo'),
  description: z.string().optional(),
  features: z.string().optional(),
  imageUrl: z.string().optional(),
});

type ClassroomFormData = z.infer<typeof classroomSchema>;

export default function AdminClassrooms() {
  const [search, setSearch] = useState('');
  const [filterBuildingId, setFilterBuildingId] = useState('');
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingClassroom, setEditingClassroom] = useState<Classroom | null>(null);
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState('');
  const [syncResult, setSyncResult] = useState<{ creados: number; existentes: number; sinEdificio: number } | null>(null);
  const [sortKey, setSortKey] = useState<string>('name');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  const fileInputRef = useRef<HTMLInputElement>(null);
  const queryClient = useQueryClient();

  const { data: classrooms, isLoading } = useQuery({
    queryKey: ['adminClassrooms'],
    queryFn: () => api.get<Classroom[]>('/classrooms'),
  });

  const sorted = useMemo(() => {
    const list = classrooms || [];
    const filtered = list.filter(c => {
      if (search && !c.name.toLowerCase().includes(search.toLowerCase()) &&
          !c.code.toLowerCase().includes(search.toLowerCase()) &&
          !(c.buildingName || '').toLowerCase().includes(search.toLowerCase()) &&
          !(c.type || '').toLowerCase().includes(search.toLowerCase()))
        return false;
      if (filterBuildingId && c.buildingId !== filterBuildingId)
        return false;
      return true;
    });
    return [...filtered].sort((a, b) => {
      let cmp = 0;
      const av = (a as any)[sortKey] ?? '';
      const bv = (b as any)[sortKey] ?? '';
      if (typeof av === 'string') cmp = av.localeCompare(bv);
      else if (typeof av === 'number') cmp = av - bv;
      return sortDir === 'asc' ? cmp : -cmp;
    });
  }, [classrooms, search, filterBuildingId, sortKey, sortDir]);

  const toggleSort = (key: string) => {
    if (sortKey === key) setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    else { setSortKey(key); setSortDir('asc'); }
  };

  const { data: buildings } = useQuery({
    queryKey: ['buildings'],
    queryFn: () => api.get<Building[]>('/buildings'),
  });

  const form = useForm<ClassroomFormData>({
    resolver: zodResolver(classroomSchema),
    defaultValues: {
      name: '',
      code: '',
      buildingId: '',
      floor: 0,
      capacity: 30,
      type: 'Lecture',
      description: '',
      features: '',
      imageUrl: '',
    },
  });

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      const token = useAuthStore.getState().token;
      const res = await fetch('/api/upload', {
        method: 'POST',
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        body: formData,
      });
      if (!res.ok) throw new Error('Error al subir imagen');
      const url = await res.text();
      const cleanUrl = url.replace(/^"|"$/g, '');
      setPreviewUrl(cleanUrl);
      form.setValue('imageUrl', cleanUrl);
    } catch {
      alert('Error al subir la imagen');
    } finally {
      setUploading(false);
    }
  };

  const removeImage = () => {
    setPreviewUrl('');
    form.setValue('imageUrl', '');
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const openCreateDialog = () => {
    setEditingClassroom(null);
    setPreviewUrl('');
    form.reset({
      name: '',
      code: '',
      buildingId: buildings?.[0]?.id || '',
      floor: 0,
      capacity: 30,
      type: 'Lecture',
      description: '',
      features: '',
      imageUrl: '',
    });
    setDialogOpen(true);
  };

  const openEditDialog = (classroom: Classroom) => {
    setEditingClassroom(classroom);
    setPreviewUrl(classroom.imageUrl || '');
    form.reset({
      name: classroom.name,
      code: classroom.code,
      buildingId: classroom.buildingId,
      floor: classroom.floor,
      capacity: classroom.capacity,
      type: classroom.type,
      description: classroom.description || '',
      features: classroom.features?.join(', ') || '',
      imageUrl: classroom.imageUrl || '',
    });
    setDialogOpen(true);
  };

  const saveMutation = useMutation({
    mutationFn: async (data: ClassroomFormData) => {
      const payload = {
        ...data,
        imageUrl: data.imageUrl || null,
        features: data.features ? data.features.split(',').map((f) => f.trim()).filter(Boolean) : [],
      };
      if (editingClassroom) {
        return api.put(`/classrooms/${editingClassroom.id}`, payload);
      }
      return api.post('/classrooms', payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminClassrooms'] });
      queryClient.invalidateQueries({ queryKey: ['classrooms'] });
      setDialogOpen(false);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/classrooms/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminClassrooms'] });
      queryClient.invalidateQueries({ queryKey: ['classrooms'] });
      setDeleteConfirm(null);
    },
  });

  const syncMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; sinEdificio: number }>('/classrooms/sync'),
    onSuccess: (data) => {
      setSyncResult({ creados: data.creados, existentes: data.existentes, sinEdificio: data.sinEdificio });
      queryClient.invalidateQueries({ queryKey: ['adminClassrooms'] });
      queryClient.invalidateQueries({ queryKey: ['classrooms'] });
    },
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Administrar Aulas</h1>
          <p className="text-primary-500 mt-1">Gestiona las aulas del sistema</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => syncMutation.mutate()} loading={syncMutation.isPending}>
            <RefreshCw className="h-4 w-4 mr-2" />
            Sincronizar
          </Button>
          <Button onClick={openCreateDialog}>
            <Plus className="h-4 w-4 mr-2" />
            Nueva Aula
          </Button>
        </div>
      </div>

      {syncResult && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3 flex items-center justify-between">
          <p className="text-sm text-green-800">
            Sincronización completada: <strong>{syncResult.creados}</strong> aulas nuevas creadas,
            <strong> {syncResult.existentes}</strong> ya existían
            {syncResult.sinEdificio > 0 && (
              <>, <strong>{syncResult.sinEdificio}</strong> sin edificio vinculado</>
            )}.
          </p>
          <button className="text-green-600 hover:text-green-800 text-sm font-medium" onClick={() => setSyncResult(null)}>
            Cerrar
          </button>
        </div>
      )}

      {isLoading ? (
        <TableSkeleton rows={8} />
      ) : !classrooms || classrooms.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <p className="font-medium">No hay aulas registradas</p>
          <Button variant="outline" className="mt-3" onClick={openCreateDialog}>
            Crear primera aula
          </Button>
        </div>
      ) : (
        <div className="space-y-3">
          <div className="flex gap-3 items-center">
            <div className="relative max-w-xs">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
              <input type="text" placeholder="Buscar aulas..." value={search}
                onChange={e => setSearch(e.target.value)}
                className="w-full pl-9 pr-3 py-2 text-sm border border-primary-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent-300 focus:border-accent-400 bg-white" />
            </div>
            <select value={filterBuildingId} onChange={e => setFilterBuildingId(e.target.value)}
              className="text-sm border border-primary-200 rounded-lg px-3 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-accent-300">
              <option value="">Todos los edificios</option>
              {buildings?.map(b => <option key={b.id} value={b.id}>{b.name}</option>)}
            </select>
          </div>
          <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead><button type="button" onClick={() => toggleSort('name')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'name' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Nombre</button></TableHead>
                <TableHead><button type="button" onClick={() => toggleSort('code')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'code' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Código</button></TableHead>
                <TableHead className="hidden sm:table-cell"><button type="button" onClick={() => toggleSort('buildingName')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'buildingName' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Edificio</button></TableHead>
                <TableHead className="hidden md:table-cell"><button type="button" onClick={() => toggleSort('type')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'type' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Tipo</button></TableHead>
                <TableHead><button type="button" onClick={() => toggleSort('capacity')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'capacity' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Capacidad</button></TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sorted.length === 0 ? (
                <TableRow><TableCell colSpan={6} className="text-center text-primary-400 py-8">Sin resultados</TableCell></TableRow>
              ) : sorted.map((c) => (
                <TableRow key={c.id}>
                  <TableCell className="font-medium">{c.name}</TableCell>
                  <TableCell className="text-primary-500">{c.code}</TableCell>
                  <TableCell className="hidden sm:table-cell text-primary-500">{c.buildingName}</TableCell>
                  <TableCell className="hidden md:table-cell">
                    <Badge>{getClassroomTypeLabel(c.type)}</Badge>
                  </TableCell>
                  <TableCell className="text-primary-500">{c.capacity}</TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-1">
                      <button
                        className="p-1.5 rounded-md text-primary-500 hover:bg-primary-50"
                        onClick={() => openEditDialog(c)}
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button
                        className="p-1.5 rounded-md text-red-500 hover:bg-red-50"
                        onClick={() => setDeleteConfirm(c.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{editingClassroom ? 'Editar Aula' : 'Nueva Aula'}</DialogTitle>
            <DialogDescription>
              {editingClassroom ? 'Modifica los datos del aula' : 'Ingresa los datos de la nueva aula'}
            </DialogDescription>
          </DialogHeader>
          <form onSubmit={form.handleSubmit((data) => saveMutation.mutate(data))}>
            <DialogBody className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <Input label="Nombre" error={form.formState.errors.name?.message} {...form.register('name')} />
                <Input label="Código" error={form.formState.errors.code?.message} {...form.register('code')} />
              </div>
              <Select
                label="Edificio"
                placeholder="Selecciona un edificio"
                error={form.formState.errors.buildingId?.message}
                value={form.watch('buildingId')}
                onValueChange={(v) => form.setValue('buildingId', v)}
                options={buildings?.map((b) => ({ value: b.id, label: b.name })) || []}
              />
              <div className="grid grid-cols-2 gap-3">
                <Input label="Piso" type="number" error={form.formState.errors.floor?.message} {...form.register('floor')} />
                <Input label="Capacidad" type="number" error={form.formState.errors.capacity?.message} {...form.register('capacity')} />
              </div>
              <Select label="Tipo" placeholder="Selecciona un tipo" error={form.formState.errors.type?.message} value={form.watch('type')} onValueChange={(v) => form.setValue('type', v)} options={[
                { value: 'Lecture', label: 'Aula' },
                { value: 'Laboratory', label: 'Laboratorio' },
                { value: 'Workshop', label: 'Taller' },
                { value: 'Seminar', label: 'Seminario' },
                { value: 'Auditorium', label: 'Auditorio' },
              ]} />
              <Textarea label="Descripción" {...form.register('description')} />
              <Input label="Características (separadas por coma)" placeholder="proyector, wifi, aire acondicionado" {...form.register('features')} />
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1.5">Imagen</label>
                {previewUrl ? (
                  <div className="relative w-full h-32 rounded-lg overflow-hidden bg-primary-100">
                    <img src={previewUrl} alt="Vista previa" className="w-full h-full object-cover" />
                    <button
                      type="button"
                      className="absolute top-1 right-1 p-1 rounded-full bg-black/50 text-white hover:bg-black/70"
                      onClick={removeImage}
                    >
                      <X className="h-4 w-4" />
                    </button>
                  </div>
                ) : (
                  <div
                    className="flex flex-col items-center justify-center w-full h-32 rounded-lg border-2 border-dashed border-primary-200 bg-primary-50/50 cursor-pointer hover:border-accent-400 transition-colors"
                    onClick={() => fileInputRef.current?.click()}
                  >
                    <Image className="h-8 w-8 text-primary-300 mb-1" />
                    <span className="text-sm text-primary-400">
                      {uploading ? 'Subiendo...' : 'Haz clic para subir imagen'}
                    </span>
                    <span className="text-xs text-primary-300 mt-0.5">JPG, PNG o WebP · Máx 5 MB</span>
                  </div>
                )}
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/png,image/webp"
                  className="hidden"
                  onChange={handleImageUpload}
                  disabled={uploading}
                />
              </div>
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>Cancelar</Button>
              <Button type="submit" loading={saveMutation.isPending}>
                {editingClassroom ? 'Guardar cambios' : 'Crear aula'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteConfirm} onOpenChange={() => setDeleteConfirm(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirmar eliminación</DialogTitle>
            <DialogDescription>¿Estás seguro de eliminar esta aula? Esta acción no se puede deshacer.</DialogDescription>
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
