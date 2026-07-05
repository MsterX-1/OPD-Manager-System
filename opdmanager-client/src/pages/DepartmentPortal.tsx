import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import * as signalR from '@microsoft/signalr';
import { ClipboardList, ArrowRight, Check, Upload } from 'lucide-react';
import api from '../api';
import toast from 'react-hot-toast';
import { useAuthStore } from '../store';

/**
 * DepartmentPortal Component
 * Provides department staff with a real-time queue of incoming patient referrals.
 * Uses SignalR for instant updates when doctors refer patients or when results are attached.
 */
export default function DepartmentPortal() {
  const queryClient = useQueryClient();
  const user = useAuthStore(state => state.user);
  const [now, setNow] = useState(new Date());
  const [selectedEntry, setSelectedEntry] = useState<any>(null);
  const [resultNotes, setResultNotes] = useState('');

  useEffect(() => {
    const timer = setInterval(() => setNow(new Date()), 60000);
    return () => clearInterval(timer);
  }, []);

  const { data: queueItems = [], isLoading } = useQuery({
    queryKey: ['departmentQueue'],
    queryFn: async () => {
      const res = await api.get('/referrals/department-queue');
      return res.data;
    }
  });

  useEffect(() => {
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5073';
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/department`, { withCredentials: true })
      .withAutomaticReconnect()
      .build();

    connection.on("ReferralReceived", () => {
      toast('New patient referral received!', { icon: '🔔' });
      queryClient.invalidateQueries({ queryKey: ['departmentQueue'] });
    });

    connection.on("ReferralUpdated", () => {
      queryClient.invalidateQueries({ queryKey: ['departmentQueue'] });
    });

    connection.on("ReferralCompleted", () => {
      queryClient.invalidateQueries({ queryKey: ['departmentQueue'] });
    });

    connection.start().catch(err => console.error("SignalR Error:", err));
    return () => { connection.stop(); };
  }, [queryClient]);

  const updateStatusMutation = useMutation({
    mutationFn: async ({ id, status }: { id: number, status: string }) => {
      await api.put(`/referrals/${id}/status`, { status });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['departmentQueue'] });
      toast.success('Status updated');
    }
  });

  const attachResultMutation = useMutation({
    mutationFn: async () => {
      if (!selectedEntry) return;
      await api.put(`/referrals/${selectedEntry.referralId}/result`, { resultNotes });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['departmentQueue'] });
      toast.success('Results attached and referral completed');
      setSelectedEntry(null);
      setResultNotes('');
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

  if (isLoading) return <div>Loading...</div>;

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1 className="page-title" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <ClipboardList size={28} />
          Department Queue
        </h1>
        <div style={{ background: 'var(--bg-secondary)', padding: '0.5rem 1rem', borderRadius: 'var(--radius-md)', fontWeight: 600 }}>
          {user?.departmentId ? `Dept ID: ${user.departmentId}` : 'Admin View'}
        </div>
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
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem', alignItems: 'center' }}>
                      <span style={{ fontSize: '1.25rem', fontWeight: 700, color: col.color, background: 'var(--bg-secondary)', padding: '0.2rem 0.6rem', borderRadius: '4px' }}>#{item.queueNumber}</span>
                      <span style={{ fontSize: '0.75rem', color: 'var(--warning)', fontWeight: 600 }}>
                        Wait: {getWaitTime(item.checkInTime, item.startTime)} min
                      </span>
                    </div>
                    
                    <div style={{ fontWeight: 600, fontSize: '1.05rem', marginBottom: '0.25rem' }}>{item.patientName}</div>
                    
                    <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>
                      <span style={{ fontWeight: 600 }}>Service:</span> {item.serviceName || 'General'}
                    </div>
                    
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '1rem' }}>
                      Ref by: Dr. {item.referringDoctorName}
                    </div>
                    
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
                      {col.id === 'Waiting' && (
                        <button 
                          className="btn btn-primary" 
                          style={{ flex: 1, padding: '0.5rem', fontSize: '0.85rem' }}
                          onClick={() => updateStatusMutation.mutate({ id: item.referralId, status: 'InProgress' })}
                        >
                          <ArrowRight size={14} /> Start Service
                        </button>
                      )}
                      
                      {col.id === 'InProgress' && (
                        <button 
                          className="btn" 
                          style={{ flex: 1, padding: '0.5rem', fontSize: '0.85rem', background: 'var(--success)', color: 'white', border: 'none' }}
                          onClick={() => setSelectedEntry(item)}
                        >
                          <Upload size={14} /> Attach Results
                        </button>
                      )}
                      
                      {col.id === 'Done' && (
                        <div style={{ fontSize: '0.8rem', color: 'var(--success)', display: 'flex', alignItems: 'center', gap: '0.25rem' }}>
                          <Check size={14} /> Results attached & sent to doctor
                        </div>
                      )}
                    </div>
                  </div>
                ))}
                
                {items.length === 0 && (
                  <div style={{ textAlign: 'center', color: 'var(--text-muted)', marginTop: '2rem' }}>
                    No pending orders
                  </div>
                )}
              </div>
            </div>
          );
        })}
      </div>

      {/* Result Attachment Modal */}
      {selectedEntry && (
        <div style={{
          position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.6)', display: 'flex', 
          alignItems: 'center', justifyContent: 'center', zIndex: 1000
        }} onClick={() => setSelectedEntry(null)}>
          <div className="glass-panel" onClick={e => e.stopPropagation()}
            style={{ width: '500px', padding: '2rem' }}>
            <h2 style={{ marginBottom: '1rem' }}>Attach Results</h2>
            <div style={{ marginBottom: '1.5rem', fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
              Patient: <span style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{selectedEntry.patientName}</span><br/>
              Service: <span style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{selectedEntry.serviceName}</span>
            </div>

            <div style={{ marginBottom: '1.5rem' }}>
              <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'block', marginBottom: '0.25rem' }}>Result Notes / Values</label>
              <textarea className="input" value={resultNotes} onChange={e => setResultNotes(e.target.value)}
                style={{ width: '100%', padding: '0.5rem', minHeight: '120px', resize: 'vertical' }}
                placeholder="Enter test results, normal values, or general findings here..." />
            </div>

            <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
              <button className="btn" onClick={() => setSelectedEntry(null)} style={{ padding: '0.5rem 1rem' }}>Cancel</button>
              <button className="btn btn-primary" onClick={() => attachResultMutation.mutate()}
                style={{ padding: '0.5rem 1rem', background: 'var(--success)', border: 'none' }}>
                <Check size={16} /> Complete & Send
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
