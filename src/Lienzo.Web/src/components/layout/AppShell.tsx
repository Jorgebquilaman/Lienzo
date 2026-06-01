import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import {
  LayoutDashboard,
  DoorOpen,
  CalendarCheck,
  CalendarDays,
  Bell,
  User,
  LogOut,
  Menu,
  X,
  Building2,
  Users,
  Settings,
  Megaphone,
  Palette,
  CalendarX2,
  BookOpen,
  GraduationCap,
  Calendar,
  ChevronDown,
  ChevronRight,
  PanelLeftClose,
  PanelLeft,
  BarChart3,
  Wrench,
  Star,
} from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/Button';
import { Avatar } from '@/components/ui/Avatar';
import { Badge } from '@/components/ui/Badge';
import { useAuthStore } from '@/stores/authStore';
import { useNotificationStore } from '@/stores/notificationStore';
import { useLogout } from '@/hooks/useAuth';
import { NotificationPanel } from '@/components/notifications/NotificationPanel';

const navItems = [
  { path: '/', label: 'Dashboard', icon: LayoutDashboard },
  { path: '/classrooms', label: 'Aulas', icon: DoorOpen },
  { path: '/reservations', label: 'Reservaciones', icon: CalendarCheck },
  { path: '/schedule', label: 'Horario', icon: CalendarDays },
  { path: '/announcements', label: 'Anuncios', icon: Megaphone },
  { path: '/surveys', label: 'Mis Encuestas', icon: Star },
];

const adminNavItems = [
  { path: '/admin/reservations', label: 'Reservaciones', icon: CalendarCheck },
  { path: '/admin/classrooms', label: 'Aulas', icon: DoorOpen },
  { path: '/admin/buildings', label: 'Edificios', icon: Building2 },
  { path: '/admin/reports', label: 'Reportes', icon: BarChart3 },
  { path: '/admin/maintenance', label: 'Mantenimiento', icon: Wrench },
  { path: '/admin/surveys', label: 'Encuestas', icon: Star },
  { path: '/admin/holidays', label: 'Feriados', icon: CalendarX2 },
];

export function AppShell({ children }: { children: React.ReactNode }) {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [showUserMenu, setShowUserMenu] = useState(false);
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);
  const location = useLocation();
  const navigate = useNavigate();
  const user = useAuthStore((s) => s.user);
  const unreadCount = useNotificationStore((s) => s.unreadCount);
  const togglePanel = useNotificationStore((s) => s.togglePanel);
  const { mutate: doLogout } = useLogout();

  const isActive = (path: string) => {
    if (path === '/') return location.pathname === '/';
    return location.pathname.startsWith(path);
  };

  return (
    <div className="min-h-screen bg-canvas">
      <header className="fixed top-0 left-0 right-0 z-40 h-16 bg-white border-b border-primary-100 shadow-sm">
        <div className="flex items-center justify-between h-full px-4 max-w-7xl mx-auto">
          <div className="flex items-center gap-3">
            <button
              className="lg:hidden p-2 -ml-2 rounded-lg hover:bg-primary-50"
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            >
              {mobileMenuOpen ? <X className="h-5 w-5" /> : <Menu className="h-5 w-5" />}
            </button>
            <Link to="/" className="flex items-center gap-2">
              <Palette className="h-7 w-7 text-accent-500" />
              <span className="font-heading text-xl font-bold text-primary-800">Lienzo</span>
            </Link>
          </div>

          <nav className="hidden lg:flex items-center gap-1">
            {navItems.map((item) => (
              <Link
                key={item.path}
                to={item.path}
                className={cn(
                  'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                  isActive(item.path)
                    ? 'bg-primary-100 text-primary-800'
                    : 'text-primary-500 hover:text-primary-700 hover:bg-primary-50'
                )}
              >
                <item.icon className="h-4 w-4" />
                {item.label}
              </Link>
            ))}
          </nav>

          <div className="flex items-center gap-2">
            <button
              onClick={togglePanel}
              className="relative p-2 rounded-lg hover:bg-primary-50 text-primary-500"
            >
              <Bell className="h-5 w-5" />
              {unreadCount > 0 && (
                <span className="absolute -top-0.5 -right-0.5 flex items-center justify-center h-4 w-4 rounded-full bg-red-500 text-white text-[10px] font-bold">
                  {unreadCount > 9 ? '9+' : unreadCount}
                </span>
              )}
            </button>

            <div className="relative">
              <button
                onClick={() => setShowUserMenu(!showUserMenu)}
                className="flex items-center gap-2 p-1.5 rounded-lg hover:bg-primary-50"
              >
                <Avatar
                  src={user?.avatarUrl}
                  alt={`${user?.firstName} ${user?.lastName}`}
                  size="sm"
                />
                <span className="hidden sm:block text-sm font-medium text-primary-700">
                  {user?.firstName}
                </span>
              </button>

              {showUserMenu && (
                <>
                  <div className="fixed inset-0 z-10" onClick={() => setShowUserMenu(false)} />
                  <div className="absolute right-0 top-full mt-1 z-20 w-56 bg-white rounded-xl border border-primary-100 shadow-lg py-2 animate-fade-in">
                    <div className="px-4 py-2 border-b border-primary-100">
                      <p className="text-sm font-medium text-primary-800">
                        {user?.firstName} {user?.lastName}
                      </p>
                      <p className="text-xs text-primary-500">{user?.email}</p>
                      <Badge variant="default" className="mt-1">
                        {user?.role === 'Admin' ? 'Administrador' : user?.role === 'Teacher' ? 'Profesor' : 'Estudiante'}
                      </Badge>
                    </div>
                    <button
                      onClick={() => { navigate('/profile'); setShowUserMenu(false); }}
                      className="w-full flex items-center gap-2 px-4 py-2 text-sm text-primary-700 hover:bg-primary-50"
                    >
                      <User className="h-4 w-4" />
                      Perfil
                    </button>
                    <button
                      onClick={() => { doLogout(); setShowUserMenu(false); }}
                      className="w-full flex items-center gap-2 px-4 py-2 text-sm text-red-600 hover:bg-red-50"
                    >
                      <LogOut className="h-4 w-4" />
                      Cerrar sesión
                    </button>
                  </div>
                </>
              )}
            </div>
          </div>
        </div>
      </header>

      {user?.role === 'Admin' && (
        <div className={cn(
          'hidden lg:block fixed left-0 top-16 bottom-16 bg-white border-r border-primary-100 p-4 overflow-y-auto transition-all duration-200',
          sidebarCollapsed ? 'w-16' : 'w-56'
        )}>
          <div className="flex items-center justify-between mb-3">
            {!sidebarCollapsed && (
              <p className="text-xs font-semibold text-primary-400 uppercase tracking-wider">Administración</p>
            )}
            <button
              onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
              className="p-1.5 rounded-lg hover:bg-primary-50 text-primary-400 hover:text-primary-600"
            >
              {sidebarCollapsed ? <PanelLeft className="h-4 w-4" /> : <PanelLeftClose className="h-4 w-4" />}
            </button>
          </div>
          <nav className="space-y-1">
            {adminNavItems.map((item) => (
              <Link
                key={item.path}
                to={item.path}
                className={cn(
                  'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                  sidebarCollapsed ? 'justify-center px-0' : '',
                  isActive(item.path)
                    ? 'bg-accent-50 text-accent-700'
                    : 'text-primary-500 hover:text-primary-700 hover:bg-primary-50'
                )}
              >
                <item.icon className="h-4 w-4 flex-shrink-0" />
                {!sidebarCollapsed && item.label}
              </Link>
            ))}
          </nav>

          <AcademicSection isActive={isActive} collapsed={sidebarCollapsed} />
        </div>
      )}

      <main
        className={cn(
          'pt-16 pb-20 lg:pb-8 min-h-screen transition-all duration-200',
          user?.role === 'Admin' && (sidebarCollapsed ? 'lg:ml-16' : 'lg:ml-56')
        )}
      >
        <div className="max-w-7xl mx-auto px-4 py-6">
          {children}
        </div>
      </main>

      <nav className="fixed bottom-0 left-0 right-0 z-40 h-16 bg-white border-t border-primary-100 lg:hidden">
        <div className="flex items-center justify-around h-full px-2">
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={cn(
                'flex flex-col items-center justify-center gap-0.5 px-3 py-1 rounded-lg text-[10px] font-medium transition-colors min-h-[44px] min-w-[44px]',
                isActive(item.path)
                  ? 'text-accent-600'
                  : 'text-primary-400 hover:text-primary-600'
              )}
            >
              <item.icon className="h-5 w-5" />
              {item.label}
            </Link>
          ))}
        </div>
      </nav>

      <NotificationPanel />
    </div>
  );
}

const academicItems = [
  { path: '/admin/periodos', label: 'Periodos', icon: Calendar },
  { path: '/admin/carreras', label: 'Carreras', icon: GraduationCap },
  { path: '/admin/actividades', label: 'Actividades', icon: BookOpen },
  { path: '/admin/users', label: 'Usuarios', icon: Users },
];

function AcademicSection({ isActive, collapsed }: { isActive: (path: string) => boolean; collapsed: boolean }) {
  const [open, setOpen] = useState(() => academicItems.some((item) => isActive(item.path)));

  return (
    <div className="mt-2">
      <button
        onClick={() => setOpen(!open)}
        className={cn(
          'flex items-center justify-between w-full rounded-lg text-xs font-semibold text-primary-400 uppercase tracking-wider hover:text-primary-600 hover:bg-primary-50 transition-colors',
          collapsed ? 'p-2 justify-center' : 'px-3 py-2'
        )}
      >
        {collapsed ? (
          <BookOpen className="h-4 w-4" />
        ) : (
          <>
            <span>Académico</span>
            {open ? <ChevronDown className="h-3 w-3" /> : <ChevronRight className="h-3 w-3" />}
          </>
        )}
      </button>
      {open && (
        <nav className="space-y-1 mt-1">
          {academicItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={cn(
                'flex items-center gap-2 px-3 py-2 rounded-lg text-sm font-medium transition-colors',
                collapsed ? 'justify-center px-0' : '',
                isActive(item.path)
                  ? 'bg-accent-50 text-accent-700'
                  : 'text-primary-500 hover:text-primary-700 hover:bg-primary-50'
              )}
            >
              <item.icon className="h-4 w-4 flex-shrink-0" />
              {!collapsed && item.label}
            </Link>
          ))}
        </nav>
      )}
    </div>
  );
}
