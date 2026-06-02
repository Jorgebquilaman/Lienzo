import { useState, useRef, useMemo } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, Pencil, BookOpen, Repeat, RefreshCw, X, Search, ArrowUpDown, ArrowUp, ArrowDown, FileDown, AlertTriangle } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogBody, DialogFooter } from '@/components/ui/Dialog';

interface Periodo { id: string; nombre: string; fechaInicio: string; fechaFin: string; anio: number; }
interface Carrera { id: string; nombre: string; codigo: string; }
interface Classroom { id: string; name: string; }
interface User { id: string; firstName: string; lastName: string; role: string; }

interface Actividad {
  id: string; nombre: string; codigoMateria: string;
  periodoId: string; periodoNombre?: string;
  carreraId: string; carreraNombre?: string;
  aulaId?: string; aulaNombre?: string;
  diaSemana?: string; horaInicio?: string; horaFin?: string;
  docenteIds: string[]; docentesNombres?: string;
}

const DAYS = [
  { value: 'Monday', label: 'Lunes' },
  { value: 'Tuesday', label: 'Martes' },
  { value: 'Wednesday', label: 'Miércoles' },
  { value: 'Thursday', label: 'Jueves' },
  { value: 'Friday', label: 'Viernes' },
  { value: 'Saturday', label: 'Sábado' },
];

export default function AdminActividades() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Actividad | null>(null);
  const [nombre, setNombre] = useState('');
  const [codigoMateria, setCodigoMateria] = useState('');
  const [periodoId, setPeriodoId] = useState('');
  const [carreraId, setCarreraId] = useState('');
  const [docenteIds, setDocenteIds] = useState<string[]>([]);
  const [aulaId, setAulaId] = useState('');
  const [diaSemana, setDiaSemana] = useState('');
  const [horaInicio, setHoraInicio] = useState('');
  const [horaFin, setHoraFin] = useState('');
  const [hasSchedule, setHasSchedule] = useState(false);
  const [docenteSearch, setDocenteSearch] = useState('');
  const [showDocenteDropdown, setShowDocenteDropdown] = useState(false);
  const [docenteNames, setDocenteNames] = useState<Map<string, string>>(new Map());
  const docenteSearchRef = useRef<HTMLDivElement>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [filterPeriodoId, setFilterPeriodoId] = useState('');
  const [filterCarreraId, setFilterCarreraId] = useState('');
  const [sortKey, setSortKey] = useState<string>('nombre');
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc');
  const [deleteConfirm, setDeleteConfirm] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const { data: response, isLoading } = useQuery({
    queryKey: ['actividades'],
    queryFn: () => api.get<Actividad[]>('/actividades'),
  });
  const actividades = response || [];

  const sorted = useMemo(() => {
    const filtered = actividades.filter((a) =>
      (!searchQuery || a.nombre.toLowerCase().includes(searchQuery.toLowerCase()) ||
      a.codigoMateria.toLowerCase().includes(searchQuery.toLowerCase()) ||
      (a.periodoNombre || '').toLowerCase().includes(searchQuery.toLowerCase()) ||
      (a.carreraNombre || '').toLowerCase().includes(searchQuery.toLowerCase())) &&
      (!filterPeriodoId || a.periodoId === filterPeriodoId) &&
      (!filterCarreraId || a.carreraId === filterCarreraId)
    );
    return [...filtered].sort((a, b) => {
      let cmp = 0;
      const av = (a as any)[sortKey] ?? '';
      const bv = (b as any)[sortKey] ?? '';
      if (typeof av === 'string') cmp = av.localeCompare(bv);
      else if (typeof av === 'number') cmp = av - bv;
      return sortDir === 'asc' ? cmp : -cmp;
    });
  }, [actividades, searchQuery, filterPeriodoId, filterCarreraId, sortKey, sortDir]);

  const toggleSort = (key: string) => {
    if (sortKey === key) setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    else { setSortKey(key); setSortDir('asc'); }
  };

  const formatHorario = (a: Actividad) =>
    a.aulaId ? `${DAYS.find(d => d.value === a.diaSemana)?.label.slice(0, 3) || ''} ${a.horaInicio}-${a.horaFin}` : 'Sin horario';

  const exportToExcel = async () => {
    const XLSX = await import('xlsx');
    const rows = sorted.map(a => ({
      Nombre: a.nombre,
      Código: a.codigoMateria,
      Periodo: a.periodoNombre || '—',
      Carrera: a.carreraNombre || '—',
      Horario: formatHorario(a),
      Docentes: a.docentesNombres || '—',
    }));
    const wb = XLSX.utils.book_new();
    const ws = XLSX.utils.json_to_sheet(rows);
    XLSX.utils.book_append_sheet(wb, ws, 'Actividades');
    XLSX.writeFile(wb, 'actividades.xlsx');
  };

  const exportToPdf = async () => {
    const [{ default: jsPDF }, { default: autoTable }] = await Promise.all([
      import('jspdf'),
      import('jspdf-autotable'),
    ]);
    const doc = new jsPDF();
    doc.setFontSize(16);
    doc.text('Actividades', 14, 15);
    doc.setFontSize(9);
    doc.text(`Exportado: ${new Date().toLocaleDateString('es-AR')}`, 14, 22);
    const body = sorted.map(a => [
      a.nombre, a.codigoMateria,
      a.periodoNombre || '—', a.carreraNombre || '—',
      formatHorario(a), a.docentesNombres || '—',
    ]);
    autoTable(doc, {
      startY: 28,
      head: [['Nombre', 'Código', 'Periodo', 'Carrera', 'Horario', 'Docentes']],
      body,
      styles: { fontSize: 7 },
      headStyles: { fillColor: [99, 102, 241] },
    });
    doc.save('actividades.pdf');
  };

  const { data: periodosRes } = useQuery({ queryKey: ['periodos'], queryFn: () => api.get<Periodo[]>('/periodos') });
  const periodos = periodosRes || [];
  const { data: carrerasRes } = useQuery({ queryKey: ['carreras'], queryFn: () => api.get<Carrera[]>('/carreras') });
  const carreras = carrerasRes || [];
  const { data: classroomsRes } = useQuery({ queryKey: ['classrooms-all'], queryFn: () => api.get<Classroom[]>('/classrooms') });
  const classrooms = classroomsRes || [];
  const { data: usersRes } = useQuery({
    queryKey: ['users'],
    queryFn: async () => {
      const r = await api.get<{ value: User[] }>('/users');
      return r.value || r;
    },
  });
  const users = (usersRes || []).filter((u: User) => u.role === 'Teacher') as User[];

  const createMutation = useMutation({
    mutationFn: (body: unknown) => api.post('/actividades', body),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['actividades'] }); closeDialog(); },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, body }: { id: string; body: unknown }) => api.put(`/actividades/${id}`, body),
    onSuccess: () => { queryClient.invalidateQueries({ queryKey: ['actividades'] }); closeDialog(); },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => api.delete(`/actividades/${id}`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['actividades'] }),
  });

  const [syncResult, setSyncResult] = useState<string | null>(null);

  const syncMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; sinPeriodo: number; sinCarrera: number; totalExterno: number; corregidos: number }>('/actividades/sync'),
    onSuccess: (data) => {
      setSyncResult(`Actividades sincronizadas: ${data.creados} creadas, ${data.existentes} existentes, ${data.corregidos} corregidas${data.sinPeriodo > 0 ? `, ${data.sinPeriodo} sin período` : ''}${data.sinCarrera > 0 ? `, ${data.sinCarrera} sin carrera` : ''}`);
      queryClient.invalidateQueries({ queryKey: ['actividades'] });
    },
  });

  const openCreate = () => {
    setEditing(null);
    setNombre(''); setCodigoMateria(''); setPeriodoId(''); setCarreraId('');
    setDocenteIds([]); setDocenteNames(new Map()); setAulaId(''); setDiaSemana(''); setHoraInicio(''); setHoraFin(''); setHasSchedule(false);
    setDialogOpen(true);
  };

  const openEdit = (a: Actividad) => {
    setEditing(a);
    setNombre(a.nombre); setCodigoMateria(a.codigoMateria);
    setPeriodoId(a.periodoId); setCarreraId(a.carreraId);
    setDocenteIds([...new Set(a.docenteIds)]);
    // Pre-build name map from docentesNombres for fallback
    const names = (a.docentesNombres || '').split(',').map(s => s.trim());
    setDocenteNames(new Map(a.docenteIds.map((id, i) => [id, names[i] || id])));
    setAulaId(a.aulaId || ''); setDiaSemana(a.diaSemana || '');
    setHoraInicio(a.horaInicio || ''); setHoraFin(a.horaFin || '');
    setHasSchedule(!!a.aulaId);
    setDialogOpen(true);
  };

  const closeDialog = () => { setDialogOpen(false); setEditing(null); setDocenteSearch(''); setShowDocenteDropdown(false); };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const body: Record<string, unknown> = { nombre, codigoMateria, periodoId, carreraId, docenteIds };
    if (hasSchedule && aulaId && diaSemana && horaInicio && horaFin) {
      body.aulaId = aulaId; body.diaSemana = diaSemana; body.horaInicio = horaInicio; body.horaFin = horaFin;
    }
    if (editing) updateMutation.mutate({ id: editing.id, body });
    else createMutation.mutate(body);
  };

  const addDocente = (uid: string) => {
    if (!docenteIds.includes(uid)) {
      setDocenteIds(prev => [...prev, uid]);
    }
    setDocenteSearch('');
    setShowDocenteDropdown(false);
  };

  const removeDocente = (uid: string) => {
    setDocenteIds(prev => prev.filter(d => d !== uid));
  };

  const availableDocentes = (users as User[]).filter(u => !docenteIds.includes(u.id));
  const filteredDocentes = availableDocentes.filter(u =>
    `${u.firstName} ${u.lastName}`.toLowerCase().includes(docenteSearch.toLowerCase()));

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Actividades</h1>
          <p className="text-primary-500 mt-1">Gestiona las materias/actividades académicas</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={exportToPdf}><FileDown className="h-4 w-4 mr-1.5" /> PDF</Button>
          <Button variant="outline" onClick={exportToExcel}><FileDown className="h-4 w-4 mr-1.5" /> Excel</Button>
          <Button variant="outline" onClick={() => syncMutation.mutate()} loading={syncMutation.isPending}>
            <RefreshCw className="h-4 w-4 mr-2" /> Sincronizar
          </Button>
          <Button variant="accent" onClick={openCreate}><Plus className="h-4 w-4 mr-1.5" /> Nueva Actividad</Button>
        </div>
      </div>

      {syncResult && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-3 flex items-center justify-between">
          <p className="text-sm text-green-800">{syncResult}</p>
          <button className="text-green-600 hover:text-green-800 text-sm font-medium" onClick={() => setSyncResult(null)}>Cerrar</button>
        </div>
      )}

      {isLoading ? <div className="text-center py-16 text-primary-400">Cargando...</div>
      : actividades.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <BookOpen className="h-12 w-12 mx-auto mb-3 opacity-50" />
          <p className="font-medium">No hay actividades registradas</p>
        </div>
      ) : (
        <div className="space-y-3">
          <div className="relative max-w-xs">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
            <input type="text" placeholder="Buscar actividades..." value={searchQuery}
              onChange={e => setSearchQuery(e.target.value)}
              className="w-full pl-9 pr-3 py-2 text-sm border border-primary-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent-300 focus:border-accent-400 bg-white" />
          </div>
          <div className="flex gap-2">
            <select value={filterPeriodoId} onChange={e => setFilterPeriodoId(e.target.value)}
              className="text-sm border border-primary-200 rounded-lg px-3 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-accent-300">
              <option value="">Todos los periodos</option>
              {periodos.map(p => <option key={p.id} value={p.id}>{p.nombre} {p.anio}</option>)}
            </select>
            <select value={filterCarreraId} onChange={e => setFilterCarreraId(e.target.value)}
              className="text-sm border border-primary-200 rounded-lg px-3 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-accent-300">
              <option value="">Todas las carreras</option>
              {carreras.map(c => <option key={c.id} value={c.id}>{c.nombre}</option>)}
            </select>
          </div>
          <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead><button type="button" onClick={() => toggleSort('nombre')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'nombre' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Nombre</button></TableHead>
                <TableHead><button type="button" onClick={() => toggleSort('codigoMateria')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'codigoMateria' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Código</button></TableHead>
                <TableHead><button type="button" onClick={() => toggleSort('periodoNombre')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'periodoNombre' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Periodo</button></TableHead>
                <TableHead><button type="button" onClick={() => toggleSort('carreraNombre')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'carreraNombre' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Carrera</button></TableHead>
                <TableHead><button type="button" onClick={() => toggleSort('diaSemana')} className="inline-flex items-center gap-1 font-medium text-primary-600 hover:text-primary-800">{sortKey === 'diaSemana' ? (sortDir === 'asc' ? <ArrowUp className="h-3 w-3" /> : <ArrowDown className="h-3 w-3" />) : <ArrowUpDown className="h-3 w-3 opacity-50" />}Horario</button></TableHead>
                <TableHead>Docentes</TableHead>
                <TableHead className="w-24 text-right">Acción</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {sorted.length === 0 ? (
                <TableRow><TableCell colSpan={7} className="text-center text-primary-400 py-8">Sin resultados</TableCell></TableRow>
              ) : sorted.map(a => (
                <TableRow key={a.id}>
                  <TableCell className="font-medium">{a.nombre}</TableCell>
                  <TableCell className="text-primary-500">{a.codigoMateria}</TableCell>
                  <TableCell className="text-primary-500">{a.periodoNombre || '—'}</TableCell>
                  <TableCell className="text-primary-500">{a.carreraNombre || '—'}</TableCell>
                  <TableCell className="text-primary-500 text-sm">
                    {a.aulaId ? (
                      <span className="flex items-center gap-1">
                        <Repeat className="h-3 w-3 text-accent-500" />
                        {a.diaSemana ? DAYS.find(d => d.value === a.diaSemana)?.label.slice(0, 3) : ''} {a.horaInicio}-{a.horaFin}
                      </span>
                    ) : <span className="text-primary-300">Sin horario</span>}
                  </TableCell>
                  <TableCell className="text-primary-500 text-xs max-w-[200px] truncate">
                    {a.docentesNombres || '—'}
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-1">
                      <button className="p-1.5 rounded-md text-primary-500 hover:bg-primary-50" onClick={() => openEdit(a)} title="Editar"><Pencil className="h-4 w-4" /></button>
                      <button className="p-1.5 rounded-md text-red-500 hover:bg-red-50" onClick={() => setDeleteConfirm(a.id)} title="Eliminar"><Trash2 className="h-4 w-4" /></button>
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
        <DialogContent className="max-w-xl">
          <form onSubmit={handleSubmit}>
            <DialogHeader>
              <DialogTitle>{editing ? 'Editar Actividad' : 'Nueva Actividad'}</DialogTitle>
            </DialogHeader>
            <DialogBody className="space-y-4">
              <div className="grid grid-cols-2 gap-3">
                <Input label="Nombre" placeholder="Ej: Álgebra I" value={nombre} onChange={e => setNombre(e.target.value)} required />
                <Input label="Código materia" placeholder="Ej: MAT-101" value={codigoMateria} onChange={e => setCodigoMateria(e.target.value)} required />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <Select label="Periodo" placeholder="Seleccionar..." value={periodoId} onChange={e => setPeriodoId(e.target.value)} options={periodos.map(p => ({ value: p.id, label: `${p.nombre} ${p.anio}` }))} required />
                <Select label="Carrera" placeholder="Seleccionar..." value={carreraId} onChange={e => setCarreraId(e.target.value)} options={carreras.map(c => ({ value: c.id, label: c.nombre }))} required />
              </div>

              <div>
                <label className="block text-sm font-medium text-primary-700 mb-2">Docentes a cargo</label>
                <div className="space-y-2">
                  <div className="flex flex-wrap gap-2 min-h-[2rem] p-2 border border-primary-200 rounded-lg bg-white">
                    {docenteIds.length === 0 && <p className="text-xs text-primary-400">Ningún docente asignado</p>}
                    {docenteIds.map(id => {
                      const u = (users as User[]).find(x => x.id === id);
                      const name = u ? `${u.firstName} ${u.lastName}` : (docenteNames.get(id) || id);
                      return (
                        <span key={id} className="inline-flex items-center gap-1 px-2.5 py-1 rounded-lg bg-accent-100 text-accent-800 text-xs font-medium">
                          {name}
                          <button type="button" onClick={() => removeDocente(id)} className="hover:text-red-600 transition-colors">
                            <X className="h-3 w-3" />
                          </button>
                        </span>
                      );
                    })}
                  </div>
                  <div className="relative" ref={docenteSearchRef}>
                    <div className="flex gap-1">
                      <div className="relative flex-1">
                        <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
                        <input type="text" placeholder="Buscar docente..." value={docenteSearch}
                          onFocus={() => setShowDocenteDropdown(true)}
                          onChange={e => { setDocenteSearch(e.target.value); setShowDocenteDropdown(true); }}
                          className="w-full pl-8 pr-3 py-1.5 text-sm border border-primary-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-accent-300 focus:border-accent-400 bg-white" />
                      </div>
                    </div>
                    {showDocenteDropdown && (
                      <>
                        <div className="absolute z-20 mt-1 w-full bg-white border border-primary-200 rounded-lg shadow-lg max-h-48 overflow-y-auto">
                          {filteredDocentes.length === 0
                            ? <p className="text-xs text-primary-400 p-3">Sin resultados</p>
                            : filteredDocentes.map(u => (
                                <button key={u.id} type="button" onClick={() => addDocente(u.id)}
                                  className="w-full text-left px-3 py-2 text-sm text-primary-700 hover:bg-primary-50 transition-colors">
                                  {u.firstName} {u.lastName}
                                </button>
                              ))}
                        </div>
                        <div className="fixed inset-0 z-10" onClick={() => setShowDocenteDropdown(false)} />
                      </>
                    )}
                  </div>
                </div>
              </div>

              <div className="border-t border-primary-100 pt-4">
                <label className="flex items-center gap-2 cursor-pointer mb-3">
                  <input type="checkbox" className="rounded border-primary-300" checked={hasSchedule} onChange={e => setHasSchedule(e.target.checked)} />
                  <span className="text-sm font-medium text-primary-700">Asignar horario (comisión)</span>
                </label>

                {hasSchedule && (
                  <div className="space-y-3 pl-6 border-l-2 border-accent-200">
                    <Select label="Aula" placeholder="Seleccionar..." value={aulaId} onChange={e => setAulaId(e.target.value)} options={classrooms.map(c => ({ value: c.id, label: c.name }))} required />
                    <Select label="Día" placeholder="Seleccionar..." value={diaSemana} onChange={e => setDiaSemana(e.target.value)} options={DAYS.map(d => ({ value: d.value, label: d.label }))} required />
                    <div className="grid grid-cols-2 gap-3">
                      <Input type="time" label="Hora inicio" value={horaInicio} onChange={e => setHoraInicio(e.target.value)} required />
                      <Input type="time" label="Hora fin" value={horaFin} onChange={e => setHoraFin(e.target.value)} required />
                    </div>
                  </div>
                )}
              </div>
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={closeDialog}>Cancelar</Button>
              <Button type="submit" variant="accent" loading={createMutation.isPending || updateMutation.isPending}>
                {editing ? 'Guardar cambios' : 'Crear actividad'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <Dialog open={!!deleteConfirm} onOpenChange={() => setDeleteConfirm(null)}>
        <DialogContent className="max-w-sm">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2 text-red-600">
              <AlertTriangle className="h-5 w-5" /> Confirmar eliminación
            </DialogTitle>
          </DialogHeader>
          <DialogBody>
            <p className="text-sm text-primary-600">¿Estás seguro de eliminar esta actividad? Esta acción no se puede deshacer.</p>
          </DialogBody>
          <DialogFooter>
            <Button variant="outline" onClick={() => setDeleteConfirm(null)}>Cancelar</Button>
            <Button variant="destructive" onClick={() => { if (deleteConfirm) deleteMutation.mutate(deleteConfirm); setDeleteConfirm(null); }}>
              Eliminar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
