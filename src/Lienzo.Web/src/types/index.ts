export enum UserRole {
  Student = 'Student',
  Teacher = 'Teacher',
  Admin = 'Admin',
}

export enum ReservationStatus {
  Pending = 'Pending',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Cancelled = 'Cancelled',
  Completed = 'Completed',
}

export enum ClassroomType {
  Lecture = 'Lecture',
  Laboratory = 'Laboratory',
  Workshop = 'Workshop',
  Seminar = 'Seminar',
  Auditorium = 'Auditorium',
  Office = 'Office',
}

export enum AnnouncementType {
  General = 'General',
  Cancellation = 'Cancellation',
  Postponement = 'Postponement',
  Emergency = 'Emergency',
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  avatarUrl?: string;
  createdAt: string;
}

export interface Building {
  id: string;
  name: string;
  code: string;
  address?: string;
  floors: number;
  codigoExterno?: number;
  floorPlanUrl?: string;
  createdAt: string;
  updatedAt: string;
}

export interface Classroom {
  id: string;
  name: string;
  code?: string;
  buildingId: string;
  buildingName?: string;
  floor: number;
  capacity: number;
  type: ClassroomType;
  description?: string;
  features: string[];
  imageUrl?: string;
  mapPositionX?: number;
  mapPositionY?: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Reservation {
  id: string;
  classroomId: string;
  classroomName?: string;
  buildingName?: string;
  userId: string;
  userName?: string;
  title: string;
  description?: string;
  date: string;
  startTime: string;
  endTime: string;
  status: ReservationStatus;
  createdAt: string;
  updatedAt: string;
  recurringGroupId?: string;
  recurrenceRule?: string;
  actividadId?: string;
  actividadNombre?: string;
  actividadPeriodo?: string;
  actividadCarrera?: string;
  actividadDocentes?: string;
  actividadDocenteIds?: string[];
}

export interface Announcement {
  id: string;
  title: string;
  body: string;
  type: AnnouncementType;
  userId: string;
  userName?: string;
  targetRole?: UserRole;
  isRead: boolean;
  createdAt: string;
}

export interface Notification {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: string;
  isRead: boolean;
  referenceId?: string;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  role: UserRole;
}

export interface PaginatedResponse<T> {
  value: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  statusCode: number;
  errors?: Record<string, string[]>;
}

export interface ReservationConflict {
  hasConflict: boolean;
  conflictingReservations: Reservation[];
}

export interface MaintenanceBlock {
  id: string;
  classroomId: string;
  classroomName: string;
  buildingName?: string;
  startTime: string;
  endTime: string;
  reason: string;
  createdBy: string;
  createdAt: string;
}

export interface ScheduleResponse {
  reservations: Reservation[];
  maintenanceBlocks: MaintenanceBlock[];
}

export interface Actividad {
  id: string;
  nombre: string;
  codigoMateria: string;
  periodoNombre?: string;
  carreraNombre?: string;
  aulaId?: string;
  aulaNombre?: string;
  diaSemana?: string;
  horaInicio?: string;
  horaFin?: string;
  docenteIds: string[];
  docentesNombres?: string;
}

export interface DashboardStats {
  totalClassrooms: number;
  activeClassrooms: number;
  reservationsToday: number;
  pendingApprovals: number;
  occupancyRate: number;
  totalUsers: number;
}
