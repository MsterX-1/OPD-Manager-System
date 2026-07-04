import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '../store';
import api from '../api';
import toast from 'react-hot-toast';

export default function QueueMonitor() {
  const token = useAuthStore(state => state.accessToken);
  const queryClient = useQueryClient();
  const [now, setNow] = useState(new Date());

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
    if (!token) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl("http://localhost:5073/hubs/queue", {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    connection.on("QueueUpdated", () => {
      refetch();
    });

    connection.start().catch(err => console.error("Queue SignalR Error: ", err));

    return () => {
      connection.stop();
    };
  }, [token, refetch]);

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

  const getWaitTime = (checkInTime: string, startTime: string | null) => {
    const start = new Date(checkInTime);
    const end = startTime ? new Date(startTime) : now;
    return Math.floor((end.getTime() - start.getTime()) / 60000);
  };

  const columns = [
    { id: 'Waiting', title: 'Waiting', color: 'var(--warning)', bg: 'rgba(245, 158, 11, 0.05)' },
    { id: 'InProgress', title: 'In Progress', color: 'var(--accent-primary)', bg: 'rgba(59, 130, 246, 0.05)' },
    { id: 'Done', title: 'Completed', color: 'var(--success)', bg: 'rgba(16, 185, 129, 0.05)' }
  ];

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div className="page-header">
        <h1 className="page-title">Live Queue Monitor</h1>
      </div>

      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(3, 1fr)', 
        gap: '1.5rem',
        flex: 1,
        minHeight: 0
      }}>
        {columns.map(col => {
          const items = queueItems.filter((q: any) => q.status === col.id);
          
          return (
            <div key={col.id} className="glass-panel" style={{ 
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
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                      <span style={{ fontSize: '1.25rem', fontWeight: 700, color: col.color }}>#{item.queueNumber}</span>
                      <span style={{ fontSize: '0.875rem', color: 'var(--text-muted)' }}>
                        {getWaitTime(item.checkInTime, item.startTime)} min wait
                      </span>
                    </div>
                    <div style={{ fontWeight: 500, marginBottom: '0.25rem' }}>{item.patientName}</div>
                    <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '1rem' }}>
                      Dr. {item.doctorName} ({item.doctorSpecialty})
                    </div>
                    
                    {col.id === 'Waiting' && (
                      <button 
                        className="btn btn-primary" 
                        style={{ width: '100%', padding: '0.5rem' }}
                        onClick={() => statusMutation.mutate({ id: item.id, status: 'InProgress' })}
                      >
                        Start Examination
                      </button>
                    )}
                    {col.id === 'InProgress' && (
                      <button 
                        className="btn" 
                        style={{ width: '100%', padding: '0.5rem', background: 'var(--success)', color: 'white' }}
                        onClick={() => statusMutation.mutate({ id: item.id, status: 'Done' })}
                      >
                        Complete
                      </button>
                    )}
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
