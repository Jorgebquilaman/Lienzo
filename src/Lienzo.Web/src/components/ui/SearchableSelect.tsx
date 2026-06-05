import { useState, useRef, useEffect, useMemo } from 'react';
import { Search, ChevronDown } from 'lucide-react';

interface Option {
  value: string;
  label: string;
}

interface SearchableSelectProps {
  label?: string;
  options: Option[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
  required?: boolean;
  disabled?: boolean;
}

export function SearchableSelect({
  label, options, value, onChange, placeholder, required, disabled,
}: SearchableSelectProps) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState('');
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const selectedLabel = options.find((o) => o.value === value)?.label || '';

  const filtered = useMemo(() => {
    const q = search.toLowerCase();
    return options.filter((o) => o.label.toLowerCase().includes(q));
  }, [options, search]);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(e.target as Node)) {
        setOpen(false);
      }
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, []);

  const handleSelect = (opt: Option) => {
    onChange(opt.value);
    setOpen(false);
    setSearch('');
  };

  return (
    <div ref={containerRef} className="relative">
      {label && <label className="block text-sm font-medium text-primary-700 mb-1">{label}</label>}
      <div
        className={`flex items-center h-10 w-full rounded-lg border bg-white px-3 text-sm cursor-pointer ${
          open ? 'border-primary-500 ring-2 ring-primary-200' : 'border-primary-200'
        } ${disabled ? 'opacity-50 pointer-events-none' : ''}`}
        onClick={() => { if (!disabled) { setOpen(!open); setTimeout(() => inputRef.current?.focus(), 50); } }}
      >
        <span className={selectedLabel ? 'text-primary-800 flex-1' : 'text-primary-400 flex-1'}>
          {selectedLabel || placeholder || 'Seleccionar...'}
        </span>
        <ChevronDown className={`h-4 w-4 text-primary-400 transition-transform ${open ? 'rotate-180' : ''}`} />
      </div>
      {open && (
        <div className="absolute z-50 top-full mt-1 left-0 right-0 bg-white border border-primary-200 rounded-lg shadow-lg overflow-hidden">
          <div className="flex items-center gap-2 px-3 py-2 border-b border-primary-100">
            <Search className="h-4 w-4 text-primary-400 flex-shrink-0" />
            <input
              ref={inputRef}
              type="text"
              className="flex-1 text-sm outline-none bg-transparent text-primary-800 placeholder:text-primary-400"
              placeholder="Buscar..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === 'Enter' && filtered.length === 1) {
                  e.preventDefault();
                  handleSelect(filtered[0]);
                }
              }}
            />
          </div>
          <div className="max-h-48 overflow-y-auto">
            {filtered.length === 0 ? (
              <div className="px-3 py-6 text-center text-sm text-primary-400">Sin resultados</div>
            ) : (
              filtered.map((opt) => (
                <div
                  key={opt.value}
                  className={`px-3 py-2 text-sm cursor-pointer hover:bg-primary-50 transition-colors ${
                    opt.value === value ? 'bg-accent-50 text-accent-700 font-medium' : 'text-primary-700'
                  }`}
                  onClick={() => handleSelect(opt)}
                >
                  {opt.label}
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
}
