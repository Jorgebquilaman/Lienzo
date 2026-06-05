import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Badge } from '@/components/ui/Badge';
import { Card, CardContent } from '@/components/ui/Card';
import { useAuthStore } from '@/stores/authStore';
import { CalendarDays, Clock, MapPin, Users, BookOpen, FileDown, FileSpreadsheet, ChevronLeft, ChevronRight } from 'lucide-react';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';

interface ClaseListItem {
  id: string;
  actividadNombre: string;
  classroomName: string;
  fecha: string;
  horaInicio: string;
  horaFin: string;
  estado: string;
  totalAlumnos: number;
  presentes: number;
  docenteNombre: string;
  createdAt: string;
}

interface PaginatedResponse<T> {
  value: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

function getStatusBadge(estado: string) {
  switch (estado) {
    case 'Abierta': return <Badge variant="approved">Abierta</Badge>;
    case 'Cerrada': return <Badge variant="cancelled">Cerrada</Badge>;
    default: return <Badge>{estado}</Badge>;
  }
}

export default function AsistenciasList() {
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const [page, setPage] = useState(1);
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [estado, setEstado] = useState('');
  const pageSize = 20;

  const { data, isLoading } = useQuery<PaginatedResponse<ClaseListItem>>({
    queryKey: ['asistencias', page, desde, hasta, estado],
    queryFn: () => {
      const params: Record<string, string> = { page: String(page), pageSize: String(pageSize) };
      if (desde) params.desde = desde;
      if (hasta) params.hasta = hasta;
      if (estado) params.estado = estado;
      return api.get<PaginatedResponse<ClaseListItem>>('/asistencia', params);
    },
  });

  async function fetchAll() {
    const params: Record<string, string> = { page: '1', pageSize: '100000' };
    if (desde) params.desde = desde;
    if (hasta) params.hasta = hasta;
    if (estado) params.estado = estado;
    const res = await api.get<PaginatedResponse<ClaseListItem>>('/asistencia', params);
    return res.value;
  }

  async function exportExcel() {
    const rows = await fetchAll();
    if (!rows.length) return;
    const ws = XLSX.utils.json_to_sheet(
      rows.map((r) => ({
        Actividad: r.actividadNombre,
        Aula: r.classroomName,
        Fecha: r.fecha,
        'Hora Inicio': r.horaInicio,
        'Hora Fin': r.horaFin,
        Alumnos: `${r.presentes}/${r.totalAlumnos}`,
        Estado: r.estado,
        Docente: r.docenteNombre,
      }))
    );
    const wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'Asistencias');
    XLSX.writeFile(wb, `asistencias_${new Date().toISOString().slice(0, 10)}.xlsx`);
  }

  async function exportPdf() {
    const rows = await fetchAll();
    if (!rows.length) return;
    const doc = new jsPDF({ orientation: 'landscape' });
    autoTable(doc, {
      head: [['Actividad', 'Aula', 'Fecha', 'Inicio', 'Fin', 'Alumnos', 'Estado', 'Docente']],
      body: rows.map((r) => [
        r.actividadNombre,
        r.classroomName,
        r.fecha,
        r.horaInicio,
        r.horaFin,
        `${r.presentes}/${r.totalAlumnos}`,
        r.estado,
        r.docenteNombre,
      ]),
    });
    doc.save(`asistencias_${new Date().toISOString().slice(0, 10)}.pdf`);
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-2xl font-bold text-primary-800">Asistencias</h1>
      </div>

      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-wrap items-end gap-3 mb-6">
            <div className="w-40">
              <Input label="Desde" type="date" value={desde} onChange={(e) => { setDesde(e.target.value); setPage(1); }} />
            </div>
            <div className="w-40">
              <Input label="Hasta" type="date" value={hasta} onChange={(e) => { setHasta(e.target.value); setPage(1); }} />
            </div>
            <div>
              <label className="block text-sm font-medium text-primary-700 mb-1">Estado</label>
              <select
                value={estado}
                onChange={(e) => { setEstado(e.target.value); setPage(1); }}
                className="h-10 rounded-lg border border-primary-200 bg-white px-3 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
              >
                <option value="">Todos</option>
                <option value="Abierta">Abierta</option>
                <option value="Cerrada">Cerrada</option>
              </select>
            </div>
            {(desde || hasta) && (
              <Button variant="ghost" size="sm" onClick={() => { setDesde(''); setHasta(''); setPage(1); }}>
                Limpiar
              </Button>
            )}
            <div className="flex gap-2 ml-auto">
              <Button variant="outline" size="sm" onClick={exportPdf}>
                <FileDown className="h-4 w-4 mr-1" /> PDF
              </Button>
              <Button variant="outline" size="sm" onClick={exportExcel}>
                <FileSpreadsheet className="h-4 w-4 mr-1" /> Excel
              </Button>
            </div>
          </div>

          {isLoading ? (
            <p className="text-primary-400 text-sm">Cargando...</p>
          ) : !data?.value?.length ? (
            <div className="text-center py-12 text-primary-400">
              <BookOpen className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p>No hay asistencias registradas</p>
            </div>
          ) : (
            <>
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b border-primary-100">
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Actividad</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Aula</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Fecha</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Horario</th>
                      <th className="text-center py-3 px-2 font-medium text-primary-500">Alumnos</th>
                      <th className="text-center py-3 px-2 font-medium text-primary-500">Estado</th>
                      <th className="text-left py-3 px-2 font-medium text-primary-500">Docente</th>
                    </tr>
                  </thead>
                  <tbody>
                    {data.value.map((clase) => (
                      <tr key={clase.id}
                        className="border-b border-primary-50 hover:bg-primary-50 cursor-pointer transition-colors"
                        onClick={() => navigate(`/asistencia/${clase.id}`)}
                      >
                        <td className="py-3 px-2 font-medium text-primary-800">{clase.actividadNombre}</td>
                        <td className="py-3 px-2 text-primary-600">
                          <div className="flex items-center gap-1">
                            <MapPin className="h-3.5 w-3.5 text-primary-400" />
                            {clase.classroomName}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-primary-600">
                          <div className="flex items-center gap-1">
                            <CalendarDays className="h-3.5 w-3.5 text-primary-400" />
                            {clase.fecha}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-primary-600">
                          <div className="flex items-center gap-1">
                            <Clock className="h-3.5 w-3.5 text-primary-400" />
                            {clase.horaInicio} — {clase.horaFin}
                          </div>
                        </td>
                        <td className="py-3 px-2 text-center">
                          <div className="flex items-center justify-center gap-1">
                            <Users className="h-3.5 w-3.5 text-primary-400" />
                            <span className={clase.presentes === clase.totalAlumnos ? 'text-green-600 font-medium' : 'text-primary-600'}>
                              {clase.presentes}/{clase.totalAlumnos}
                            </span>
                          </div>
                        </td>
                        <td className="py-3 px-2 text-center">{getStatusBadge(clase.estado)}</td>
                        <td className="py-3 px-2 text-primary-600 text-xs">{clase.docenteNombre}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              {data.totalPages > 1 && (
                <div className="flex items-center justify-between pt-4 border-t border-primary-100 mt-4">
                  <p className="text-sm text-primary-500">
                    Página {data.page} de {data.totalPages} ({data.totalCount} registros)
                  </p>
                  <div className="flex gap-2">
                    <Button variant="outline" size="sm" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
                      <ChevronLeft className="h-4 w-4" />
                    </Button>
                    <Button variant="outline" size="sm" disabled={page >= data.totalPages} onClick={() => setPage((p) => p + 1)}>
                      <ChevronRight className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
