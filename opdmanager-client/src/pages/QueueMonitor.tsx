import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState, useMemo } from 'react';
import * as signalR from '@microsoft/signalr';

import api from '../api';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';
import { Filter, User } from 'lucide-react';
import ConfirmModal from '../components/ConfirmModal';

/**
 * QueueMonitor Component
 * High-density Kanban board for tracking patients.
 */
export default function QueueMonitor() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [now, setNow] = useState(new Date());
  const [selectedDoctorFilter, setSelectedDoctorFilter] = useState<string | null>(null);
  const [patientToSkip, setPatientToSkip] = useState<any | null>(null);

  // Force re-render every minute to update waiting times
  useEffect(() => {
    const timer = setInterval(() => setNow(new Date()), 60000);
    return () => clearInterval(timer);
  }, []);

  const { data: queueItems = [], refetch } = useQuery({
    queryKey: ['todayQueue'],
    queryFn: async () => {
      const res = await api.get('/queue/today');
      return res.data;
    }
  });

  useEffect(() => {
    let isMounted = true;
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5073';
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/queue`, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    connection.on("QueueUpdated", () => {
      refetch();
    });

    connection.start().catch(err => {
      if (isMounted) console.error("Queue SignalR Error: ", err);
    });

    return () => {
      isMounted = false;
      connection.stop();
    };
  }, [refetch]);

  const statusMutation = useMutation({
    mutationFn: async ({ id, status }: { id: number, status: string }) => {
      await api.patch(`/queue/${id}/status?status=${status}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['todayQueue'] });
    },
    onError: () => {
      toast.error('Failed to update status');
    }
  });

  // Fix timezone by ensuring dates are parsed as UTC
  const parseUtcDate = (dateString: string) => {
    return new Date(dateString.endsWith('Z') ? dateString : `${dateString}Z`);
  };

  const getWaitTime = (checkInTime: string, startTime: string | null) => {
    const start = parseUtcDate(checkInTime);
    const end = startTime ? parseUtcDate(startTime) : now;
    return Math.max(0, Math.floor((end.getTime() - start.getTime()) / 60000));
  };

  // Generate unique predictable color for each doctor
  const getDoctorColor = (doctorName: string) => {
    if (!doctorName) return 'var(--text-muted)';
    const colors = [
      '#0ea5e9', // Sky Blue
      '#8b5cf6', // Violet
      '#ec4899', // Pink
      '#f59e0b', // Amber
      '#10b981', // Emerald
      '#f43f5e', // Rose
      '#3b82f6', // Blue
      '#14b8a6', // Teal
      '#84cc16', // Lime
      '#a855f7', // Purple
    ];
    let hash = 0;
    for (let i = 0; i < doctorName.length; i++) {
      hash = doctorName.charCodeAt(i) + ((hash << 5) - hash);
    }
    return colors[Math.abs(hash) % colors.length];
  };

  const getWaitTimeColor = (minutes: number) => {
    if (minutes < 15) return 'var(--success)';
    if (minutes < 30) return 'var(--warning)';
    return 'var(--danger)';
  };

  const uniqueDoctors = useMemo(() => {
    const names = Array.from(new Set(queueItems.map((q: any) => q.doctorName))) as string[];
    return names.filter(Boolean).sort();
  }, [queueItems]);

  const columns = [
    { id: 'Waiting', title: t('Waiting'), color: 'var(--warning)', bg: 'rgba(245, 158, 11, 0.03)' },
    { id: 'InProgress', title: t('In Progress'), color: 'var(--accent-primary)', bg: 'rgba(13, 148, 136, 0.03)' },
    { id: 'OnHold', title: t('On Hold / Lab'), color: 'var(--danger)', bg: 'rgba(239, 68, 68, 0.03)' },
    { id: 'Done', title: t('Completed'), color: 'var(--success)', bg: 'rgba(16, 185, 129, 0.03)' }
  ];

  const filteredItems = useMemo(() => {
    if (!selectedDoctorFilter) return queueItems;
    return queueItems.filter((q: any) => q.doctorName === selectedDoctorFilter);
  }, [queueItems, selectedDoctorFilter]);

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div className="page-header" style={{ marginBottom: '1rem' }}>
        <h1 className="page-title">{t('Queue Monitor')}</h1>
      </div>

      {/* Doctor Filter Pill Bar */}
      {uniqueDoctors.length > 0 && (
        <div className="surface" style={{ padding: '0.75rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.75rem', flexWrap: 'wrap' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-muted)', fontSize: '0.875rem', fontWeight: 500, marginRight: '0.5rem' }}>
            <Filter size={16} /> Filter by Doctor:
          </div>
          
          <button
            className="btn"
            style={{ 
              padding: '0.25rem 0.75rem', 
              borderRadius: '999px',
              fontSize: '0.85rem',
              fontWeight: 500,
              background: selectedDoctorFilter === null ? 'var(--text-primary)' : 'var(--bg-tertiary)',
              color: selectedDoctorFilter === null ? 'white' : 'var(--text-secondary)',
              border: 'none'
            }}
            onClick={() => setSelectedDoctorFilter(null)}
          >
            All Doctors
          </button>

          {uniqueDoctors.map(doc => {
            const docColor = getDoctorColor(doc);
            const isSelected = selectedDoctorFilter === doc;
            return (
              <button
                key={doc}
                className="btn animate-fade-in"
                style={{ 
                  padding: '0.25rem 0.75rem', 
                  borderRadius: '999px',
                  fontSize: '0.85rem',
                  fontWeight: 500,
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.4rem',
                  background: isSelected ? `${docColor}1A` : 'var(--bg-secondary)', // 1A is ~10% opacity
                  color: isSelected ? docColor : 'var(--text-secondary)',
                  border: `1px solid ${isSelected ? docColor : 'var(--border-color)'}`,
                  boxShadow: isSelected ? `0 0 0 1px ${docColor}` : 'none'
                }}
                onClick={() => setSelectedDoctorFilter(doc)}
              >
                <div style={{ width: '8px', height: '8px', borderRadius: '50%', background: docColor }}></div>
                Dr. {doc}
              </button>
            );
          })}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-4" style={{ 
        gap: '1rem',
        flex: 1,
        minHeight: 0
      }}>
        {columns.map(col => {
          const items = filteredItems.filter((q: any) => q.status === col.id);
          
          return (
            <div key={col.id} className="surface" style={{ 
              display: 'flex', 
              flexDirection: 'column',
              background: col.bg,
              borderTop: `4px solid ${col.color}`,
              boxShadow: 'var(--shadow-sm)'
            }}>
              <div style={{ padding: '0.75rem 1rem', borderBottom: '1px solid var(--border-color)', background: 'var(--bg-secondary)' }}>
                <h3 style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', fontSize: '1rem', margin: 0 }}>
                  {col.title}
                  <span className="badge" style={{ background: col.color, color: 'white', border: 'none', padding: '0.15rem 0.5rem', fontSize: '0.75rem' }}>
                    {items.length}
                  </span>
                </h3>
              </div>
              
              <div style={{ padding: '0.75rem', overflowY: 'auto', flex: 1, display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                {items.map((item: any) => {
                  const waitMinutes = getWaitTime(item.checkInTime, item.startTime);
                  const accumulatedMinutes = item.accumulatedExaminationSeconds ? Math.floor(item.accumulatedExaminationSeconds / 60) : 0;
                  const activeMinutes = item.startTime ? getWaitTime(item.startTime, item.endTime) : 0;
                  const progressMinutes = item.examinationMinutes ?? (accumulatedMinutes + activeMinutes);
                  const docColor = getDoctorColor(item.doctorName);
                  
                  return (
                    <div key={item.id} className="card animate-fade-in" style={{ padding: '0.75rem', borderLeft: `4px solid ${docColor}`, display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                      
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                        <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'baseline' }}>
                          <span style={{ fontSize: '1.2rem', fontWeight: 700, color: 'var(--text-primary)' }}>#{item.queueNumber}</span>
                          <span style={{ fontWeight: 600, fontSize: '0.95rem', color: 'var(--text-primary)' }}>{item.patientName}</span>
                        </div>
                        
                        {/* High-density metrics right-aligned */}
                        <div style={{ textAlign: 'right' }}>
                          <span style={{ fontSize: '0.75rem', fontWeight: 600, color: getWaitTimeColor(waitMinutes) }}>
                            {waitMinutes}m wait
                          </span>
                          {(item.status === 'InProgress' || item.status === 'Done') && (
                            <span style={{ fontSize: '0.75rem', color: 'var(--accent-primary)', fontWeight: 600, marginLeft: '0.5rem' }}>
                              {progressMinutes}m exam
                            </span>
                          )}
                        </div>
                      </div>

                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                        <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                          <div style={{ width: '12px', height: '12px', borderRadius: '3px', background: docColor }}></div>
                          Dr. {item.doctorName}
                        </div>
                        
                        {/* Compact Inline Buttons */}
                        {col.id === 'Waiting' && (
                          <div style={{ display: 'flex', gap: '0.35rem' }}>
                            <button 
                              className="btn btn-secondary" 
                              style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem', color: 'var(--danger)', borderColor: 'var(--border-color)' }}
                              onClick={() => setPatientToSkip(item)}
                              title={t('Patient did not show up')}
                            >
                              {t('Skip')}
                            </button>
                            <button 
                              className="btn btn-primary" 
                              style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem' }}
                              onClick={() => statusMutation.mutate({ id: item.id, status: 'InProgress' })}
                            >
                              {t('Start Exam')}
                            </button>
                          </div>
                        )}
                        {col.id === 'InProgress' && (
                          <div style={{ display: 'flex', gap: '0.35rem' }}>
                            <button 
                              className="btn" 
                              style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem', background: 'var(--danger)', color: 'white', border: 'none' }}
                              title="Send to Lab (Hold)"
                              onClick={() => statusMutation.mutate({ id: item.id, status: 'OnHold' })}
                            >
                              Hold
                            </button>
                            <button 
                              className="btn" 
                              style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem', background: 'var(--success)', color: 'white', border: 'none' }}
                              onClick={() => statusMutation.mutate({ id: item.id, status: 'Done' })}
                            >
                              Complete
                            </button>
                          </div>
                        )}
                        {col.id === 'OnHold' && (
                          <button 
                            className="btn btn-primary" 
                            style={{ padding: '0.25rem 0.5rem', fontSize: '0.75rem' }}
                            onClick={() => statusMutation.mutate({ id: item.id, status: 'Waiting' })}
                          >
                            Re-Queue
                          </button>
                        )}
                      </div>

                    </div>
                  );
                })}
                
                {items.length === 0 && (
                  <div style={{ textAlign: 'center', color: 'var(--text-muted)', marginTop: '2rem', fontSize: '0.875rem' }}>
                    {t('Empty')}
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {patientToSkip && (
        <ConfirmModal
          title={t('Skip Patient')}
          message={t(`Are you sure you want to mark ${patientToSkip.patientName} as No Show (Skip)?`)}
          confirmText={t('Skip')}
          onConfirm={() => {
            statusMutation.mutate({ id: patientToSkip.id, status: 'Skipped' });
            setPatientToSkip(null);
          }}
          onCancel={() => setPatientToSkip(null)}
        />
      )}
    </div>
  );
}
