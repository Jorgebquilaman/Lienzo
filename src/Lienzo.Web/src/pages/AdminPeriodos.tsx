import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, Calendar, RefreshCw } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';

interface Periodo {
  id: string;
  nombre: string;
  fechaInicio: string;
  fechaFin: string;
  anio: number;
}

export default function AdminPeriodos() {
  const [nombre, setNombre] = useState('');
  const [fechaInicio, setFechaInicio] = useState('');
  const [fechaFin, setFechaFin] = useState('');
  const [anio, setAnio] = useState(new Date().getFullYear().toString());
  const queryClient = useQueryClient();

  const { data: response, isLoading } = useQuery({
    queryKey: ['periodos'],
    queryFn: () => api.get<Periodo[]>('/periodos'),
  });
  const periodos = response || [];

  const createMutation = useMutation({
    mutationFn: (body: { nombre: string; fechaInicio: string; fechaFin: string; anio: number }) =>
      api.post('/periodos', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['periodos'] });
      setNombre(''); setFechaInicio(''); setFechaFin(''); setAnio(new Date().getFullYear().toString());
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/periodos/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['periodos'] }),
  });

  const [syncResult, setSyncResult] = useState<string | null>(null);

  const syncTiposMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; totalExterno: number }>('/periodos/sync-tipos'),
    onSuccess: (data) => {
      setSyncResult(`Tipos de período sincronizados: ${data.creados} creados, ${data.existentes} existentes`);
      queryClient.invalidateQueries({ queryKey: ['periodos'] });
    },
  });

  const syncPeriodosMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; actualizados: number; totalExterno: number }>('/periodos/sync'),
    onSuccess: (data) => {
      setSyncResult(`Períodos sincronizados: ${data.creados} creados, ${data.actualizados} actualizados, ${data.existentes} existentes`);
      queryClient.invalidateQueries({ queryKey: ['periodos'] });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!nombre || !fechaInicio || !fechaFin) return;
    createMutation.mutate({ nombre, fechaInicio, fechaFin, anio: parseInt(anio) });
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Periodos</h1>
          <p className="text-primary-500 mt-1">Gestiona los periodos académicos (cuatrimestres, anuales)</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => syncTiposMutation.mutate()} loading={syncTiposMutation.isPending}>
            <RefreshCw className="h-4 w-4 mr-2" /> Sinc. Tipos
          </Button>
          <Button variant="outline" onClick={() => syncPeriodosMutation.mutate()} loading={syncPeriodosMutation.isPending}>
            <RefreshCw className="h-4 w-4 mr-2" /> Sinc. Períodos
          </Button>
        </div>
      </div>

      {syncResult && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3 flex items-center justify-between">
          <p className="text-sm text-green-800">{syncResult}</p>
          <button className="text-green-600 hover:text-green-800 text-sm font-medium" onClick={() => setSyncResult(null)}>Cerrar</button>
        </div>
      )}

      <form onSubmit={handleSubmit} className="flex flex-wrap gap-3 items-end">
        <div className="w-48">
          <label className="block text-sm font-medium text-primary-700 mb-1">Nombre</label>
          <Input placeholder="Ej: 1er Cuatrimestre" value={nombre} onChange={e => setNombre(e.target.value)} required />
        </div>
        <div className="w-44">
          <label className="block text-sm font-medium text-primary-700 mb-1">Inicio</label>
          <Input type="date" value={fechaInicio} onChange={e => setFechaInicio(e.target.value)} required />
        </div>
        <div className="w-44">
          <label className="block text-sm font-medium text-primary-700 mb-1">Fin</label>
          <Input type="date" value={fechaFin} onChange={e => setFechaFin(e.target.value)} required />
        </div>
        <div className="w-28">
          <label className="block text-sm font-medium text-primary-700 mb-1">Año</label>
          <Input type="number" value={anio} onChange={e => setAnio(e.target.value)} required />
        </div>
        <Button type="submit" variant="accent" disabled={!nombre || !fechaInicio || !fechaFin}>
          <Plus className="h-4 w-4 mr-1.5" /> Agregar
        </Button>
      </form>

      {isLoading ? <div className="text-center py-16 text-primary-400">Cargando...</div>
      : periodos.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <Calendar className="h-12 w-12 mx-auto mb-3 opacity-50" />
          <p className="font-medium">No hay periodos registrados</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Inicio</TableHead>
                <TableHead>Fin</TableHead>
                <TableHead>Año</TableHead>
                <TableHead className="w-20 text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {[...periodos].sort((a, b) => b.anio - a.anio || a.nombre.localeCompare(b.nombre)).map(p => (
                <TableRow key={p.id}>
                  <TableCell className="font-medium">{p.nombre}</TableCell>
                  <TableCell className="text-primary-500">{p.fechaInicio}</TableCell>
                  <TableCell className="text-primary-500">{p.fechaFin}</TableCell>
                  <TableCell className="text-primary-500">{p.anio}</TableCell>
                  <TableCell className="text-right">
                    <button className="p-1.5 rounded-md text-red-500 hover:bg-red-50" onClick={() => deleteMutation.mutate(p.id)} title="Eliminar"><Trash2 className="h-4 w-4" /></button>
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
