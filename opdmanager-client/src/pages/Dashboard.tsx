import { useQuery } from '@tanstack/react-query';
import { useEffect } from 'react';
import * as signalR from '@microsoft/signalr';

import api from '../api';
import { Users, Activity, Clock, CheckCircle2 } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';

export default function Dashboard() {
  const { data: stats, refetch } = useQuery({
    queryKey: ['dashboardStats'],
    queryFn: async () => {
      const res = await api.get('/dashboard');
      return res.data;
    }
  });

  useEffect(() => {

    let isMounted = true;
    const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5073';
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${API_URL}/hubs/dashboard`, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    connection.on("DashboardUpdated", () => {
      refetch();
    });

    connection.start().catch(err => {
      if (isMounted) console.error("SignalR Connection Error: ", err);
    });

    return () => {
      isMounted = false;
      connection.stop();
    };
  }, [refetch]);

  if (!stats) return <div className="p-4">Loading stats...</div>;

  const statCards = [
    { title: "Total Patients Today", value: stats.totalPatientsToday, icon: Users, color: "var(--info)" },
    { title: "Waiting", value: stats.waitingPatients, icon: Clock, color: "var(--warning)" },
    { title: "In Progress", value: stats.inProgressPatients, icon: Activity, color: "var(--accent-primary)" },
    { title: "Completed", value: stats.completedPatients, icon: CheckCircle2, color: "var(--success)" },
  ];

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Dashboard Overview</h1>
        <div className="badge badge-neutral">Live Updates Active</div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem', marginBottom: '2rem' }}>
        {statCards.map((s, i) => {
          const Icon = s.icon;
          return (
            <div key={i} className="card" style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
              <div style={{ background: `${s.color}20`, padding: '1rem', borderRadius: 'var(--radius-md)', color: s.color }}>
                <Icon size={24} />
              </div>
              <div>
                <div style={{ color: 'var(--text-secondary)', fontSize: '0.875rem' }}>{s.title}</div>
                <div style={{ fontSize: '1.75rem', fontWeight: 700 }}>{s.value}</div>
              </div>
            </div>
          );
        })}
      </div>

      <div className="grid" style={{ gridTemplateColumns: '1fr', gap: '1.5rem' }}>
        <div className="card">
          <h3 style={{ marginBottom: '1.5rem', fontSize: '1.1rem' }}>Doctor Performance Today</h3>
          {stats.doctorPerformance && stats.doctorPerformance.length > 0 ? (
            <div style={{ height: '300px' }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={stats.doctorPerformance}>
                  <XAxis dataKey="doctorName" stroke="var(--text-secondary)" />
                  <YAxis stroke="var(--text-secondary)" />
                  <Tooltip 
                    contentStyle={{ background: 'var(--bg-tertiary)', border: '1px solid var(--border-color)', borderRadius: '8px' }}
                    itemStyle={{ color: 'var(--text-primary)' }}
                  />
                  <Bar dataKey="patientsExamined" name="Patients Examined" fill="var(--accent-primary)" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          ) : (
            <div style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem 0' }}>
              No performance data available for today yet.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
