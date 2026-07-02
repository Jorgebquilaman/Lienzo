import { useState, useRef, useCallback, useEffect } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { Upload, Save, MapPin, X, CheckCircle2, Circle } from 'lucide-react';
import { api } from '@/lib/api';
import { useAuthStore } from '@/stores/authStore';
import { Button } from '@/components/ui/Button';
import { Select } from '@/components/ui/Select';
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
  const [selectedClassroomId, setSelectedClassroomId] = useState('');
  const [floorPlanUrl, setFloorPlanUrl] = useState(building.floorPlanUrl || '');
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const imageRef = useRef<HTMLImageElement>(null);
  const queryClient = useQueryClient();

  const { data: classrooms } = useQuery({
    queryKey: ['buildingClassrooms', building.id],
    queryFn: () => api.get<Classroom[]>(`/buildings/${building.id}/classrooms`),
  });

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
    if (!selectedClassroomId || !imageRef.current) return;

    const img = imageRef.current;
    const rect = img.getBoundingClientRect();
    const x = Math.round(((e.clientX - rect.left) / rect.width) * 100);
    const y = Math.round(((e.clientY - rect.top) / rect.height) * 100);

    setPositions((prev) => ({
      ...prev,
      [selectedClassroomId]: { classroomId: selectedClassroomId, x, y },
    }));
    setSelectedClassroomId('');
  }, [selectedClassroomId]);

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

  const selectedClassroom = classrooms?.find((c) => c.id === selectedClassroomId);
  const placedClassrooms = classrooms?.filter((c) => positions[c.id] != null) ?? [];

  const classroomOptions = (classrooms ?? [])
    .filter((c) => positions[c.id] == null)
    .sort((a, b) => a.name.localeCompare(b.name))
    .map((c) => ({ value: c.id, label: c.name }));

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
        <div className="space-y-4">
          {/* Classroom selector */}
          <div className="flex items-start gap-4">
            <div className="flex-1">
              <Select
                label="Seleccioná un aula para colocar en el plano"
                placeholder="— Elegir aula —"
                options={classroomOptions}
                value={selectedClassroomId}
                onValueChange={setSelectedClassroomId}
              />
            </div>
            <div className="min-w-[180px] pt-6">
              {selectedClassroom && (
                <div className="text-sm text-blue-700 bg-blue-50 rounded-lg px-3 py-2 flex items-center gap-2">
                  <MapPin className="h-4 w-4" />
                  Colocando: {selectedClassroom.name}
                </div>
              )}
            </div>
          </div>

          {/* Placed classrooms summary */}
          {placedClassrooms.length > 0 && (
            <div className="flex flex-wrap items-center gap-2">
              <span className="text-xs text-primary-500 font-medium">Ubicadas:</span>
              {placedClassrooms.map((c) => (
                <span
                  key={c.id}
                  className="inline-flex items-center gap-1 text-xs bg-green-50 text-green-700 rounded-full px-2.5 py-1"
                >
                  <CheckCircle2 className="h-3 w-3" />
                  {c.name}
                  <button
                    className="text-green-400 hover:text-green-600 ml-0.5"
                    onClick={() => handleRemovePosition(c.id)}
                  >
                    <X className="h-3 w-3" />
                  </button>
                </span>
              ))}
            </div>
          )}

          {/* Floor plan image fit to viewport */}
          <div className="relative bg-primary-50 rounded-lg border overflow-hidden" style={{ height: 'calc(100vh - 340px)' }}>
            {selectedClassroomId && (
              <div className="absolute top-2 left-2 z-10 bg-blue-600 text-white text-xs px-2.5 py-1.5 rounded shadow">
                Hacé click en el plano para colocar &quot;{selectedClassroom?.name}&quot;
              </div>
            )}
            <div
              className="relative w-full h-full flex items-start justify-center cursor-crosshair"
              onClick={handleImageClick}
            >
              <img
                ref={imageRef}
                src={floorPlanUrl}
                alt={`Plano de ${building.name}`}
                className="max-w-full max-h-full w-auto h-auto rounded-lg"
                draggable={false}
              />
              {Object.entries(positions).map(([classroomId, pos]) => {
                const classroom = classrooms?.find((c) => c.id === classroomId);
                return (
                  <div
                    key={classroomId}
                    className="absolute flex items-center gap-1"
                    style={{ left: `${pos.x}%`, top: `${pos.y}%`, transform: 'translate(-50%, -50%)' }}
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

          {/* Save button */}
          {Object.keys(positions).length > 0 && (
            <div className="flex justify-end">
              <Button onClick={() => savePositionsMutation.mutate(Object.values(positions))} loading={savePositionsMutation.isPending}>
                <Save className="h-4 w-4 mr-2" />
                Guardar posiciones
              </Button>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
