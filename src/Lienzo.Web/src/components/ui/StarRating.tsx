import { useState } from 'react';
import { Star } from 'lucide-react';
import { cn } from '@/lib/utils';

interface StarRatingProps {
  value: number;
  onChange?: (value: number) => void;
  max?: number;
  size?: 'sm' | 'md' | 'lg';
  readonly?: boolean;
}

export function StarRating({ value, onChange, max = 5, size = 'md', readonly = false }: StarRatingProps) {
  const [hover, setHover] = useState(0);

  const sizeClass = size === 'sm' ? 'h-4 w-4' : size === 'lg' ? 'h-7 w-7' : 'h-5 w-5';

  const handleClick = (star: number) => {
    if (readonly || !onChange) return;
    const half = star - 0.5;
    const newVal = value === star ? half : value === half ? star : star;
    onChange(newVal);
  };

  const stars = Array.from({ length: max }, (_, i) => i + 1);

  return (
    <div className="flex items-center gap-0.5">
      {stars.map((star) => {
        const isFull = (hover || value) >= star;
        const isHalf = !isFull && (hover || value) >= star - 0.5;
        return (
          <button
            key={star}
            type="button"
            disabled={readonly}
            className={cn(
              'relative transition-colors',
              readonly ? 'cursor-default' : 'cursor-pointer hover:scale-110',
              (isFull || isHalf) ? 'text-yellow-400' : 'text-gray-300'
            )}
            onMouseEnter={() => !readonly && setHover(star)}
            onMouseLeave={() => !readonly && setHover(0)}
            onClick={() => handleClick(star)}
            title={`${star} estrella${star !== 1 ? 's' : ''}`}
          >
            <Star
              className={cn(sizeClass, 'transition-all', isHalf && 'absolute inset-0')}
              fill={isFull ? 'currentColor' : 'none'}
            />
            {isHalf && (
              <Star
                className={cn(sizeClass, 'absolute inset-0 text-yellow-400')}
                fill="currentColor"
                style={{ clipPath: 'inset(0 50% 0 0)' }}
              />
            )}
          </button>
        );
      })}
      <span className="ml-2 text-sm font-medium text-primary-600">
        {value > 0 ? value.toFixed(1) : '—'}
      </span>
    </div>
  );
}
