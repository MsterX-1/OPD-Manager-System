import { Navigate, Outlet } from 'react-router-dom';
import { useAuthStore } from '../store';
import Layout from './Layout';

export function PrivateRoute() {
  const accessToken = useAuthStore(state => state.accessToken);
  return accessToken ? (
    <Layout />
  ) : (
    <Navigate to="/login" replace />
  );
}

export function AdminRoute() {
  const { accessToken, user } = useAuthStore(state => state);
  
  if (!accessToken) return <Navigate to="/login" replace />;
  if (user?.role !== 'Admin') return <Navigate to="/" replace />;
  
  return <Outlet />;
}
