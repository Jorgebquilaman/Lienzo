import { useState, useEffect } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { Star } from 'lucide-react';
import { api } from '@/lib/api';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription, DialogBody, DialogFooter } from '@/components/ui/Dialog';
import { Button } from '@/components/ui/Button';
import { Textarea } from '@/components/ui/Textarea';
import { StarRating } from '@/components/ui/StarRating';

interface SurveyModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  reservationId: string;
  classroomName: string;
}

export function SurveyModal({ open, onOpenChange, reservationId, classroomName }: SurveyModalProps) {
  const [condition, setCondition] = useState(0);
  const [equipment, setEquipment] = useState(0);
  const [cleanliness, setCleanliness] = useState(0);
  const [comment, setComment] = useState('');
  const [error, setError] = useState('');
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!open) {
      setCondition(0);
      setEquipment(0);
      setCleanliness(0);
      setComment('');
      setError('');
    }
  }, [open]);

  const mutation = useMutation({
    mutationFn: (body: { reservationId: string; conditionRating: number; equipmentRating: number; cleanlinessRating: number; comment?: string }) =>
      api.post('/surveys', body),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['myReservations'] });
      queryClient.invalidateQueries({ queryKey: ['mySurveys'] });
      queryClient.invalidateQueries({ queryKey: ['pendingSurveys'] });
      onOpenChange(false);
    },
    onError: (err: Error) => {
      setError(err.message || 'Error al enviar la evaluación');
      queryClient.invalidateQueries({ queryKey: ['myReservations'] });
      queryClient.invalidateQueries({ queryKey: ['mySurveys'] });
      queryClient.invalidateQueries({ queryKey: ['pendingSurveys'] });
    },
  });

  const canSubmit = condition > 0 && equipment > 0 && cleanliness > 0;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (condition === 0 || equipment === 0 || cleanliness === 0) return;
    setError('');
    mutation.mutate({
      reservationId,
      conditionRating: condition,
      equipmentRating: equipment,
      cleanlinessRating: cleanliness,
      comment: comment.trim() || undefined,
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Evaluar aula</DialogTitle>
          <DialogDescription>
            Califica tu experiencia en <strong>{classroomName}</strong>
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit}>
          <DialogBody className="space-y-5">
            {error && (
              <div className="bg-red-50 border border-red-200 text-red-700 text-sm rounded-md px-3 py-2">
                {error}
              </div>
            )}
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <label className="text-sm font-medium text-primary-700">Condiciones del aula</label>
                <StarRating value={condition} onChange={setCondition} size="md" />
              </div>
              <div className="flex items-center justify-between">
                <label className="text-sm font-medium text-primary-700">Equipamiento</label>
                <StarRating value={equipment} onChange={setEquipment} size="md" />
              </div>
              <div className="flex items-center justify-between">
                <label className="text-sm font-medium text-primary-700">Limpieza</label>
                <StarRating value={cleanliness} onChange={setCleanliness} size="md" />
              </div>
            </div>
            <Textarea
              label="Comentario (opcional)"
              placeholder="Comparte tu experiencia..."
              value={comment}
              onChange={e => setComment(e.target.value)}
              rows={3}
            />
          </DialogBody>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>Cancelar</Button>
            <Button type="submit" disabled={!canSubmit} loading={mutation.isPending}>
              <Star className="h-4 w-4 mr-1.5" />
              Enviar evaluación
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
