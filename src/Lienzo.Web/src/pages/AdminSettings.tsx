import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardContent } from '@/components/ui/Card';

export default function AdminSettings() {
  const queryClient = useQueryClient();
  const [publicUrl, setPublicUrl] = useState('');

  const { isLoading } = useQuery({
    queryKey: ['settings', 'public-url'],
    queryFn: async () => {
      const data = await api.get<{ url: string }>('/settings/public-url');
      setPublicUrl(data.url);
      return data;
    },
  });

  const saveMutation = useMutation({
    mutationFn: (url: string) => api.put('/settings/public-url', { url }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['settings'] });
      alert('URL pública actualizada correctamente');
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al guardar');
    },
  });

  return (
    <div className="space-y-6">
      <h1 className="font-heading text-2xl font-bold text-primary-800">Configuración</h1>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <div className="max-w-md">
            <Input
              label="URL pública para QR"
              placeholder="https://midominio.com"
              value={publicUrl}
              onChange={(e) => setPublicUrl(e.target.value)}
            />
          </div>
          <p className="text-xs text-primary-400">
            Usada para generar el código QR de asistencia. Si está vacío, se usará la URL del servidor actual.
            Ejemplo: https://tu-dominio.ngrok-free.app
          </p>
          <Button
            variant="accent"
            onClick={() => saveMutation.mutate(publicUrl)}
            disabled={saveMutation.isPending}
          >
            {saveMutation.isPending ? 'Guardando...' : 'Guardar'}
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
