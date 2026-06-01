import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { CalendarCheck, XCircle, CheckCircle, Pencil, Repeat, Star, ChevronLeft, ChevronRight } from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';
import { Skeleton } from '@/components/ui/Skeleton';
import { ReservationModal } from '@/components/classrooms/ReservationModal';
import { SurveyModal } from '@/components/surveys/SurveyModal';
import { getStatusLabel, formatDateTime } from '@/lib/utils';
import { useAuthStore } from '@/stores/authStore';
import type { Reservation, PaginatedResponse, UserRole } from '@/types';

const tabs = [
  { value: 'upcoming', label: 'Próximas' },
  { value: 'pending', label: 'Pendientes' },
  { value: 'past', label: 'Pasadas' },
  { value: 'cancelled', label: 'Canceladas' },
];

export default function MyReservations() {
  const [activeTab, setActiveTab] = useState('upcoming');
  const [editingReservation, setEditingReservation] = useState<Reservation | null>(null);
  const [surveyReservation, setSurveyReservation] = useState<Reservation | null>(null);
  const queryClient = useQueryClient();
  const user = useAuthStore((s) => s.user);
  const isAdmin = user?.role === 'Admin';

  const [page, setPage] = useState(1);
  const pageSize = 10;

  const statusMap: Record<string, string> = {
    pending: 'Pending',
    cancelled: 'Cancelled',
  };

  const { data: response, isLoading } = useQuery({
    queryKey: ['myReservations', activeTab, page],
    queryFn: () => {
      const params = `page=${page}&pageSize=${pageSize}`;
      if (activeTab === 'upcoming' || activeTab === 'past')
        return api.get<PaginatedResponse<Reservation>>(`/reservations?filter=${activeTab}&${params}`);
      return api.get<PaginatedResponse<Reservation>>(`/reservations?status=${statusMap[activeTab] || ''}&${params}`);
    },
  });
  const data = response?.value;
  const totalPages = response?.totalPages ?? 1;

  const { data: mySurveys } = useQuery({
    queryKey: ['mySurveys'],
    queryFn: () => api.get<{ items: { reservationId: string }[] }>('/surveys/my'),
    enabled: activeTab === 'past',
  });
  const ratedReservations = new Set(mySurveys?.items?.map((s) => s.reservationId) ?? []);

  const cancelMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/reservations/${id}/cancel`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['myReservations'] });
    },
  });

  const approveMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/reservations/${id}/approve`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['myReservations'] });
    },
  });

  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    setPage(1);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Mis Reservaciones</h1>
        <p className="text-primary-500 mt-1">Gestiona tus reservaciones de aulas</p>
      </div>

      <Tabs value={activeTab} onValueChange={handleTabChange}>
        <TabsList className="w-full sm:w-auto overflow-x-auto">
          {tabs.map((tab) => (
            <TabsTrigger key={tab.value} value={tab.value}>
              {tab.label}
            </TabsTrigger>
          ))}
        </TabsList>

        {tabs.map((tab) => (
          <TabsContent key={tab.value} value={tab.value}>
            {isLoading ? (
              <div className="space-y-3">
                {[1, 2, 3].map((i) => (
                  <Skeleton key={i} variant="rectangular" className="h-24 w-full" />
                ))}
              </div>
            ) : !data || data.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-16">
                <CalendarCheck className="h-16 w-16 text-primary-200 mb-4" />
                <h3 className="font-heading text-lg font-semibold text-primary-600">
                  Sin reservaciones {tab.label.toLowerCase()}
                </h3>
                <p className="text-primary-400 text-sm mt-1">
                  {tab.value === 'upcoming' || tab.value === 'pending'
                    ? 'Reserva un aula para comenzar'
                    : 'No hay reservaciones en esta categoría'}
                </p>
              </div>
            ) : (
              <div className="space-y-3">
                {data.map((reservation) => (
                  <Card key={reservation.id} className="hover:shadow-sm transition-shadow">
                    <CardContent className="p-4">
                      <div className="flex items-start justify-between">
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <h3 className="font-medium text-primary-800 truncate">
                              {reservation.title}
                            </h3>
                            {reservation.recurringGroupId && (
                              <span className="flex-shrink-0 text-accent-500" title="Reservación periódica">
                                <Repeat className="h-3.5 w-3.5" />
                              </span>
                            )}
                            <Badge
                              variant={
                                reservation.status === 'Approved'
                                  ? 'approved'
                                  : reservation.status === 'Pending'
                                  ? 'pending'
                                  : reservation.status === 'Rejected'
                                  ? 'rejected'
                                  : reservation.status === 'Cancelled'
                                  ? 'cancelled'
                                  : 'completed'
                              }
                              className="flex-shrink-0"
                            >
                              {getStatusLabel(reservation.status)}
                            </Badge>
                          </div>
                          <p className="text-sm text-primary-500">
                            {reservation.classroomName}
                          </p>
                          <p className="text-xs text-primary-400 mt-0.5">
                            {reservation.date.split('T')[0].split('-').reverse().join('/')} · {reservation.startTime} - {reservation.endTime}
                          </p>
                          {reservation.description && (
                            <p className="text-xs text-primary-400 mt-1 line-clamp-2">
                              {reservation.description}
                            </p>
                          )}
                        </div>
                        <div className="flex items-center gap-1 flex-shrink-0 ml-2">
                          {isAdmin && reservation.status === 'Pending' && (
                            <button
                              className="p-1.5 rounded-md text-green-600 hover:bg-green-50"
                              onClick={() => approveMutation.mutate(reservation.id)}
                              title="Aprobar"
                            >
                              <CheckCircle className="h-4 w-4" />
                            </button>
                          )}
                          {activeTab === 'past' && reservation.status === 'Approved' && !ratedReservations.has(reservation.id) && (
                            <button
                              className="p-1.5 rounded-md text-yellow-600 hover:bg-yellow-50"
                              onClick={() => setSurveyReservation(reservation)}
                              title="Calificar aula"
                            >
                              <Star className="h-4 w-4" />
                            </button>
                          )}
                          {(reservation.status === 'Pending' || reservation.status === 'Approved') && (
                            <>
                              <button
                                className="p-1.5 rounded-md text-primary-500 hover:bg-primary-50"
                                onClick={() => setEditingReservation(reservation)}
                                title="Editar"
                              >
                                <Pencil className="h-4 w-4" />
                              </button>
                              <Button
                                variant="ghost"
                                size="sm"
                                className="text-red-500 hover:text-red-700 hover:bg-red-50"
                                onClick={() => cancelMutation.mutate(reservation.id)}
                                loading={cancelMutation.isPending}
                              >
                                <XCircle className="h-4 w-4" />
                              </Button>
                            </>
                          )}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
                {totalPages > 1 && (
                  <div className="flex items-center justify-center gap-2 pt-4">
                    <button
                      className="p-2 rounded-lg text-primary-500 hover:bg-primary-50 disabled:opacity-30 disabled:cursor-not-allowed"
                      disabled={page <= 1}
                      onClick={() => setPage((p) => Math.max(1, p - 1))}
                    >
                      <ChevronLeft className="h-5 w-5" />
                    </button>
                    {Array.from({ length: totalPages }, (_, i) => i + 1).map((p) => (
                      <button
                        key={p}
                        className={`w-9 h-9 rounded-lg text-sm font-medium transition-colors ${
                          p === page
                            ? 'bg-primary-600 text-white'
                            : 'text-primary-600 hover:bg-primary-50'
                        }`}
                        onClick={() => setPage(p)}
                      >
                        {p}
                      </button>
                    ))}
                    <button
                      className="p-2 rounded-lg text-primary-500 hover:bg-primary-50 disabled:opacity-30 disabled:cursor-not-allowed"
                      disabled={page >= totalPages}
                      onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
                    >
                      <ChevronRight className="h-5 w-5" />
                    </button>
                  </div>
                )}
              </div>
            )}
          </TabsContent>
        ))}
      </Tabs>

      <ReservationModal
        open={!!editingReservation}
        onOpenChange={(open) => { if (!open) setEditingReservation(null); }}
        reservation={editingReservation ?? undefined}
        onSuccess={() => {
          setEditingReservation(null);
          queryClient.invalidateQueries({ queryKey: ['myReservations'] });
        }}
      />

      <SurveyModal
        open={!!surveyReservation}
        onOpenChange={(open) => { if (!open) setSurveyReservation(null); }}
        reservationId={surveyReservation?.id || ''}
        classroomName={surveyReservation?.classroomName || ''}
      />
    </div>
  );
}
