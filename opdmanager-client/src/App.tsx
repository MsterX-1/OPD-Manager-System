import React, { Suspense } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { PrivateRoute, AdminRoute, DoctorRoute, DepartmentStaffRoute } from './components/AuthGuards';

const Login = React.lazy(() => import('./pages/Login'));
const Dashboard = React.lazy(() => import('./pages/Dashboard'));
const QueueMonitor = React.lazy(() => import('./pages/QueueMonitor'));
const Appointments = React.lazy(() => import('./pages/Appointments'));
const Patients = React.lazy(() => import('./pages/Patients'));
const Doctors = React.lazy(() => import('./pages/Doctors'));
const Issues = React.lazy(() => import('./pages/Issues'));
const Reports = React.lazy(() => import('./pages/Reports'));
const Users = React.lazy(() => import('./pages/Users'));
const Settings = React.lazy(() => import('./pages/Settings'));
const Analytics = React.lazy(() => import('./pages/Analytics'));
const DoctorPortal = React.lazy(() => import('./pages/DoctorPortal'));
const Encounter = React.lazy(() => import('./pages/Encounter'));
const PatientChart = React.lazy(() => import('./pages/PatientChart'));
const DepartmentManagement = React.lazy(() => import('./pages/DepartmentManagement'));
const DepartmentPortal = React.lazy(() => import('./pages/DepartmentPortal'));

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 1, // 1 minute
      refetchOnWindowFocus: false,
    },
  },
});

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Suspense fallback={<div style={{ display: 'flex', height: '100vh', alignItems: 'center', justifyContent: 'center', color: 'var(--text-muted)' }}>Loading...</div>}>
          <Routes>
            <Route path="/login" element={<Login />} />
            
            <Route element={<PrivateRoute />}>
              <Route path="/" element={<Dashboard />} />
              <Route path="/queue" element={<QueueMonitor />} />
              <Route path="/appointments" element={<Appointments />} />
              <Route path="/patients" element={<Patients />} />
              <Route path="/patient-chart/:patientId" element={<PatientChart />} />
              <Route path="/issues" element={<Issues />} />
              <Route path="/reports" element={<Reports />} />
              
              {/* Doctor Routes */}
              <Route element={<DoctorRoute />}>
                <Route path="/doctor-portal" element={<DoctorPortal />} />
                <Route path="/encounter/:appointmentId" element={<Encounter />} />
              </Route>

              {/* Department Staff Routes */}
              <Route element={<DepartmentStaffRoute />}>
                <Route path="/department-portal" element={<DepartmentPortal />} />
              </Route>

              {/* Admin Only Routes */}
              <Route element={<AdminRoute />}>
                <Route path="/doctors" element={<Doctors />} />
                <Route path="/departments" element={<DepartmentManagement />} />
                <Route path="/users" element={<Users />} />
                <Route path="/settings" element={<Settings />} />
                <Route path="/analytics" element={<Analytics />} />
              </Route>
            </Route>
            
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        </Suspense>
      </BrowserRouter>
      
      <Toaster 
        position="top-right"
        toastOptions={{
          style: {
            background: 'var(--bg-tertiary)',
            color: 'var(--text-primary)',
            border: '1px solid var(--border-color)',
          }
        }}
      />
    </QueryClientProvider>
  );
}

export default App;
