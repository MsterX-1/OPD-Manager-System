import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../api';
import toast from 'react-hot-toast';
import { Plus, Trash2, Edit2, X, Search, ChevronLeft, ChevronRight, History, FileText } from 'lucide-react';
import ConfirmModal from '../components/ConfirmModal';

interface Patient {
  id: number;
  fullName: string;
  phoneNumber: string;
  dateOfBirth: string | null;
  notes: string | null;
  nationalId: string | null;
  gender: string;
  knownAllergies: string | null;
  createdAt: string;
}

export default function Patients() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [showHistoryModal, setShowHistoryModal] = useState(false);
  const [selectedPatient, setSelectedPatient] = useState<Patient | null>(null);
  const [patientToDelete, setPatientToDelete] = useState<Patient | null>(null);
  
  const [formData, setFormData] = useState<Omit<Patient, 'id' | 'createdAt'>>({
    fullName: '',
    phoneNumber: '',
    dateOfBirth: '',
    notes: '',
    nationalId: '',
    gender: 'Unknown',
    knownAllergies: ''
  });

  // Debounce search
  useState(() => {
    const handler = setTimeout(() => {
      setDebouncedSearch(searchTerm);
      setPage(1);
    }, 500);
    return () => clearTimeout(handler);
  });

  const { data, isLoading } = useQuery({
    queryKey: ['patients', debouncedSearch, page],
    queryFn: async () => {
      const url = debouncedSearch 
        ? `/patients/search?term=${encodeURIComponent(debouncedSearch)}&page=${page}`
        : `/patients?page=${page}`;
      const res = await api.get(url);
      return res.data;
    }
  });

  const { data: historyData, isLoading: historyLoading } = useQuery({
    queryKey: ['patientHistory', selectedPatient?.id],
    queryFn: async () => {
      if (!selectedPatient) return null;
      const res = await api.get(`/patients/${selectedPatient.id}/history`);
      return res.data;
    },
    enabled: !!selectedPatient && showHistoryModal
  });

  const createMutation = useMutation({
    mutationFn: async (data: any) => await api.post('/patients', data),
    onSuccess: () => {
      toast.success('Patient created successfully');
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      queryClient.invalidateQueries({ queryKey: ['allPatients'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to create patient')
  });

  const updateMutation = useMutation({
    mutationFn: async ({ id, data }: { id: number, data: any }) => await api.put(`/patients/${id}`, data),
    onSuccess: () => {
      toast.success('Patient updated successfully');
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      queryClient.invalidateQueries({ queryKey: ['allPatients'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to update patient')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => await api.delete(`/patients/${id}`),
    onSuccess: () => {
      toast.success('Patient deleted');
      queryClient.invalidateQueries({ queryKey: ['patients'] });
      queryClient.invalidateQueries({ queryKey: ['allPatients'] });
    },
    onError: () => toast.error('Failed to delete patient. Ensure they have no appointments.')
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const payload = {
      ...formData,
      dateOfBirth: formData.dateOfBirth || null
    };

    if (selectedPatient && !showHistoryModal) {
      updateMutation.mutate({ id: selectedPatient.id, data: payload });
    } else {
      createMutation.mutate(payload);
    }
  };

  const openNew = () => {
    setSelectedPatient(null);
    setFormData({ fullName: '', phoneNumber: '', dateOfBirth: '', notes: '', nationalId: '', gender: 'Unknown', knownAllergies: '' });
    setShowModal(true);
  };

  const openEdit = (patient: Patient) => {
    setSelectedPatient(patient);
    setFormData({ 
      fullName: patient.fullName, 
      phoneNumber: patient.phoneNumber || '', 
      dateOfBirth: patient.dateOfBirth ? patient.dateOfBirth.split('T')[0] : '', 
      notes: patient.notes || '',
      nationalId: patient.nationalId || '',
      gender: patient.gender || 'Unknown',
      knownAllergies: patient.knownAllergies || ''
    });
    setShowModal(true);
  };

  const openHistory = (patient: Patient) => {
    setSelectedPatient(patient);
    setShowHistoryModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setShowHistoryModal(false);
    setSelectedPatient(null);
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Patient Directory</h1>
        <div style={{ display: 'flex', gap: '1rem' }}>
          <div style={{ position: 'relative' }}>
            <Search size={18} style={{ position: 'absolute', left: '10px', top: '10px', color: 'var(--text-muted)' }} />
            <input 
              type="text" 
              placeholder="Search patients..." 
              value={searchTerm}
              onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
              style={{ paddingLeft: '2.5rem', width: '300px' }}
            />
          </div>
          <button className="btn btn-primary" onClick={openNew}>
            <Plus size={18} /> Add Patient
          </button>
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Phone</th>
              <th>Gender</th>
              <th>Date of Birth</th>
              <th>Registered</th>
              <th style={{ textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={5} style={{ textAlign: 'center' }}>Loading...</td></tr>
            ) : data?.items?.length === 0 ? (
              <tr><td colSpan={5} style={{ textAlign: 'center' }}>No patients found.</td></tr>
            ) : data?.items?.map((p: Patient) => (
              <tr key={p.id}>
                <td style={{ fontWeight: 500 }}>{p.fullName}</td>
                <td>{p.phoneNumber || '-'}</td>
                <td>{p.gender}</td>
                <td>{p.dateOfBirth ? new Date(p.dateOfBirth).toLocaleDateString() : '-'}</td>
                <td>{new Date(p.createdAt).toLocaleDateString()}</td>
                <td style={{ textAlign: 'right' }}>
                  <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                    <a href={`/patient-chart/${p.id}`} className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem', textDecoration: 'none' }} title="Medical Chart">
                      <FileText size={16} />
                    </a>
                    <button className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem' }} onClick={() => openHistory(p)} title="History">
                      <History size={16} />
                    </button>
                    <button className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem' }} onClick={() => openEdit(p)} title="Edit">
                      <Edit2 size={16} />
                    </button>
                    <button className="btn btn-danger" style={{ padding: '0.35rem 0.5rem' }} onClick={() => setPatientToDelete(p)} title="Delete">
                      <Trash2 size={16} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        
        {/* Pagination */}
        {data && data.totalPages > 1 && (
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '1rem', borderTop: '1px solid var(--border-color)' }}>
            <div style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
              Showing {(data.page - 1) * data.pageSize + 1} to {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} entries
            </div>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button 
                className="btn btn-secondary" 
                disabled={!data.hasPrevious} 
                onClick={() => setPage(p => p - 1)}
                style={{ padding: '0.5rem' }}
              >
                <ChevronLeft size={18} />
              </button>
              <button 
                className="btn btn-secondary" 
                disabled={!data.hasNext} 
                onClick={() => setPage(p => p + 1)}
                style={{ padding: '0.5rem' }}
              >
                <ChevronRight size={18} />
              </button>
            </div>
          </div>
        )}
      </div>

      {showModal && !showHistoryModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
          padding: '2rem'
        }}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '500px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>{selectedPatient ? 'Edit Patient' : 'New Patient'}</h2>
              <button onClick={closeModal} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Full Name</label>
                <input required type="text" value={formData.fullName} onChange={e => setFormData({...formData, fullName: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Phone Number</label>
                <input type="text" value={formData.phoneNumber} onChange={e => setFormData({...formData, phoneNumber: e.target.value})} />
              </div>
              <div className="form-group" style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div>
                  <label>Date of Birth</label>
                  <input type="date" value={formData.dateOfBirth || ''} onChange={e => setFormData({...formData, dateOfBirth: e.target.value})} />
                </div>
                <div>
                  <label>Gender</label>
                  <select value={formData.gender} onChange={e => setFormData({...formData, gender: e.target.value})}>
                    <option value="Unknown">Unknown</option>
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label>National ID</label>
                <input type="text" value={formData.nationalId || ''} onChange={e => setFormData({...formData, nationalId: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Known Allergies</label>
                <input type="text" value={formData.knownAllergies || ''} onChange={e => setFormData({...formData, knownAllergies: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Notes (Optional)</label>
                <textarea rows={3} value={formData.notes || ''} onChange={e => setFormData({...formData, notes: e.target.value})} />
              </div>

              <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={createMutation.isPending || updateMutation.isPending}>
                  Save Patient
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showHistoryModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
          padding: '2rem'
        }}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '700px', maxHeight: '90vh', display: 'flex', flexDirection: 'column' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>Patient History: {selectedPatient?.fullName}</h2>
              <button onClick={closeModal} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <div style={{ overflowY: 'auto', flex: 1 }}>
              {historyLoading ? (
                <div style={{ textAlign: 'center', padding: '2rem' }}>Loading history...</div>
              ) : historyData?.items?.length === 0 ? (
                <div style={{ textAlign: 'center', color: 'var(--text-muted)', padding: '2rem' }}>No appointment history found.</div>
              ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
                  {historyData?.items?.map((appt: any) => (
                    <div key={appt.id} className="glass-panel" style={{ padding: '1rem' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                        <strong style={{ color: 'var(--accent-primary)' }}>{new Date(appt.appointmentDate).toLocaleDateString()}</strong>
                        <span className={`badge badge-${appt.status === 'Completed' ? 'success' : appt.status === 'Cancelled' || appt.status === 'NoShow' ? 'danger' : 'info'}`}>
                          {appt.status}
                        </span>
                      </div>
                      <div style={{ marginBottom: '0.5rem' }}>Dr. {appt.doctorName} ({appt.doctorSpecialty})</div>
                      {appt.notes && <div style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', background: 'var(--bg-tertiary)', padding: '0.5rem', borderRadius: '4px' }}>Note: {appt.notes}</div>}
                    </div>
                  ))}
                </div>
              )}
            </div>
            
            <div style={{ marginTop: '1.5rem', textAlign: 'right' }}>
              <button className="btn btn-secondary" onClick={closeModal}>Close</button>
            </div>
          </div>
        </div>
      )}

      {patientToDelete && (
        <ConfirmModal
          title="Delete Patient"
          message={`Are you sure you want to delete ${patientToDelete.fullName}? This cannot be undone.`}
          confirmText="Delete Patient"
          onConfirm={() => deleteMutation.mutate(patientToDelete.id)}
          onCancel={() => setPatientToDelete(null)}
        />
      )}
    </div>
  );
}
