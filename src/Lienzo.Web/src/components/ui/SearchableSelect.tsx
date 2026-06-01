import { useState, useRef, useEffect, useMemo } from 'react';
import { cn } from '@/lib/utils';
import { Search, X, ChevronDown } from 'lucide-react';

interface SearchableSelectProps {
  label?: string;
  placeholder?: string;
  value: string;
  onChange: (value: string) => void;
  options: { value: string; label: string }[];
  error?: string;
}

export function SearchableSelect({ label, placeholder = 'Buscar...', value, onChange, options, error }: SearchableSelectProps) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);
  const containerRef = useRef<HTMLDivElement>(null);

  const selected = useMemo(() => options.find((o) => o.value === value), [options, value]);

  const filtered = useMemo(() => {
    if (!search) return options;
    const q = search.toLowerCase();
    return options.filter((o) => o.label.toLowerCase().includes(q));
  }, [options, search]);

  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  return (
    <div className="w-full" ref={containerRef}>
      {label && (
        <label className="block text-sm font-medium text-primary-700 mb-1.5">{label}</label>
      )}
      <div className="relative">
        {selected && !open ? (
          <div
            onClick={() => { setOpen(true); setSearch(''); setTimeout(() => inputRef.current?.focus(), 0); }}
            className="flex h-10 w-full cursor-pointer items-center rounded-lg border border-primary-200 bg-white px-3 py-2 pr-8 text-sm"
          >
            <span className="truncate">{selected.label}</span>
            <button
              type="button"
              onClick={(e) => { e.stopPropagation(); onChange(''); setSearch(''); }}
              className="absolute right-8 top-1/2 -translate-y-1/2 text-primary-400 hover:text-primary-600"
            >
              <X className="h-4 w-4" />
            </button>
            <ChevronDown className="absolute right-2 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400 pointer-events-none" />
          </div>
        ) : (
          <div className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-primary-400" />
            <input
              ref={inputRef}
              type="text"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onFocus={() => setOpen(true)}
              placeholder={selected ? selected.label : placeholder}
              className={cn(
                'flex h-10 w-full rounded-lg border border-primary-200 bg-white pl-9 pr-8 py-2 text-sm outline-none focus-visible:ring-2 focus-visible:ring-primary-500 focus-visible:ring-offset-2',
                error && 'border-red-500 focus-visible:ring-red-500'
              )}
            />
            {search && (
              <button
                type="button"
                onClick={() => setSearch('')}
                className="absolute right-2 top-1/2 -translate-y-1/2 text-primary-400 hover:text-primary-600"
              >
                <X className="h-4 w-4" />
              </button>
            )}
          </div>
        )}

        {open && (
          <div className="absolute z-50 mt-1 max-h-60 w-full overflow-auto rounded-lg border border-primary-200 bg-white shadow-lg">
            {filtered.length === 0 ? (
              <p className="px-3 py-2 text-sm text-primary-500">Sin resultados</p>
            ) : (
              filtered.map((opt) => (
                <button
                  key={opt.value}
                  type="button"
                  onClick={() => { onChange(opt.value); setSearch(''); setOpen(false); }}
                  className={cn(
                    'w-full px-3 py-2 text-left text-sm hover:bg-primary-50 transition-colors',
                    opt.value === value && 'bg-accent-50 text-accent-700 font-medium'
                  )}
                >
                  {opt.label}
                </button>
              ))
            )}
          </div>
        )}
      </div>
      {error && <p className="mt-1 text-xs text-red-600">{error}</p>}
    </div>
  );
}
