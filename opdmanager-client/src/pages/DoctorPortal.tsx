import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import * as signalR from '@microsoft/signalr';
import { Stethoscope, Eye, ArrowRight, Clock } from 'lucide-react';
import api from '../api';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';

/**
 * DoctorPortal Component
 * Displays the doctor's active patient queue and pending referrals.
 * Synchronized via SignalR to instantly reflect front-desk check-ins.
 */
export default function DoctorPortal() {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const navigate = useNavigate();
  const [now, setNow] = useState(new Date());

  useEffect(() => {
    const timer = setInterval(() => setNow(new Date()), 60000);
    return () => clearInterval(timer);
  }, []);

  const { data: queueItems = [] } = useQuery({
    queryKey: ['myQueue'],
    queryFn: async () => {
      const res = await api.get('/doctors/me/queue');
      return res.data;
    }
  });

  const { data: referrals } = useQuery({
    queryKey: ['myReferrals'],
    queryFn: async () => {
      const res = await api.get('/doctors/me/referrals');
      return res.data;
    }
  });

  useEffect(() => {
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5073';
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/queue`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connection.on("QueueUpdated", () => {
      queryClient.invalidateQueries({ queryKey: ['myQueue'] });
    });

    connection.start().catch(err => console.error("SignalR Error:", err));
    return () => { connection.stop(); };
  }, [queryClient]);

  const statusMutation = useMutation({
    mutationFn: async ({ id, status }: { id: number, status: string }) => {
      await api.patch(`/queue/${id}/status?status=${status}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['myQueue'] });
      toast.success('Status updated');
    },
    onError: () => toast.error('Failed to update status')
  });

  const getWaitTime = (checkInTime: string, startTime: string | null) => {
    const start = new Date(checkInTime);
    const end = startTime ? new Date(startTime) : now;
    return Math.floor((end.getTime() - start.getTime()) / 60000);
  };

    const columns = [
    { id: 'Waiting', title: t('Waiting'), color: 'var(--warning)', bg: 'rgba(245, 158, 11, 0.05)' },
    { id: 'InProgress', title: t('In Progress'), color: 'var(--accent-primary)', bg: 'rgba(59, 130, 246, 0.05)' },
    { id: 'OnHold', title: t('On Hold / Lab'), color: 'var(--danger)', bg: 'rgba(239, 68, 68, 0.05)' },
    { id: 'Done', title: t('Completed'), color: 'var(--success)', bg: 'rgba(16, 185, 129, 0.05)' }
  ];

  const pendingReferrals = referrals?.received?.filter((r: any) => r.status === 'Pending') || [];

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="page-header">
        <h1 className="page-title" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <Stethoscope size={28} />
          {t('My Queue')}
        </h1>
      </div>

      {/* Pending referrals to me */}
      {pendingReferrals.length > 0 && (
        <div className="surface" style={{ padding: '1rem', borderLeft: '4px solid var(--accent-secondary)' }}>
          <h3 style={{ marginBottom: '0.75rem', fontSize: '0.95rem', color: 'var(--accent-secondary)' }}>
            📥 Incoming Referrals ({pendingReferrals.length})
          </h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {pendingReferrals.map((ref: any) => (
              <div key={ref.id} style={{ 
                display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                padding: '0.75rem', background: 'var(--bg-tertiary)', borderRadius: 'var(--radius-sm)'
              }}>
                <div>
                  <span style={{ fontWeight: 600 }}>{ref.patientName}</span>
                  <span style={{ color: 'var(--text-muted)', marginLeft: '0.5rem', fontSize: '0.85rem' }}>
                    from Dr. {ref.referringDoctorName}
                  </span>
                  {ref.reason && (
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', marginTop: '0.25rem' }}>
                      {ref.reason}
                    </div>
                  )}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Kanban Board — same pattern as QueueMonitor but scoped to me */}
      <div className="grid grid-cols-1 md:grid-cols-4" style={{ 
        gap: '1.5rem',
        flex: 1,
        minHeight: 0
      }}>
        {columns.map(col => {
          const items = queueItems.filter((q: any) => q.status === col.id);
          
          return (
            <div key={col.id} className="surface" style={{ 
              display: 'flex', 
              flexDirection: 'column',
              background: col.bg,
              borderTop: `4px solid ${col.color}`
            }}>
              <div style={{ padding: '1rem', borderBottom: '1px solid var(--border-color)' }}>
                <h3 style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  {col.title}
                  <span className="badge badge-neutral" style={{ background: col.color, color: 'white', border: 'none' }}>
                    {items.length}
                  </span>
                </h3>
              </div>
              
              <div style={{ padding: '1rem', overflowY: 'auto', flex: 1, display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                {items.map((item: any) => (
                  <div key={item.id} className="card" style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem', alignItems: 'center' }}>
                      <span style={{ fontSize: '1.25rem', fontWeight: 700, color: col.color, background: 'var(--bg-secondary)', padding: '0.2rem 0.6rem', borderRadius: '4px' }}>#{item.queueNumber}</span>
                      <div style={{ textAlign: 'right' }}>
                        <span style={{ fontSize: '0.75rem', color: 'var(--warning)', fontWeight: 600, display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                          <Clock size={12} /> {getWaitTime(item.checkInTime, item.startTime)} min wait
                        </span>
                        {(item.status === 'InProgress' || item.status === 'Done') && (
                          <span style={{ fontSize: '0.75rem', color: 'var(--accent-primary)', fontWeight: 600 }}>
                            {item.examinationMinutes ?? ((item.accumulatedExaminationSeconds ? Math.floor(item.accumulatedExaminationSeconds / 60) : 0) + (item.startTime ? getWaitTime(item.startTime, null) : 0))} min exam
                          </span>
                        )}
                      </div>
                    </div>
                    <div style={{ fontWeight: 600, fontSize: '1.05rem', marginBottom: '0.75rem' }}>{item.patientName}</div>
                    
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      {col.id === 'Waiting' && (
                        <button 
                          className="btn btn-primary" 
                          style={{ flex: 1, padding: '0.5rem', fontSize: '0.85rem' }}
                          onClick={() => statusMutation.mutate({ id: item.id, status: 'InProgress' })}
                        >
                          <ArrowRight size={14} /> Start
                        </button>
                      )}
                      {col.id === 'InProgress' && (
                        <>
                          <button 
                            className="btn" 
                            style={{ flex: 1, padding: '0.5rem', fontSize: '0.85rem', background: 'var(--accent-primary)', color: 'white', border: 'none' }}
                            onClick={() => navigate(`/encounter/${item.appointmentId}`)}
                          >
                            <Eye size={14} /> Encounter
                          </button>
                          <button 
                            className="btn" 
                            style={{ padding: '0.5rem', fontSize: '0.85rem', background: 'var(--danger)', color: 'white', border: 'none' }}
                            title="Send to Lab (Hold)"
                            onClick={() => statusMutation.mutate({ id: item.id, status: 'OnHold' })}
                          >
                            Hold
                          </button>
                          <button 
                            className="btn" 
                            style={{ padding: '0.5rem', background: 'var(--success)', color: 'white', border: 'none', fontSize: '0.85rem' }}
                            title="Complete"
                            onClick={() => statusMutation.mutate({ id: item.id, status: 'Done' })}
                          >
                            ✓
                          </button>
                        </>
                      )}
                      {col.id === 'OnHold' && (
                        <button 
                          className="btn btn-primary" 
                          style={{ flex: 1, padding: '0.5rem', fontSize: '0.85rem' }}
                          onClick={() => statusMutation.mutate({ id: item.id, status: 'Waiting' })}
                        >
                          <ArrowRight size={14} /> Re-Queue
                        </button>
                      )}
                      {col.id === 'Done' && item.appointmentId && (
                        <button 
                          className="btn" 
                          style={{ flex: 1, padding: '0.5rem', fontSize: '0.85rem', background: 'transparent', border: '1px solid var(--border-color)', color: 'var(--text-secondary)' }}
                          onClick={() => navigate(`/encounter/${item.appointmentId}`)}
                        >
                          <Eye size={14} /> View Record
                        </button>
                      )}
                    </div>
                  </div>
                ))}
                
                {items.length === 0 && (
                  <div style={{ textAlign: 'center', color: 'var(--text-muted)', marginTop: '2rem' }}>
                    No patients
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
