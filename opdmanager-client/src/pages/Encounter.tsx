import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import { useState, useEffect } from 'react';
import { ArrowLeft, AlertTriangle, Plus, Trash2, Send, Save, Edit2 } from 'lucide-react';
import api from '../api';
import toast from 'react-hot-toast';

interface Prescription {
  id?: number;
  medicalRecordId?: number;
  medicationName: string;
  dosage: string;
  frequency: string;
  durationDays: number | null;
  instructions: string;
}

/**
 * Encounter Component
 * The main clinical interface for an active patient visit.
 * Allows doctors to record vitals, diagnoses, and prescribe medications or lab tests.
 */
export default function Encounter() {
  const { appointmentId } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [showReferralModal, setShowReferralModal] = useState(false);

  // ── Vitals state ──────────────────────────────────────────────────────────
  const [vitals, setVitals] = useState({
    bloodPressureSystolic: '' as string | number,
    bloodPressureDiastolic: '' as string | number,
    temperatureC: '' as string | number,
    pulseRate: '' as string | number,
    respiratoryRate: '' as string | number,
    weightKg: '' as string | number,
    heightCm: '' as string | number,
    oxygenSaturation: '' as string | number,
  });

  // ── Clinical state ────────────────────────────────────────────────────────
  const [clinical, setClinical] = useState({
    chiefComplaint: '',
    diagnosis: '',
    clinicalNotes: '',
  });

  const [prescriptions, setPrescriptions] = useState<Prescription[]>([]);
  const [editingRxId, setEditingRxId] = useState<number | null>(null);
  const [newRx, setNewRx] = useState<Prescription>({
    medicationName: '', dosage: '', frequency: '', durationDays: null, instructions: ''
  });

  // ── Fetch medical record ─────────────────────────────────────────────────
  const { data: record, isLoading } = useQuery({
    queryKey: ['medicalRecord', appointmentId],
    queryFn: async () => {
      try {
        const res = await api.get(`/medical-records/by-appointment/${appointmentId}`);
        return res.data;
      } catch (e: any) {
        if (e.response?.status === 404) return null;
        throw e;
      }
    }
  });

  const { data: appointment } = useQuery({
    queryKey: ['appointment', appointmentId],
    queryFn: async () => {
      const res = await api.get(`/appointments/${appointmentId}`);
      return res.data;
    }
  });

  const { data: patient } = useQuery({
    queryKey: ['patient', appointment?.patientId],
    queryFn: async () => {
      if (!appointment?.patientId) return null;
      const res = await api.get(`/patients/${appointment.patientId}`);
      return res.data;
    },
    enabled: !!appointment?.patientId
  });

  // Populate form when record loads
  useEffect(() => {
    if (record) {
      setVitals({
        bloodPressureSystolic: record.bloodPressureSystolic ?? '',
        bloodPressureDiastolic: record.bloodPressureDiastolic ?? '',
        temperatureC: record.temperatureC ?? '',
        pulseRate: record.pulseRate ?? '',
        respiratoryRate: record.respiratoryRate ?? '',
        weightKg: record.weightKg ?? '',
        heightCm: record.heightCm ?? '',
        oxygenSaturation: record.oxygenSaturation ?? '',
      });
      setClinical({
        chiefComplaint: record.chiefComplaint ?? '',
        diagnosis: record.diagnosis ?? '',
        clinicalNotes: record.clinicalNotes ?? '',
      });
      setPrescriptions(record.prescriptions ?? []);
    }
  }, [record]);

  // ── Mutations ─────────────────────────────────────────────────────────────
  const createRecordMutation = useMutation({
    mutationFn: async () => {
      const res = await api.post('/medical-records', { appointmentId: Number(appointmentId) });
      return res.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['medicalRecord', appointmentId] });
      toast.success('Medical record created');
    },
    onError: (e: any) => toast.error(e.response?.data?.message || 'Failed to create record')
  });

  const saveVitalsMutation = useMutation({
    mutationFn: async () => {
      const payload = {
        bloodPressureSystolic: vitals.bloodPressureSystolic || null,
        bloodPressureDiastolic: vitals.bloodPressureDiastolic || null,
        temperatureC: vitals.temperatureC || null,
        pulseRate: vitals.pulseRate || null,
        respiratoryRate: vitals.respiratoryRate || null,
        weightKg: vitals.weightKg || null,
        heightCm: vitals.heightCm || null,
        oxygenSaturation: vitals.oxygenSaturation || null,
      };
      await api.put(`/medical-records/${record.id}/vitals`, payload);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['medicalRecord', appointmentId] });
      toast.success('Vitals saved');
    },
    onError: () => toast.error('Failed to save vitals')
  });

  const saveDiagnosisMutation = useMutation({
    mutationFn: async () => {
      await api.put(`/medical-records/${record.id}/diagnosis`, clinical);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['medicalRecord', appointmentId] });
      toast.success('Diagnosis saved');
    },
    onError: () => toast.error('Failed to save diagnosis')
  });

  const addPrescriptionMutation = useMutation({
    mutationFn: async () => {
      const res = await api.post('/prescriptions', {
        medicalRecordId: record.id,
        ...newRx,
        durationDays: newRx.durationDays || null
      });
      return res.data;
    },
    onSuccess: (data) => {
      setPrescriptions(prev => [...prev, data]);
      setNewRx({ medicationName: '', dosage: '', frequency: '', durationDays: null, instructions: '' });
      toast.success('Prescription added');
    },
    onError: () => toast.error('Failed to add prescription')
  });

  const updatePrescriptionMutation = useMutation({
    mutationFn: async () => {
      const res = await api.put(`/prescriptions/${editingRxId}`, {
        medicalRecordId: record.id,
        ...newRx,
        durationDays: newRx.durationDays || null
      });
      return res.data;
    },
    onSuccess: (data) => {
      setPrescriptions(prev => prev.map(p => p.id === editingRxId ? data : p));
      setEditingRxId(null);
      setNewRx({ medicationName: '', dosage: '', frequency: '', durationDays: null, instructions: '' });
      toast.success('Prescription updated');
    },
    onError: () => toast.error('Failed to update prescription')
  });

  const deletePrescriptionMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/prescriptions/${id}`);
    },
    onSuccess: (_, id) => {
      setPrescriptions(prev => prev.filter(p => p.id !== id));
      toast.success('Prescription removed');
    },
    onError: () => toast.error('Failed to remove prescription')
  });

  if (isLoading) return <div style={{ textAlign: 'center', padding: '3rem', color: 'var(--text-muted)' }}>Loading...</div>;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      {/* Header */}
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <button className="btn" onClick={() => navigate(-1)} style={{ padding: '0.5rem', background: 'transparent', border: '1px solid var(--border-color)' }}>
            <ArrowLeft size={18} />
          </button>
          <h1 className="page-title">Patient Encounter</h1>
        </div>
      </div>

      {/* Patient Info Banner */}
      {patient && (
        <div className="glass-panel" style={{ padding: '1rem 1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <div>
            <div style={{ fontWeight: 700, fontSize: '1.2rem' }}>{patient.fullName}</div>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
              {patient.gender} · DOB: {new Date(patient.dateOfBirth).toLocaleDateString()} · ID: {patient.nationalId || 'N/A'}
            </div>
          </div>
          {patient.knownAllergies && (
            <div style={{ 
              display: 'flex', alignItems: 'center', gap: '0.5rem',
              padding: '0.5rem 1rem', background: 'rgba(239, 68, 68, 0.1)', 
              borderRadius: 'var(--radius-sm)', border: '1px solid rgba(239, 68, 68, 0.3)',
              color: 'var(--danger)', fontWeight: 600
            }}>
              <AlertTriangle size={16} />
              Allergies: {patient.knownAllergies}
            </div>
          )}
          <button className="btn" onClick={() => navigate(`/patient-chart/${patient.id}`)}
            style={{ background: 'transparent', border: '1px solid var(--border-color)', color: 'var(--text-secondary)', fontSize: '0.85rem' }}>
            View Full Chart
          </button>
        </div>
      )}

      {/* Create record if not exists */}
      {!record && (
        <div className="glass-panel" style={{ padding: '3rem', textAlign: 'center' }}>
          <p style={{ color: 'var(--text-muted)', marginBottom: '1rem' }}>No medical record for this visit yet.</p>
          <button className="btn btn-primary" onClick={() => createRecordMutation.mutate()}>
            Start Medical Record
          </button>
        </div>
      )}

      {record && (
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1.5rem' }}>
          {/* Vitals */}
          <div className="glass-panel" style={{ padding: '1.5rem' }}>
            <h3 style={{ marginBottom: '1rem', color: 'var(--accent-primary)' }}>Vitals</h3>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem' }}>
              {[
                { label: 'BP Systolic', key: 'bloodPressureSystolic', unit: 'mmHg' },
                { label: 'BP Diastolic', key: 'bloodPressureDiastolic', unit: 'mmHg' },
                { label: 'Temperature', key: 'temperatureC', unit: '°C' },
                { label: 'Pulse Rate', key: 'pulseRate', unit: 'bpm' },
                { label: 'Resp. Rate', key: 'respiratoryRate', unit: '/min' },
                { label: 'O₂ Saturation', key: 'oxygenSaturation', unit: '%' },
                { label: 'Weight', key: 'weightKg', unit: 'kg' },
                { label: 'Height', key: 'heightCm', unit: 'cm' },
              ].map(field => (
                <div key={field.key}>
                  <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem', display: 'block' }}>
                    {field.label} ({field.unit})
                  </label>
                  <input
                    type="number"
                    className="input"
                    value={(vitals as any)[field.key]}
                    onChange={e => setVitals(prev => ({ ...prev, [field.key]: e.target.value }))}
                    style={{ width: '100%', padding: '0.5rem' }}
                    step="any"
                  />
                </div>
              ))}
            </div>
            <button className="btn btn-primary" onClick={() => saveVitalsMutation.mutate()} 
              style={{ marginTop: '1rem', width: '100%' }}>
              <Save size={16} /> Save Vitals
            </button>
          </div>

          {/* Diagnosis & Notes */}
          <div className="glass-panel" style={{ padding: '1.5rem' }}>
            <h3 style={{ marginBottom: '1rem', color: 'var(--accent-secondary)' }}>Diagnosis & Notes</h3>
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
              <div>
                <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem', display: 'block' }}>Chief Complaint</label>
                <input className="input" value={clinical.chiefComplaint}
                  onChange={e => setClinical(prev => ({ ...prev, chiefComplaint: e.target.value }))}
                  style={{ width: '100%', padding: '0.5rem' }} placeholder="Patient's main complaint..." />
              </div>
              <div>
                <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem', display: 'block' }}>Diagnosis</label>
                <textarea className="input" value={clinical.diagnosis}
                  onChange={e => setClinical(prev => ({ ...prev, diagnosis: e.target.value }))}
                  style={{ width: '100%', padding: '0.5rem', minHeight: '80px', resize: 'vertical' }} placeholder="Diagnosis..." />
              </div>
              <div>
                <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginBottom: '0.25rem', display: 'block' }}>Clinical Notes</label>
                <textarea className="input" value={clinical.clinicalNotes}
                  onChange={e => setClinical(prev => ({ ...prev, clinicalNotes: e.target.value }))}
                  style={{ width: '100%', padding: '0.5rem', minHeight: '80px', resize: 'vertical' }} placeholder="Additional notes..." />
              </div>
            </div>
            <button className="btn btn-primary" onClick={() => saveDiagnosisMutation.mutate()} 
              style={{ marginTop: '1rem', width: '100%' }}>
              <Save size={16} /> Save Diagnosis
            </button>
          </div>

          {/* Prescriptions */}
          <div className="glass-panel" style={{ padding: '1.5rem', gridColumn: '1 / -1' }}>
            <h3 style={{ marginBottom: '1rem', color: 'var(--success)' }}>Prescriptions</h3>
            
            {prescriptions.length > 0 && (
              <table className="table" style={{ marginBottom: '1rem' }}>
                <thead>
                  <tr>
                    <th>Medication</th><th>Dosage</th><th>Frequency</th><th>Days</th><th>Instructions</th><th></th>
                  </tr>
                </thead>
                <tbody>
                  {prescriptions.map((rx, i) => (
                    <tr key={rx.id || i}>
                      <td style={{ fontWeight: 600 }}>{rx.medicationName}</td>
                      <td>{rx.dosage}</td>
                      <td>{rx.frequency}</td>
                      <td>{rx.durationDays}</td>
                      <td style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{rx.instructions}</td>
                      <td>
                        {rx.id && (
                          <div style={{ display: 'flex', gap: '0.25rem', justifyContent: 'flex-end' }}>
                            <button className="btn btn-secondary" onClick={() => {
                              setEditingRxId(rx.id!);
                              setNewRx({ ...rx });
                            }} style={{ padding: '0.3rem', background: 'transparent', border: 'none' }}>
                              <Edit2 size={14} />
                            </button>
                            <button className="btn" onClick={() => deletePrescriptionMutation.mutate(rx.id!)}
                              style={{ padding: '0.3rem', background: 'transparent', border: 'none', color: 'var(--danger)' }}>
                              <Trash2 size={14} />
                            </button>
                          </div>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            )}

            {/* Add prescription row */}
            <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr 0.5fr 2fr auto', gap: '0.5rem', alignItems: 'end' }}>
              <div>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Medication</label>
                <input className="input" value={newRx.medicationName}
                  onChange={e => setNewRx(prev => ({ ...prev, medicationName: e.target.value }))}
                  style={{ width: '100%', padding: '0.4rem' }} placeholder="Name" />
              </div>
              <div>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Dosage</label>
                <input className="input" value={newRx.dosage}
                  onChange={e => setNewRx(prev => ({ ...prev, dosage: e.target.value }))}
                  style={{ width: '100%', padding: '0.4rem' }} placeholder="e.g. 500mg" />
              </div>
              <div>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Frequency</label>
                <input className="input" value={newRx.frequency}
                  onChange={e => setNewRx(prev => ({ ...prev, frequency: e.target.value }))}
                  style={{ width: '100%', padding: '0.4rem' }} placeholder="e.g. 3x/day" />
              </div>
              <div>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Days</label>
                <input className="input" type="number" value={newRx.durationDays ?? ''}
                  onChange={e => setNewRx(prev => ({ ...prev, durationDays: e.target.value ? Number(e.target.value) : null }))}
                  style={{ width: '100%', padding: '0.4rem' }} />
              </div>
              <div>
                <label style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>Instructions</label>
                <input className="input" value={newRx.instructions}
                  onChange={e => setNewRx(prev => ({ ...prev, instructions: e.target.value }))}
                  style={{ width: '100%', padding: '0.4rem' }} placeholder="After meals..." />
              </div>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                <button className="btn btn-primary" 
                  onClick={() => editingRxId ? updatePrescriptionMutation.mutate() : addPrescriptionMutation.mutate()}
                  disabled={!newRx.medicationName || addPrescriptionMutation.isPending || updatePrescriptionMutation.isPending} 
                  style={{ padding: '0.4rem 0.75rem' }}>
                  {editingRxId ? <Save size={16} /> : <Plus size={16} />}
                </button>
                {editingRxId && (
                  <button className="btn btn-secondary" 
                    onClick={() => {
                      setEditingRxId(null);
                      setNewRx({ medicationName: '', dosage: '', frequency: '', durationDays: null, instructions: '' });
                    }} 
                    style={{ padding: '0.4rem 0.75rem' }}>
                    Cancel
                  </button>
                )}
              </div>
            </div>
          </div>

          {/* Actions */}
          <div style={{ gridColumn: '1 / -1', display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
            <button className="btn" onClick={() => setShowReferralModal(true)}
              style={{ padding: '0.75rem 1.5rem', background: 'var(--accent-secondary)', color: 'white', border: 'none' }}>
              <Send size={16} /> Refer Patient
            </button>
          </div>
        </div>
      )}

      {/* Referral Modal */}
      {showReferralModal && record && (
        <ReferralModal
          appointmentId={Number(appointmentId)}
          onClose={() => setShowReferralModal(false)}
        />
      )}
    </div>
  );
}

// ── Referral Modal (inline) ─────────────────────────────────────────────────

function ReferralModal({ appointmentId, onClose }: { appointmentId: number; onClose: () => void }) {
  const [referralType, setReferralType] = useState<'ToDoctor' | 'ToDepartment'>('ToDepartment');
  const [targetDoctorId, setTargetDoctorId] = useState('');
  const [targetDepartmentId, setTargetDepartmentId] = useState('');
  const [targetServiceId, setTargetServiceId] = useState('');
  const [reason, setReason] = useState('');

  const { data: doctors = [] } = useQuery({
    queryKey: ['allDoctors'],
    queryFn: async () => { const r = await api.get('/doctors'); return r.data; }
  });

  const { data: departments = [] } = useQuery({
    queryKey: ['allDepartments'],
    queryFn: async () => { const r = await api.get('/departments'); return r.data; }
  });

  const selectedDept = departments.find((d: any) => d.id === Number(targetDepartmentId));

  const createReferralMutation = useMutation({
    mutationFn: async () => {
      await api.post('/referrals', {
        sourceAppointmentId: appointmentId,
        referralType,
        targetDoctorId: referralType === 'ToDoctor' ? Number(targetDoctorId) : null,
        targetDepartmentId: referralType === 'ToDepartment' ? Number(targetDepartmentId) : null,
        targetDepartmentServiceId: targetServiceId ? Number(targetServiceId) : null,
        reason
      });
    },
    onSuccess: () => {
      toast.success('Referral created');
      onClose();
    },
    onError: (e: any) => toast.error(e.response?.data?.message || 'Failed to create referral')
  });

  return (
    <div style={{
      position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.6)', display: 'flex', 
      alignItems: 'center', justifyContent: 'center', zIndex: 1000
    }} onClick={onClose}>
      <div className="glass-panel" onClick={e => e.stopPropagation()}
        style={{ width: '500px', padding: '2rem', maxHeight: '80vh', overflowY: 'auto' }}>
        <h2 style={{ marginBottom: '1.5rem' }}>Refer Patient</h2>

        <div style={{ display: 'flex', gap: '1rem', marginBottom: '1.5rem' }}>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
            <input type="radio" checked={referralType === 'ToDepartment'} 
              onChange={() => setReferralType('ToDepartment')} />
            To Department
          </label>
          <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer' }}>
            <input type="radio" checked={referralType === 'ToDoctor'} 
              onChange={() => setReferralType('ToDoctor')} />
            To Doctor
          </label>
        </div>

        {referralType === 'ToDoctor' && (
          <div style={{ marginBottom: '1rem' }}>
            <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'block', marginBottom: '0.25rem' }}>Target Doctor</label>
            <select className="input" value={targetDoctorId} onChange={e => setTargetDoctorId(e.target.value)}
              style={{ width: '100%', padding: '0.5rem' }}>
              <option value="">Select doctor...</option>
              {doctors.map((d: any) => (
                <option key={d.id} value={d.id}>Dr. {d.name} — {d.specialty}</option>
              ))}
            </select>
          </div>
        )}

        {referralType === 'ToDepartment' && (
          <>
            <div style={{ marginBottom: '1rem' }}>
              <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'block', marginBottom: '0.25rem' }}>Department</label>
              <select className="input" value={targetDepartmentId} 
                onChange={e => { setTargetDepartmentId(e.target.value); setTargetServiceId(''); }}
                style={{ width: '100%', padding: '0.5rem' }}>
                <option value="">Select department...</option>
                {departments.map((d: any) => (
                  <option key={d.id} value={d.id}>{d.name} ({d.type})</option>
                ))}
              </select>
            </div>
            {selectedDept?.services?.length > 0 && (
              <div style={{ marginBottom: '1rem' }}>
                <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'block', marginBottom: '0.25rem' }}>Service</label>
                <select className="input" value={targetServiceId} onChange={e => setTargetServiceId(e.target.value)}
                  style={{ width: '100%', padding: '0.5rem' }}>
                  <option value="">Select service...</option>
                  {selectedDept.services.map((s: any) => (
                    <option key={s.id} value={s.id}>{s.name} — {s.fee} EGP</option>
                  ))}
                </select>
              </div>
            )}
          </>
        )}

        <div style={{ marginBottom: '1.5rem' }}>
          <label style={{ fontSize: '0.8rem', color: 'var(--text-muted)', display: 'block', marginBottom: '0.25rem' }}>Reason</label>
          <textarea className="input" value={reason} onChange={e => setReason(e.target.value)}
            style={{ width: '100%', padding: '0.5rem', minHeight: '80px', resize: 'vertical' }}
            placeholder="Reason for referral..." />
        </div>

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
          <button className="btn" onClick={onClose} style={{ padding: '0.5rem 1rem' }}>Cancel</button>
          <button className="btn btn-primary" onClick={() => createReferralMutation.mutate()}
            style={{ padding: '0.5rem 1rem' }}>
            <Send size={16} /> Create Referral
          </button>
        </div>
      </div>
    </div>
  );
}
