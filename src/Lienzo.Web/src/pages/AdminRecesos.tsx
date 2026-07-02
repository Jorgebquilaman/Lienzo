import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, CalendarOff } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';

interface Receso {
  id: string;
  startDate: string;
  endDate: string;
  description: string;
}

function formatDate(dateStr: string): string {
  return dateStr.split('-').reverse().join('/');
}

export default function AdminRecesos() {
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [description, setDescription] = useState('');
  const queryClient = useQueryClient();

  const { data: response, isLoading } = useQuery({
    queryKey: ['recesos'],
    queryFn: () => api.get<Receso[]>('/recesos'),
  });
  const recesos = response || [];

  const createMutation = useMutation({
    mutationFn: (body: { startDate: string; endDate: string; description: string }) =>
      api.post('/recesos', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['recesos'] });
      setStartDate('');
      setEndDate('');
      setDescription('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/recesos/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['recesos'] }),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!startDate || !endDate || !description.trim()) return;
    createMutation.mutate({ startDate, endDate, description: description.trim() });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Receso Académico</h1>
        <p className="text-primary-500 mt-1">Gestiona los períodos de receso académico donde no se permiten reservas</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col sm:flex-row gap-3 items-end">
        <div className="flex-1 w-full">
          <label className="block text-sm font-medium text-primary-700 mb-1">Fecha inicio</label>
          <Input type="date" value={startDate} onChange={(e) => setStartDate(e.target.value)} required />
        </div>
        <div className="flex-1 w-full">
          <label className="block text-sm font-medium text-primary-700 mb-1">Fecha fin</label>
          <Input type="date" value={endDate} onChange={(e) => setEndDate(e.target.value)} required />
        </div>
        <div className="flex-[2] w-full">
          <label className="block text-sm font-medium text-primary-700 mb-1">Motivo</label>
          <Input
            placeholder="Ej: Receso de verano"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
          />
        </div>
        <Button type="submit" variant="accent" disabled={!startDate || !endDate || !description.trim()}>
          <Plus className="h-4 w-4 mr-1.5" />
          Agregar
        </Button>
      </form>

      {isLoading ? (
        <div className="text-center py-16 text-primary-400">Cargando...</div>
      ) : recesos.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <CalendarOff className="h-12 w-12 mx-auto mb-3 opacity-50" />
          <p className="font-medium">No hay períodos de receso registrados</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha inicio</TableHead>
                <TableHead>Fecha fin</TableHead>
                <TableHead>Motivo</TableHead>
                <TableHead className="w-20 text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {[...recesos]
                .sort((a: Receso, b: Receso) => a.startDate.localeCompare(b.startDate))
                .map((r: Receso) => (
                  <TableRow key={r.id}>
                    <TableCell className="font-medium">{formatDate(r.startDate)}</TableCell>
                    <TableCell className="font-medium">{formatDate(r.endDate)}</TableCell>
                    <TableCell className="text-primary-500">{r.description}</TableCell>
                    <TableCell className="text-right">
                      <button
                        className="p-1.5 rounded-md text-red-500 hover:bg-red-50"
                        onClick={() => deleteMutation.mutate(r.id)}
                        title="Eliminar"
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
    </div>
  );
}
