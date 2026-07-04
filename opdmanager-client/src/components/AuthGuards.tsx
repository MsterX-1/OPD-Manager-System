import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store';
import Layout from './Layout';

export function PrivateRoute() {
  const user = useAuthStore(state => state.user);
  return user ? (
    <Layout />
  ) : (
    <Navigate to="/login" replace />
  );
}

export function AdminRoute() {
  const user = useAuthStore(state => state.user);
  
  if (!user) return <Navigate to="/login" replace />;
  if (user?.role !== 'Admin') return <Navigate to="/" replace />;
  
  return <Outlet />;
}
