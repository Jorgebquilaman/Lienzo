import * as React from 'react';
import { cn } from '@/lib/utils';

interface TabsProps {
  value: string;
  onValueChange: (value: string) => void;
  children: React.ReactNode;
  className?: string;
}

function Tabs({ value, onValueChange, children, className }: TabsProps) {
  return (
    <div className={className} data-value={value}>
      {React.Children.map(children, (child) => {
        if (React.isValidElement(child)) {
          return React.cloneElement(child as React.ReactElement<any>, { _value: value, _onValueChange: onValueChange });
        }
        return child;
      })}
    </div>
  );
}

interface TabsListProps {
  children: React.ReactNode;
  className?: string;
  _value?: string;
  _onValueChange?: (value: string) => void;
}

function TabsList({ children, className, _value, _onValueChange }: TabsListProps) {
  return (
    <div className={cn('inline-flex h-10 items-center justify-center rounded-lg bg-primary-50 p-1', className)}>
      {React.Children.map(children, (child) => {
        if (React.isValidElement(child)) {
          return React.cloneElement(child as React.ReactElement<any>, { _value, _onValueChange });
        }
        return child;
      })}
    </div>
  );
}

interface TabsTriggerProps {
  value: string;
  children: React.ReactNode;
  className?: string;
  _value?: string;
  _onValueChange?: (value: string) => void;
}

function TabsTrigger({ value, children, className, _value, _onValueChange }: TabsTriggerProps) {
  const isActive = _value === value;
  return (
    <button
      type="button"
      role="tab"
      className={cn(
        'inline-flex items-center justify-center whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-medium transition-all',
        isActive
          ? 'bg-white text-primary-800 shadow-sm'
          : 'text-primary-500 hover:text-primary-700',
        className
      )}
      onClick={() => _onValueChange?.(value)}
    >
      {children}
    </button>
  );
}

interface TabsContentProps {
  value: string;
  children: React.ReactNode;
  className?: string;
  _value?: string;
}

function TabsContent({ value, children, className, _value }: TabsContentProps) {
  if (_value !== value) return null;
  return <div className={cn('mt-4', className)}>{children}</div>;
}

export { Tabs, TabsList, TabsTrigger, TabsContent };
