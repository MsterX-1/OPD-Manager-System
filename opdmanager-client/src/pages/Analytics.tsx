import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import api from '../api';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts';
import { Clock, Activity } from 'lucide-react';
import { useTranslation } from 'react-i18next';

export default function Analytics() {
  const { t } = useTranslation();
  const [fromDate, setFromDate] = useState(
    new Date(new Date().setDate(new Date().getDate() - 7)).toISOString().split('T')[0]
  );
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
  const [selectedDoctor, setSelectedDoctor] = useState<number | null>(null);

  const { data: performance = [], isLoading } = useQuery({
    queryKey: ['analytics', fromDate, toDate],
    queryFn: async () => {
      const res = await api.get(`/reports/analytics?from=${fromDate}&to=${toDate}`);
      return res.data;
    }
  });

  const { data: details = [], isLoading: isLoadingDetails } = useQuery({
    queryKey: ['analyticsDetails', fromDate, toDate, selectedDoctor],
    queryFn: async () => {
      if (!selectedDoctor) return [];
      const res = await api.get(`/reports/analytics/details?from=${fromDate}&to=${toDate}&doctorId=${selectedDoctor}`);
      return res.data;
    },
    enabled: !!selectedDoctor
  });

  return (
    <div>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
        <div>
          <h1 className="page-title">{t('Performance Analytics')}</h1>
          <p style={{ color: 'var(--text-muted)' }}>{t('Admin tracking for patient wait times and doctor progress')}</p>
        </div>
        
        <div style={{ display: 'flex', gap: '1rem', background: 'var(--bg-secondary)', padding: '0.5rem', borderRadius: '8px', flexWrap: 'wrap' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{t('From:')}</span>
            <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} className="form-control" style={{ border: 'none', background: 'var(--bg-tertiary)', padding: '0.25rem 0.5rem' }} />
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{t('To:')}</span>
            <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} className="form-control" style={{ border: 'none', background: 'var(--bg-tertiary)', padding: '0.25rem 0.5rem' }} />
          </div>
        </div>
      </div>

      {isLoading ? (
        <div style={{ padding: '2rem', textAlign: 'center' }}>{t('Loading analytics data...')}</div>
      ) : performance.length === 0 ? (
        <div className="surface" style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-muted)' }}>
          {t('No performance data found for the selected date range.')}
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))', gap: '1.5rem' }}>
            {/* Wait Time Chart */}
            <div className="surface" style={{ padding: '1.5rem' }}>
              <h3 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--warning)' }}>
                <Clock size={20} /> {t('Average Wait Time per Doctor')}
              </h3>
              <div style={{ height: '300px' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={performance} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" vertical={false} />
                    <XAxis dataKey="doctorName" stroke="var(--text-secondary)" fontSize={12} />
                    <YAxis stroke="var(--text-secondary)" fontSize={12} tickFormatter={(val) => `${val}m`} />
                    <Tooltip 
                      contentStyle={{ background: 'var(--bg-tertiary)', border: '1px solid var(--border-color)', borderRadius: '8px' }}
                      itemStyle={{ color: 'var(--text-primary)' }}
                      formatter={(value: any) => [`${Math.round(value)} ${t('mins')}`, t('Avg Wait Time')]}
                    />
                    <Bar dataKey="averageWaitingMinutes" name={t('Wait Time')} fill="var(--warning)" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>

            {/* Examination Time Chart */}
            <div className="surface" style={{ padding: '1.5rem' }}>
              <h3 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1.5rem', color: 'var(--accent-primary)' }}>
                <Activity size={20} /> {t('Average Progress Time per Doctor')}
              </h3>
              <div style={{ height: '300px' }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={performance} margin={{ top: 10, right: 10, left: -20, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="var(--border-color)" vertical={false} />
                    <XAxis dataKey="doctorName" stroke="var(--text-secondary)" fontSize={12} />
                    <YAxis stroke="var(--text-secondary)" fontSize={12} tickFormatter={(val) => `${val}m`} />
                    <Tooltip 
                      contentStyle={{ background: 'var(--bg-tertiary)', border: '1px solid var(--border-color)', borderRadius: '8px' }}
                      itemStyle={{ color: 'var(--text-primary)' }}
                      formatter={(value: any) => [`${Math.round(value)} ${t('mins')}`, t('Avg Progress Time')]}
                    />
                    <Bar dataKey="averageExaminationMinutes" name={t('Progress Time')} fill="var(--accent-primary)" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          </div>

          {/* Detailed Data Table */}
          <div className="surface table-container" style={{ padding: '1.5rem' }}>
            <h3 style={{ marginBottom: '1.5rem' }}>{t('Detailed Breakdown')}</h3>
            <table>
              <thead>
                <tr>
                  <th>{t('Doctor Name')}</th>
                  <th style={{ textAlign: 'center' }}>{t('Patients Examined')}</th>
                  <th style={{ textAlign: 'right' }}>{t('Total Wait Time')}</th>
                  <th style={{ textAlign: 'right' }}>{t('Avg Wait Time')}</th>
                  <th style={{ textAlign: 'right' }}>{t('Total Progress Time')}</th>
                  <th style={{ textAlign: 'right' }}>{t('Avg Progress Time')}</th>
                </tr>
              </thead>
              <tbody>
                {performance.map((doc: any) => (
                  <tr 
                    key={doc.doctorId} 
                    onClick={() => setSelectedDoctor(selectedDoctor === doc.doctorId ? null : doc.doctorId)}
                    style={{ cursor: 'pointer', background: selectedDoctor === doc.doctorId ? 'var(--bg-tertiary)' : 'transparent' }}
                    className="hover-bg-tertiary"
                  >
                    <td style={{ fontWeight: 500 }}>{t('Dr.')} {doc.doctorName}</td>
                    <td style={{ textAlign: 'center' }}>
                      <span className="badge badge-info">{doc.patientsExamined}</span>
                    </td>
                    <td style={{ textAlign: 'right', color: 'var(--text-secondary)' }}>
                      {Math.round(doc.totalWaitingMinutes || 0)} {t('mins')}
                    </td>
                    <td style={{ textAlign: 'right', fontWeight: 600, color: 'var(--warning)' }}>
                      {doc.averageWaitingMinutes ? Math.round(doc.averageWaitingMinutes) : 0} {t('mins')}
                    </td>
                    <td style={{ textAlign: 'right', color: 'var(--text-secondary)' }}>
                      {Math.round(doc.totalExaminationMinutes || 0)} {t('mins')}
                    </td>
                    <td style={{ textAlign: 'right', fontWeight: 600, color: 'var(--accent-primary)' }}>
                      {doc.averageExaminationMinutes ? Math.round(doc.averageExaminationMinutes) : 0} {t('mins')}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Expanded Details Section */}
          {selectedDoctor && (
            <div className="surface table-container" style={{ padding: '1.5rem', borderTop: '4px solid var(--accent-primary)', animation: 'fadeIn 0.3s' }}>
              <h3 style={{ marginBottom: '1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span>{t('Patient Breakdown')} ({t('Dr.')} {performance.find((d: any) => d.doctorId === selectedDoctor)?.doctorName})</span>
                <button className="btn btn-secondary" style={{ padding: '0.25rem 0.5rem', fontSize: '0.85rem' }} onClick={() => setSelectedDoctor(null)}>{t('Close')}</button>
              </h3>
              
              {isLoadingDetails ? (
                <div style={{ padding: '2rem', textAlign: 'center' }}>{t('Loading details...')}</div>
              ) : details.length === 0 ? (
                <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>{t('No detailed records found.')}</div>
              ) : (
                <table>
                  <thead>
                    <tr>
                      <th>{t('Patient Name')}</th>
                      <th>{t('Date')}</th>
                      <th style={{ textAlign: 'right' }}>{t('Wait Time')}</th>
                      <th style={{ textAlign: 'right' }}>{t('Progress Time')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {details.map((item: any) => (
                      <tr key={item.id}>
                        <td style={{ fontWeight: 500 }}>{item.patientName}</td>
                        <td style={{ color: 'var(--text-secondary)' }}>
                          {new Date(item.checkInTime).toLocaleDateString('en-US', { 
                            weekday: 'short', 
                            year: 'numeric', 
                            month: 'short', 
                            day: 'numeric' 
                          })}
                        </td>
                        <td style={{ textAlign: 'right', color: 'var(--warning)' }}>{item.waitingMinutes} {t('mins')}</td>
                        <td style={{ textAlign: 'right', color: 'var(--accent-primary)' }}>{item.examinationMinutes} {t('mins')}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
          )}

        </div>
      )}
    </div>
  );
}
