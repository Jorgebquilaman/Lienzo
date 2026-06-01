import { useState, useMemo, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Star, BarChart3, TrendingUp, ThumbsUp, ThumbsDown, Building2, ClipboardCheck, FileDown } from 'lucide-react';
import { api } from '@/lib/api';
import { StarRating } from '@/components/ui/StarRating';
import { Card, CardContent } from '@/components/ui/Card';
import { Badge } from '@/components/ui/Badge';
import { Button } from '@/components/ui/Button';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, PieChart, Pie, Cell, Legend } from 'recharts';
import type { Building } from '@/types';

interface SurveyDto {
  id: string;
  reservationId: string;
  userName: string;
  classroomName: string;
  conditionRating: number;
  equipmentRating: number;
  cleanlinessRating: number;
  overallRating: number;
  comment?: string;
  createdAt: string;
}

interface SurveyListResponse {
  items: SurveyDto[];
  totalCount: number;
}

interface ClassroomRatingSummary {
  classroomId: string;
  classroomName: string;
  averageOverall: number;
  averageCondition: number;
  averageEquipment: number;
  averageCleanliness: number;
  totalSurveys: number;
}

export default function AdminSurveys() {
  const [tab, setTab] = useState('list');
  const [buildingFilter, setBuildingFilter] = useState('');

  const { data: buildings } = useQuery({
    queryKey: ['buildings'],
    queryFn: () => api.get<Building[]>('/buildings'),
  });

  const params = useMemo(() => {
    const p: Record<string, string> = {};
    if (buildingFilter) p.buildingId = buildingFilter;
    return p;
  }, [buildingFilter]);

  const { data: surveysResponse, isLoading: surveysLoading } = useQuery({
    queryKey: ['surveys', buildingFilter],
    queryFn: () => api.get<SurveyListResponse>('/surveys', Object.keys(params).length ? params : undefined),
  });

  const { data: ratings, isLoading: ratingsLoading } = useQuery({
    queryKey: ['surveyRatings', buildingFilter],
    queryFn: () => api.get<ClassroomRatingSummary[]>('/surveys/ratings', Object.keys(params).length ? params : undefined),
  });

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Encuestas</h1>
          <p className="text-primary-500 mt-1">Evaluaciones de aulas realizadas por docentes</p>
        </div>
        <div className="w-56">
          <select
            value={buildingFilter}
            onChange={(e) => setBuildingFilter(e.target.value)}
            className="w-full text-sm border border-primary-200 rounded-lg px-3 py-2 bg-white focus:outline-none focus:ring-2 focus:ring-accent-300"
          >
            <option value="">Todos los edificios</option>
            {buildings?.map((b) => (
              <option key={b.id} value={b.id}>{b.name}</option>
            ))}
          </select>
        </div>
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="list"><Star className="h-4 w-4 mr-1.5" />Respuestas</TabsTrigger>
          <TabsTrigger value="ratings"><BarChart3 className="h-4 w-4 mr-1.5" />Resumen por aula</TabsTrigger>
          <TabsTrigger value="analytics"><TrendingUp className="h-4 w-4 mr-1.5" />Analítica</TabsTrigger>
        </TabsList>

        <TabsContent value="list">
          {surveysLoading ? (
            <div className="text-center py-16 text-primary-400">Cargando...</div>
          ) : !surveysResponse || surveysResponse.items.length === 0 ? (
            <div className="text-center py-16 text-primary-400">
              <Star className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p className="font-medium">No hay encuestas registradas</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Aula</TableHead>
                    <TableHead>Docente</TableHead>
                    <TableHead>Condiciones</TableHead>
                    <TableHead>Equipamiento</TableHead>
                    <TableHead>Limpieza</TableHead>
                    <TableHead>Gral</TableHead>
                    <TableHead>Comentario</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {surveysResponse.items.map((s) => (
                    <TableRow key={s.id}>
                      <TableCell className="font-medium">{s.classroomName}</TableCell>
                      <TableCell className="text-primary-500 text-sm">{s.userName}</TableCell>
                      <TableCell><StarRating value={s.conditionRating} size="sm" readonly /></TableCell>
                      <TableCell><StarRating value={s.equipmentRating} size="sm" readonly /></TableCell>
                      <TableCell><StarRating value={s.cleanlinessRating} size="sm" readonly /></TableCell>
                      <TableCell className="font-semibold">{s.overallRating.toFixed(1)}</TableCell>
                      <TableCell className="text-primary-400 text-sm max-w-[200px] truncate">{s.comment || '—'}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </TabsContent>

        <TabsContent value="ratings">
          {ratingsLoading ? (
            <div className="text-center py-16 text-primary-400">Cargando...</div>
          ) : !ratings || ratings.length === 0 ? (
            <div className="text-center py-16 text-primary-400">
              <BarChart3 className="h-12 w-12 mx-auto mb-3 opacity-50" />
              <p className="font-medium">No hay calificaciones disponibles</p>
            </div>
          ) : (
            <div className="overflow-x-auto">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Aula</TableHead>
                    <TableHead>General</TableHead>
                    <TableHead>Condiciones</TableHead>
                    <TableHead>Equipamiento</TableHead>
                    <TableHead>Limpieza</TableHead>
                    <TableHead>Encuestas</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {ratings.map((r) => (
                    <TableRow key={r.classroomId}>
                      <TableCell className="font-medium">{r.classroomName}</TableCell>
                      <TableCell><StarRating value={r.averageOverall} size="sm" readonly /></TableCell>
                      <TableCell><StarRating value={r.averageCondition} size="sm" readonly /></TableCell>
                      <TableCell><StarRating value={r.averageEquipment} size="sm" readonly /></TableCell>
                      <TableCell><StarRating value={r.averageCleanliness} size="sm" readonly /></TableCell>
                      <TableCell className="text-primary-500">{r.totalSurveys}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </TabsContent>

        <TabsContent value="analytics">
          <SurveyAnalytics surveys={surveysResponse?.items || []} ratings={ratings || []} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

const COLORS = ['#ef4444', '#f97316', '#eab308', '#22c55e', '#22d3ee'];

function SurveyAnalytics({ surveys, ratings }: { surveys: SurveyDto[]; ratings: ClassroomRatingSummary[] }) {
  const stats = useMemo(() => {
    if (surveys.length === 0) return null;

    const total = surveys.length;
    const avgCondition = surveys.reduce((s, r) => s + r.conditionRating, 0) / total;
    const avgEquipment = surveys.reduce((s, r) => s + r.equipmentRating, 0) / total;
    const avgCleanliness = surveys.reduce((s, r) => s + r.cleanlinessRating, 0) / total;
    const avgOverall = surveys.reduce((s, r) => s + r.overallRating, 0) / total;

    const distribution = [1, 2, 3, 4, 5].map((star) => ({
      name: `${star} ★`,
      value: surveys.filter((r) => Math.round(r.overallRating) === star).length,
    }));

    const sortedByRating = [...ratings].sort((a, b) => b.averageOverall - a.averageOverall);
    const top5 = sortedByRating.slice(0, 5);
    const bottom5 = sortedByRating.slice(-5).reverse();

    return { total, avgCondition, avgEquipment, avgCleanliness, avgOverall, distribution, top5, bottom5 };
  }, [surveys, ratings]);

  const exportPdf = useCallback(async () => {
    const [{ default: jsPDF }, { default: autoTable }] = await Promise.all([
      import('jspdf'),
      import('jspdf-autotable'),
    ]);
    const doc = new jsPDF();
    const s = stats!;

    doc.setFontSize(18);
    doc.text('Analítica de Encuestas', 14, 20);
    doc.setFontSize(9);
    doc.text(`Exportado: ${new Date().toLocaleDateString('es-AR')}`, 14, 27);

    doc.setFontSize(12);
    doc.text('Resumen General', 14, 37);
    const kpiBody = [
      ['Total encuestas', s.total.toString()],
      ['Promedio general', s.avgOverall.toFixed(1)],
      ['Condiciones', s.avgCondition.toFixed(1)],
      ['Equipamiento', s.avgEquipment.toFixed(1)],
      ['Limpieza', s.avgCleanliness.toFixed(1)],
    ];
    autoTable(doc, {
      startY: 42,
      head: [['Indicador', 'Valor']],
      body: kpiBody,
      styles: { fontSize: 9 },
      headStyles: { fillColor: [99, 102, 241] },
      columnStyles: { 0: { cellWidth: 60 }, 1: { cellWidth: 30 } },
    });

    const distY = (doc as any).lastAutoTable.finalY + 12;
    doc.setFontSize(12);
    doc.text('Distribución de Calificaciones', 14, distY);
    const distBody = s.distribution.map(d => [d.name, d.value.toString()]);
    autoTable(doc, {
      startY: distY + 5,
      head: [['Calificación', 'Encuestas']],
      body: distBody,
      styles: { fontSize: 9 },
      headStyles: { fillColor: [99, 102, 241] },
      columnStyles: { 0: { cellWidth: 60 }, 1: { cellWidth: 30 } },
    });

    const rankY = (doc as any).lastAutoTable.finalY + 12;
    doc.setFontSize(12);
    doc.text('Mejor Evaluadas', 14, rankY);
    const topBody = s.top5.map((r, i) => [`${i + 1}. ${r.classroomName}`, r.averageOverall.toFixed(1)]);
    autoTable(doc, {
      startY: rankY + 5,
      head: [['Aula', 'Promedio']],
      body: topBody,
      styles: { fontSize: 9 },
      headStyles: { fillColor: [34, 197, 94] },
      columnStyles: { 0: { cellWidth: 80 }, 1: { cellWidth: 30 } },
    });

    const lowY = (doc as any).lastAutoTable.finalY + 8;
    doc.setFontSize(12);
    doc.text('Por Mejorar', 14, lowY);
    const lowBody = s.bottom5.map((r, i) => [`${i + 1}. ${r.classroomName}`, r.averageOverall.toFixed(1)]);
    autoTable(doc, {
      startY: lowY + 5,
      head: [['Aula', 'Promedio']],
      body: lowBody,
      styles: { fontSize: 9 },
      headStyles: { fillColor: [239, 68, 68] },
      columnStyles: { 0: { cellWidth: 80 }, 1: { cellWidth: 30 } },
    });

    doc.save('analitica-encuestas.pdf');
  }, [stats]);

  if (surveys.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-20 text-primary-400">
        <BarChart3 className="h-16 w-16 mb-4 opacity-50" />
        <p className="font-medium">No hay datos suficientes para mostrar analítica</p>
        <p className="text-sm mt-1">Espera a que haya encuestas registradas</p>
      </div>
    );
  }

  return (
    <div className="space-y-6 mt-4">
      <div className="flex items-center justify-between">
        <div />
        <Button variant="outline" size="sm" onClick={exportPdf}>
          <FileDown className="h-4 w-4 mr-1.5" />
          Exportar PDF
        </Button>
      </div>
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-3">
        <SummaryCard icon={<ClipboardCheck className="h-5 w-5" />} label="Total encuestas" value={stats!.total.toString()} color="text-blue-600" bg="bg-blue-50" borderColor="border-blue-600" />
        <SummaryCard icon={<TrendingUp className="h-5 w-5" />} label="Promedio general" value={stats!.avgOverall.toFixed(1)} color="text-emerald-600" bg="bg-emerald-50" borderColor="border-emerald-600" />
        <SummaryCard icon={<Building2 className="h-5 w-5" />} label="Condiciones" value={stats!.avgCondition.toFixed(1)} color="text-amber-600" bg="bg-amber-50" borderColor="border-amber-600" />
        <SummaryCard icon={<Star className="h-5 w-5" />} label="Equipamiento" value={stats!.avgEquipment.toFixed(1)} color="text-purple-600" bg="bg-purple-50" borderColor="border-purple-600" />
        <SummaryCard icon={<Star className="h-5 w-5" />} label="Limpieza" value={stats!.avgCleanliness.toFixed(1)} color="text-rose-600" bg="bg-rose-50" borderColor="border-rose-600" />
      </div>

      <div className="grid lg:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-5">
            <h3 className="font-heading font-semibold text-primary-800 mb-4">Distribución de calificaciones</h3>
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={stats!.distribution}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e4e1d8" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                <Tooltip formatter={(v: number) => [v, 'Encuestas']} />
                <Bar dataKey="value" radius={[6, 6, 0, 0]}>
                  {stats!.distribution.map((_, i) => <Cell key={i} fill={COLORS[i]} />)}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-5">
            <h3 className="font-heading font-semibold text-primary-800 mb-4">Comparativa por categoría</h3>
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={[
                { name: 'Condiciones', value: Math.round(stats!.avgCondition * 10) / 10 },
                { name: 'Equipamiento', value: Math.round(stats!.avgEquipment * 10) / 10 },
                { name: 'Limpieza', value: Math.round(stats!.avgCleanliness * 10) / 10 },
              ]}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e4e1d8" />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis domain={[0, 5]} tick={{ fontSize: 12 }} />
                <Tooltip formatter={(v: number) => [v.toFixed(1), 'Promedio']} />
                <Bar dataKey="value" fill="#f5a623" radius={[6, 6, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>
      </div>

      <div className="grid lg:grid-cols-2 gap-6">
        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-2 mb-4">
              <ThumbsUp className="h-4 w-4 text-emerald-500" />
              <h3 className="font-heading font-semibold text-primary-800">Mejor evaluadas</h3>
            </div>
            {stats!.top5.length === 0 ? (
              <p className="text-sm text-primary-400">Sin datos</p>
            ) : (
              <div className="space-y-2">
                {stats!.top5.map((r, i) => (
                  <div key={r.classroomId} className="flex items-center justify-between p-2 rounded-lg bg-primary-50">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-bold text-primary-400 w-5">{i + 1}.</span>
                      <span className="text-sm font-medium text-primary-700">{r.classroomName}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <StarRating value={r.averageOverall} size="sm" readonly />
                      <span className="text-xs text-primary-400 font-semibold">{r.averageOverall.toFixed(1)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardContent className="p-5">
            <div className="flex items-center gap-2 mb-4">
              <ThumbsDown className="h-4 w-4 text-red-500" />
              <h3 className="font-heading font-semibold text-primary-800">Por mejorar</h3>
            </div>
            {stats!.bottom5.length === 0 ? (
              <p className="text-sm text-primary-400">Sin datos</p>
            ) : (
              <div className="space-y-2">
                {stats!.bottom5.map((r, i) => (
                  <div key={r.classroomId} className="flex items-center justify-between p-2 rounded-lg bg-primary-50">
                    <div className="flex items-center gap-2">
                      <span className="text-xs font-bold text-primary-400 w-5">{i + 1}.</span>
                      <span className="text-sm font-medium text-primary-700">{r.classroomName}</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <StarRating value={r.averageOverall} size="sm" readonly />
                      <span className="text-xs text-primary-400 font-semibold">{r.averageOverall.toFixed(1)}</span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}

function SummaryCard({ icon, label, value, color, bg, borderColor }: { icon: React.ReactNode; label: string; value: string; color: string; bg: string; borderColor: string }) {
  return (
    <div className={`${bg} rounded-xl p-4 border border-primary-100 border-l-4 ${borderColor}`}>
      <div className={`${color} mb-2`}>{icon}</div>
      <p className="text-2xl font-bold text-primary-800">{value}</p>
      <p className="text-xs text-primary-500 mt-0.5">{label}</p>
    </div>
  );
}
