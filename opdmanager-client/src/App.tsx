import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { Toaster } from 'react-hot-toast';
import { PrivateRoute, AdminRoute, DoctorRoute, DepartmentStaffRoute } from './components/AuthGuards';

import Login from './pages/Login';
import Dashboard from './pages/Dashboard';
import QueueMonitor from './pages/QueueMonitor';
import Appointments from './pages/Appointments';
import Patients from './pages/Patients';
import Doctors from './pages/Doctors';
import Issues from './pages/Issues';
import Reports from './pages/Reports';
import Users from './pages/Users';
import Settings from './pages/Settings';
import Analytics from './pages/Analytics';
import DoctorPortal from './pages/DoctorPortal';
import Encounter from './pages/Encounter';
import PatientChart from './pages/PatientChart';
import DepartmentManagement from './pages/DepartmentManagement';
import DepartmentPortal from './pages/DepartmentPortal';

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
