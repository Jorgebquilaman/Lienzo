import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, CalendarX2 } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';

interface Holiday {
  id: string;
  date: string;
  description: string;
}

export default function AdminHolidays() {
  const [date, setDate] = useState('');
  const [description, setDescription] = useState('');
  const queryClient = useQueryClient();

  const { data: response, isLoading } = useQuery({
    queryKey: ['holidays'],
    queryFn: () => api.get<Holiday[]>('/holidays'),
  });
  const holidays = response || [];

  const createMutation = useMutation({
    mutationFn: (body: { date: string; description: string }) =>
      api.post('/holidays', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['holidays'] });
      setDate('');
      setDescription('');
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/holidays/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['holidays'] }),
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!date || !description.trim()) return;
    createMutation.mutate({ date, description: description.trim() });
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Feriados</h1>
        <p className="text-primary-500 mt-1">Gestiona los días feriados del calendario</p>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col sm:flex-row gap-3 items-end">
        <div className="flex-1 w-full">
          <label className="block text-sm font-medium text-primary-700 mb-1">Fecha</label>
          <Input type="date" value={date} onChange={(e) => setDate(e.target.value)} required />
        </div>
        <div className="flex-[2] w-full">
          <label className="block text-sm font-medium text-primary-700 mb-1">Motivo</label>
          <Input
            placeholder="Ej: Día de la Independencia"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
          />
        </div>
        <Button type="submit" variant="accent" disabled={!date || !description.trim()}>
          <Plus className="h-4 w-4 mr-1.5" />
          Agregar
        </Button>
      </form>

      {isLoading ? (
        <div className="text-center py-16 text-primary-400">Cargando...</div>
      ) : holidays.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <CalendarX2 className="h-12 w-12 mx-auto mb-3 opacity-50" />
          <p className="font-medium">No hay feriados registrados</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Fecha</TableHead>
                <TableHead>Motivo</TableHead>
                <TableHead className="w-20 text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {[...holidays]
                .sort((a: Holiday, b: Holiday) => a.date.localeCompare(b.date))
                .map((h: Holiday) => (
                  <TableRow key={h.id}>
                    <TableCell className="font-medium">{h.date.split('T')[0].split('-').reverse().join('/')}</TableCell>
                    <TableCell className="text-primary-500">{h.description}</TableCell>
                    <TableCell className="text-right">
                      <button
                        className="p-1.5 rounded-md text-red-500 hover:bg-red-50"
                        onClick={() => deleteMutation.mutate(h.id)}
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
