import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Pencil, Trash2, Check, X } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardContent } from '@/components/ui/Card';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';

interface Accessory {
  id: string;
  name: string;
  description: string | null;
  isActive: boolean;
  isMovable: boolean;
}

function Checkbox({ checked, onChange, label }: { checked: boolean; onChange: (v: boolean) => void; label: string }) {
  return (
    <label className="flex items-center gap-2 text-sm text-primary-700 cursor-pointer select-none">
      <input type="checkbox" checked={checked} onChange={(e) => onChange(e.target.checked)}
        className="h-4 w-4 rounded border-primary-300 text-primary-600 focus:ring-primary-500" />
      {label}
    </label>
  );
}

export default function AdminAccesorios() {
  const queryClient = useQueryClient();
  const [formName, setFormName] = useState('');
  const [formDesc, setFormDesc] = useState('');
  const [formMovil, setFormMovil] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);

  const { data: items, isLoading } = useQuery<Accessory[]>({
    queryKey: ['accessories'],
    queryFn: () => api.get<Accessory[]>('/accessories'),
  });

  const createMutation = useMutation({
    mutationFn: (body: { name: string; description?: string; isMovable?: boolean }) =>
      api.post('/accessories', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accessories'] });
      setFormName('');
      setFormDesc('');
      setFormMovil(false);
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, name, description, isActive, isMovable }: { id: string; name: string; description?: string; isActive: boolean; isMovable: boolean }) =>
      api.put(`/accessories/${id}`, { name, description, isActive, isMovable }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accessories'] });
      setEditingId(null);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/accessories/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['accessories'] }),
  });

  const startEdit = (item: Accessory) => {
    setEditingId(item.id);
    setFormName(item.name);
    setFormDesc(item.description || '');
    setFormMovil(item.isMovable);
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!formName.trim()) return;
    createMutation.mutate({ name: formName.trim(), description: formDesc.trim() || undefined, isMovable: formMovil });
  };

  return (
    <div className="space-y-6">
      <h1 className="font-heading text-2xl font-bold text-primary-800">Accesorios de Bedelía</h1>
      <p className="text-primary-500 text-sm -mt-4">
        Elementos que se pueden entregar junto con las llaves. Los móviles pueden llevarse entre aulas.
      </p>

      <Card>
        <CardContent className="pt-6">
          <form onSubmit={handleSubmit} className="flex flex-wrap items-end gap-3">
            <div className="w-64">
              <Input label="Nombre" placeholder="Ej: Control remoto TV" value={formName}
                onChange={(e) => setFormName(e.target.value)} required />
            </div>
            <div className="w-80">
              <Input label="Descripción (opcional)" placeholder="Ej: Control del aula 101" value={formDesc}
                onChange={(e) => setFormDesc(e.target.value)} />
            </div>
            <div className="pb-2">
              <Checkbox checked={formMovil} onChange={setFormMovil} label="Móvil (se lleva entre aulas)" />
            </div>
            <Button type="submit" loading={createMutation.isPending}>
              <Plus className="h-4 w-4 mr-1" /> Agregar
            </Button>
          </form>
        </CardContent>
      </Card>

      {isLoading ? (
        <p className="text-primary-400 text-sm">Cargando...</p>
      ) : !items?.length ? (
        <p className="text-primary-400 text-sm text-center py-8">No hay accesorios registrados</p>
      ) : (
        <Card>
          <CardContent className="pt-6">
            <Table>
              <TableHeader>
                <TableHead>Nombre</TableHead>
                <TableHead>Descripción</TableHead>
                <TableHead>Activo</TableHead>
                <TableHead>Móvil</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableHeader>
              <TableBody>
                {items.map((item) => (
                  <TableRow key={item.id}>
                    {editingId === item.id ? (
                      <>
                        <TableCell>
                          <Input value={formName} onChange={(e) => setFormName(e.target.value)} />
                        </TableCell>
                        <TableCell colSpan={2}>
                          <div className="flex items-end gap-2">
                            <Input value={formDesc} onChange={(e) => setFormDesc(e.target.value)} />
                            <Button variant="outline" size="sm"
                              onClick={() => updateMutation.mutate({ id: item.id, name: formName || item.name, description: formDesc || item.description || undefined, isActive: !item.isActive, isMovable: formMovil })}>
                              {item.isActive ? 'Desactivar' : 'Activar'}
                            </Button>
                          </div>
                        </TableCell>
                        <TableCell>
                          <Checkbox checked={formMovil} onChange={setFormMovil} label="Móvil" />
                        </TableCell>
                        <TableCell className="text-right">
                          <Button variant="ghost" size="sm" onClick={() => {
                            updateMutation.mutate({ id: item.id, name: formName || item.name, description: formDesc || item.description || undefined, isActive: item.isActive, isMovable: formMovil });
                          }}>
                            <Check className="h-4 w-4 text-green-600" />
                          </Button>
                          <Button variant="ghost" size="sm" onClick={() => setEditingId(null)}>
                            <X className="h-4 w-4" />
                          </Button>
                        </TableCell>
                      </>
                    ) : (
                      <>
                        <TableCell className="font-medium">{item.name}</TableCell>
                        <TableCell className="text-primary-400 text-sm">{item.description || '—'}</TableCell>
                        <TableCell>
                          <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${item.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-500'}`}>
                            {item.isActive ? 'Sí' : 'No'}
                          </span>
                        </TableCell>
                        <TableCell>
                          <span className={`text-xs font-medium px-2 py-0.5 rounded-full ${item.isMovable ? 'bg-blue-100 text-blue-700' : 'bg-gray-100 text-gray-500'}`}>
                            {item.isMovable ? 'Sí' : 'No'}
                          </span>
                        </TableCell>
                        <TableCell className="text-right">
                          <Button variant="ghost" size="sm" onClick={() => startEdit(item)}>
                            <Pencil className="h-4 w-4" />
                          </Button>
                          <Button variant="ghost" size="sm" onClick={() => deleteMutation.mutate(item.id)}
                            disabled={deleteMutation.isPending}>
                            <Trash2 className="h-4 w-4 text-red-500" />
                          </Button>
                        </TableCell>
                      </>
                    )}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
