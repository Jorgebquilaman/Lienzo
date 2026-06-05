import { useState, useEffect, useMemo, useCallback } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { format } from 'date-fns';
import { es } from 'date-fns/locale';
import { Maximize2, Minimize2, Sun, Moon, Building2, Eye } from 'lucide-react';

interface CampusStatusResponse {
  buildings: Building[];
  timestamp: string;
}

interface Building {
  id: string;
  name: string;
  floors: Floor[];
}

interface Floor {
  floor: number;
  classrooms: ClassroomStatus[];
}

interface ClassroomStatus {
  id: string;
  name: string;
  capacity: number;
  type: string;
  status: string;
  currentReservation: ReservationInfo | null;
}

interface ReservationInfo {
  reservationId: string;
  title: string;
  userName: string;
  startTime: string;
  endTime: string;
}

interface ReservationDto {
  id: string;
  classroomId: string;
  classroomName: string;
  buildingName: string;
  userId: string;
  userName: string;
  title: string;
  description: string | null;
  date: string;
  startTime: string;
  endTime: string;
  status: string;
  actividadNombre: string | null;
  actividadDocentes: string | null;
}

interface ThemeClasses {
  bg: string;
  text: string;
  textMuted: string;
  textDim: string;
  headerBg: string;
  cardBg: string;
  cardBorder: string;
  cardText: string;
  cardSub: string;
  footerBg: string;
  footerText: string;
  sectionText: string;
  dotInactive: string;
  statsLabel: string;
}

const DARK: ThemeClasses = {
  bg: 'bg-gradient-to-br from-gray-900 via-primary-900 to-gray-900',
  text: 'text-white',
  textMuted: 'text-white/80',
  textDim: 'text-white/40',
  headerBg: 'bg-black/20',
  cardBg: 'bg-green-500/10',
  cardBorder: 'border-green-500/25',
  cardText: 'text-white',
  cardSub: 'text-white/50',
  footerBg: 'bg-black/30',
  footerText: 'text-white/40',
  sectionText: 'text-white/40',
  dotInactive: 'bg-white/20',
  statsLabel: 'text-white/60',
};

const LIGHT: ThemeClasses = {
  bg: 'bg-gradient-to-br from-white via-primary-50 to-white',
  text: 'text-gray-900',
  textMuted: 'text-gray-700',
  textDim: 'text-gray-400',
  headerBg: 'bg-primary-100/50',
  cardBg: 'bg-green-100',
  cardBorder: 'border-green-300',
  cardText: 'text-gray-900',
  cardSub: 'text-gray-600',
  footerBg: 'bg-primary-100/70',
  footerText: 'text-gray-500',
  sectionText: 'text-gray-500',
  dotInactive: 'bg-gray-300',
  statsLabel: 'text-gray-600',
};

const VIEW_DURATION = 15000;
const CONFIG_TIMEOUT = 60000;

const STATUS_COLORS: Record<string, string> = {
  available: 'bg-green-500',
  occupied: 'bg-red-500',
  maintenance: 'bg-yellow-500',
  inactive: 'bg-gray-400',
};

export default function TVDashboard() {
  const [currentView, setCurrentView] = useState(0);
  const [transitioning, setTransitioning] = useState(false);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const [isPortrait, setIsPortrait] = useState(() => window.innerHeight > window.innerWidth);
  const [theme, setTheme] = useState<'dark' | 'light'>(() => (localStorage.getItem('tv-theme') as 'dark' | 'light') || 'dark');
  const [showConfig, setShowConfig] = useState(true);
  const [selectedBuildingId, setSelectedBuildingId] = useState<string | null>(null);
  const [configCountdown, setConfigCountdown] = useState(CONFIG_TIMEOUT / 1000);

  useEffect(() => {
    const mq = window.matchMedia('(orientation: portrait)');
    const handler = (e: MediaQueryListEvent) => setIsPortrait(e.matches);
    mq.addEventListener('change', handler);
    return () => mq.removeEventListener('change', handler);
  }, []);

  const t = theme === 'dark' ? DARK : LIGHT;

  const toggleTheme = useCallback(() => {
    setTheme((prev) => {
      const next = prev === 'dark' ? 'light' : 'dark';
      localStorage.setItem('tv-theme', next);
      return next;
    });
  }, []);

  const toggleFullscreen = async () => {
    if (!document.fullscreenElement) {
      await document.documentElement.requestFullscreen();
      setIsFullscreen(true);
    } else {
      await document.exitFullscreen();
      setIsFullscreen(false);
    }
  };

  useEffect(() => {
    const onFsChange = () => setIsFullscreen(!!document.fullscreenElement);
    document.addEventListener('fullscreenchange', onFsChange);
    return () => document.removeEventListener('fullscreenchange', onFsChange);
  }, []);

  const todayStr = useMemo(() => {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }, []);

  const campusQuery = useQuery<CampusStatusResponse>({
    queryKey: ['tv-campus-status'],
    queryFn: () => api.get<CampusStatusResponse>('/public/campus-status'),
    refetchInterval: 30000,
  });

  const scheduleQuery = useQuery<ReservationDto[]>({
    queryKey: ['tv-schedule', todayStr],
    queryFn: () => api.get<ReservationDto[]>('/public/schedule', {
      fromDate: `${todayStr}T00:00:00`,
      toDate: `${todayStr}T23:59:59`,
    }),
    refetchInterval: 30000,
  });

  const buildings = campusQuery.data?.buildings ?? [];
  const reservations = scheduleQuery.data ?? [];

  const buildingsWithClassrooms = useMemo(
    () => buildings.filter((b) => b.floors.some((f) => f.classrooms.length > 0)),
    [buildings]
  );

  // Auto-dismiss config overlay after 60 seconds
  useEffect(() => {
    if (!showConfig) return;
    const interval = setInterval(() => {
      setConfigCountdown((prev) => {
        if (prev <= 1) {
          setShowConfig(false);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);
    return () => clearInterval(interval);
  }, [showConfig]);

  const handleSelectBuilding = (buildingId: string | null) => {
    setSelectedBuildingId(buildingId);
    setShowConfig(false);
    setCurrentView(0);
  };

  // Filter buildings when a specific one is selected
  const filteredBuildings = useMemo(() => {
    if (!selectedBuildingId) return buildingsWithClassrooms;
    const b = buildingsWithClassrooms.find((b) => b.id === selectedBuildingId);
    return b ? [b] : buildingsWithClassrooms;
  }, [buildingsWithClassrooms, selectedBuildingId]);

  const buildingCount = filteredBuildings.length;
  const totalViews = buildingCount + 2;
  const isBuildingView = currentView < buildingCount;
  const isScheduleView = currentView === buildingCount;
  const isStatsView = currentView === buildingCount + 1;

  // Auto-cycle views (but not when config overlay is shown)
  useEffect(() => {
    if (totalViews === 2 || showConfig) return;
    const interval = setInterval(() => {
      setTransitioning(true);
      setTimeout(() => {
        setCurrentView((prev) => (prev + 1) % totalViews);
        setTransitioning(false);
      }, 300);
    }, VIEW_DURATION);
    return () => clearInterval(interval);
  }, [totalViews, showConfig]);

  const stats = useMemo(() => {
    const allClassrooms = buildings.flatMap((b) => b.floors.flatMap((f) => f.classrooms));
    return {
      total: allClassrooms.length,
      available: allClassrooms.filter((c) => c.status === 'available').length,
      occupied: allClassrooms.filter((c) => c.status === 'occupied').length,
      maintenance: allClassrooms.filter((c) => c.status === 'maintenance').length,
      classesToday: reservations.filter((r) => r.status === 'Approved').length,
    };
  }, [buildings, reservations]);

  const upcomingReservations = useMemo(
    () => reservations.filter((r) => r.status === 'Approved').sort((a, b) => a.startTime.localeCompare(b.startTime)),
    [reservations]
  );

  const now = new Date();
  const currentTimeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;

  const currentClasses = upcomingReservations.filter(
    (r) => r.startTime <= currentTimeStr && r.endTime > currentTimeStr
  );

  const sectionLabel = showConfig
    ? 'Configuración'
    : isBuildingView && filteredBuildings[currentView]
      ? filteredBuildings[currentView].name
      : isScheduleView ? 'Clases de Hoy' : 'Resumen';

  const sectionIcon = showConfig ? <Eye className="h-7 w-7" /> : null;

  return (
    <div className={`h-screen w-screen overflow-hidden ${t.bg} ${t.text}`}>
      {/* Config overlay */}
      {showConfig && (
        <div className="absolute inset-0 z-50 flex flex-col items-center justify-center bg-black/60 backdrop-blur-sm">
          <div className="max-w-3xl w-full px-8 text-center">
            <Building2 className="h-16 w-16 mx-auto mb-4 text-primary-300" />
            <h2 className="text-4xl font-bold mb-2">Seleccionar Edificio</h2>
            <p className="text-lg text-white/60 mb-2">
              Elegí qué edificio mostrar o mirá todos
            </p>
            <p className="text-sm text-white/40 mb-8">
              Se mostrarán todos en {configCountdown}s
            </p>
            <div className="flex flex-wrap justify-center gap-4">
              <button
                onClick={() => handleSelectBuilding(null)}
                className="px-8 py-4 rounded-xl bg-white/10 border border-white/20 hover:bg-white/20 transition-colors text-white text-lg font-semibold"
              >
                <Eye className="h-5 w-5 inline mr-2 -mt-0.5" />
                Todos los edificios
              </button>
              {buildingsWithClassrooms.map((b) => (
                <button
                  key={b.id}
                  onClick={() => handleSelectBuilding(b.id)}
                  className="px-8 py-4 rounded-xl bg-white/10 border border-white/20 hover:bg-white/20 transition-colors text-white text-lg font-semibold"
                >
                  {b.name}
                </button>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Header */}
      <div className={`flex items-center justify-between ${isPortrait ? 'px-4 py-2' : 'px-8 py-4'} ${t.headerBg}`}>
        <div className="flex items-center gap-3">
          <h1 className={`font-bold tracking-tight ${isPortrait ? 'text-xl' : 'text-3xl'}`}>Lienzo</h1>
          <span className={`${isPortrait ? 'text-base' : 'text-xl'} ${theme === 'dark' ? 'text-primary-300/70' : 'text-primary-400'} font-light`}>|</span>
          <span className={`font-light ${t.textMuted} flex items-center gap-2 ${isPortrait ? 'text-lg' : 'text-2xl'}`}>
            {sectionIcon} {sectionLabel}
          </span>
        </div>
        <div className={`flex items-center ${isPortrait ? 'gap-2' : 'gap-6'}`}>
          <span className={`font-light ${t.textMuted} ${isPortrait ? 'text-sm hidden md:inline' : 'text-2xl'}`}>
            {format(now, "EEEE, d 'de' MMMM 'de' yyyy", { locale: es })}
          </span>
          <span className={`font-mono font-bold ${theme === 'dark' ? 'text-primary-300' : 'text-primary-600'} ${isPortrait ? 'text-base' : 'text-2xl'}`}>
            {format(now, "HH:mm")}
          </span>
          <button
            onClick={toggleTheme}
            className={`p-1 rounded-lg hover:bg-white/10 transition-colors ${t.textMuted}`}
            title={theme === 'dark' ? 'Tema claro' : 'Tema oscuro'}
          >
            {theme === 'dark' ? <Sun className={`${isPortrait ? 'h-4 w-4' : 'h-5 w-5'}`} /> : <Moon className={`${isPortrait ? 'h-4 w-4' : 'h-5 w-5'}`} />}
          </button>
          <button
            onClick={() => { setShowConfig(true); setConfigCountdown(CONFIG_TIMEOUT / 1000); }}
            className={`p-1 rounded-lg hover:bg-white/10 transition-colors ${t.textMuted}`}
            title="Cambiar edificio"
          >
            <Building2 className={`${isPortrait ? 'h-4 w-4' : 'h-5 w-5'}`} />
          </button>
          <button
            onClick={toggleFullscreen}
            className={`p-1 rounded-lg hover:bg-white/10 transition-colors ${t.textMuted}`}
            title={isFullscreen ? 'Salir de pantalla completa' : 'Pantalla completa'}
          >
            {isFullscreen ? <Minimize2 className={`${isPortrait ? 'h-4 w-4' : 'h-5 w-5'}`} /> : <Maximize2 className={`${isPortrait ? 'h-4 w-4' : 'h-5 w-5'}`} />}
          </button>
          <span className={`text-xs ${t.textDim} ${isPortrait ? 'hidden' : ''}`}>
            {currentView + 1} / {totalViews}
          </span>
        </div>
      </div>

      {/* View indicator dots */}
      {!showConfig && totalViews > 0 && (
        <div className={`flex justify-center gap-1.5 ${isPortrait ? 'py-1 px-4' : 'py-1.5 px-8'} overflow-x-auto`}>
          {Array.from({ length: totalViews }).map((_, i) => (
            <div
              key={i}
              className={`shrink-0 rounded-full transition-all duration-500 ${
                i === currentView
                  ? `${isPortrait ? 'w-4 h-1.5' : 'w-6 h-2'} ${theme === 'dark' ? 'bg-primary-400' : 'bg-primary-500'}`
                  : `${isPortrait ? 'w-1 h-1' : 'w-1.5 h-1.5'} ${t.dotInactive}`
              }`}
            />
          ))}
        </div>
      )}

      {/* Content */}
      <div className={isPortrait ? 'px-3 pb-3' : 'px-8 pb-6'} style={{ height: `calc(100vh - ${isPortrait ? 70 : 110}px)` }}>
        <div
          className={`h-full transition-all duration-300 ${
            transitioning ? 'opacity-0 scale-[0.98]' : 'opacity-100 scale-100'
          }`}
        >
          {!showConfig && isBuildingView && filteredBuildings.length > 0 && filteredBuildings[currentView] && (
            <BuildingView building={filteredBuildings[currentView]} reservations={upcomingReservations} theme={theme} isPortrait={isPortrait} />
          )}
          {!showConfig && isScheduleView && <ScheduleView reservations={upcomingReservations} theme={theme} isPortrait={isPortrait} />}
          {!showConfig && isStatsView && <StatsView stats={stats} currentClasses={currentClasses} theme={theme} isPortrait={isPortrait} />}
        </div>
      </div>

      {/* Footer */}
      <div className={`absolute bottom-0 left-0 right-0 flex items-center justify-between ${isPortrait ? 'px-4 py-2' : 'px-8 py-3'} ${t.footerBg} ${t.footerText} text-sm`}>
        <span>Datos actualizados automáticamente</span>
        <span>Lienzo - Sistema de Gestión de Aulas</span>
      </div>
    </div>
  );
}

function BuildingView({ building, reservations, theme, isPortrait }: { building: Building; reservations: ReservationDto[]; theme: 'dark' | 'light'; isPortrait: boolean }) {
  const allRooms = building.floors.flatMap((f) => f.classrooms);
  const available = allRooms.filter((c) => c.status === 'available').length;
  const total = allRooms.length;
  const t = theme === 'dark' ? DARK : LIGHT;

  const now = new Date();
  const currentTime = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;

  const nextForRoom = (roomId: string) =>
    reservations.find(
      (r) => r.classroomId === roomId && r.startTime > currentTime
    );

  const size = total <= 6 ? 'xl' : total <= 14 ? 'lg' : total <= 28 ? 'md' : 'sm';

  const landscapeSizes = {
    xl: { card: 'p-6', name: 'text-2xl', dot: 'h-4 w-4', sub: 'text-xs', cols: 'grid-cols-2 md:grid-cols-3', gap: 'gap-3', section: 'text-lg', sectionBar: 'h-5' },
    lg: { card: 'p-4', name: 'text-xl', dot: 'h-3 w-3', sub: 'text-[11px]', cols: 'grid-cols-2 md:grid-cols-4', gap: 'gap-2', section: 'text-base', sectionBar: 'h-4' },
    md: { card: 'p-3', name: 'text-base', dot: 'h-2.5 w-2.5', sub: 'text-[10px]', cols: 'grid-cols-3 md:grid-cols-6', gap: 'gap-1.5', section: 'text-sm', sectionBar: 'h-3.5' },
    sm: { card: 'p-2', name: 'text-xs', dot: 'h-2 w-2', sub: 'text-[9px]', cols: 'grid-cols-4 md:grid-cols-8 lg:grid-cols-10', gap: 'gap-1', section: 'text-xs', sectionBar: 'h-3' },
  };

  const portraitSizes = {
    xl: { card: 'p-5', name: 'text-xl', dot: 'h-3.5 w-3.5', sub: 'text-xs', cols: 'grid-cols-1 md:grid-cols-2', gap: 'gap-2', section: 'text-base', sectionBar: 'h-4' },
    lg: { card: 'p-4', name: 'text-lg', dot: 'h-3 w-3', sub: 'text-[11px]', cols: 'grid-cols-1 md:grid-cols-2', gap: 'gap-2', section: 'text-sm', sectionBar: 'h-3.5' },
    md: { card: 'p-3', name: 'text-base', dot: 'h-2.5 w-2.5', sub: 'text-[10px]', cols: 'grid-cols-2', gap: 'gap-1.5', section: 'text-sm', sectionBar: 'h-3' },
    sm: { card: 'p-2', name: 'text-sm', dot: 'h-2 w-2', sub: 'text-[9px]', cols: 'grid-cols-2', gap: 'gap-1', section: 'text-xs', sectionBar: 'h-2.5' },
  };

  const s = (isPortrait ? portraitSizes : landscapeSizes)[size];

  return (
    <div className="h-full flex flex-col py-2">
      <div className={`flex items-center justify-center text-center mb-2 shrink-0 ${isPortrait ? 'gap-2' : 'gap-4'}`}>
        <div className={`${theme === 'dark' ? 'bg-green-500/10 border-green-500/30' : 'bg-green-100 border-green-300'} border rounded-lg ${isPortrait ? 'px-3 py-1' : 'px-4 py-1.5'}`}>
          <div className={`font-bold text-green-600 ${isPortrait ? 'text-lg' : 'text-xl'}`}>{available}</div>
          <div className={`text-xs ${t.statsLabel}`}>Libres</div>
        </div>
        <div className={`${theme === 'dark' ? 'bg-red-500/10 border-red-500/30' : 'bg-red-100 border-red-300'} border rounded-lg ${isPortrait ? 'px-3 py-1' : 'px-4 py-1.5'}`}>
          <div className={`font-bold text-red-600 ${isPortrait ? 'text-lg' : 'text-xl'}`}>{total - available}</div>
          <div className={`text-xs ${t.statsLabel}`}>Ocupadas</div>
        </div>
        <div className={`text-xs ${t.textDim}`}>{total} aulas</div>
      </div>

      <div className="flex-1 flex flex-col justify-center gap-2 min-h-0">
        {building.floors
          .sort((a, b) => b.floor - a.floor)
          .map((floor) => (
            <div key={floor.floor}>
              <h3 className={`${s.section} font-semibold ${t.sectionText} mb-1 flex items-center gap-2`}>
                <span className={`w-1 ${s.sectionBar} bg-primary-400 rounded-full inline-block shrink-0`} />
                Piso {floor.floor}
              </h3>
              <div className={`grid ${s.cols} ${s.gap}`}>
                {[...floor.classrooms]
                  .sort((a, b) => a.name.localeCompare(b.name, 'es', { numeric: true }))
                  .map((room) => {
                    const next = room.status === 'available' ? nextForRoom(room.id) : null;
                    const statusStyle = room.status === 'available'
                      ? `${theme === 'dark' ? 'bg-green-500/10 border-green-500/25' : 'bg-green-100 border-green-300'}`
                      : room.status === 'occupied'
                      ? `${theme === 'dark' ? 'bg-red-500/10 border-red-500/25' : 'bg-red-100 border-red-300'}`
                      : room.status === 'maintenance'
                      ? `${theme === 'dark' ? 'bg-yellow-500/10 border-yellow-500/25' : 'bg-yellow-100 border-yellow-300'}`
                      : `${theme === 'dark' ? 'bg-gray-500/10 border-gray-500/25' : 'bg-gray-100 border-gray-300'}`;
                    return (
                      <div
                        key={room.id}
                        className={`rounded-lg ${s.card} border ${statusStyle}`}
                      >
                        <div className="flex items-center justify-between gap-1">
                          <span className={`font-bold ${s.name} ${t.cardText} truncate`}>{room.name}</span>
                          <div className={`${s.dot} rounded-full shrink-0 ${STATUS_COLORS[room.status] || 'bg-gray-500'}`} />
                        </div>
                        {room.status === 'occupied' && room.currentReservation && (
                          <div className={`${s.sub} ${t.cardSub} leading-tight truncate`}>
                            {room.currentReservation.title}
                          </div>
                        )}
                        {room.status === 'available' && (
                          <div className={`${s.sub} ${t.textDim} leading-tight truncate`}>
                            {next ? next.title : ''}
                          </div>
                        )}
                        {room.status === 'maintenance' && (
                          <div className={`${s.sub} ${t.textDim} leading-tight truncate`}>Mantenimiento</div>
                        )}
                        {room.status === 'inactive' && (
                          <div className={`${s.sub} ${t.textDim} leading-tight truncate`}>Inactivo</div>
                        )}
                      </div>
                    );
                  })}
              </div>
            </div>
          ))}
      </div>
    </div>
  );
}

function ScheduleView({ reservations, theme, isPortrait }: { reservations: ReservationDto[]; theme: 'dark' | 'light'; isPortrait: boolean }) {
  if (!reservations.length) {
    return (
      <div className={`flex flex-col items-center justify-center h-full text-2xl ${theme === 'dark' ? 'text-white/40' : 'text-gray-400'}`}>
        <div className="text-6xl mb-4 opacity-30">📅</div>
        No hay clases programadas para hoy
      </div>
    );
  }

  return (
    <div className="h-full overflow-y-auto py-4">
      <div className={isPortrait ? 'space-y-2' : 'space-y-3'}>
        {reservations.map((r) => {
          const isActive = r.startTime <= format(new Date(), 'HH:mm') && r.endTime > format(new Date(), 'HH:mm');
          const isPast = r.endTime <= format(new Date(), 'HH:mm');
          const activeStyle = isActive
            ? `${theme === 'dark' ? 'bg-green-500/15 border-green-500/40 ring-1 ring-green-500/30' : 'bg-green-100 border-green-400 ring-1 ring-green-300'}`
            : isPast
            ? `${theme === 'dark' ? 'bg-white/5 border-white/10' : 'bg-gray-50 border-gray-200'} opacity-50`
            : `${theme === 'dark' ? 'bg-white/5 border-white/10' : 'bg-gray-50 border-gray-200'}`;
          return (
            <div
              key={r.id}
              className={`flex items-center rounded-xl border transition-colors ${activeStyle} ${isPortrait ? 'gap-3 p-3' : 'gap-4 p-5'}`}
            >
              <div className={`flex flex-col items-center ${isPortrait ? 'min-w-[60px]' : 'min-w-[100px]'}`}>
                <span className={`font-mono font-bold ${isActive ? 'text-green-600' : theme === 'dark' ? 'text-white/70' : 'text-gray-700'} ${isPortrait ? 'text-lg' : 'text-2xl'}`}>
                  {r.startTime.slice(0, 5)}
                </span>
                <span className={`text-sm ${theme === 'dark' ? 'text-white/40' : 'text-gray-400'} ${isPortrait ? 'text-xs' : 'text-sm'}`}>{r.endTime.slice(0, 5)}</span>
              </div>
              <div className="flex-1 min-w-0">
                <div className={`flex items-center gap-2 ${isPortrait ? 'gap-1 flex-wrap' : 'gap-3'}`}>
                  <span className={`font-bold truncate ${theme === 'dark' ? 'text-white' : 'text-gray-900'} ${isPortrait ? 'text-base' : 'text-xl'}`}>{r.title}</span>
                  {isActive && (
                    <span className="px-2 py-0.5 rounded-full bg-green-500/20 text-green-600 text-xs font-semibold shrink-0">
                      AHORA
                    </span>
                  )}
                  {r.actividadNombre && (
                    <span className={`truncate ${isPortrait ? 'text-sm' : 'text-lg'} ${theme === 'dark' ? 'text-white/60' : 'text-gray-500'}`}>{r.actividadNombre}</span>
                  )}
                </div>
                <div className={`flex items-center gap-2 mt-0.5 ${theme === 'dark' ? 'text-white/50' : 'text-gray-500'} ${isPortrait ? 'text-xs flex-wrap' : 'text-base gap-4'}`}>
                  <span>{r.classroomName}</span>
                  {r.buildingName && <span>· {r.buildingName}</span>}
                  {r.userName && <span>· {r.userName}</span>}
                  {r.actividadDocentes && <span>· {r.actividadDocentes}</span>}
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}

function StatsView({ stats, currentClasses, theme, isPortrait }: { stats: { total: number; available: number; occupied: number; maintenance: number; classesToday: number }; currentClasses: ReservationDto[]; theme: 'dark' | 'light'; isPortrait: boolean }) {
  return (
    <div className={`flex flex-col items-center justify-center h-full ${isPortrait ? 'gap-4' : 'gap-8'}`}>
      <div className={`grid ${isPortrait ? 'grid-cols-2 gap-3 w-full' : 'grid-cols-2 lg:grid-cols-4 gap-6 w-full max-w-5xl'}`}>
        <div className={`${theme === 'dark' ? 'bg-green-500/10 border-green-500/30' : 'bg-green-100 border-green-300'} border rounded-2xl text-center ${isPortrait ? 'p-4' : 'p-8'}`}>
          <div className={`font-bold text-green-600 mb-1 ${isPortrait ? 'text-4xl' : 'text-6xl'}`}>{stats.available}</div>
          <div className={`${theme === 'dark' ? 'text-white/60' : 'text-gray-600'} ${isPortrait ? 'text-sm' : 'text-xl'}`}>Aulas Libres</div>
        </div>
        <div className={`${theme === 'dark' ? 'bg-red-500/10 border-red-500/30' : 'bg-red-100 border-red-300'} border rounded-2xl text-center ${isPortrait ? 'p-4' : 'p-8'}`}>
          <div className={`font-bold text-red-600 mb-1 ${isPortrait ? 'text-4xl' : 'text-6xl'}`}>{stats.occupied}</div>
          <div className={`${theme === 'dark' ? 'text-white/60' : 'text-gray-600'} ${isPortrait ? 'text-sm' : 'text-xl'}`}>Aulas Ocupadas</div>
        </div>
        <div className={`${theme === 'dark' ? 'bg-yellow-500/10 border-yellow-500/30' : 'bg-yellow-100 border-yellow-300'} border rounded-2xl text-center ${isPortrait ? 'p-4' : 'p-8'}`}>
          <div className={`font-bold text-yellow-600 mb-1 ${isPortrait ? 'text-4xl' : 'text-6xl'}`}>{stats.maintenance}</div>
          <div className={`${theme === 'dark' ? 'text-white/60' : 'text-gray-600'} ${isPortrait ? 'text-sm' : 'text-xl'}`}>En Mantenimiento</div>
        </div>
        <div className={`${theme === 'dark' ? 'bg-blue-500/10 border-blue-500/30' : 'bg-blue-100 border-blue-300'} border rounded-2xl text-center ${isPortrait ? 'p-4' : 'p-8'}`}>
          <div className={`font-bold text-blue-600 mb-1 ${isPortrait ? 'text-4xl' : 'text-6xl'}`}>{stats.classesToday}</div>
          <div className={`${theme === 'dark' ? 'text-white/60' : 'text-gray-600'} ${isPortrait ? 'text-sm' : 'text-xl'}`}>Clases Hoy</div>
        </div>
      </div>

      {currentClasses.length > 0 && (
        <div className={`w-full ${isPortrait ? 'max-w-full px-2' : 'max-w-2xl'}`}>
          <h3 className={`font-semibold ${theme === 'dark' ? 'text-white/60' : 'text-gray-500'} mb-3 text-center ${isPortrait ? 'text-base' : 'text-xl'}`}>Clases en este momento</h3>
          <div className={isPortrait ? 'space-y-2' : 'space-y-3'}>
            {currentClasses.slice(0, 4).map((r) => (
              <div key={r.id} className={`${theme === 'dark' ? 'bg-green-500/10 border-green-500/30' : 'bg-green-100 border-green-300'} border rounded-xl text-center ${isPortrait ? 'p-3' : 'p-4'}`}>
                <div className={`font-bold ${theme === 'dark' ? 'text-white' : 'text-gray-900'} ${isPortrait ? 'text-base' : 'text-xl'}`}>{r.title}</div>
                <div className={`${theme === 'dark' ? 'text-white/60' : 'text-gray-600'} ${isPortrait ? 'text-xs' : 'text-base'}`}>{r.classroomName} · {r.startTime.slice(0, 5)} - {r.endTime.slice(0, 5)}</div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}


