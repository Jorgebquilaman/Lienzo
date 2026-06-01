import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Megaphone, AlertTriangle, Clock, Info, Send } from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Textarea } from '@/components/ui/Textarea';
import { Select } from '@/components/ui/Select';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';
import { Skeleton } from '@/components/ui/Skeleton';
import { useAuthStore } from '@/stores/authStore';
import { getAnnouncementTypeColor, formatDateTime } from '@/lib/utils';
import type { Announcement } from '@/types';

const announcementSchema = z.object({
  title: z.string().min(3, 'El título debe tener al menos 3 caracteres'),
  body: z.string().min(10, 'El contenido debe tener al menos 10 caracteres'),
  type: z.string().min(1, 'Selecciona un tipo'),
  targetAudience: z.string().optional(),
});

type AnnouncementFormData = z.infer<typeof announcementSchema>;

export default function AnnouncementsPage() {
  const user = useAuthStore((s) => s.user);
  const isTeacher = user?.role === 'Teacher' || user?.role === 'Admin';
  const [activeTab, setActiveTab] = useState(isTeacher ? 'compose' : 'inbox');

  if (isTeacher) {
    return (
      <div className="space-y-6">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Anuncios</h1>
          <p className="text-primary-500 mt-1">Comunícate con los estudiantes</p>
        </div>
        <Tabs value={activeTab} onValueChange={setActiveTab}>
          <TabsList>
            <TabsTrigger value="compose">
              <Send className="h-4 w-4 mr-2" />
              Redactar
            </TabsTrigger>
            <TabsTrigger value="sent">
              <Clock className="h-4 w-4 mr-2" />
              Enviados
            </TabsTrigger>
          </TabsList>
          <TabsContent value="compose">
            <ComposeForm />
          </TabsContent>
          <TabsContent value="sent">
            <SentList />
          </TabsContent>
        </Tabs>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-heading text-2xl font-bold text-primary-800">Anuncios</h1>
        <p className="text-primary-500 mt-1">Mantente al tanto de las novedades</p>
      </div>
      <InboxList />
    </div>
  );
}

function ComposeForm() {
  const queryClient = useQueryClient();
  const form = useForm<AnnouncementFormData>({
    resolver: zodResolver(announcementSchema),
    defaultValues: {
      title: '',
      body: '',
      type: 'General',
      targetAudience: '',
    },
  });

  const createMutation = useMutation({
    mutationFn: (data: AnnouncementFormData) => api.post('/announcements', {
      ...data,
      targetAudience: data.targetAudience || 'All',
    }),
    onSuccess: () => {
      form.reset();
      queryClient.invalidateQueries({ queryKey: ['announcements'] });
    },
  });

  return (
    <Card>
      <CardContent className="p-6">
        <form onSubmit={form.handleSubmit((data) => createMutation.mutate(data))} className="space-y-4">
          <Input label="Título" placeholder="Título del anuncio" error={form.formState.errors.title?.message} {...form.register('title')} />
          <Textarea label="Contenido" placeholder="Escribe tu anuncio aquí..." rows={6} error={form.formState.errors.body?.message} {...form.register('body')} />
          <div className="grid sm:grid-cols-2 gap-4">
            <Select label="Tipo" error={form.formState.errors.type?.message} value={form.watch('type')} onValueChange={(v) => form.setValue('type', v)} options={[
              { value: 'General', label: 'General' },
              { value: 'Cancellation', label: 'Cancelación' },
              { value: 'Postponement', label: 'Postergación' },
              { value: 'Emergency', label: 'Emergencia' },
            ]} />
            <Select label="Audiencia (opcional)" placeholder="Todos" value={form.watch('targetAudience')} onValueChange={(v) => form.setValue('targetAudience', v)} options={[
              { value: 'AllStudents', label: 'Estudiantes' },
              { value: 'AllTeachers', label: 'Profesores' },
            ]} />
          </div>
          {createMutation.isSuccess && (
            <div className="bg-green-50 border border-green-200 rounded-lg p-3 text-sm text-green-700">
              Anuncio publicado exitosamente
            </div>
          )}
          {createMutation.isError && (
            <div className="bg-red-50 border border-red-200 rounded-lg p-3 text-sm text-red-700">
              {createMutation.error instanceof Error ? createMutation.error.message : 'Error al publicar el anuncio'}
            </div>
          )}
          <Button type="submit" variant="accent" loading={createMutation.isPending}>
            <Megaphone className="h-4 w-4 mr-2" />
            Publicar Anuncio
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}

function SentList() {
  const { data, isLoading } = useQuery({
    queryKey: ['announcements', 'sent'],
    queryFn: () => api.get<Announcement[]>('/announcements/my'),
  });

  if (isLoading) {
    return <div className="space-y-3">{[1, 2, 3].map((i) => <Skeleton key={i} className="h-20 w-full" />)}</div>;
  }

  if (!data || data.length === 0) {
    return (
      <div className="text-center py-16 text-primary-400">
        <Megaphone className="h-12 w-12 mx-auto mb-3 text-primary-200" />
        <p className="font-medium">No has enviado anuncios</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {data.map((a) => (
        <Card key={a.id}>
          <CardContent className="p-4">
            <div className="flex items-start gap-3">
              <div className={`p-2 rounded-lg ${getAnnouncementTypeColor(a.type)}`}>
                {a.type === 'Cancellation' ? <AlertTriangle className="h-4 w-4" /> :
                 a.type === 'Postponement' ? <Clock className="h-4 w-4" /> :
                 <Info className="h-4 w-4" />}
              </div>
              <div className="flex-1 min-w-0">
                <h3 className="font-medium text-primary-800">{a.title}</h3>
                <p className="text-sm text-primary-500 mt-0.5 line-clamp-2">{a.body}</p>
                <p className="text-xs text-primary-400 mt-1">{formatDateTime(a.createdAt)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}

function InboxList() {
  const { data, isLoading } = useQuery({
    queryKey: ['announcements', 'inbox'],
    queryFn: () => api.get<Announcement[]>('/announcements'),
  });

  const markAsReadMutation = useMutation({
    mutationFn: (id: string) => api.patch(`/announcements/${id}/read`),
  });

  if (isLoading) {
    return <div className="space-y-3">{[1, 2, 3].map((i) => <Skeleton key={i} className="h-20 w-full" />)}</div>;
  }

  if (!data || data.length === 0) {
    return (
      <div className="text-center py-16 text-primary-400">
        <Megaphone className="h-12 w-12 mx-auto mb-3 text-primary-200" />
        <p className="font-medium">No hay anuncios</p>
        <p className="text-sm mt-1">Los anuncios de tus profesores aparecerán aquí</p>
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {data.map((a) => (
        <Card
          key={a.id}
          className={`cursor-pointer transition-colors ${!a.isRead ? 'border-accent-200 bg-accent-50/20' : ''}`}
          onClick={() => !a.isRead && markAsReadMutation.mutate(a.id)}
        >
          <CardContent className="p-4">
            <div className="flex items-start gap-3">
              <div className={`p-2 rounded-lg ${getAnnouncementTypeColor(a.type)}`}>
                {a.type === 'Cancellation' ? <AlertTriangle className="h-4 w-4" /> :
                 a.type === 'Postponement' ? <Clock className="h-4 w-4" /> :
                 <Info className="h-4 w-4" />}
              </div>
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2">
                  <h3 className={`text-sm ${!a.isRead ? 'font-semibold text-primary-800' : 'font-medium text-primary-600'}`}>
                    {a.title}
                  </h3>
                  {!a.isRead && <span className="h-2 w-2 rounded-full bg-accent-500 flex-shrink-0" />}
                </div>
                <p className="text-sm text-primary-500 mt-0.5 line-clamp-2">{a.body}</p>
                <div className="flex items-center gap-2 mt-1">
                  <span className="text-xs text-primary-400">{a.userName}</span>
                  <span className="text-xs text-primary-300">·</span>
                  <span className="text-xs text-primary-400">{formatDateTime(a.createdAt)}</span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      ))}
    </div>
  );
}
