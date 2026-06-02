import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { BarChart3, TrendingUp, FileText, Users, Clock } from 'lucide-react';
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
      </Tabs>
    </div>
  );
}
