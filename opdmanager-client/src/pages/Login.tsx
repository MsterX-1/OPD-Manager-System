import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store';
import api from '../api';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';

export default function Login() {
  const { t } = useTranslation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const setAuth = useAuthStore(state => state.setAuth);
  const navigate = useNavigate();

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const response = await api.post('/auth/login', {
        email,
        password
      });
      
      const { user } = response.data;
      setAuth(user);
      
      toast.success(t('Welcome back,') + ` ${user.fullName}`);
      navigate('/');
    } catch (error: any) {
      toast.error(error.response?.data?.message || t('Login failed'));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      padding: '1rem',
      background: 'var(--bg-primary)'
    }}>
      <div className="surface animate-fade-in" style={{
        padding: '3rem',
        width: '100%',
        maxWidth: '440px',
        textAlign: 'center',
        boxShadow: 'var(--shadow-lg)'
      }}>
        <div style={{ marginBottom: '2.5rem' }}>
          <h1 style={{
            fontSize: '2rem',
            color: 'var(--text-primary)',
            fontWeight: 700,
            marginBottom: '0.5rem'
          }}>OPD Manager</h1>
          <p style={{ color: 'var(--text-secondary)' }}>{t('Sign in to continue')}</p>
        </div>

        <form onSubmit={handleLogin} style={{ textAlign: 'start' }}>
          <div className="form-group">
            <label>{t('Email Address')}</label>
            <input 
              type="email" 
              required 
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="admin@opd.local"
            />
          </div>
          
          <div className="form-group" style={{ marginBottom: '2rem' }}>
            <label>{t('Password')}</label>
            <input 
              type="password" 
              required 
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
            />
          </div>

          <button 
            type="submit" 
            className="btn btn-primary" 
            style={{ width: '100%', padding: '0.875rem' }}
            disabled={loading}
          >
            {loading ? t('Signing in...') : t('Sign In')}
          </button>
        </form>
      </div>
    </div>
  );
}
