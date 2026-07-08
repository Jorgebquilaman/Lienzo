import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Card, CardContent } from '@/components/ui/Card';

export default function AdminSettings() {
  const queryClient = useQueryClient();

  const [publicUrl, setPublicUrl] = useState('');
  const [smtpHost, setSmtpHost] = useState('');
  const [smtpPort, setSmtpPort] = useState('');
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [fromAddress, setFromAddress] = useState('');
  const [fromName, setFromName] = useState('');
  const [testEmail, setTestEmail] = useState('');
  const [testResult, setTestResult] = useState<{ ok: boolean; message: string } | null>(null);

  useQuery({
    queryKey: ['settings', 'public-url'],
    queryFn: async () => {
      const data = await api.get<{ url: string }>('/settings/public-url');
      setPublicUrl(data.url);
      return data;
    },
  });

  useQuery({
    queryKey: ['settings', 'email'],
    queryFn: async () => {
      const data = await api.get<{
        smtpHost: string;
        smtpPort: string;
        username: string;
        password: string;
        fromAddress: string;
        fromName: string;
      }>('/settings/email');
      setSmtpHost(data.smtpHost);
      setSmtpPort(data.smtpPort);
      setUsername(data.username);
      setPassword(data.password);
      setFromAddress(data.fromAddress);
      setFromName(data.fromName);
      return data;
    },
  });

  const savePublicUrlMutation = useMutation({
    mutationFn: (url: string) => api.put('/settings/public-url', { url }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['settings'] });
      alert('URL pública actualizada correctamente');
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al guardar');
    },
  });

  const saveEmailMutation = useMutation({
    mutationFn: (data: {
      smtpHost: string;
      smtpPort: string;
      username: string;
      password: string;
      fromAddress: string;
      fromName: string;
    }) => api.put('/settings/email', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['settings', 'email'] });
      alert('Configuración de email guardada correctamente');
    },
    onError: (err: any) => {
      alert(err?.message || 'Error al guardar configuración de email');
    },
  });

  const testEmailMutation = useMutation({
    mutationFn: (to: string) => api.post('/settings/email/test', { to }),
    onSuccess: (data: any) => {
      setTestResult({ ok: true, message: data.message || 'Email de prueba enviado' });
    },
    onError: (err: any) => {
      setTestResult({ ok: false, message: err?.message || 'Error al enviar email de prueba' });
    },
  });

  return (
    <div className="space-y-6">
      <h1 className="font-heading text-2xl font-bold text-primary-800">Configuración</h1>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <h2 className="font-heading text-lg font-semibold text-primary-800">URL Pública</h2>
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
            onClick={() => savePublicUrlMutation.mutate(publicUrl)}
            disabled={savePublicUrlMutation.isPending}
          >
            {savePublicUrlMutation.isPending ? 'Guardando...' : 'Guardar'}
          </Button>
        </CardContent>
      </Card>

      <Card>
        <CardContent className="pt-6 space-y-4">
          <h2 className="font-heading text-lg font-semibold text-primary-800">Configuración de Email (Gmail SMTP)</h2>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 max-w-2xl">
            <Input
              label="Servidor SMTP"
              placeholder="smtp.gmail.com"
              value={smtpHost}
              onChange={(e) => setSmtpHost(e.target.value)}
            />
            <Input
              label="Puerto SMTP"
              placeholder="587"
              value={smtpPort}
              onChange={(e) => setSmtpPort(e.target.value)}
            />
            <Input
              label="Usuario (correo Gmail)"
              placeholder="tucorreo@gmail.com"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
            />
            <Input
              label="Contraseña de aplicación"
              type="password"
              placeholder="Contraseña de aplicación"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
            />
            <Input
              label="Dirección From"
              placeholder="tucorreo@gmail.com"
              value={fromAddress}
              onChange={(e) => setFromAddress(e.target.value)}
            />
            <Input
              label="Nombre From"
              placeholder="Lienzo"
              value={fromName}
              onChange={(e) => setFromName(e.target.value)}
            />
          </div>
          <p className="text-xs text-primary-400">
            Usá una contraseña de aplicación de Gmail (no tu contraseña personal). 
            Activá "2FA" en tu cuenta de Google y generá una desde 
            https://myaccount.google.com/apppasswords.
          </p>

          <div className="flex items-center gap-4">
            <Button
              variant="accent"
              onClick={() =>
                saveEmailMutation.mutate({
                  smtpHost,
                  smtpPort,
                  username,
                  password,
                  fromAddress,
                  fromName,
                })
              }
              disabled={saveEmailMutation.isPending}
            >
              {saveEmailMutation.isPending ? 'Guardando...' : 'Guardar configuración'}
            </Button>

            <div className="flex items-center gap-2 ml-4">
              <Input
                placeholder="correo@ejemplo.com"
                value={testEmail}
                onChange={(e) => setTestEmail(e.target.value)}
              />
              <Button
                variant="default"
                onClick={() => testEmailMutation.mutate(testEmail)}
                disabled={testEmailMutation.isPending || !testEmail}
              >
                {testEmailMutation.isPending ? 'Enviando...' : 'Enviar prueba'}
              </Button>
            </div>
          </div>

          {testResult && (
            <p className={`text-sm ${testResult.ok ? 'text-green-600' : 'text-red-600'}`}>
              {testResult.message}
            </p>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
