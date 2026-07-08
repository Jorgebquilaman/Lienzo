import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Star, ClipboardList, Clock, ArrowLeft } from 'lucide-react';
import { api } from '@/lib/api';
import { StarRating } from '@/components/ui/StarRating';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Skeleton } from '@/components/ui/Skeleton';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';
import { SurveyModal } from '@/components/surveys/SurveyModal';

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

interface PendingSurvey {
  id: string;
  classroomName: string;
  title: string;
  date: string;
  startTime: string;
  endTime: string;
}

export default function MySurveys() {
  const navigate = useNavigate();
  const [tab, setTab] = useState('pending');
  const [surveyTarget, setSurveyTarget] = useState<PendingSurvey | null>(null);

  const { data: completedResponse, isLoading: completedLoading } = useQuery({
    queryKey: ['mySurveys'],
    queryFn: () => api.get<SurveyListResponse>('/surveys/my'),
  });

  const { data: pendingData, isLoading: pendingLoading } = useQuery({
    queryKey: ['pendingSurveys'],
    queryFn: () => api.get<PendingSurvey[]>('/surveys/my/pending'),
  });

  const surveys = completedResponse?.items ?? [];
  const pending = pendingData ?? [];

  return (
    <div className="space-y-6">
      <button onClick={() => navigate(-1)} className="flex items-center gap-1.5 text-sm text-primary-500 hover:text-primary-700 transition-colors sm:hidden mb-2">
        <ArrowLeft className="h-4 w-4" /> Volver
      </button>
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Mis Encuestas</h1>
        <p className="text-primary-500 mt-1">Evaluaciones de aulas que has realizado</p>
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="pending">
            Pendientes
            {pending.length > 0 && (
              <span className="ml-1.5 bg-yellow-400 text-white text-xs rounded-full w-5 h-5 inline-flex items-center justify-center">
                {pending.length}
              </span>
            )}
          </TabsTrigger>
          <TabsTrigger value="completed">Realizadas</TabsTrigger>
        </TabsList>

        <TabsContent value="pending">
          {pendingLoading ? (
            <div className="space-y-3">
              {[1, 2].map((i) => (
                <Skeleton key={i} variant="rectangular" className="h-24 w-full" />
              ))}
            </div>
          ) : pending.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16">
              <ClipboardList className="h-16 w-16 text-primary-200 mb-4" />
              <h3 className="font-heading text-lg font-semibold text-primary-600">
                Sin pendientes
              </h3>
              <p className="text-primary-400 text-sm mt-1">
                No tienes aulas pendientes de evaluar.
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {pending.map((r) => (
                <Card key={r.id} className="hover:shadow-sm transition-shadow border-l-4 border-l-yellow-400">
                  <CardContent className="p-4">
                    <div className="flex items-start justify-between">
                      <div>
                        <h3 className="font-medium text-primary-800">{r.title || r.classroomName}</h3>
                        <p className="text-sm text-primary-500">{r.classroomName}</p>
                        <p className="text-xs text-primary-400 mt-0.5">
                          {r.date.split('T')[0]?.split('-').reverse().join('/') ?? r.date} · {r.startTime} - {r.endTime}
                        </p>
                      </div>
                      <Button size="sm" onClick={() => setSurveyTarget(r)}>
                        <Star className="h-4 w-4 mr-1.5" />
                        Calificar
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>

        <TabsContent value="completed">
          {completedLoading ? (
            <div className="space-y-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} variant="rectangular" className="h-28 w-full" />
              ))}
            </div>
          ) : surveys.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-16">
              <ClipboardList className="h-16 w-16 text-primary-200 mb-4" />
              <h3 className="font-heading text-lg font-semibold text-primary-600">
                Sin encuestas
              </h3>
              <p className="text-primary-400 text-sm mt-1">
                Aún no has evaluado ninguna aula.
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {surveys.map((survey) => (
                <Card key={survey.id} className="hover:shadow-sm transition-shadow">
                  <CardContent className="p-4">
                    <div className="flex items-start justify-between">
                      <div className="flex-1 min-w-0">
                        <h3 className="font-medium text-primary-800">{survey.classroomName}</h3>
                        <p className="text-xs text-primary-400 mt-0.5">
                          {new Date(survey.createdAt).toLocaleDateString('es-MX', {
                            year: 'numeric', month: 'long', day: 'numeric',
                          })}
                        </p>
                        <div className="mt-3 space-y-1.5">
                          <div className="flex items-center gap-4">
                            <span className="text-xs text-primary-500 w-24">Condiciones</span>
                            <StarRating value={survey.conditionRating} size="sm" readonly />
                          </div>
                          <div className="flex items-center gap-4">
                            <span className="text-xs text-primary-500 w-24">Equipamiento</span>
                            <StarRating value={survey.equipmentRating} size="sm" readonly />
                          </div>
                          <div className="flex items-center gap-4">
                            <span className="text-xs text-primary-500 w-24">Limpieza</span>
                            <StarRating value={survey.cleanlinessRating} size="sm" readonly />
                          </div>
                          <div className="flex items-center gap-4">
                            <span className="text-xs text-primary-500 w-24">General</span>
                            <span className="text-sm font-semibold text-primary-700">{survey.overallRating.toFixed(1)}</span>
                          </div>
                        </div>
                        {survey.comment && (
                          <p className="text-sm text-primary-500 mt-2 italic">"{survey.comment}"</p>
                        )}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>

      <SurveyModal
        open={!!surveyTarget}
        onOpenChange={(open) => { if (!open) setSurveyTarget(null); }}
        reservationId={surveyTarget?.id || ''}
        classroomName={surveyTarget?.classroomName || ''}
      />
    </div>
  );
}
