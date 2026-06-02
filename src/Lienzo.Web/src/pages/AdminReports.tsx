import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { BarChart3, TrendingUp, FileText, Users, Clock, CalendarDays, MapPin, User, CheckCircle2, XCircle, AlertCircle } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';

interface UsageReportItem {
  group: string;
  totalReservations: number;
  approvedReservations: number;
  cancelledReservations: number;
  noShowReservations: number;
  totalHours: number;
  cancellationRate: number;
  usagePercentage: number;
}

interface UsageReportResponse {
  items: UsageReportItem[];
  grandTotalReservations: number;
  grandTotalHours: number;
  overallCancellationRate: number;
}

interface DemandMetricItem {
  hour: number;
  classroomType: string;
  reservationCount: number;
  occupancyPercentage: number;
}

interface ClassroomDemandSummary {
  classroomType: string;
  totalReservations: number;
  totalHours: number;
  utilizationRate: number;
}

interface DemandMetricsResponse {
  items: DemandMetricItem[];
  peakHours: DemandMetricItem[];
  byClassroomType: ClassroomDemandSummary[];
}

interface UsageByProposalItem {
  group: string;
  totalReservations: number;
  approvedReservations: number;
  cancelledReservations: number;
  totalHours: number;
  cancellationRate: number;
  usagePercentage: number;
}

interface UsageByProposalResponse {
  items: UsageByProposalItem[];
  grandTotalReservations: number;
  grandTotalHours: number;
  overallCancellationRate: number;
}

interface MesCargaHoraria {
  mes: string;
  reservations: number;
  horas: number;
}

interface DocenteCargaHorariaItem {
  docenteId: string;
  docenteNombre: string;
  totalReservations: number;
  totalHoras: number;
  horasPorMes: MesCargaHoraria[];
}

interface DocenteCargaHorariaResponse {
  items: DocenteCargaHorariaItem[];
  totalDocentes: number;
  granTotalHoras: number;
}

interface TimelineReservationItem {
  id: string;
  classroomId: string;
  title: string;
  date: string;
  startTime: string;
  endTime: string;
  status: string;
  userName: string | null;
  actividadNombre: string | null;
}

interface ClassroomTimelineItem {
  classroomId: string;
  classroomName: string;
  reservations: TimelineReservationItem[];
}

interface ClassroomTimelineResponse {
  items: ClassroomTimelineItem[];
  dates: string[];
  totalReservations: number;
}

const STATUS_LABELS: Record<string, string> = {
  Pending: 'Pendiente',
  Approved: 'Aprobada',
  Rejected: 'Rechazada',
  Cancelled: 'Cancelada',
};

const STATUS_COLORS: Record<string, string> = {
  Pending: 'bg-yellow-400',
  Approved: 'bg-green-500',
  Rejected: 'bg-red-300',
  Cancelled: 'bg-gray-300',
  Completed: 'bg-blue-500',
};

function formatDateShort(dateStr: string): string {
  const d = new Date(dateStr + 'T12:00:00');
  return d.toLocaleDateString('es-MX', { weekday: 'short', day: 'numeric' });
}

function formatDateFull(dateStr: string): string {
  const d = new Date(dateStr + 'T12:00:00');
  return d.toLocaleDateString('es-MX', { day: 'numeric', month: 'long', year: 'numeric' });
}

const MES_LABELS: Record<string, string> = {
  '01': 'Ene', '02': 'Feb', '03': 'Mar', '04': 'Abr',
  '05': 'May', '06': 'Jun', '07': 'Jul', '08': 'Ago',
  '09': 'Sep', '10': 'Oct', '11': 'Nov', '12': 'Dic',
};

function formatMes(mes: string): string {
  const [, m] = mes.split('-');
  return MES_LABELS[m] || mes;
}

export default function AdminReports() {
  const [tab, setTab] = useState('usage');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [runUsage, setRunUsage] = useState(false);
  const [runDemand, setRunDemand] = useState(false);
  const [runProposal, setRunProposal] = useState(false);
  const [runCarga, setRunCarga] = useState(false);
  const [runTimeline, setRunTimeline] = useState(false);
  const [groupBy, setGroupBy] = useState('propuesta');

  const { data: usageData, isLoading: usageLoading } = useQuery({
    queryKey: ['usageReport', fromDate, toDate, runUsage],
    queryFn: () => api.post<UsageReportResponse>('/reports/usage', { fromDate: fromDate || null, toDate: toDate || null }),
    enabled: runUsage,
  });

  const { data: demandData, isLoading: demandLoading } = useQuery({
    queryKey: ['demandMetrics', fromDate, toDate, runDemand],
    queryFn: () => {
      const params: Record<string, string> = {};
      if (fromDate) params.fromDate = fromDate;
      if (toDate) params.toDate = toDate;
      return api.get<DemandMetricsResponse>('/reports/demand-metrics', params);
    },
    enabled: runDemand,
  });

  const { data: cargaData, isLoading: cargaLoading } = useQuery({
    queryKey: ['docenteCargaHoraria', fromDate, toDate, runCarga],
    queryFn: () => api.post<DocenteCargaHorariaResponse>('/reports/docente-carga-horaria', {
      fromDate: fromDate || null,
      toDate: toDate || null,
    }),
    enabled: runCarga,
  });

  const { data: timelineData, isLoading: timelineLoading } = useQuery({
    queryKey: ['classroomTimeline', fromDate, toDate, runTimeline],
    queryFn: () => api.post<ClassroomTimelineResponse>('/reports/classroom-timeline', {
      fromDate: fromDate || null,
      toDate: toDate || null,
    }),
    enabled: runTimeline,
  });

  const { data: proposalData, isLoading: proposalLoading } = useQuery({
    queryKey: ['usageByProposal', fromDate, toDate, groupBy, runProposal],
    queryFn: () => api.post<UsageByProposalResponse>('/reports/usage-by-proposal', {
      fromDate: fromDate || null,
      toDate: toDate || null,
      groupBy,
    }),
    enabled: runProposal,
  });

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Reportes</h1>
        <p className="text-primary-500 mt-1">Análisis de uso y demanda del espacio</p>
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="usage"><FileText className="h-4 w-4 mr-1.5" />Reporte de uso</TabsTrigger>
          <TabsTrigger value="demand"><TrendingUp className="h-4 w-4 mr-1.5" />Métricas de demanda</TabsTrigger>
          <TabsTrigger value="proposal"><Users className="h-4 w-4 mr-1.5" />Por propuesta/docente</TabsTrigger>
          <TabsTrigger value="carga"><Clock className="h-4 w-4 mr-1.5" />Carga horaria docente</TabsTrigger>
          <TabsTrigger value="timeline"><CalendarDays className="h-4 w-4 mr-1.5" />Historial de aulas</TabsTrigger>
        </TabsList>

        <TabsContent value="usage">
          <div className="space-y-4">
            <div className="flex flex-wrap gap-3 items-end bg-white p-4 rounded-lg border border-primary-100">
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Desde</label>
                <Input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
              </div>
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Hasta</label>
                <Input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
              </div>
              <Button variant="accent" onClick={() => setRunUsage(true)}>
                <BarChart3 className="h-4 w-4 mr-1.5" />
                Generar reporte
              </Button>
            </div>

            {usageLoading ? (
              <div className="text-center py-16 text-primary-400">Cargando...</div>
            ) : usageData ? (
              <div className="space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Total Reservas</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{usageData.grandTotalReservations}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Horas Totales</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{usageData.grandTotalHours}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Cancelación</p>
                    <p className="text-2xl font-bold text-red-500 mt-1">{usageData.overallCancellationRate}%</p>
                  </div>
                </div>

                <div className="overflow-x-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Grupo</TableHead>
                        <TableHead>Reservas</TableHead>
                        <TableHead>Aprobadas</TableHead>
                        <TableHead>Canceladas</TableHead>
                        <TableHead>Horas</TableHead>
                        <TableHead>% Cancelación</TableHead>
                        <TableHead>% Uso</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {usageData.items.map((item, i) => (
                        <TableRow key={i}>
                          <TableCell className="font-medium">{item.group}</TableCell>
                          <TableCell>{item.totalReservations}</TableCell>
                          <TableCell>{item.approvedReservations}</TableCell>
                          <TableCell>{item.cancelledReservations}</TableCell>
                          <TableCell>{item.totalHours}</TableCell>
                          <TableCell>{item.cancellationRate}%</TableCell>
                          <TableCell>{item.usagePercentage}%</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            ) : (
              <div className="text-center py-16 text-primary-400">
                <BarChart3 className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="font-medium">Selecciona un rango de fechas y genera el reporte</p>
              </div>
            )}
          </div>
        </TabsContent>

        <TabsContent value="demand">
          <div className="space-y-4">
            <div className="flex flex-wrap gap-3 items-end bg-white p-4 rounded-lg border border-primary-100">
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Desde</label>
                <Input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
              </div>
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Hasta</label>
                <Input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
              </div>
              <Button variant="accent" onClick={() => setRunDemand(true)}>
                <TrendingUp className="h-4 w-4 mr-1.5" />
                Analizar demanda
              </Button>
            </div>

            {demandLoading ? (
              <div className="text-center py-16 text-primary-400">Cargando...</div>
            ) : demandData ? (
              <div className="space-y-6">
                <div>
                  <h3 className="font-heading text-lg font-semibold text-primary-700 mb-3">Horas Pico</h3>
                  <div className="overflow-x-auto">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Hora</TableHead>
                          <TableHead>Tipo</TableHead>
                          <TableHead>Reservas</TableHead>
                          <TableHead>% Ocupación</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {demandData.peakHours.map((item, i) => (
                          <TableRow key={i}>
                            <TableCell className="font-medium">{item.hour}:00</TableCell>
                            <TableCell>{item.classroomType}</TableCell>
                            <TableCell>{item.reservationCount}</TableCell>
                            <TableCell>{item.occupancyPercentage}%</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </div>

                <div>
                  <h3 className="font-heading text-lg font-semibold text-primary-700 mb-3">Demanda por tipo de aula</h3>
                  <div className="overflow-x-auto">
                    <Table>
                      <TableHeader>
                        <TableRow>
                          <TableHead>Tipo de Aula</TableHead>
                          <TableHead>Reservas</TableHead>
                          <TableHead>Horas</TableHead>
                          <TableHead>% Utilización</TableHead>
                        </TableRow>
                      </TableHeader>
                      <TableBody>
                        {demandData.byClassroomType.map((item, i) => (
                          <TableRow key={i}>
                            <TableCell className="font-medium">{item.classroomType}</TableCell>
                            <TableCell>{item.totalReservations}</TableCell>
                            <TableCell>{item.totalHours}</TableCell>
                            <TableCell>{item.utilizationRate}%</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </div>
                </div>
              </div>
            ) : (
              <div className="text-center py-16 text-primary-400">
                <TrendingUp className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="font-medium">Selecciona un rango de fechas para ver las métricas</p>
              </div>
            )}
          </div>
        </TabsContent>

        <TabsContent value="proposal">
          <div className="space-y-4">
            <div className="flex flex-wrap gap-3 items-end bg-white p-4 rounded-lg border border-primary-100">
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Desde</label>
                <Input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
              </div>
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Hasta</label>
                <Input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
              </div>
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Agrupar por</label>
                <select
                  className="h-10 rounded-md border border-primary-200 bg-white px-3 text-sm text-primary-700 focus:outline-none focus:ring-2 focus:ring-accent-500"
                  value={groupBy}
                  onChange={e => setGroupBy(e.target.value)}
                >
                  <option value="propuesta">Propuesta</option>
                  <option value="docente">Docente</option>
                </select>
              </div>
              <Button variant="accent" onClick={() => setRunProposal(true)}>
                <BarChart3 className="h-4 w-4 mr-1.5" />
                Generar reporte
              </Button>
            </div>

            {proposalLoading ? (
              <div className="text-center py-16 text-primary-400">Cargando...</div>
            ) : proposalData ? (
              <div className="space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Total Reservas</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{proposalData.grandTotalReservations}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Horas Totales</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{proposalData.grandTotalHours}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Cancelación</p>
                    <p className="text-2xl font-bold text-red-500 mt-1">{proposalData.overallCancellationRate}%</p>
                  </div>
                </div>

                <div className="overflow-x-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>{groupBy === 'docente' ? 'Docente' : 'Propuesta'}</TableHead>
                        <TableHead>Reservas</TableHead>
                        <TableHead>Aprobadas</TableHead>
                        <TableHead>Canceladas</TableHead>
                        <TableHead>Horas</TableHead>
                        <TableHead>% Cancelación</TableHead>
                        <TableHead>% Uso</TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {proposalData.items.map((item, i) => (
                        <TableRow key={i}>
                          <TableCell className="font-medium">{item.group}</TableCell>
                          <TableCell>{item.totalReservations}</TableCell>
                          <TableCell>{item.approvedReservations}</TableCell>
                          <TableCell>{item.cancelledReservations}</TableCell>
                          <TableCell>{item.totalHours}</TableCell>
                          <TableCell>{item.cancellationRate}%</TableCell>
                          <TableCell>{item.usagePercentage}%</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            ) : (
              <div className="text-center py-16 text-primary-400">
                <Users className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="font-medium">Selecciona fechas y genera el reporte</p>
              </div>
            )}
          </div>
        </TabsContent>

        <TabsContent value="carga">
          <div className="space-y-4">
            <div className="flex flex-wrap gap-3 items-end bg-white p-4 rounded-lg border border-primary-100">
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Desde</label>
                <Input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
              </div>
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Hasta</label>
                <Input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
              </div>
              <Button variant="accent" onClick={() => setRunCarga(true)}>
                <Clock className="h-4 w-4 mr-1.5" />
                Calcular carga horaria
              </Button>
            </div>

            {cargaLoading ? (
              <div className="text-center py-16 text-primary-400">Cargando...</div>
            ) : cargaData ? (
              <div className="space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Docentes</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{cargaData.totalDocentes}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Horas Totales</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{cargaData.granTotalHoras}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Promedio x Docente</p>
                    <p className="text-2xl font-bold text-accent-500 mt-1">
                      {cargaData.totalDocentes > 0 ? (cargaData.granTotalHoras / cargaData.totalDocentes).toFixed(1) : '0'}
                    </p>
                  </div>
                </div>

                <div className="overflow-x-auto">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Docente</TableHead>
                        <TableHead>Reservas</TableHead>
                        <TableHead>Total Horas</TableHead>
                        {cargaData.items[0]?.horasPorMes.map(m => (
                          <TableHead key={m.mes}>{formatMes(m.mes)}</TableHead>
                        ))}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {cargaData.items.map((item, i) => (
                        <TableRow key={i}>
                          <TableCell className="font-medium">{item.docenteNombre}</TableCell>
                          <TableCell>{item.totalReservations}</TableCell>
                          <TableCell className="font-semibold text-accent-600">{item.totalHoras}</TableCell>
                          {item.horasPorMes.map(m => (
                            <TableCell key={m.mes}>{m.horas}</TableCell>
                          ))}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              </div>
            ) : (
              <div className="text-center py-16 text-primary-400">
                <Clock className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="font-medium">Selecciona un rango de fechas para calcular la carga horaria</p>
              </div>
            )}
          </div>
        </TabsContent>

        <TabsContent value="timeline">
          <div className="space-y-4">
            <div className="flex flex-wrap gap-3 items-end bg-white p-4 rounded-lg border border-primary-100">
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Desde</label>
                <Input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
              </div>
              <div>
                <label className="block text-xs font-medium text-primary-600 mb-1">Hasta</label>
                <Input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
              </div>
              <Button variant="accent" onClick={() => setRunTimeline(true)}>
                <CalendarDays className="h-4 w-4 mr-1.5" />
                Ver historial
              </Button>
            </div>

            {timelineLoading ? (
              <div className="text-center py-16 text-primary-400">Cargando...</div>
            ) : timelineData ? (
              <div className="space-y-4">
                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Aulas</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{timelineData.items.length}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Reservas</p>
                    <p className="text-2xl font-bold text-primary-800 mt-1">{timelineData.totalReservations}</p>
                  </div>
                  <div className="bg-white rounded-lg border border-primary-100 p-4">
                    <p className="text-xs text-primary-500 font-medium uppercase tracking-wider">Días</p>
                    <p className="text-2xl font-bold text-accent-500 mt-1">{timelineData.dates.length}</p>
                  </div>
                </div>

                <div className="space-y-6">
                  {timelineData.items.map((classroom) => {
                    const resByDate = useMemo(() => {
                      const map: Record<string, TimelineReservationItem[]> = {};
                      for (const r of classroom.reservations) {
                        if (!map[r.date]) map[r.date] = [];
                        map[r.date].push(r);
                      }
                      return map;
                    }, [classroom.reservations]);

                    return (
                      <div key={classroom.classroomId} className="bg-white rounded-lg border border-primary-100 overflow-hidden">
                        <div className="px-4 py-2.5 bg-primary-50/50 border-b border-primary-100 flex items-center gap-2">
                          <MapPin className="h-4 w-4 text-primary-400" />
                          <h3 className="font-semibold text-primary-800">{classroom.classroomName}</h3>
                          <span className="text-xs text-primary-400 ml-auto">{classroom.reservations.length} reservas</span>
                        </div>

                        <div className="overflow-x-auto">
                          <div className="flex min-w-[600px]">
                            {/* Day columns */}
                            {timelineData.dates.map((dateStr) => {
                              const dayRes = resByDate[dateStr] || [];
                              const d = new Date(dateStr + 'T12:00:00');
                              const isWeekend = d.getDay() === 0 || d.getDay() === 6;
                              const isToday = dateStr === new Date().toISOString().split('T')[0];
                              return (
                                <div
                                  key={dateStr}
                                  className={`flex-1 min-w-[80px] border-l border-primary-100 ${
                                    isToday ? 'bg-accent-50/30' : isWeekend ? 'bg-primary-50/30' : ''
                                  }`}
                                >
                                  <div className="text-center py-1.5 border-b border-primary-100 bg-primary-50/30">
                                    <p className="text-[10px] font-medium text-primary-500 uppercase">
                                      {d.toLocaleDateString('es-MX', { weekday: 'short' }).replace('.', '')}
                                    </p>
                                    <p className="text-xs font-semibold text-primary-700">{d.getDate()}</p>
                                  </div>
                                  <div className="p-1 space-y-1 min-h-[60px]">
                                    {dayRes.map((r) => (
                                      <div
                                        key={r.id}
                                        className={`text-[10px] rounded px-1 py-0.5 text-white truncate cursor-default ${STATUS_COLORS[r.status] || 'bg-primary-400'}`}
                                        title={`${r.title}\n${r.startTime}-${r.endTime}\n${r.userName || ''}${r.actividadNombre ? `\n${r.actividadNombre}` : ''}`}
                                      >
                                        {r.startTime}-{r.endTime}
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              );
                            })}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>

                {/* Legend */}
                <div className="flex items-center gap-4 text-xs text-primary-400 flex-wrap">
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-green-500 inline-block" /> Aprobada</span>
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-yellow-400 inline-block" /> Pendiente</span>
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-blue-500 inline-block" /> Completada</span>
                  <span className="flex items-center gap-1"><span className="w-3 h-3 rounded bg-gray-300 inline-block" /> Cancelada</span>
                </div>
              </div>
            ) : (
              <div className="text-center py-16 text-primary-400">
                <CalendarDays className="h-12 w-12 mx-auto mb-3 opacity-50" />
                <p className="font-medium">Selecciona un rango de fechas para ver el historial de aulas</p>
              </div>
            )}
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
