import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Search, CheckCircle, XCircle, Filter, Plus, Repeat, ArrowUpDown, ArrowUp, ArrowDown, CalendarX2, FileDown } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { TableSkeleton } from '@/components/ui/Skeleton';
import { getStatusLabel, getStatusColor, formatDateTime } from '@/lib/utils';
import type { Reservation, PaginatedResponse } from '@/types';

export default function AdminReservations() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [sortKey, setSortKey] = useState<string>('');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const toggleSort = (key: string) => {
    if (sortKey === key) {
      setSortDir((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      setSortKey(key);
      setSortDir('asc');
    }
  };

  const SortIcon = ({ column }: { column: string }) => {
    if (sortKey !== column) return <ArrowUpDown className="h-3 w-3 ml-1 inline text-primary-300" />;
    return sortDir === 'asc'
      ? <ArrowUp className="h-3 w-3 ml-1 inline text-accent-600" />
      : <ArrowDown className="h-3 w-3 ml-1 inline text-accent-600" />;
  };

  const { data: response, isLoading } = useQuery({
    queryKey: ['adminReservations', search, statusFilter, page],
    queryFn: () => {
      const params: Record<string, string> = { page: String(page) };
      if (search) params.search = search;
      if (statusFilter) params.status = statusFilter;
      return api.get<PaginatedResponse<Reservation>>('/reservations', params);
    },
  });
  const data = response?.value;
  const totalPages = response?.totalPages || 1;

  const handleSearch = (val: string) => { setSearch(val); setPage(1); };
  const handleStatusFilter = (val: string) => { setStatusFilter(val); setPage(1); };

  const sortedData = [...(data || [])].sort((a, b) => {
    if (!sortKey) return 0;
    const dir = sortDir === 'asc' ? 1 : -1;
    const getVal = (r: Reservation) => {
      switch (sortKey) {
        case 'title': return r.title.toLowerCase();
        case 'userName': return (r.userName || '').toLowerCase();
        case 'classroomName': return (r.classroomName || '').toLowerCase();
        case 'buildingName': return (r.buildingName || '').toLowerCase();
        case 'date': return r.date + r.startTime;
        case 'status': return r.status;
        default: return '';
      }
    };
    const va = getVal(a), vb = getVal(b);
    return va < vb ? -dir : va > vb ? dir : 0;
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/reservations/${id}/approve`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['adminReservations'] }),
  });

  const rejectMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/reservations/${id}/reject`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['adminReservations'] }),
  });

  const [selectedIds, setSelectedIds] = useState<string[]>([]);

  const bulkApprove = async () => {
    for (const id of selectedIds) {
      await approveMutation.mutateAsync(id);
    }
    setSelectedIds([]);
  };

  const bulkReject = async () => {
    for (const id of selectedIds) {
      await rejectMutation.mutateAsync(id);
    }
    setSelectedIds([]);
  };

  const toggleSelect = (id: string) => {
    setSelectedIds((prev) =>
      prev.includes(id) ? prev.filter((i) => i !== id) : [...prev, id]
    );
  };

  const exportToExcel = async () => {
    const XLSX = await import('xlsx');
    const rows = sortedData.map(r => ({
      Título: r.title,
      Usuario: r.userName || '—',
      Aula: r.classroomName,
      Edificio: r.buildingName || '—',
      Fecha: r.date.split('T')[0].split('-').reverse().join('/'),
      Horario: `${r.startTime}-${r.endTime}`,
      Estado: getStatusLabel(r.status),
    }));
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.json_to_sheet(rows);
    XLSX.utils.book_append_sheet(wb, ws, 'Reservaciones');
    XLSX.writeFile(wb, 'reservaciones.xlsx');
  };

  const exportToPdf = async () => {
    const [{ default: jsPDF }, { default: autoTable }] = await Promise.all([
      import('jspdf'),
      import('jspdf-autotable'),
    ]);
    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text('Reservaciones', 14, 15);
    doc.setFontSize(9);
    doc.text(`Exportado: ${new Date().toLocaleDateString('es-AR')}`, 14, 22);
    const body = sortedData.map(r => [
      r.title,
      r.userName || '—',
      r.classroomName,
      r.buildingName || '—',
      r.date.split('T')[0].split('-').reverse().join('/'),
      `${r.startTime}-${r.endTime}`,
      getStatusLabel(r.status),
    ] as string[]);
    autoTable(doc, {
      startY: 28,
      head: [['Título', 'Usuario', 'Aula', 'Edificio', 'Fecha', 'Horario', 'Estado']],
      body,
      styles: { fontSize: 7 },
      headStyles: { fillColor: [99, 102, 241] },
    });
    doc.save('reservaciones.pdf');
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Administrar Reservaciones</h1>
          <p className="text-primary-500 mt-1">Aprueba o rechaza solicitudes de reservación</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={exportToExcel}>
            <FileDown className="h-4 w-4 mr-1.5" />
            Excel
          </Button>
          <Button variant="outline" onClick={exportToPdf}>
            <FileDown className="h-4 w-4 mr-1.5" />
            PDF
          </Button>
          <Button variant="outline" onClick={() => navigate('/admin/holidays')}>
            <CalendarX2 className="h-4 w-4 mr-1.5" />
            Feriados
          </Button>
          <Button variant="accent" onClick={() => navigate('/classrooms')}>
            <Plus className="h-4 w-4 mr-1.5" />
            Nueva Reservación
          </Button>
        </div>
      </div>

      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
          <input
            placeholder="Buscar reservaciones..."
            value={search}
            onChange={(e) => handleSearch(e.target.value)}
            className="w-full h-10 pl-9 pr-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
          />
        </div>
        <Select
          placeholder="Todos los estados"
          value={statusFilter}
          onChange={(e) => handleStatusFilter(e.target.value)}
          options={[
            { value: 'Pending', label: 'Pendientes' },
            { value: 'Approved', label: 'Aprobadas' },
            { value: 'Rejected', label: 'Rechazadas' },
            { value: 'Cancelled', label: 'Canceladas' },
            { value: 'Completed', label: 'Completadas' },
          ]}
          className="sm:w-48"
        />
      </div>

      {selectedIds.length > 0 && (
        <div className="flex items-center gap-2 p-3 bg-accent-50 rounded-lg">
          <span className="text-sm text-accent-800 font-medium">
            {selectedIds.length} seleccionada(s)
          </span>
          <Button size="sm" variant="default" onClick={bulkApprove}>
            <CheckCircle className="h-4 w-4 mr-1" />
            Aprobar
          </Button>
          <Button size="sm" variant="destructive" onClick={bulkReject}>
            <XCircle className="h-4 w-4 mr-1" />
            Rechazar
          </Button>
        </div>
      )}

      {isLoading ? (
        <TableSkeleton rows={8} />
      ) : !data || data.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <p className="font-medium">No se encontraron reservaciones</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-10">
                  <input
                    type="checkbox"
                    className="rounded"
                    onChange={(e) => {
                      if (e.target.checked) setSelectedIds(sortedData.map((r) => r.id));
                      else setSelectedIds([]);
                    }}
                  />
                </TableHead>
                <TableHead>
                  <button className="flex items-center gap-0.5 font-medium" onClick={() => toggleSort('title')}>
                    Título <SortIcon column="title" />
                  </button>
                </TableHead>
                <TableHead className="hidden md:table-cell">
                  <button className="flex items-center gap-0.5 font-medium" onClick={() => toggleSort('userName')}>
                    Usuario <SortIcon column="userName" />
                  </button>
                </TableHead>
                <TableHead className="hidden sm:table-cell">
                  <button className="flex items-center gap-0.5 font-medium" onClick={() => toggleSort('classroomName')}>
                    Aula <SortIcon column="classroomName" />
                  </button>
                </TableHead>
                <TableHead className="hidden md:table-cell">
                  <button className="flex items-center gap-0.5 font-medium" onClick={() => toggleSort('buildingName')}>
                    Edificio <SortIcon column="buildingName" />
                  </button>
                </TableHead>
                <TableHead>
                  <button className="flex items-center gap-0.5 font-medium" onClick={() => toggleSort('date')}>
                    Fecha <SortIcon column="date" />
                  </button>
                </TableHead>
                <TableHead>
                  <button className="flex items-center gap-0.5 font-medium" onClick={() => toggleSort('status')}>
                    Estado <SortIcon column="status" />
                  </button>
                </TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sortedData.map((reservation) => (
                <TableRow key={reservation.id}>
                  <TableCell>
                    <input
                      type="checkbox"
                      checked={selectedIds.includes(reservation.id)}
                      onChange={() => toggleSelect(reservation.id)}
                      className="rounded"
                    />
                  </TableCell>
                  <TableCell className="font-medium">
                    <span className="flex items-center gap-1.5">
                      {reservation.recurringGroupId && (
                        <Repeat className="h-3.5 w-3.5 text-accent-500 flex-shrink-0" />
                      )}
                      {reservation.title}
                    </span>
                  </TableCell>
                  <TableCell className="hidden md:table-cell text-primary-500">
                    {reservation.userName || '—'}
                  </TableCell>
                  <TableCell className="hidden sm:table-cell text-primary-500">
                    {reservation.classroomName}
                  </TableCell>
                  <TableCell className="hidden md:table-cell text-primary-500">
                    {reservation.buildingName || '—'}
                  </TableCell>
                  <TableCell className="text-primary-500 text-sm">
                    {reservation.date.split('T')[0].split('-').reverse().join('/')}
                    <span className="block text-xs text-primary-400">
                      {reservation.startTime}-{reservation.endTime}
                    </span>
                  </TableCell>
                  <TableCell>
                    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${getStatusColor(reservation.status)}`}>
                      {getStatusLabel(reservation.status)}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-1">
                      {reservation.status === 'Pending' && (
                        <>
                          <button
                            className="p-1.5 rounded-md text-green-600 hover:bg-green-50"
                            onClick={() => approveMutation.mutate(reservation.id)}
                            title="Aprobar"
                          >
                            <CheckCircle className="h-4 w-4" />
                          </button>
                          <button
                            className="p-1.5 rounded-md text-red-600 hover:bg-red-50"
                            onClick={() => rejectMutation.mutate(reservation.id)}
                            title="Rechazar"
                          >
                            <XCircle className="h-4 w-4" />
                          </button>
                        </>
                      )}
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-between gap-4 pt-2">
          <p className="text-sm text-primary-400">
            Página {page} de {totalPages}
          </p>
          <div className="flex items-center gap-1">
            <Button
              variant="outline"
              size="sm"
              disabled={page <= 1}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
            >
              Anterior
            </Button>
            {Array.from({ length: totalPages }, (_, i) => i + 1)
              .filter((p) => p === 1 || p === totalPages || Math.abs(p - page) <= 2)
              .map((p, idx, arr) => (
                <span key={p} className="flex items-center">
                  {idx > 0 && arr[idx - 1] !== p - 1 && (
                    <span className="px-1 text-primary-300 text-sm">…</span>
                  )}
                  <button
                    className={`min-w-[2rem] h-8 rounded-md text-sm font-medium transition-colors ${
                      p === page
                        ? 'bg-accent-600 text-white'
                        : 'text-primary-600 hover:bg-primary-100'
                    }`}
                    onClick={() => setPage(p)}
                  >
                    {p}
                  </button>
                </span>
              ))}
            <Button
              variant="outline"
              size="sm"
              disabled={page >= totalPages}
              onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
            >
              Siguiente
            </Button>
          </div>
        </div>
      )}
    </div>
  );
}
