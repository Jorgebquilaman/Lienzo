import { useState, useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Search, MapPin, Users, Filter, X, Map, Clock, AlertTriangle, Wifi, KeyRound } from 'lucide-react';
import { api } from '@/lib/api';
import { Card, CardContent } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import { Select } from '@/components/ui/Select';
import { Badge } from '@/components/ui/Badge';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui/Tabs';
import { CardSkeleton } from '@/components/ui/Skeleton';
import { getClassroomTypeLabel } from '@/lib/utils';
import CampusMap from '@/components/campus/CampusMap';
import type { Classroom, Building } from '@/types';

interface CampusStatusClassroom {
  id: string;
  name: string;
  status: 'available' | 'occupied' | 'maintenance' | 'inactive';
  currentReservation?: { startTime: string; endTime: string; title: string; userName: string } | null;
}

const STATUS_STYLES: Record<string, { border: string; dot: string; label: string; text: string }> = {
  available: { border: 'border-l-accent-500', dot: 'bg-accent-500', label: 'Disponible', text: 'text-accent-700' },
  occupied: { border: 'border-l-red-500', dot: 'bg-red-500', label: 'Ocupado', text: 'text-red-700' },
  maintenance: { border: 'border-l-yellow-500', dot: 'bg-yellow-500', label: 'Mantenimiento', text: 'text-yellow-700' },
  inactive: { border: 'border-l-gray-400', dot: 'bg-gray-400', label: 'Inactivo', text: 'text-gray-500' },
};

export default function ClassroomBrowser() {
  const navigate = useNavigate();
  const [tab, setTab] = useState('list');
  const [search, setSearch] = useState('');
  const [buildingFilter, setBuildingFilter] = useState('');
  const [typeFilter, setTypeFilter] = useState('');
  const [sortBy, setSortBy] = useState('name-asc');
  const [showFilters, setShowFilters] = useState(false);

  const { data: buildings } = useQuery({
    queryKey: ['buildings'],
    queryFn: () => api.get<Building[]>('/buildings'),
  });

  const { data: classrooms, isLoading } = useQuery({
    queryKey: ['classrooms', search, buildingFilter, typeFilter],
    queryFn: () => {
      const params: Record<string, string> = {};
      if (search) params.search = search;
      if (buildingFilter) params.buildingId = buildingFilter;
      if (typeFilter) params.type = typeFilter;
      return api.get<Classroom[]>('/classrooms', params);
    },
  });

  const { data: campusStatus } = useQuery({
    queryKey: ['campus-status-browser'],
    queryFn: () => api.get<{ buildings: { id: string; floors: { classrooms: CampusStatusClassroom[] }[] }[] }>('/campus/status'),
    refetchInterval: 30_000,
  });

  const { data: keyDeliveries } = useQuery({
    queryKey: ['keydelivery-active'],
    queryFn: () => api.get<{ items: { classroomId: string; deliveredToName: string }[] }>('/keydelivery/active'),
    refetchInterval: 30_000,
  });

  const statusMap = useMemo(() => {
    const map: Record<string, CampusStatusClassroom> = {};
    for (const b of campusStatus?.buildings || []) {
      for (const f of b.floors || []) {
        for (const cr of f.classrooms || []) {
          map[cr.id] = cr;
        }
      }
    }
    return map;
  }, [campusStatus]);

  const keyMap = useMemo(() => {
    const map: Record<string, string> = {};
    for (const d of keyDeliveries?.items || []) {
      map[d.classroomId] = d.deliveredToName;
    }
    return map;
  }, [keyDeliveries]);

  const classroomTypes = [
    { value: 'Lecture', label: 'Aula' },
    { value: 'Laboratory', label: 'Laboratorio' },
    { value: 'Workshop', label: 'Taller' },
    { value: 'Seminar', label: 'Seminario' },
    { value: 'Auditorium', label: 'Auditorio' },
    { value: 'Office', label: 'Oficina' },
  ];

  const filtersActive = buildingFilter || typeFilter || search;

  const sortedClassrooms = useMemo(() => {
    if (!classrooms) return [];
    const list = [...classrooms];
    switch (sortBy) {
      case 'name-desc': return list.sort((a, b) => b.name.localeCompare(a.name));
      case 'capacity-asc': return list.sort((a, b) => a.capacity - b.capacity);
      case 'capacity-desc': return list.sort((a, b) => b.capacity - a.capacity);
      default: return list.sort((a, b) => a.name.localeCompare(b.name));
    }
  }, [classrooms, sortBy]);

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 className="font-heading text-2xl font-bold text-primary-800">Aulas</h1>
          <p className="text-primary-500 mt-1">Encuentra el aula perfecta para tu clase</p>
        </div>
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          <TabsTrigger value="list"><MapPin className="h-4 w-4 mr-1.5" />Lista</TabsTrigger>
          <TabsTrigger value="map"><Map className="h-4 w-4 mr-1.5" />Mapa</TabsTrigger>
        </TabsList>

        <TabsContent value="list">
          <div className="flex gap-4 mt-4">
            <div className="hidden sm:block w-64 flex-shrink-0 space-y-4">
              <div className="bg-white rounded-xl border border-primary-100 p-4 space-y-4">
                <h3 className="font-medium text-sm text-primary-700">Filtros</h3>
                <Input
                  placeholder="Buscar aulas..."
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
                <Select
                  label="Edificio"
                  placeholder="Todos"
                  value={buildingFilter}
                  onChange={(e) => setBuildingFilter(e.target.value)}
                  options={buildings?.map((b) => ({ value: b.id, label: b.name })) || []}
                />
                <Select
                  label="Tipo"
                  placeholder="Todos"
                  value={typeFilter}
                  onChange={(e) => setTypeFilter(e.target.value)}
                  options={classroomTypes}
                />
                <Select
                  label="Ordenar"
                  value={sortBy}
                  onChange={(e) => setSortBy(e.target.value)}
                  options={[
                    { value: 'name-asc', label: 'Nombre A-Z' },
                    { value: 'name-desc', label: 'Nombre Z-A' },
                    { value: 'capacity-asc', label: 'Capacidad ↑' },
                    { value: 'capacity-desc', label: 'Capacidad ↓' },
                  ]}
                />
                {filtersActive && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="w-full text-primary-400"
                    onClick={() => { setSearch(''); setBuildingFilter(''); setTypeFilter(''); }}
                  >
                    <X className="h-3 w-3 mr-1" />
                    Limpiar filtros
                  </Button>
                )}
              </div>
            </div>

            {showFilters && (
              <div className="fixed inset-0 z-50 sm:hidden">
                <div className="absolute inset-0 bg-black/30" onClick={() => setShowFilters(false)} />
                <div className="absolute bottom-0 left-0 right-0 bg-white rounded-t-2xl p-6 space-y-4 animate-slide-up">
                  <div className="flex items-center justify-between">
                    <h3 className="font-heading font-semibold text-primary-800">Filtros</h3>
                    <button onClick={() => setShowFilters(false)}>
                      <X className="h-5 w-5 text-primary-400" />
                    </button>
                  </div>
                  <Input
                    placeholder="Buscar aulas..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                  />
                  <Select
                    label="Edificio"
                    placeholder="Todos"
                    value={buildingFilter}
                    onChange={(e) => setBuildingFilter(e.target.value)}
                    options={buildings?.map((b) => ({ value: b.id, label: b.name })) || []}
                  />
                  <Select
                    label="Tipo"
                    placeholder="Todos"
                    value={typeFilter}
                    onChange={(e) => setTypeFilter(e.target.value)}
                    options={classroomTypes}
                  />
                  <Select
                    label="Ordenar"
                    value={sortBy}
                    onChange={(e) => setSortBy(e.target.value)}
                    options={[
                      { value: 'name-asc', label: 'Nombre A-Z' },
                      { value: 'name-desc', label: 'Nombre Z-A' },
                      { value: 'capacity-asc', label: 'Capacidad ↑' },
                      { value: 'capacity-desc', label: 'Capacidad ↓' },
                    ]}
                  />
                  <Button className="w-full" onClick={() => setShowFilters(false)}>
                    Aplicar filtros
                  </Button>
                </div>
              </div>
            )}

            <div className="flex-1">
              {isLoading ? (
                <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {[1, 2, 3, 4, 5, 6].map((i) => (
                    <CardSkeleton key={i} />
                  ))}
                </div>
              ) : !classrooms || classrooms.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-16">
                  <MapPin className="h-16 w-16 text-primary-200 mb-4" />
                  <h3 className="font-heading text-lg font-semibold text-primary-600">Sin resultados</h3>
                  <p className="text-primary-400 text-sm mt-1">
                    {filtersActive ? 'Intenta con otros filtros' : 'No hay aulas disponibles'}
                  </p>
                  {filtersActive && (
                    <Button
                      variant="outline"
                      size="sm"
                      className="mt-4"
                      onClick={() => { setSearch(''); setBuildingFilter(''); setTypeFilter(''); }}
                    >
                      Limpiar filtros
                    </Button>
                  )}
                </div>
              ) : (
                <div className="grid sm:grid-cols-2 lg:grid-cols-3 gap-4">
                  {sortedClassrooms.map((classroom) => (
                    <Card
                      key={classroom.id}
                      className={`group cursor-pointer hover:shadow-md transition-shadow border-l-4 ${
                        STATUS_STYLES[statusMap[classroom.id]?.status]?.border || 'border-l-transparent'
                      }`}
                      onClick={() => navigate(`/classrooms/${classroom.id}`)}
                    >
                      <div
                        className="h-40 rounded-t-xl flex items-center justify-center bg-cover bg-center"
                        style={classroom.imageUrl ? { backgroundImage: `url(${classroom.imageUrl})` } : undefined}
                      >
                        {!classroom.imageUrl && (
                          <div className="text-center">
                            <p className="font-heading text-3xl font-bold text-primary-400">
                              {classroom.code}
                            </p>
                          </div>
                        )}
                      </div>
                      <CardContent className="p-4">
                        <div className="flex items-start justify-between mb-2">
                          <div className="flex-1 min-w-0">
                            <div className="flex items-center gap-2">
                              {statusMap[classroom.id] && (
                                <span className={`h-2.5 w-2.5 rounded-full flex-shrink-0 ${
                                  STATUS_STYLES[statusMap[classroom.id].status]?.dot || 'bg-gray-400'
                                }`} />
                              )}
                              <h3 className="font-medium text-primary-800 group-hover:text-accent-600 transition-colors truncate">
                                {classroom.name}
                              </h3>
                            </div>
                            <p className="text-xs text-primary-400 flex items-center gap-1 mt-0.5 ml-5">
                              <MapPin className="h-3 w-3" />
                              {classroom.buildingName} · Piso {classroom.floor}
                            </p>
                            {keyMap[classroom.id] && (
                              <p className="text-xs flex items-center gap-1 mt-0.5 ml-5 text-accent-600">
                                <KeyRound className="h-3 w-3" />
                                Llave: {keyMap[classroom.id]}
                              </p>
                            )}
                          </div>
                        </div>

                        {statusMap[classroom.id] && statusMap[classroom.id].status !== 'available' && (
                          <div className={`text-xs flex items-center gap-1 mb-2 ml-5 ${
                            STATUS_STYLES[statusMap[classroom.id].status]?.text || 'text-gray-500'
                          }`}>
                            {statusMap[classroom.id].status === 'occupied' ? (
                              <><Clock className="h-3 w-3" /> Ocupado {statusMap[classroom.id].currentReservation?.startTime?.slice(0,5) || ''}-{statusMap[classroom.id].currentReservation?.endTime?.slice(0,5) || ''}</>
                            ) : statusMap[classroom.id].status === 'maintenance' ? (
                              <><Wifi className="h-3 w-3" /> En mantenimiento</>
                            ) : (
                              <><AlertTriangle className="h-3 w-3" /> Inactivo</>
                            )}
                          </div>
                        )}

                        <div className="flex items-center justify-between mt-3">
                          <div className="flex items-center gap-2">
                            <Badge variant="default">{getClassroomTypeLabel(classroom.type)}</Badge>
                            <span className="text-xs text-primary-400 flex items-center gap-0.5">
                              <Users className="h-3 w-3" />
                              {classroom.capacity}
                            </span>
                          </div>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={(e) => {
                              e.stopPropagation();
                              navigate(`/classrooms/${classroom.id}`);
                            }}
                          >
                            Ver
                          </Button>
                        </div>
                      </CardContent>
                    </Card>
                  ))}
                </div>
              )}
            </div>
          </div>
        </TabsContent>

        <TabsContent value="map">
          <div className="mt-4">
            <CampusMap />
          </div>
        </TabsContent>
      </Tabs>
    </div>
  );
}
