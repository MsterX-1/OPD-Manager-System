import api from '../api';
import { useState } from 'react';
import toast from 'react-hot-toast';
import { useAuthStore } from '../store';
import { useQuery } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';

export default function Reports() {
  const { t } = useTranslation();
  const user = useAuthStore(state => state.user);
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [fromDate, setFromDate] = useState(new Date().toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedDoctorId, setSelectedDoctorId] = useState<string>('');

  const { data: doctors = [] } = useQuery({
    queryKey: ['doctors'],
    queryFn: async () => {
      const res = await api.get('/doctors');
      return res.data;
    }
  });

  const downloadReport = async (type: string, format: string) => {
    try {
      let url = '';
      let filename = '';
      
      if (type === 'appointments') {
        url = `/reports/appointments/${format}?date=${date}`;
        filename = `appointments_${date}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      } else {
        url = `/reports/performance/${format}?from=${fromDate}&to=${toDate}`;
        if (selectedDoctorId) {
          url += `&doctorId=${selectedDoctorId}`;
        }
        filename = `performance_${fromDate}_to_${toDate}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      }

      const response = await api.get(url, { responseType: 'blob' });
      const blob = new Blob([response.data]);
      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success(t('Report downloaded successfully'));
    } catch (err) {
      toast.error(t('Failed to download report'));
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">{t('Reports')}</h1>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem' }}>
        <div className="surface" style={{ padding: '1.5rem' }}>
          <h3 style={{ marginBottom: '1rem' }}>{t('Appointments Report')}</h3>
          <div className="form-group">
            <label>{t('Date')}</label>
            <input type="date" value={date} onChange={e => setDate(e.target.value)} />
          </div>
          <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
            <button className="btn btn-primary" onClick={() => downloadReport('appointments', 'pdf')}>{t('Download PDF')}</button>
            <button className="btn btn-secondary" onClick={() => downloadReport('appointments', 'excel')}>{t('Download Excel')}</button>
          </div>
        </div>

        {user?.role === 'Admin' && (
          <div className="surface" style={{ padding: '1.5rem' }}>
            <h3 style={{ marginBottom: '1rem' }}>{t('Doctor Performance')}</h3>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '1rem' }}>
                <div className="form-group">
                  <label>{t('From Date')}</label>
                  <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} className="form-control" />
                </div>
                <div className="form-group">
                  <label>{t('To Date')}</label>
                  <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} className="form-control" />
                </div>
              </div>
              <div className="form-group">
                <label>{t('Filter by Doctor (Optional)')}</label>
                <select value={selectedDoctorId} onChange={e => setSelectedDoctorId(e.target.value)} className="form-control">
                  <option value="">{t('All Doctors')}</option>
                  {doctors.map((d: any) => (
                    <option key={d.id} value={d.id}>{t('Dr.')} {d.name}</option>
                  ))}
                </select>
              </div>
            </div>
            <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
              <button className="btn btn-primary" onClick={() => downloadReport('performance', 'pdf')}>{t('Download PDF')}</button>
              <button className="btn btn-secondary" onClick={() => downloadReport('performance', 'excel')}>{t('Download Excel')}</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
