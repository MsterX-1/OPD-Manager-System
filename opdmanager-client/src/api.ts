import axios from 'axios';
import { useAuthStore } from './store';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5073';

const api = axios.create({
  baseURL: `${API_URL}/api`,
  withCredentials: true
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as any;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const response = await axios.post(`${API_URL}/api/auth/refresh`, {}, { withCredentials: true });
        
        const { user } = response.data;
        useAuthStore.getState().setAuth(user);
        
        return api(originalRequest);
      } catch (e) {
        useAuthStore.getState().logout();
        window.location.href = '/login';
        return Promise.reject(e);
      }
    }
    return Promise.reject(error);
  }
);

export default api;
