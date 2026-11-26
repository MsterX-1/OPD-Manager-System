import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../api';
import toast from 'react-hot-toast';
import { Plus, Trash2, Edit2, X, Check, XCircle, Search } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import ConfirmModal from '../components/ConfirmModal';

interface DoctorSchedule {
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  isWorking: boolean;
}

interface Doctor {
  id: number;
  name: string;
  specialty: string;
  phoneNumber: string;
  isActive: boolean;
  averageExaminationMinutes: number | null;
  schedules: DoctorSchedule[];
}

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

const DEFAULT_SCHEDULES = DAYS.map((_, i) => ({
  dayOfWeek: i,
  startTime: '09:00',
  endTime: '17:00',
  isWorking: i > 0 && i < 6 // Mon-Fri
}));

const format12Hour = (time24: string) => {
  if (!time24) return '';
  const [hours, minutes] = time24.split(':');
  const h = parseInt(hours, 10);
  const ampm = h >= 12 ? 'pm' : 'am';
  const h12 = h % 12 || 12;
  return `${h12}:${minutes}${ampm}`;
};

export default function Doctors() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [showModal, setShowModal] = useState(false);
  const [editingDoctor, setEditingDoctor] = useState<Doctor | null>(null);
  const [doctorToDelete, setDoctorToDelete] = useState<Doctor | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  
  const [formData, setFormData] = useState<Omit<Doctor, 'id'>>({
    name: '',
    specialty: '',
    phoneNumber: '',
    isActive: true,
    averageExaminationMinutes: 15,
    schedules: [...DEFAULT_SCHEDULES]
  });

  const { data: doctors, isLoading } = useQuery({
    queryKey: ['doctors'],
    queryFn: async () => {
      const res = await api.get('/doctors');
      return res.data as Doctor[];
    }
  });

  const filteredDoctors = doctors?.filter(d => 
    d.name.toLowerCase().includes(searchTerm.toLowerCase()) || 
    d.specialty.toLowerCase().includes(searchTerm.toLowerCase()) ||
    (d.phoneNumber && d.phoneNumber.includes(searchTerm))
  );

  const createMutation = useMutation({
    mutationFn: async (data: Omit<Doctor, 'id'>) => await api.post('/doctors', data),
    onSuccess: () => {
      toast.success('Doctor created successfully');
      queryClient.invalidateQueries({ queryKey: ['doctors'] });
      queryClient.invalidateQueries({ queryKey: ['activeDoctors'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to create doctor')
  });

  const updateMutation = useMutation({
    mutationFn: async ({ id, data }: { id: number, data: Omit<Doctor, 'id'> }) => await api.put(`/doctors/${id}`, data),
    onSuccess: () => {
      toast.success('Doctor updated successfully');
      queryClient.invalidateQueries({ queryKey: ['doctors'] });
      queryClient.invalidateQueries({ queryKey: ['activeDoctors'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to update doctor')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => await api.delete(`/doctors/${id}`),
    onSuccess: () => {
      toast.success('Doctor deleted');
      queryClient.invalidateQueries({ queryKey: ['doctors'] });
      queryClient.invalidateQueries({ queryKey: ['activeDoctors'] });
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to delete doctor (may have appointments)')
  });
  
  const toggleActiveMutation = useMutation({
    mutationFn: async (id: number) => await api.patch(`/doctors/${id}/toggle-active`),
    onSuccess: () => {
      toast.success('Doctor status toggled');
      queryClient.invalidateQueries({ queryKey: ['doctors'] });
      queryClient.invalidateQueries({ queryKey: ['activeDoctors'] });
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      ...formData,
      schedules: formData.schedules.map(s => ({
        ...s,
        startTime: s.startTime.length === 5 ? `${s.startTime}:00` : s.startTime,
        endTime: s.endTime.length === 5 ? `${s.endTime}:00` : s.endTime
      }))
    };
    if (editingDoctor) {
      updateMutation.mutate({ id: editingDoctor.id, data: payload });
    } else {
      createMutation.mutate(payload);
    }
  };

  const openNew = () => {
    setEditingDoctor(null);
    setFormData({ 
      name: '', specialty: '', phoneNumber: '', isActive: true, 
      averageExaminationMinutes: 15, schedules: [...DEFAULT_SCHEDULES] 
    });
    setShowModal(true);
  };

  const openEdit = (doctor: Doctor) => {
    setEditingDoctor(doctor);
    // Ensure all 7 days exist
    const schedules = DAYS.map((_, i) => {
      const existing = doctor.schedules?.find(s => 
        // Handle both number (if backend updated) and string (if old cached data)
        s.dayOfWeek === i || (s.dayOfWeek as any) === DAYS[i]
      );
      
      if (existing) {
        return { 
          ...existing, 
          dayOfWeek: i,
          startTime: existing.startTime.substring(0, 5), 
          endTime: existing.endTime.substring(0, 5) 
        };
      }
      return { dayOfWeek: i, startTime: '09:00', endTime: '17:00', isWorking: false };
    });
    setFormData({ ...doctor, schedules });
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditingDoctor(null);
  };

  const handleScheduleChange = (dayOfWeek: number, field: string, value: any) => {
    const newSchedules = [...formData.schedules];
    const index = newSchedules.findIndex(s => s.dayOfWeek === dayOfWeek);
    if (index >= 0) {
      newSchedules[index] = { ...newSchedules[index], [field]: value };
      setFormData({ ...formData, schedules: newSchedules });
    }
  };

  return (
    <div>
      <div className="page-header" style={{ flexDirection: 'column', alignItems: 'stretch', gap: '1rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
          <h1 className="page-title">{t('Doctors & Schedules')}</h1>
          <button className="btn btn-primary" onClick={openNew}>
            <Plus size={18} /> {t('Add Doctor')}
          </button>
        </div>
        <div style={{ position: 'relative' }}>
          <Search size={18} style={{ position: 'absolute', left: '1rem', top: '50%', transform: 'translateY(-50%)', color: 'var(--text-muted)' }} />
          <input 
            type="text" 
            placeholder={t('Search doctors by name, specialty, or phone number...')} 
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
            style={{ 
              width: '100%', padding: '0.75rem 1rem 0.75rem 2.5rem', 
              borderRadius: '8px', border: '1px solid var(--border-color)', 
              background: 'var(--bg-secondary)', color: 'var(--text-primary)' 
            }} 
          />
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>{t('Name')}</th>
              <th>{t('Specialty')}</th>
              <th>{t('Phone')}</th>
              <th>{t('Schedule')}</th>
              <th>{t('Status')}</th>
              <th style={{ textAlign: 'right' }}>{t('Actions')}</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={6} style={{ textAlign: 'center' }}>{t('Loading...')}</td></tr>
            ) : filteredDoctors?.length === 0 ? (
              <tr><td colSpan={6} style={{ textAlign: 'center' }}>{t('No doctors found.')}</td></tr>
            ) : filteredDoctors?.map((d) => (
              <tr key={d.id}>
                <td style={{ fontWeight: 500 }}>Dr. {d.name}</td>
                <td>{d.specialty}</td>
                <td>{d.phoneNumber}</td>
                <td>
                  <div style={{ display: 'flex', gap: '0.25rem', flexWrap: 'wrap' }}>
                    {DAYS.map((day, i) => {
                      const schedule = d.schedules?.find(s => s.dayOfWeek === i || (s.dayOfWeek as any) === day);
                      if (!schedule?.isWorking) return null;
                      return (
                        <span key={day} className="badge badge-neutral" style={{ fontSize: '0.7rem', padding: '0.2rem 0.4rem', display: 'inline-flex', alignItems: 'center', gap: '4px' }}>
                          <span style={{ fontWeight: 600 }}>{day.substring(0, 3)}</span>
                          <span style={{ opacity: 0.8, fontSize: '0.65rem' }}>{format12Hour(schedule.startTime.substring(0, 5))}-{format12Hour(schedule.endTime.substring(0, 5))}</span>
                        </span>
                      );
                    })}
                  </div>
                </td>
                <td>
                  <button 
                    onClick={() => toggleActiveMutation.mutate(d.id)}
                    style={{ background: 'none', border: 'none', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '0.5rem' }}
                  >
                    {d.isActive ? (
                      <span className="badge badge-success"><Check size={14} style={{ marginRight: '4px' }} /> {t('Active')}</span>
                    ) : (
                      <span className="badge badge-danger"><XCircle size={14} style={{ marginRight: '4px' }} /> {t('Inactive')}</span>
                    )}
                  </button>
                </td>
                <td style={{ textAlign: 'right' }}>
                  <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                    <button aria-label={t('Edit Doctor')} className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem' }} onClick={() => openEdit(d)}>
                      <Edit2 size={16} />
                    </button>
                    <button aria-label={t('Delete Doctor')} className="btn btn-danger" style={{ padding: '0.35rem 0.5rem' }} onClick={() => setDoctorToDelete(d)}>
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
          padding: '2rem'
        }}>
          <div className="surface animate-fade-in" style={{ width: '100%', maxWidth: '700px', maxHeight: '90vh', overflowY: 'auto', padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>{editingDoctor ? t('Edit Doctor') : t('New Doctor')}</h2>
              <button aria-label={t('Close Modal')} onClick={closeModal} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit}>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '1rem', marginBottom: '1.5rem' }}>
                <div className="form-group">
                  <label>{t('Full Name')}</label>
                  <input required minLength={3} maxLength={100} type="text" value={formData.name} onChange={e => setFormData({...formData, name: e.target.value})} placeholder="John Doe" />
                </div>
                <div className="form-group">
                  <label>{t('Specialty')}</label>
                  <input required minLength={2} maxLength={100} type="text" value={formData.specialty} onChange={e => setFormData({...formData, specialty: e.target.value})} placeholder="Cardiology" />
                </div>
                <div className="form-group">
                  <label>{t('Phone Number')}</label>
                  <input type="tel" pattern="[0-9\+\-\s\(\)]+" value={formData.phoneNumber} onChange={e => setFormData({...formData, phoneNumber: e.target.value})} placeholder="+1 234 567 8900" />
                </div>
                <div className="form-group">
                  <label>{t('Avg. Examination (Minutes)')}</label>
                  <input required type="number" min="1" max="120" value={formData.averageExaminationMinutes || ''} onChange={e => setFormData({...formData, averageExaminationMinutes: parseInt(e.target.value) || null})} />
                </div>
              </div>

              <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                {t('Weekly Schedule')}
              </h3>
              
              <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', marginBottom: '2rem' }}>
                {formData.schedules.map((schedule) => (
                  <div key={schedule.dayOfWeek} style={{ display: 'grid', gridTemplateColumns: '100px 100px 1fr 1fr', gap: '1rem', alignItems: 'center' }}>
                    <div style={{ fontWeight: 500 }}>{DAYS[schedule.dayOfWeek]}</div>
                    <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', margin: 0 }}>
                      <input 
                        type="checkbox" 
                        style={{ width: 'auto' }}
                        checked={schedule.isWorking} 
                        onChange={e => handleScheduleChange(schedule.dayOfWeek, 'isWorking', e.target.checked)} 
                      />
                      Working
                    </label>
                    <input 
                      type="time" 
                      disabled={!schedule.isWorking}
                      value={schedule.startTime} 
                      onChange={e => handleScheduleChange(schedule.dayOfWeek, 'startTime', e.target.value)} 
                    />
                    <input 
                      type="time" 
                      disabled={!schedule.isWorking}
                      value={schedule.endTime} 
                      onChange={e => handleScheduleChange(schedule.dayOfWeek, 'endTime', e.target.value)} 
                    />
                  </div>
                ))}
              </div>

              <div style={{ display: 'flex', gap: '1rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={createMutation.isPending || updateMutation.isPending}>
                  Save Doctor
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {doctorToDelete && (
        <ConfirmModal
          title={t('Delete Doctor')}
          message={t(`Are you sure you want to delete Dr.`) + ` ${doctorToDelete.name}? ` + t(`This action cannot be undone and may fail if they have appointments.`)}
          confirmText={t('Delete Doctor')}
          onConfirm={() => deleteMutation.mutate(doctorToDelete.id)}
          onCancel={() => setDoctorToDelete(null)}
        />
      )}
    </div>
  );
}
