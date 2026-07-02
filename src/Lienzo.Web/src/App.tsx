import { useEffect } from 'react';
import { Routes, Route } from 'react-router-dom';
import { useAuthStore } from '@/stores/authStore';
import { useSignalR } from '@/hooks/useSignalR';
import { AppShell } from '@/components/layout/AppShell';
import { ProtectedRoute, AdminRoute } from '@/components/layout/ProtectedRoute';
import LoginPage from '@/pages/LoginPage';
import RegisterPage from '@/pages/RegisterPage';
import ForgotPasswordPage from '@/pages/ForgotPasswordPage';
import ResetPasswordPage from '@/pages/ResetPasswordPage';
import DashboardPage from '@/pages/DashboardPage';
import ClassroomBrowser from '@/pages/ClassroomBrowser';
import ClassroomDetail from '@/pages/ClassroomDetail';
import CampusMap from '@/components/campus/CampusMap';
import MyReservations from '@/pages/MyReservations';
import SchedulePage from '@/pages/SchedulePage';
import AdminReservations from '@/pages/AdminReservations';
import AdminClassrooms from '@/pages/AdminClassrooms';
import AdminBuildings from '@/pages/AdminBuildings';
import AdminUsers from '@/pages/AdminUsers';
import AdminHolidays from '@/pages/AdminHolidays';
import AdminRecesos from '@/pages/AdminRecesos';
import AdminPeriodos from '@/pages/AdminPeriodos';
import AdminCarreras from '@/pages/AdminCarreras';
import AdminActividades from '@/pages/AdminActividades';
import AdminReports from '@/pages/AdminReports';
import AdminMaintenance from '@/pages/AdminMaintenance';
import AdminSurveys from '@/pages/AdminSurveys';
import AdminSettings from '@/pages/AdminSettings';
import AdminBedelia from '@/pages/AdminBedelia';
import AdminAccesorios from '@/pages/AdminAccesorios';
import AnnouncementsPage from '@/pages/AnnouncementsPage';
import MySurveys from '@/pages/MySurveys';
import ProfilePage from '@/pages/ProfilePage';
import AsistenciaDocente from '@/pages/AsistenciaDocente';
import AsistenciaAlumno from '@/pages/AsistenciaAlumno';
import AsistenciasList from '@/pages/AsistenciasList';
import TVDashboard from '@/pages/TVDashboard';

export default function App() {
  const checkAuth = useAuthStore((s) => s.checkAuth);
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);

  useEffect(() => {
    checkAuth();
  }, [checkAuth]);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const signalR = useSignalR();

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password" element={<ResetPasswordPage />} />
      <Route path="/tv" element={<TVDashboard />} />

      <Route element={<ProtectedRoute />}>
        <Route
          path="/"
          element={
            <AppShell>
              <DashboardPage />
            </AppShell>
          }
        />
        <Route
          path="/classrooms"
          element={
            <AppShell>
              <ClassroomBrowser />
            </AppShell>
          }
        />
        <Route
          path="/mapa"
          element={
            <AppShell>
              <CampusMap />
            </AppShell>
          }
        />
        <Route
          path="/classrooms/:id"
          element={
            <AppShell>
              <ClassroomDetail />
            </AppShell>
          }
        />
        <Route
          path="/reservations"
          element={
            <AppShell>
              <MyReservations />
            </AppShell>
          }
        />
        <Route
          path="/schedule"
          element={
            <AppShell>
              <SchedulePage />
            </AppShell>
          }
        />
        <Route
          path="/announcements"
          element={
            <AppShell>
              <AnnouncementsPage />
            </AppShell>
          }
        />
        <Route
          path="/profile"
          element={
            <AppShell>
              <ProfilePage />
            </AppShell>
          }
        />
        <Route
          path="/surveys"
          element={
            <AppShell>
              <MySurveys />
            </AppShell>
          }
        />
        <Route
          path="/asistencia/:claseId"
          element={
            <AppShell>
              <AsistenciaDocente />
            </AppShell>
          }
        />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route
          path="/asistencia/marcar"
          element={<AsistenciaAlumno />}
        />
      </Route>

      <Route element={<ProtectedRoute />}>
        <Route
          path="/asistencias"
          element={
            <AppShell>
              <AsistenciasList />
            </AppShell>
          }
        />
      </Route>

      <Route element={<AdminRoute />}>
        <Route
          path="/admin/reservations"
          element={
            <AppShell>
              <AdminReservations />
            </AppShell>
          }
        />
        <Route
          path="/admin/classrooms"
          element={
            <AppShell>
              <AdminClassrooms />
            </AppShell>
          }
        />
          <Route
          path="/admin/buildings"
          element={
            <AppShell>
              <AdminBuildings />
            </AppShell>
          }
        />
        <Route
          path="/admin/users"
          element={
            <AppShell>
              <AdminUsers />
            </AppShell>
          }
        />
        <Route
            path="/admin/holidays"
            element={
              <AppShell>
                <AdminHolidays />
              </AppShell>
            }
          />
          <Route
            path="/admin/recesos"
            element={
              <AppShell>
                <AdminRecesos />
              </AppShell>
            }
          />
        <Route
          path="/admin/periodos"
          element={
            <AppShell>
              <AdminPeriodos />
            </AppShell>
          }
        />
        <Route
          path="/admin/carreras"
          element={
            <AppShell>
              <AdminCarreras />
            </AppShell>
          }
        />
        <Route
          path="/admin/actividades"
          element={
            <AppShell>
              <AdminActividades />
            </AppShell>
          }
        />
        <Route
          path="/admin/reports"
          element={
            <AppShell>
              <AdminReports />
            </AppShell>
          }
        />
        <Route
          path="/admin/maintenance"
          element={
            <AppShell>
              <AdminMaintenance />
            </AppShell>
          }
        />
        <Route
          path="/admin/surveys"
          element={
            <AppShell>
              <AdminSurveys />
            </AppShell>
          }
        />
        <Route
          path="/admin/settings"
          element={
            <AppShell>
              <AdminSettings />
            </AppShell>
          }
        />
        <Route
          path="/admin/bedelia"
          element={
            <AppShell>
              <AdminBedelia />
            </AppShell>
          }
        />
        <Route
          path="/admin/accessories"
          element={
            <AppShell>
              <AdminAccesorios />
            </AppShell>
          }
        />
      </Route>
    </Routes>
  );
}
