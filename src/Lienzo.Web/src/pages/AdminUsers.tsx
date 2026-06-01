import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Plus, Pencil, Shield, ShieldOff, RefreshCw } from 'lucide-react';
import { api } from '@/lib/api';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Table, TableHeader, TableBody, TableHead, TableRow, TableCell } from '@/components/ui/Table';
import { TableSkeleton } from '@/components/ui/Skeleton';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { Badge } from '@/components/ui/Badge';

interface AdminUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  avatarUrl: string | null;
  isActive: boolean;
  createdAt: string;
}

const roleBadge: Record<string, 'default' | 'approved' | 'outline'> = {
  Admin: 'outline',
  Teacher: 'approved',
  Student: 'default',
};

const roleLabel: Record<string, string> = {
  Admin: 'Administrador',
  Teacher: 'Profesor',
  Student: 'Estudiante',
};

const userSchema = z.object({
  firstName: z.string().min(2, 'El nombre debe tener al menos 2 caracteres'),
  lastName: z.string().min(2, 'El apellido debe tener al menos 2 caracteres'),
  email: z.string().email('Correo electrónico inválido'),
  password: z.string().min(6, 'La contraseña debe tener al menos 6 caracteres').optional().or(z.literal('')),
  role: z.string().min(1, 'Selecciona un rol'),
});

type UserFormData = z.infer<typeof userSchema>;

export default function AdminUsers() {
  const [createOpen, setCreateOpen] = useState(false);
  const [editing, setEditing] = useState<AdminUser | null>(null);
  const [syncResult, setSyncResult] = useState<string | null>(null);
  const queryClient = useQueryClient();

  const form = useForm<UserFormData>({
    resolver: zodResolver(userSchema),
    defaultValues: { firstName: '', lastName: '', email: '', password: '', role: 'Student' },
  });

  const { data, isLoading } = useQuery({
    queryKey: ['adminUsers'],
    queryFn: () => api.get<AdminUser[]>('/users'),
  });

  const toggleStatusMutation = useMutation({
    mutationFn: (userId: string) => api.patch(`/users/${userId}/toggle-status`),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['adminUsers'] }),
  });

  const createMutation = useMutation({
    mutationFn: (data: UserFormData) => api.post('/users', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminUsers'] });
      setCreateOpen(false);
      form.reset();
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: UserFormData }) =>
      api.put(`/users/${id}`, { email: data.email, firstName: data.firstName, lastName: data.lastName, role: data.role }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adminUsers'] });
      setEditing(null);
    },
  });

  const syncDocentesMutation = useMutation({
    mutationFn: () => api.post<{ creados: number; existentes: number; errores: number }>('/actividades/sync-docentes', { anioAcademico: 2026 }),
    onSuccess: (data) => {
      setSyncResult(`Docentes sincronizados: ${data.creados} creados, ${data.existentes} existentes${data.errores > 0 ? `, ${data.errores} errores` : ''}`);
      queryClient.invalidateQueries({ queryKey: ['adminUsers'] });
    },
    onError: () => setSyncResult('Error al sincronizar docentes'),
  });

  const openCreate = () => {
    setEditing(null);
    form.reset({ firstName: '', lastName: '', email: '', password: '', role: 'Student' });
    setCreateOpen(true);
  };

  const openEdit = (user: AdminUser) => {
    setEditing(user);
    form.reset({ firstName: user.firstName, lastName: user.lastName, email: user.email, password: '', role: user.role });
    setCreateOpen(true);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Administrar Usuarios</h1>
          <p className="text-primary-500 mt-1">Gestiona los usuarios del sistema</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => syncDocentesMutation.mutate()} loading={syncDocentesMutation.isPending}>
            <RefreshCw className="h-4 w-4 mr-2" /> Sincronizar Docentes
          </Button>
          <Button onClick={openCreate}>
            <Plus className="h-4 w-4 mr-2" />
            Nuevo Usuario
          </Button>
        </div>
      </div>

      {syncResult && (
        <div className={`px-4 py-3 rounded-lg text-sm ${
          syncResult.startsWith('Error') ? 'bg-red-50 text-red-700' : 'bg-green-50 text-green-700'
        }`}>
          {syncResult}
          <button className="ml-2 font-bold" onClick={() => setSyncResult(null)}>×</button>
        </div>
      )}

      {isLoading ? (
        <TableSkeleton rows={8} />
      ) : !data || data.length === 0 ? (
        <div className="text-center py-16 text-primary-400">
          <p className="font-medium">No hay usuarios registrados</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Nombre</TableHead>
                <TableHead>Correo</TableHead>
                <TableHead>Rol</TableHead>
                <TableHead className="hidden sm:table-cell">Registro</TableHead>
                <TableHead>Estado</TableHead>
                <TableHead className="text-right">Acciones</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.map((u) => (
                <TableRow key={u.id}>
                  <TableCell className="font-medium">{u.firstName} {u.lastName}</TableCell>
                  <TableCell className="text-primary-500 text-sm">{u.email}</TableCell>
                  <TableCell>
                    <Badge variant={roleBadge[u.role] || 'default'}>
                      {roleLabel[u.role] || u.role}
                    </Badge>
                  </TableCell>
                  <TableCell className="hidden sm:table-cell text-primary-400 text-sm">
                    {new Date(u.createdAt).toLocaleDateString()}
                  </TableCell>
                  <TableCell>
                    <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium ${
                      u.isActive ? 'bg-green-50 text-green-700' : 'bg-red-50 text-red-700'
                    }`}>
                      {u.isActive ? 'Activo' : 'Inactivo'}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="flex justify-end gap-1">
                      <button
                        className="p-1.5 rounded-md text-primary-500 hover:bg-primary-50"
                        onClick={() => openEdit(u)}
                        title="Editar"
                      >
                        <Pencil className="h-4 w-4" />
                      </button>
                      <button
                        className={`p-1.5 rounded-md ${u.isActive ? 'text-red-500 hover:bg-red-50' : 'text-green-500 hover:bg-green-50'}`}
                        onClick={() => toggleStatusMutation.mutate(u.id)}
                        title={u.isActive ? 'Desactivar' : 'Activar'}
                      >
                        {u.isActive ? <ShieldOff className="h-4 w-4" /> : <Shield className="h-4 w-4" />}
                      </button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <Dialog open={createOpen} onOpenChange={(open) => { if (!open) { setCreateOpen(false); setEditing(null); } }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? 'Editar Usuario' : 'Nuevo Usuario'}</DialogTitle>
            <DialogDescription>{editing ? 'Modifica los datos del usuario' : 'Ingresa los datos del nuevo usuario'}</DialogDescription>
          </DialogHeader>
          <form onSubmit={form.handleSubmit((data) => {
            if (editing) {
              updateMutation.mutate({ id: editing.id, data });
            } else {
              createMutation.mutate(data);
            }
          })}>
            <DialogBody className="space-y-4">
              <Input label="Nombre" error={form.formState.errors.firstName?.message} {...form.register('firstName')} />
              <Input label="Apellido" error={form.formState.errors.lastName?.message} {...form.register('lastName')} />
              <Input label="Correo electrónico" type="email" error={form.formState.errors.email?.message} {...form.register('email')} />
              {!editing && (
                <Input label="Contraseña" type="password" error={form.formState.errors.password?.message} {...form.register('password')} />
              )}
              <div>
                <label className="block text-sm font-medium text-primary-700 mb-1.5">Rol</label>
                <select
                  className="flex h-10 w-full rounded-lg border border-primary-200 bg-white px-3 py-2 text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
                  {...form.register('role')}
                >
                  <option value="Student">Estudiante</option>
                  <option value="Teacher">Profesor</option>
                  <option value="Admin">Administrador</option>
                </select>
                {form.formState.errors.role && (
                  <p className="mt-1 text-xs text-red-600">{form.formState.errors.role.message}</p>
                )}
              </div>
            </DialogBody>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => { setCreateOpen(false); setEditing(null); }}>Cancelar</Button>
              <Button type="submit" loading={createMutation.isPending || updateMutation.isPending}>
                {editing ? 'Guardar cambios' : 'Crear Usuario'}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
