import { useQuery } from '@tanstack/react-query';
import { useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import { useTranslation } from 'react-i18next';

import api from '../api';
import { Users, Activity, Clock, CheckCircle2, BarChart2 } from 'lucide-react';
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer } from 'recharts';

export default function Dashboard() {
  const { t } = useTranslation();
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

  if (!stats) return <div className="p-4" style={{ color: 'var(--text-muted)' }}>{t("Loading stats...")}</div>;

  const statCards = [
    { title: t("Total Patients Today"), value: stats.totalPatientsToday, icon: Users, color: "var(--info)" },
    { title: t("Waiting"), value: stats.waitingPatients, icon: Clock, color: "var(--warning)" },
    { title: t("In Progress"), value: stats.inProgressPatients, icon: Activity, color: "var(--accent-primary)" },
    { title: t("Completed"), value: stats.completedPatients, icon: CheckCircle2, color: "var(--success)" },
  ];

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">{t("Dashboard")}</h1>
        <div className="badge badge-neutral">
          <span style={{ display: 'inline-block', width: '8px', height: '8px', borderRadius: '50%', background: 'var(--success)', marginInlineEnd: '6px' }}></span>
          {t("Live Updates")}
        </div>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem', marginBottom: '2rem' }}>
        {statCards.map((s, i) => {
          const Icon = s.icon;
          return (
            <div key={i} className={`card animate-fade-in stagger-${(i % 4) + 1}`} style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
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

      <div className="grid animate-fade-in stagger-3" style={{ gridTemplateColumns: '1fr', gap: '1.5rem' }}>
        <div className="card">
          <h3 style={{ marginBottom: '1.5rem', fontSize: '1.1rem' }}>{t("Doctor Performance Today")}</h3>
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
            <div style={{ 
              display: 'flex', 
              flexDirection: 'column', 
              alignItems: 'center', 
              justifyContent: 'center',
              textAlign: 'center', 
              color: 'var(--text-muted)', 
              padding: '3rem 1rem',
              border: '1px dashed var(--border-color)',
              borderRadius: 'var(--radius-md)',
              background: 'var(--bg-primary)'
            }}>
              <BarChart2 size={48} style={{ color: 'var(--border-hover)', marginBottom: '1rem' }} />
              <p style={{ fontWeight: 500, color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>{t("No data available")}</p>
              <p style={{ fontSize: '0.875rem' }}>{t("Performance metrics will appear here once doctors begin examining patients today.")}</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
