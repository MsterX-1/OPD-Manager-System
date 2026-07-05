import { useQuery } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Stethoscope, FileText, Share2 } from 'lucide-react';
import api from '../api';

export default function PatientChart() {
  const { patientId } = useParams();
  const navigate = useNavigate();

  const { data: chart, isLoading } = useQuery({
    queryKey: ['patientChart', patientId],
    queryFn: async () => {
      const res = await api.get(`/patients/${patientId}/chart`);
      return res.data;
    }
  });

  if (isLoading) return <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-muted)' }}>Loading...</div>;
  if (!chart) return <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--danger)' }}>Chart not found</div>;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <button className="btn" onClick={() => navigate(-1)} style={{ padding: '0.5rem', background: 'transparent', border: '1px solid var(--border-color)' }}>
            <ArrowLeft size={18} />
          </button>
          <h1 className="page-title" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <FileText size={28} />
            Patient Chart
          </h1>
        </div>
      </div>

      <div className="glass-panel" style={{ padding: '1.5rem' }}>
        <h2 style={{ fontSize: '1.5rem', fontWeight: 700, marginBottom: '0.25rem' }}>{chart.patientName}</h2>
        {chart.knownAllergies && (
          <div style={{ color: 'var(--danger)', fontWeight: 600, fontSize: '0.9rem', marginTop: '0.5rem' }}>
            ⚠️ Known Allergies: {chart.knownAllergies}
          </div>
        )}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: '1.5rem' }}>
        {/* Medical Records */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <h3 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--accent-primary)' }}>
            <Stethoscope size={20} />
            Encounter History
          </h3>
          
          {chart.medicalRecords?.length === 0 && (
            <div className="glass-panel" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
              No medical records found.
            </div>
          )}

          {chart.medicalRecords?.map((record: any) => (
            <div key={record.id} className="glass-panel" style={{ padding: '1.5rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                <span style={{ fontWeight: 600, color: 'var(--accent-primary)' }}>
                  {new Date(record.createdAt).toLocaleDateString()}
                </span>
                <span style={{ color: 'var(--text-secondary)' }}>Dr. {record.doctorName}</span>
              </div>

              {/* Vitals summary */}
              <div style={{ display: 'flex', gap: '1rem', marginBottom: '1rem', flexWrap: 'wrap' }}>
                {record.bloodPressureSystolic && (
                  <span className="badge badge-neutral">BP: {record.bloodPressureSystolic}/{record.bloodPressureDiastolic}</span>
                )}
                {record.temperatureC && <span className="badge badge-neutral">Temp: {record.temperatureC}°C</span>}
                {record.pulseRate && <span className="badge badge-neutral">Pulse: {record.pulseRate} bpm</span>}
                {record.weightKg && <span className="badge badge-neutral">Weight: {record.weightKg} kg</span>}
              </div>

              {record.chiefComplaint && (
                <div style={{ marginBottom: '0.5rem' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.85rem', color: 'var(--text-muted)' }}>Chief Complaint:</span>
                  <p style={{ margin: 0 }}>{record.chiefComplaint}</p>
                </div>
              )}

              {record.diagnosis && (
                <div style={{ marginBottom: '0.5rem' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.85rem', color: 'var(--text-muted)' }}>Diagnosis:</span>
                  <p style={{ margin: 0 }}>{record.diagnosis}</p>
                </div>
              )}

              {record.clinicalNotes && (
                <div style={{ marginBottom: '1rem' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.85rem', color: 'var(--text-muted)' }}>Notes:</span>
                  <p style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{record.clinicalNotes}</p>
                </div>
              )}

              {/* Prescriptions summary */}
              {record.prescriptions?.length > 0 && (
                <div style={{ marginTop: '1rem', background: 'var(--bg-tertiary)', padding: '1rem', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.85rem', color: 'var(--success)', display: 'block', marginBottom: '0.5rem' }}>Prescriptions:</span>
                  <ul style={{ margin: 0, paddingLeft: '1.2rem', fontSize: '0.9rem' }}>
                    {record.prescriptions.map((rx: any) => (
                      <li key={rx.id}>
                        <span style={{ fontWeight: 600 }}>{rx.medicationName}</span> {rx.dosage} — {rx.frequency} 
                        {rx.durationDays ? ` for ${rx.durationDays} days` : ''}
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* Referrals */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <h3 style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--accent-secondary)' }}>
            <Share2 size={20} />
            Referrals & Tests
          </h3>

          {chart.referrals?.length === 0 && (
            <div className="glass-panel" style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
              No referrals found.
            </div>
          )}

          {chart.referrals?.map((ref: any) => (
            <div key={ref.id} className="glass-panel" style={{ padding: '1.25rem', borderLeft: `3px solid ${ref.status === 'Completed' ? 'var(--success)' : 'var(--warning)'}` }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                <span style={{ fontWeight: 600 }}>
                  {ref.referralType === 'ToDepartment' ? ref.targetDepartmentName : `Dr. ${ref.targetDoctorName}`}
                </span>
                <span className={`badge badge-${ref.status === 'Completed' ? 'success' : 'warning'}`}>
                  {ref.status}
                </span>
              </div>
              
              <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.5rem' }}>
                {new Date(ref.createdAt).toLocaleDateString()} · Ordered by Dr. {ref.referringDoctorName}
              </div>

              {ref.targetServiceName && (
                <div style={{ fontSize: '0.9rem', marginBottom: '0.5rem' }}>
                  <span style={{ fontWeight: 600 }}>Service:</span> {ref.targetServiceName}
                </div>
              )}

              {ref.resultNotes && (
                <div style={{ marginTop: '0.75rem', padding: '0.75rem', background: 'var(--bg-tertiary)', borderRadius: 'var(--radius-sm)' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.8rem', color: 'var(--text-muted)', display: 'block', marginBottom: '0.25rem' }}>Result:</span>
                  <p style={{ margin: 0, fontSize: '0.9rem', whiteSpace: 'pre-wrap' }}>{ref.resultNotes}</p>
                </div>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
