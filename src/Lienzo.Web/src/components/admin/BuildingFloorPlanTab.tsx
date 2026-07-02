import { useState, useRef, useCallback, useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Upload, Search, Save, MapPin, X } from 'lucide-react';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import { Button } from '@/components/ui/Button';
import { Input } from '@/components/ui/Input';
import type { Building, Classroom } from '@/types';

interface Position {
  classroomId: string;
  x: number;
  y: number;
}

interface Props {
  building: Building;
}

export default function BuildingFloorPlanTab({ building }: Props) {
  const [positions, setPositions] = useState<Record<string, Position>>({});
  const [selectedClassroom, setSelectedClassroom] = useState<string | null>(null);
  const [search, setSearch] = useState('');
  const [floorPlanUrl, setFloorPlanUrl] = useState(building.floorPlanUrl || '');
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const imageRef = useRef<HTMLImageElement>(null);
  const queryClient = useQueryClient();

  const { data: classrooms } = useQuery({
    queryKey: ['buildingClassrooms', building.id],
    queryFn: () => api.get<Classroom[]>(`/buildings/${building.id}/classrooms`),
  });

  // Load existing positions
  useEffect(() => {
    if (classrooms) {
      const loaded: Record<string, Position> = {};
      for (const c of classrooms) {
        if (c.mapPositionX != null && c.mapPositionY != null) {
          loaded[c.id] = { classroomId: c.id, x: c.mapPositionX, y: c.mapPositionY };
        }
      }
      setPositions(loaded);
    }
  }, [classrooms]);

  const uploadMutation = useMutation({
    mutationFn: async (file: File) => {
      const fd = new FormData();
      fd.append('file', file);
      const token = useAuthStore.getState().token;
      const res = await fetch('/api/upload/floorplan', {
        method: 'POST',
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        body: fd,
      });
      if (!res.ok) throw new Error('Upload failed');
      return (await res.text()).replace(/^"|"$/g, '');
    },
    onSuccess: async (url) => {
      await api.put(`/buildings/${building.id}/floorplan`, { floorPlanUrl: url });
      setFloorPlanUrl(url);
      queryClient.invalidateQueries({ queryKey: ['adminBuildings'] });
    },
  });

  const savePositionsMutation = useMutation({
    mutationFn: (positionsList: Position[]) =>
      api.put(`/buildings/${building.id}/classroom-positions`, { positions: positionsList }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['buildingClassrooms', building.id] });
    },
  });

  const handleImageClick = useCallback((e: React.MouseEvent<HTMLDivElement>) => {
    if (!selectedClassroom || !imageRef.current) return;

    const rect = imageRef.current.getBoundingClientRect();
    const x = Math.round(((e.clientX - rect.left) / rect.width) * 100);
    const y = Math.round(((e.clientY - rect.top) / rect.height) * 100);

    setPositions((prev) => ({
      ...prev,
      [selectedClassroom]: { classroomId: selectedClassroom, x, y },
    }));
  }, [selectedClassroom]);

  const handleRemovePosition = (classroomId: string) => {
    setPositions((prev) => {
      const next = { ...prev };
      delete next[classroomId];
      return next;
    });
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setUploading(true);
      uploadMutation.mutate(file, {
        onSettled: () => setUploading(false),
      });
    }
  };

  const sortedClassrooms = classrooms
    ?.filter((c) => c.name.toLowerCase().includes(search.toLowerCase()))
    .sort((a, b) => a.name.localeCompare(b.name));

  return (
    <div className="space-y-4">
      {/* Upload section */}
      <div className="flex items-center gap-4">
        <input
          ref={fileInputRef}
          type="file"
          accept=".pdf,.png,.jpg,.jpeg,.webp"
          className="hidden"
          onChange={handleFileChange}
        />
        <Button variant="outline" onClick={() => fileInputRef.current?.click()} loading={uploading}>
          <Upload className="h-4 w-4 mr-2" />
          {floorPlanUrl ? 'Cambiar plano' : 'Subir plano'}
        </Button>
        {floorPlanUrl && (
          <Button variant="outline" onClick={() => fileInputRef.current?.click()} loading={uploading}>
            Reemplazar
          </Button>
        )}
        {floorPlanUrl && (
          <Button
            variant="outline"
            onClick={() => {
              setFloorPlanUrl('');
              api.put(`/buildings/${building.id}/floorplan`, { floorPlanUrl: null });
            }}
          >
            <X className="h-4 w-4 mr-2" />
            Quitar plano
          </Button>
        )}
      </div>

      {!floorPlanUrl ? (
        <div className="flex items-center justify-center h-64 bg-primary-50 rounded-lg border-2 border-dashed border-primary-200">
          <p className="text-primary-400">Subí un PDF o imagen del plano del edificio</p>
        </div>
      ) : (
        <div className="flex gap-6">
          {/* Interactive floor plan */}
          <div className="flex-1 relative">
            {selectedClassroom && (
              <div className="absolute top-2 left-2 z-10 bg-blue-600 text-white text-xs px-2 py-1 rounded shadow">
                Click en el plano para colocar &quot;{classrooms?.find((c) => c.id === selectedClassroom)?.name}&quot;
                <button
                  className="ml-2 text-white/80 hover:text-white"
                  onClick={() => setSelectedClassroom(null)}
                >
                  <X className="h-3 w-3 inline" />
                </button>
              </div>
            )}
            <div
              className="relative inline-block cursor-crosshair"
              onClick={handleImageClick}
            >
              <img
                ref={imageRef}
                src={floorPlanUrl}
                alt={`Plano de ${building.name}`}
                className="max-w-full h-auto rounded-lg border"
                draggable={false}
              />
              {/* Render markers */}
              {Object.entries(positions).map(([classroomId, pos]) => {
                const classroom = classrooms?.find((c) => c.id === classroomId);
                return (
                  <div
                    key={classroomId}
                    className="absolute flex items-center gap-1 cursor-grab active:cursor-grabbing"
                    style={{ left: `${pos.x}%`, top: `${pos.y}%`, transform: 'translate(-50%, -100%)' }}
                    title={classroom?.name}
                  >
                    <div className="bg-blue-600 text-white text-xs font-medium px-2 py-0.5 rounded shadow whitespace-nowrap flex items-center gap-1">
                      <MapPin className="h-3 w-3" />
                      {classroom?.name || '?'}
                      <button
                        className="text-white/60 hover:text-white ml-0.5"
                        onClick={(e) => {
                          e.stopPropagation();
                          handleRemovePosition(classroomId);
                        }}
                      >
                        <X className="h-3 w-3" />
                      </button>
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Classroom list */}
          <div className="w-64 flex-shrink-0">
            <div className="relative mb-3">
              <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
              <input
                placeholder="Buscar aula..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full h-9 pl-8 pr-3 rounded-lg border border-primary-200 bg-white text-sm focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary-500"
              />
            </div>
            <div className="space-y-1 max-h-[500px] overflow-y-auto">
              {sortedClassrooms?.map((c) => {
                const hasPosition = positions[c.id] != null;
                return (
                  <button
                    key={c.id}
                    className={`w-full text-left px-3 py-2 rounded-md text-sm flex items-center justify-between gap-2 transition-colors ${
                      selectedClassroom === c.id
                        ? 'bg-blue-100 text-blue-700'
                        : hasPosition
                          ? 'bg-green-50 text-green-700'
                          : 'hover:bg-primary-50 text-primary-700'
                    }`}
                    onClick={() => setSelectedClassroom(c.id)}
                  >
                    <span className="truncate">{c.name}</span>
                    {hasPosition ? (
                      <MapPin className="h-3.5 w-3.5 flex-shrink-0 text-green-500" />
                    ) : (
                      <div className="h-3.5 w-3.5 flex-shrink-0 rounded-full border-2 border-dashed border-primary-300" />
                    )}
                  </button>
                );
              })}
              {(!sortedClassrooms || sortedClassrooms.length === 0) && (
                <p className="text-sm text-primary-400 text-center py-4">
                  {search ? 'Sin resultados' : 'Sin aulas'}
                </p>
              )}
            </div>
          </div>
        </div>
      )}

      {/* Save button */}
      {floorPlanUrl && Object.keys(positions).length > 0 && (
        <div className="flex justify-end">
          <Button onClick={() => savePositionsMutation.mutate(Object.values(positions))} loading={savePositionsMutation.isPending}>
            <Save className="h-4 w-4 mr-2" />
            Guardar posiciones
          </Button>
        </div>
      )}
    </div>
  );
}
