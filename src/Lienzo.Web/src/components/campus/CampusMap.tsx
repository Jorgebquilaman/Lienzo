import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';
import {
  Building2, MapPin, Users, Clock, Wifi, AlertTriangle,
  X, RefreshCw, Loader2
} from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { Badge } from '@/components/ui/Badge';
import { ReservationModal } from '@/components/classrooms/ReservationModal';
import type { Classroom } from '@/types';

interface CampusReservationInfo {
  reservationId: string;
  title: string;
  userName: string;
  startTime: string;
  endTime: string;
}

interface CampusClassroom {
  id: string;
  name: string;
  capacity: number;
  type: string;
  status: 'available' | 'occupied' | 'maintenance' | 'inactive';
  currentReservation?: CampusReservationInfo | null;
}

interface CampusFloor {
  floor: number;
  classrooms: CampusClassroom[];
}

interface CampusBuilding {
  id: string;
  name: string;
  floors: CampusFloor[];
}

interface CampusStatus {
  buildings: CampusBuilding[];
  timestamp: string;
}

const statusConfig: Record<string, { bg: string; border: string; text: string; label: string; dot: string }> = {
  available: { bg: 'bg-green-50', border: 'border-green-400', text: 'text-green-700', label: 'Disponible', dot: 'bg-green-500' },
  occupied: { bg: 'bg-red-50', border: 'border-red-400', text: 'text-red-700', label: 'Ocupado', dot: 'bg-red-500' },
  maintenance: { bg: 'bg-yellow-50', border: 'border-yellow-400', text: 'text-yellow-700', label: 'Mantenimiento', dot: 'bg-yellow-500' },
  inactive: { bg: 'bg-gray-50', border: 'border-gray-300', text: 'text-gray-500', label: 'Inactivo', dot: 'bg-gray-400' },
};

const typeLabel: Record<string, string> = {
  Lecture: 'Aula',
  Laboratory: 'Lab',
  Workshop: 'Taller',
  Seminar: 'Seminario',
  Auditorium: 'Auditorio',
};

export default function CampusMap() {
  const [selectedClassroom, setSelectedClassroom] = useState<CampusClassroom | null>(null);
  const [selectedBuildingName, setSelectedBuildingName] = useState('');
  const [showReservation, setShowReservation] = useState(false);

  const { data, isLoading, isError } = useQuery<CampusStatus>({
    queryKey: ['campus-status'],
    queryFn: () => api.get<CampusStatus>('/campus/status'),
    refetchInterval: 30_000,
  });

  const handleClassroomClick = (classroom: CampusClassroom, buildingName: string) => {
    setSelectedClassroom(classroom);
    setSelectedBuildingName(buildingName);
  };

  const handleReserveNow = () => {
    setShowReservation(true);
  };

  const classroomForModal: Classroom | undefined = selectedClassroom
    ? {
        id: selectedClassroom.id,
        name: selectedClassroom.name,
        code: '',
        buildingId: '',
        buildingName: selectedBuildingName,
        floor: 0,
        capacity: selectedClassroom.capacity,
        type: selectedClassroom.type as any,
        features: [],
        isActive: selectedClassroom.status !== 'inactive',
        createdAt: '',
        updatedAt: '',
      }
    : undefined;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-16">
        <Loader2 className="h-8 w-8 animate-spin text-primary-300" />
      </div>
    );
  }

  if (isError || !data) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <AlertTriangle className="h-12 w-12 text-red-300 mb-3" />
        <p className="text-primary-500 text-sm">Error al cargar el mapa del campus</p>
      </div>
    );
  }

  const buildingsWithClassrooms = data.buildings.filter(
    (b) => b.floors.some((f) => f.classrooms.length > 0)
  );

  if (buildingsWithClassrooms.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-16">
        <Building2 className="h-12 w-12 text-primary-200 mb-3" />
        <p className="text-primary-400 text-sm">No hay edificios disponibles</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <div className="flex items-center gap-3 text-xs">
            <span className="flex items-center gap-1.5">
              <span className="h-2.5 w-2.5 rounded-full bg-green-500" />
              Disponible
            </span>
            <span className="flex items-center gap-1.5">
              <span className="h-2.5 w-2.5 rounded-full bg-red-500" />
              Ocupado
            </span>
            <span className="flex items-center gap-1.5">
              <span className="h-2.5 w-2.5 rounded-full bg-yellow-500" />
              Mantenimiento
            </span>
            <span className="flex items-center gap-1.5">
              <span className="h-2.5 w-2.5 rounded-full bg-gray-400" />
              Inactivo
            </span>
          </div>
        </div>
        <span className="text-xs text-primary-400 flex items-center gap-1">
          <RefreshCw className="h-3 w-3" />
          Actualizado cada 30s
        </span>
      </div>

      <div className="space-y-6">
        {buildingsWithClassrooms.map((building) => (
          <div key={building.id} className="bg-white rounded-xl border border-primary-100 overflow-hidden">
            <div className="bg-primary-50 px-5 py-3 border-b border-primary-100">
              <div className="flex items-center gap-2">
                <Building2 className="h-4 w-4 text-primary-500" />
                <h3 className="font-heading font-semibold text-primary-800">{building.name}</h3>
              </div>
            </div>

            <div className="p-4 space-y-4">
              {building.floors.length === 0 ? (
                <p className="text-sm text-primary-400 text-center py-4">Sin aulas en este edificio</p>
              ) : (
                building.floors.map((floor) => (
                  <div key={floor.floor}>
                    <div className="flex items-center gap-2 mb-2">
                      <MapPin className="h-3.5 w-3.5 text-primary-400" />
                      <span className="text-xs font-medium text-primary-500 uppercase tracking-wider">
                        Piso {floor.floor}
                      </span>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {[...floor.classrooms].sort((a, b) => a.name.localeCompare(b.name, 'es', { numeric: true })).map((cr) => {
                        const cfg = statusConfig[cr.status] || statusConfig.inactive;
                        return (
                          <button
                            key={cr.id}
                            onClick={() => handleClassroomClick(cr, building.name)}
                            className={`${cfg.bg} ${cfg.border} border-2 rounded-lg px-3 py-2 text-left min-w-[130px] hover:shadow-md transition-shadow cursor-pointer`}
                          >
                            <div className="flex items-center gap-1.5 mb-1">
                              <span className={`h-2 w-2 rounded-full ${cfg.dot}`} />
                              <span className="text-sm font-semibold text-primary-800">{cr.name}</span>
                            </div>
                            <div className="flex items-center gap-2 text-xs text-primary-500">
                              <span>{typeLabel[cr.type] || cr.type}</span>
                              <span>·</span>
                              <span className="flex items-center gap-0.5">
                                <Users className="h-3 w-3" />
                                {cr.capacity}
                              </span>
                            </div>
                            {cr.status === 'occupied' && cr.currentReservation && (
                              <div className="mt-1.5 text-xs text-red-600 truncate">
                                <Clock className="h-3 w-3 inline mr-0.5" />
                                {cr.currentReservation.startTime.slice(0, 5)} - {cr.currentReservation.endTime.slice(0, 5)}
                              </div>
                            )}
                            {cr.status === 'maintenance' && (
                              <div className="mt-1.5 text-xs text-yellow-600">
                                <Wifi className="h-3 w-3 inline mr-0.5" />
                                En mantenimiento
                              </div>
                            )}
                          </button>
                        );
                      })}
                    </div>
                  </div>
                ))
              )}
            </div>
          </div>
        ))}
      </div>

      <Dialog open={!!selectedClassroom} onOpenChange={(open) => { if (!open) setSelectedClassroom(null); }}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{selectedClassroom?.name}</DialogTitle>
            <DialogDescription>
              {selectedBuildingName}
            </DialogDescription>
          </DialogHeader>
          {selectedClassroom && (
            <DialogBody className="space-y-4">
              <div className="flex items-center gap-2">
                <span className={`h-3 w-3 rounded-full ${statusConfig[selectedClassroom.status].dot}`} />
                <span className={`text-sm font-medium ${statusConfig[selectedClassroom.status].text}`}>
                  {statusConfig[selectedClassroom.status].label}
                </span>
              </div>

              <div className="grid grid-cols-2 gap-3 text-sm">
                <div className="bg-primary-50 rounded-lg p-3">
                  <p className="text-primary-400 text-xs">Tipo</p>
                  <p className="font-medium text-primary-800">{typeLabel[selectedClassroom.type] || selectedClassroom.type}</p>
                </div>
                <div className="bg-primary-50 rounded-lg p-3">
                  <p className="text-primary-400 text-xs">Capacidad</p>
                  <p className="font-medium text-primary-800">{selectedClassroom.capacity} personas</p>
                </div>
              </div>

              {selectedClassroom.currentReservation && (
                <div className="bg-red-50 border border-red-200 rounded-lg p-3 space-y-1">
                  <p className="text-xs font-medium text-red-700">Reservado actualmente</p>
                  <p className="text-sm text-red-800 font-medium">{selectedClassroom.currentReservation.title}</p>
                  <p className="text-xs text-red-600">
                    {selectedClassroom.currentReservation.userName} · {selectedClassroom.currentReservation.startTime.slice(0, 5)} - {selectedClassroom.currentReservation.endTime.slice(0, 5)}
                  </p>
                </div>
              )}

              {selectedClassroom.status === 'available' && (
                <Button variant="accent" className="w-full" onClick={handleReserveNow}>
                  Reservar ahora
                </Button>
              )}
            </DialogBody>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setSelectedClassroom(null)}>
              Cerrar
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <ReservationModal
        open={showReservation}
        onOpenChange={setShowReservation}
        classroom={classroomForModal}
      />
    </div>
  );
}
