import axios from 'axios';
import { useAuthStore } from './store';

const api = axios.create({
  baseURL: 'http://localhost:5073/api',
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const refreshToken = useAuthStore.getState().refreshToken;
        if (!refreshToken) throw new Error('No refresh token');
        
        const response = await axios.post('http://localhost:5073/api/auth/refresh', {
          refreshToken,
        });
        
        const { accessToken, refreshToken: newRefreshToken, user } = response.data;
        useAuthStore.getState().setAuth(accessToken, newRefreshToken, user);
        
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return axios(originalRequest);
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
