import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatDate(date: string | Date): string {
  return new Intl.DateTimeFormat('es-MX', {
    dateStyle: 'long',
  }).format(new Date(date));
}

export function formatTime(date: string | Date): string {
  return new Intl.DateTimeFormat('es-MX', {
    timeStyle: 'short',
  }).format(new Date(date));
}

export function formatDateTime(date: string | Date): string {
  return new Intl.DateTimeFormat('es-MX', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(new Date(date));
}

export function getStatusColor(status: string): string {
  const map: Record<string, string> = {
    Pending: 'bg-yellow-100 text-yellow-800 border-yellow-200',
    Approved: 'bg-green-100 text-green-800 border-green-200',
    Rejected: 'bg-red-100 text-red-800 border-red-200',
    Cancelled: 'bg-gray-100 text-gray-800 border-gray-200',
    Completed: 'bg-blue-100 text-blue-800 border-blue-200',
  };
  return map[status] ?? 'bg-gray-100 text-gray-800 border-gray-200';
}

export function getStatusLabel(status: string): string {
  const map: Record<string, string> = {
    Pending: 'Pendiente',
    Approved: 'Aprobada',
    Rejected: 'Rechazada',
    Cancelled: 'Cancelada',
    Completed: 'Completada',
  };
  return map[status] ?? status;
}

export function getAnnouncementTypeColor(type: string): string {
  const map: Record<string, string> = {
    Cancellation: 'text-red-600 bg-red-50',
    Postponement: 'text-yellow-600 bg-yellow-50',
    General: 'text-blue-600 bg-blue-50',
    Emergency: 'text-red-700 bg-red-100',
  };
  return map[type] ?? 'text-blue-600 bg-blue-50';
}

export function getClassroomTypeLabel(type: string): string {
  const map: Record<string, string> = {
    Lecture: 'Aula',
    Laboratory: 'Laboratorio',
    Workshop: 'Taller',
    Seminar: 'Seminario',
    Auditorium: 'Auditorio',
  };
  return map[type] ?? type;
}
