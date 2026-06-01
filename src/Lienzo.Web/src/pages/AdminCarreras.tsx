import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, GraduationCap, RefreshCw } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';

interface Carrera {
  id: string;
  nombre: string;
  codigo: string;
}

export default function AdminCarreras() {
  const [nombre, setNombre] = useState('');
  const [codigo, setCodigo] = useState('');
  const queryClient = useQueryClient();

  const { data: response, isLoading } = useQuery({
    queryKey: ['carreras'],
    queryFn: () => api.get<Carrera[]>('/carreras'),
  });
  const carreras = response || [];

  const createMutation = useMutation({
    mutationFn: (body: { nombre: string; codigo: string }) => api.post('/carreras', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['carreras'] });
      setNombre(''); setCodigo('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/carreras/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['carreras'] }),
  });

  const [syncResult, setSyncResult] = useState<string | null>(null);
  const syncMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; totalExterno: number }>('/carreras/sync'),
    onSuccess: (data) => {
      setSyncResult(`Carreras sincronizadas: ${data.creados} creadas, ${data.existentes} existentes (${data.totalExterno} en total)`);
      queryClient.invalidateQueries({ queryKey: ['carreras'] });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!nombre || !codigo) return;
    createMutation.mutate({ nombre, codigo });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Carreras</h1>
          <p className="text-primary-500 mt-1">Gestiona las propuestas académicas / carreras</p>
        </div>
        <Button variant="outline" onClick={() => syncMutation.mutate()} loading={syncMutation.isPending}>
          <RefreshCw className="h-4 w-4 mr-2" /> Sincronizar
        </Button>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-wrap gap-3 items-end">
        <div className="w-72">
          <label className="block text-sm font-medium text-primary-700 mb-1">Nombre</label>
          <Input placeholder="Ej: Ingeniería en Sistemas" value={nombre} onChange={e => setNombre(e.target.value)} required />
        </div>
        <div className="w-36">
          <label className="block text-sm font-medium text-primary-700 mb-1">Código</label>
          <Input placeholder="Ej: IS-01" value={codigo} onChange={e => setCodigo(e.target.value)} required />
        </div>
        <Button type="submit" variant="accent" disabled={!nombre || !codigo}>
          <Plus className="h-4 w-4 mr-1.5" /> Agregar
        </Button>
      </form>

      {syncResult && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3 flex items-center justify-between">
          <p className="text-sm text-green-800">{syncResult}</p>
          <button className="text-green-600 hover:text-green-800 text-sm font-medium" onClick={() => setSyncResult(null)}>Cerrar</button>
        </div>
      )}

      {isLoading ? <div className="text-center py-16 text-primary-400">Cargando...</div>
      : carreras.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <GraduationCap className="h-12 w-12 mx-auto mb-3 opacity-50" />
          <p className="font-medium">No hay carreras registradas</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Código</TableHead>
                <TableHead className="w-20 text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {carreras.sort((a, b) => a.nombre.localeCompare(b.nombre)).map(c => (
                <TableRow key={c.id}>
                  <TableCell className="font-medium">{c.nombre}</TableCell>
                  <TableCell className="text-primary-500">{c.codigo}</TableCell>
                  <TableCell className="text-right">
                    <button className="p-1.5 rounded-md text-red-500 hover:bg-red-50" onClick={() => deleteMutation.mutate(c.id)} title="Eliminar"><Trash2 className="h-4 w-4" /></button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
    </div>
  );
}
