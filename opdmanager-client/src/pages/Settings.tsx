import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../api';
import toast from 'react-hot-toast';
import { Save } from 'lucide-react';

interface AppSettings {
  hospitalName: string;
  waitingAlertMinutes: number;
  enableDailyAppointmentCap: boolean;
  defaultAverageExaminationMinutes: number;
}

export default function Settings() {
  const queryClient = useQueryClient();
  const [formData, setFormData] = useState<AppSettings>({
    hospitalName: '',
    waitingAlertMinutes: 30,
    enableDailyAppointmentCap: false,
    defaultAverageExaminationMinutes: 15
  });

  const { data: settings, isLoading } = useQuery({
    queryKey: ['settings'],
    queryFn: async () => {
      const res = await api.get('/settings');
      return res.data as AppSettings;
    }
  });

  useEffect(() => {
    if (settings) {
      setFormData(settings);
    }
  }, [settings]);

  const updateMutation = useMutation({
    mutationFn: async (data: AppSettings) => {
      await api.put('/settings', data);
    },
    onSuccess: () => {
      toast.success('Settings updated successfully');
      queryClient.invalidateQueries({ queryKey: ['settings'] });
    },
    onError: () => {
      toast.error('Failed to update settings');
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    updateMutation.mutate(formData);
  };

  if (isLoading) return <div className="p-4">Loading settings...</div>;

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">System Settings</h1>
      </div>

      <div className="card" style={{ maxWidth: '600px' }}>
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label>Hospital / Clinic Name</label>
            <input 
              type="text" 
              required
              value={formData.hospitalName}
              onChange={e => setFormData({ ...formData, hospitalName: e.target.value })}
            />
            <small style={{ color: 'var(--text-muted)' }}>This name will appear on printed reports and the sidebar.</small>
          </div>

          <div className="form-group">
            <label>Waiting Alert Threshold (Minutes)</label>
            <input 
              type="number" 
              min="1"
              required
              value={formData.waitingAlertMinutes}
              onChange={e => setFormData({ ...formData, waitingAlertMinutes: parseInt(e.target.value) || 30 })}
            />
            <small style={{ color: 'var(--text-muted)' }}>Patients waiting longer than this will be highlighted in the Queue Monitor.</small>
          </div>

          <div className="form-group" style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', margin: '2rem 0 1rem' }}>
            <input 
              type="checkbox" 
              id="enableCap"
              style={{ width: 'auto' }}
              checked={formData.enableDailyAppointmentCap}
              onChange={e => setFormData({ ...formData, enableDailyAppointmentCap: e.target.checked })}
            />
            <label htmlFor="enableCap" style={{ margin: 0, cursor: 'pointer' }}>Enable Daily Appointment Cap</label>
          </div>

          {formData.enableDailyAppointmentCap && (
            <div className="form-group p-4" style={{ background: 'var(--bg-tertiary)', borderRadius: 'var(--radius-md)' }}>
              <label>Default Average Examination Minutes</label>
              <input 
                type="number" 
                min="5"
                required
                value={formData.defaultAverageExaminationMinutes}
                onChange={e => setFormData({ ...formData, defaultAverageExaminationMinutes: parseInt(e.target.value) || 15 })}
              />
              <small style={{ color: 'var(--text-muted)', display: 'block', marginTop: '0.5rem' }}>
                Used to calculate a doctor's maximum daily patient capacity if the doctor doesn't have a specific average set.
                (Total working minutes / Average examination minutes).
              </small>
            </div>
          )}

          <div style={{ marginTop: '2rem', paddingTop: '1.5rem', borderTop: '1px solid var(--border-color)' }}>
            <button type="submit" className="btn btn-primary" disabled={updateMutation.isPending}>
              <Save size={18} />
              {updateMutation.isPending ? 'Saving...' : 'Save Settings'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
