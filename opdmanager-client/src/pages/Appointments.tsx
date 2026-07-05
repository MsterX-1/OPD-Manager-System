import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState, useEffect } from 'react';
import * as signalR from '@microsoft/signalr';
import api from '../api';
import toast from 'react-hot-toast';
import { Search, Plus, X, Edit2, Trash2, Calendar } from 'lucide-react';
import Select from 'react-select';
import ConfirmModal from '../components/ConfirmModal';

const customStyles = {
  control: (base: any, state: any) => ({
    ...base,
    background: 'var(--bg-primary)',
    borderColor: state.isFocused ? 'var(--accent-primary)' : 'var(--border-color)',
    color: 'var(--text-primary)',
    boxShadow: 'none',
    '&:hover': {
      borderColor: 'var(--border-hover)'
    }
  }),
  menu: (base: any) => ({
    ...base,
    background: 'var(--bg-secondary)',
    border: '1px solid var(--border-color)',
    zIndex: 100
  }),
  option: (base: any, state: any) => ({
    ...base,
    background: state.isSelected ? 'var(--accent-primary)' : state.isFocused ? 'var(--bg-tertiary)' : 'transparent',
    color: state.isSelected ? '#fff' : 'var(--text-primary)',
    cursor: 'pointer',
    '&:active': {
      background: 'var(--accent-primary)'
    }
  }),
  singleValue: (base: any) => ({
    ...base,
    color: 'var(--text-primary)',
  }),
  input: (base: any) => ({
    ...base,
    color: 'var(--text-primary)',
  })
};

const FULL_DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

const format12H = (time24: string) => {
  if (!time24) return '';
  const [h, m] = time24.split(':');
  const hours = parseInt(h, 10);
  const suffix = hours >= 12 ? 'PM' : 'AM';
  const hours12 = hours % 12 || 12;
  return `${hours12}:${m} ${suffix}`;
};

const getNextAvailableDates = (doctor: any, count = 14) => {
  if (!doctor || !doctor.schedules) return [];
  const workingDays = doctor.schedules.filter((s: any) => s.isWorking);
  if (workingDays.length === 0) return [];
  
  const dates = [];
  const today = new Date();
  today.setHours(0, 0, 0, 0); // Start at midnight for comparison
  let current = new Date(today);
  
  while (dates.length < count) {
    const dayOfWeek = current.getDay();
    if (workingDays.some((s: any) => s.dayOfWeek === dayOfWeek || s.dayOfWeek === FULL_DAYS[dayOfWeek])) {
      dates.push(new Date(current));
    }
    current.setDate(current.getDate() + 1);
    
    // Safety break to prevent infinite loops if something goes wrong
    if (dates.length === 0 && current.getTime() - today.getTime() > 30 * 24 * 60 * 60 * 1000) break;
  }
  return dates;
};

const isWorkingDay = (dateStr: string, doctor: any) => {
  if (!dateStr || !doctor) return true;
  const [year, month, day] = dateStr.split('-').map(Number);
  const dateObj = new Date(year, month - 1, day);
  const dayOfWeek = dateObj.getDay();
  const schedule = doctor.schedules.find((s: any) => s.dayOfWeek === dayOfWeek || s.dayOfWeek === FULL_DAYS[dayOfWeek]);
  return !!(schedule && schedule.isWorking);
};

/**
 * Appointments Component
 * Allows receptionists to book, cancel, or check-in scheduled patient appointments.
 */
export default function Appointments() {
  const queryClient = useQueryClient();
  const [mode, setMode] = useState<'date' | 'search'>('date');
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  
  // Search state
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [appointmentToCancel, setAppointmentToCancel] = useState<any | null>(null);
  const [appointmentToDelete, setAppointmentToDelete] = useState<any | null>(null);
  const [customDateMode, setCustomDateMode] = useState(false);
  
  const [formData, setFormData] = useState({
    patientId: '',
    doctorId: '',
    appointmentDate: new Date().toISOString().split('T')[0],
    notes: ''
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
    queryKey: ['appointments', mode, date, debouncedSearch, page],
    queryFn: async () => {
      if (mode === 'date') {
        const res = await api.get(`/appointments/by-date?date=${date}&page=${page}&pageSize=15`);
        return res.data;
      } else {
        const url = debouncedSearch 
          ? `/appointments/search?patientName=${encodeURIComponent(debouncedSearch)}&page=${page}&pageSize=15`
          : `/appointments/search?page=${page}&pageSize=15`;
        const res = await api.get(url);
        return res.data;
      }
    }
  });

  const { data: patients } = useQuery({
    queryKey: ['allPatients'],
    queryFn: async () => {
      const res = await api.get('/patients?page=1&pageSize=1000');
      return res.data.items;
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
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    });

    connection.start().catch(err => {
      if (isMounted) console.error("Appointments SignalR Error: ", err);
    });

    return () => {
      isMounted = false;
      connection.stop();
    };
  }, [queryClient]);

  const { data: doctors } = useQuery({
    queryKey: ['activeDoctors'],
    queryFn: async () => {
      const res = await api.get('/doctors/active');
      return res.data;
    }
  });

  const selectedDoctorId = formData.doctorId;
  const selectedDate = formData.appointmentDate;
  
  const { data: availability } = useQuery({
    queryKey: ['availability', selectedDoctorId, selectedDate],
    queryFn: async () => {
      if (!selectedDoctorId || !selectedDate) return null;
      const res = await api.get(`/appointments/availability?doctorId=${selectedDoctorId}&date=${selectedDate}`);
      return res.data;
    },
    enabled: !!selectedDoctorId && !!selectedDate
  });

  const checkInMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.post(`/queue/check-in/${id}`);
    },
    onSuccess: () => {
      toast.success('Patient checked in');
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || 'Failed to check in');
    }
  });

  const cancelMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.patch(`/appointments/${id}/cancel`);
    },
    onSuccess: () => {
      toast.success('Appointment cancelled');
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    }
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/appointments/${id}`);
    },
    onSuccess: () => {
      toast.success('Appointment deleted');
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
    },
    onError: () => toast.error('Failed to delete appointment')
  });

  const bookMutation = useMutation({
    mutationFn: async (data: any) => {
      await api.post('/appointments', data);
    },
    onSuccess: () => {
      toast.success('Appointment booked successfully');
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      setShowModal(false);
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || 'Failed to book appointment');
    }
  });

  const updateMutation = useMutation({
    mutationFn: async ({ id, data }: { id: number, data: any }) => {
      await api.put(`/appointments/${id}`, data);
    },
    onSuccess: () => {
      toast.success('Appointment updated successfully');
      queryClient.invalidateQueries({ queryKey: ['appointments'] });
      setShowModal(false);
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || 'Failed to update appointment');
    }
  });

  const handleBook = (e: React.FormEvent) => {
    e.preventDefault();
    
    const payload = {
      patientId: parseInt(formData.patientId),
      doctorId: parseInt(formData.doctorId),
      appointmentDate: formData.appointmentDate,
      notes: formData.notes
    };

    if (editingId) {
      updateMutation.mutate({ id: editingId, data: payload });
    } else {
      bookMutation.mutate(payload);
    }
  };

  const openNew = () => {
    setEditingId(null);
    setCustomDateMode(false);
    setFormData({
      patientId: '',
      doctorId: '',
      appointmentDate: '',
      notes: ''
    });
    setShowModal(true);
  };

  const openEdit = (appointment: any) => {
    setEditingId(appointment.id);
    setCustomDateMode(true);
    const [datePart] = appointment.appointmentDate.split('T');
    setFormData({
      patientId: appointment.patientId.toString(),
      doctorId: appointment.doctorId.toString(),
      appointmentDate: datePart,
      notes: appointment.notes || ''
    });
    setShowModal(true);
  };

  const patientOptions = patients?.map((p: any) => ({
    value: p.id.toString(),
    label: `${p.fullName} (${p.phoneNumber || 'No phone'})`
  })) || [];

  const doctorOptions = doctors?.map((d: any) => ({
    value: d.id.toString(),
    label: `Dr. ${d.name} (${d.specialty})`,
    doctor: d
  })) || [];

  const selectedDoctor = doctorOptions.find((o: any) => o.value === formData.doctorId)?.doctor;
  const isValidDate = selectedDoctor && formData.appointmentDate ? isWorkingDay(formData.appointmentDate, selectedDoctor) : true;

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Appointments</h1>
        
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          
          <div style={{ display: 'flex', background: 'var(--bg-tertiary)', borderRadius: 'var(--radius-md)', padding: '0.25rem' }}>
            <button 
              className={`btn ${mode === 'date' ? 'btn-primary' : ''}`}
              style={mode !== 'date' ? { background: 'transparent', border: 'none', color: 'var(--text-secondary)' } : { padding: '0.5rem 1rem' }}
              onClick={() => setMode('date')}
            >
              <Calendar size={16} /> By Date
            </button>
            <button 
              className={`btn ${mode === 'search' ? 'btn-primary' : ''}`}
              style={mode !== 'search' ? { background: 'transparent', border: 'none', color: 'var(--text-secondary)' } : { padding: '0.5rem 1rem' }}
              onClick={() => setMode('search')}
            >
              <Search size={16} /> Search
            </button>
          </div>

          {mode === 'date' ? (
            <input 
              type="date" 
              value={date} 
              onChange={e => { setDate(e.target.value); setPage(1); }}
              style={{ width: 'auto' }}
            />
          ) : (
            <div style={{ position: 'relative' }}>
              <Search size={16} style={{ position: 'absolute', left: '10px', top: '12px', color: 'var(--text-muted)' }} />
              <input 
                type="text" 
                placeholder="Search patient name..." 
                value={searchTerm}
                onChange={e => { setSearchTerm(e.target.value); setPage(1); }}
                style={{ paddingLeft: '2.25rem', width: '250px' }}
              />
            </div>
          )}
          
          <button className="btn btn-primary" onClick={openNew}>
            <Plus size={18} /> Book Appointment
          </button>
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              {mode === 'search' && <th>Date</th>}
              <th>Patient</th>
              <th>Doctor</th>
              <th>Status</th>
              <th>Queue #</th>
              <th style={{ textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={mode === 'search' ? 6 : 5} style={{ textAlign: 'center' }}>Loading...</td></tr>
            ) : data?.items?.length === 0 ? (
              <tr><td colSpan={mode === 'search' ? 6 : 5} style={{ textAlign: 'center' }}>No appointments found</td></tr>
            ) : (
              data?.items?.map((a: any) => (
                <tr key={a.id}>
                  {mode === 'search' && <td>
                    {(() => {
                      const [y, m, d] = a.appointmentDate.split('T')[0].split('-').map(Number);
                      const dateObj = new Date(y, m - 1, d);
                      return `${FULL_DAYS[dateObj.getDay()]}, ${dateObj.toLocaleDateString()}`;
                    })()}
                  </td>}
                  <td style={{ fontWeight: 500 }}>{a.patientName}</td>
                  <td>Dr. {a.doctorName} <span style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>({a.doctorSpecialty})</span></td>
                  <td>
                    <span className={`badge badge-${a.status === 'Scheduled' ? 'info' : a.status === 'Completed' ? 'success' : a.status === 'Cancelled' ? 'danger' : 'warning'}`}>
                      {a.status}
                    </span>
                  </td>
                  <td>{a.queueNumber ? `#${a.queueNumber}` : '-'}</td>
                  <td style={{ textAlign: 'right' }}>
                    <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                      {a.status === 'Scheduled' && (
                        <>
                          <button 
                            className="btn btn-primary" 
                            style={{ padding: '0.25rem 0.75rem', fontSize: '0.85rem' }}
                            onClick={() => checkInMutation.mutate(a.id)}
                          >
                            Check-in
                          </button>
                          <button 
                            className="btn btn-secondary" 
                            style={{ padding: '0.35rem 0.5rem' }}
                            onClick={() => openEdit(a)}
                            title="Edit"
                          >
                            <Edit2 size={16} />
                          </button>
                          <button 
                            className="btn btn-secondary" 
                            style={{ padding: '0.35rem 0.5rem', color: 'var(--warning)', borderColor: 'var(--warning)' }}
                            onClick={() => setAppointmentToCancel(a)}
                            title="Cancel"
                          >
                            <X size={16} />
                          </button>
                        </>
                      )}
                      
                      <button 
                        className="btn btn-danger" 
                        style={{ padding: '0.35rem 0.5rem' }}
                        onClick={() => setAppointmentToDelete(a)}
                        title="Delete Permanently"
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
        
        {data && data.totalPages > 1 && (
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '1rem', borderTop: '1px solid var(--border-color)' }}>
            <div style={{ color: 'var(--text-muted)', fontSize: '0.875rem' }}>
              Showing {(data.page - 1) * data.pageSize + 1} to {Math.min(data.page * data.pageSize, data.totalCount)} of {data.totalCount} entries
            </div>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button className="btn btn-secondary" disabled={!data.hasPrevious} onClick={() => setPage(p => p - 1)} style={{ padding: '0.5rem' }}>Prev</button>
              <button className="btn btn-secondary" disabled={!data.hasNext} onClick={() => setPage(p => p + 1)} style={{ padding: '0.5rem' }}>Next</button>
            </div>
          </div>
        )}
      </div>

      {showModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
          padding: '2rem'
        }}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '500px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>{editingId ? 'Edit Appointment' : 'Book Appointment'}</h2>
              <button onClick={() => setShowModal(false)} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleBook}>
              <div className="form-group" style={{ marginBottom: '1.25rem' }}>
                <label>Patient</label>
                <Select 
                  styles={customStyles}
                  options={patientOptions}
                  value={patientOptions.find((o: any) => o.value === formData.patientId) || null}
                  onChange={(option: any) => setFormData({...formData, patientId: option ? option.value : ''})}
                  isDisabled={!!editingId}
                  placeholder="Search for a patient..."
                  noOptionsMessage={() => "No patients found"}
                />
                {!!editingId && <small style={{ color: 'var(--warning)', display: 'block', marginTop: '0.25rem' }}>Cannot change patient for an existing appointment.</small>}
              </div>

              <div className="form-group" style={{ marginBottom: '1.25rem' }}>
                <label>Doctor</label>
                <Select 
                  styles={customStyles}
                  options={doctorOptions}
                  value={doctorOptions.find((o: any) => o.value === formData.doctorId) || null}
                  onChange={(option: any) => setFormData({...formData, doctorId: option ? option.value : ''})}
                  placeholder="Search for a doctor..."
                  noOptionsMessage={() => "No active doctors found"}
                />
                
                {selectedDoctor && (
                  <div className="animate-fade-in" style={{ marginTop: '0.75rem', padding: '0.75rem', background: 'rgba(59, 130, 246, 0.08)', border: '1px solid rgba(59, 130, 246, 0.2)', borderRadius: 'var(--radius-md)' }}>
                    <p style={{ margin: '0 0 0.5rem 0', fontWeight: 600, fontSize: '0.85rem', color: 'var(--accent-primary)', display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
                      <Calendar size={14} /> Doctor's Working Schedule
                    </p>
                    <div style={{ display: 'grid', gridTemplateColumns: '1fr', gap: '0.35rem' }}>
                      {selectedDoctor.schedules.filter((s: any) => s.isWorking).map((s: any) => {
                        const dayName = typeof s.dayOfWeek === 'string' ? s.dayOfWeek : FULL_DAYS[s.dayOfWeek];
                        return (
                          <div key={s.dayOfWeek} style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                            <span style={{ fontWeight: 500, color: 'var(--text-primary)', display: 'inline-block', width: '90px' }}>
                              {dayName}
                            </span> 
                            {format12H(s.startTime)} - {format12H(s.endTime)}
                          </div>
                        );
                      })}
                      {selectedDoctor.schedules.filter((s: any) => s.isWorking).length === 0 && (
                        <div style={{ fontSize: '0.85rem', color: 'var(--warning)' }}>No working days configured.</div>
                      )}
                    </div>
                  </div>
                )}
              </div>

              <div className="form-group" style={{ marginBottom: '1.25rem' }}>
                <label>Date</label>
                <div style={{ display: 'flex', gap: '0.75rem' }}>
                  {!customDateMode && selectedDoctor && !editingId ? (
                    <select 
                      required 
                      value={formData.appointmentDate}
                      onChange={(e) => {
                        const val = e.target.value;
                        if (val === 'custom') {
                          setCustomDateMode(true);
                          setFormData({...formData, appointmentDate: ''});
                        } else {
                          setFormData({...formData, appointmentDate: val});
                        }
                      }}
                      style={{ flex: 1 }}
                    >
                      <option value="">-- Select Date --</option>
                      {getNextAvailableDates(selectedDoctor).map((d: Date) => {
                        const y = d.getFullYear();
                        const m = String(d.getMonth() + 1).padStart(2, '0');
                        const day = String(d.getDate()).padStart(2, '0');
                        const dateStr = `${y}-${m}-${day}`;
                        const displayStr = `${FULL_DAYS[d.getDay()]}, ${d.toLocaleDateString()}`;
                        return <option key={dateStr} value={dateStr}>{displayStr}</option>;
                      })}
                      <option value="custom">Other Date...</option>
                    </select>
                  ) : (
                    <input 
                      required 
                      type="date" 
                      value={formData.appointmentDate} 
                      onChange={e => setFormData({...formData, appointmentDate: e.target.value})} 
                      style={{ flex: 1 }}
                    />
                  )}
                </div>
                {customDateMode && formData.appointmentDate && selectedDoctor && !isValidDate && (
                  <div className="animate-fade-in" style={{ color: 'var(--danger)', fontSize: '0.85rem', marginTop: '0.5rem', background: 'rgba(239, 68, 68, 0.1)', padding: '0.5rem', borderRadius: '4px', border: '1px solid rgba(239, 68, 68, 0.2)' }}>
                    {(() => {
                      const [y, m, d] = formData.appointmentDate.split('-').map(Number);
                      const dayName = FULL_DAYS[new Date(y, m - 1, d).getDay()];
                      return `Doctor does not work on ${dayName}s. Please pick a valid date.`;
                    })()}
                  </div>
                )}
                {customDateMode && !editingId && selectedDoctor && (
                  <button type="button" onClick={() => setCustomDateMode(false)} style={{ background: 'none', border: 'none', color: 'var(--accent-primary)', fontSize: '0.85rem', marginTop: '0.5rem', cursor: 'pointer', padding: 0 }}>
                    Show suggested dates
                  </button>
                )}

                {availability?.isCapEnabled && (
                  <div className="animate-fade-in" style={{ marginTop: '0.75rem', padding: '0.75rem', background: availability.remainingSlots <= 2 ? 'rgba(239, 68, 68, 0.08)' : 'rgba(16, 185, 129, 0.08)', border: `1px solid ${availability.remainingSlots <= 2 ? 'rgba(239, 68, 68, 0.2)' : 'rgba(16, 185, 129, 0.2)'}`, borderRadius: 'var(--radius-md)' }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                      <span style={{ fontWeight: 500, fontSize: '0.85rem', color: availability.remainingSlots <= 2 ? 'var(--danger)' : 'var(--success)' }}>
                        Daily Capacity Status
                      </span>
                      <span className={`badge badge-${availability.remainingSlots <= 2 ? 'danger' : 'success'}`}>
                        {availability.remainingSlots} slots left
                      </span>
                    </div>
                    <div style={{ marginTop: '0.5rem', height: '6px', background: 'var(--bg-tertiary)', borderRadius: '3px', overflow: 'hidden' }}>
                      <div style={{ height: '100%', width: `${Math.min(100, (availability.currentCount / availability.maxCap) * 100)}%`, background: availability.remainingSlots <= 2 ? 'var(--danger)' : 'var(--success)', transition: 'width 0.3s ease' }} />
                    </div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginTop: '0.25rem', fontSize: '0.75rem', color: 'var(--text-muted)' }}>
                      <span>Booked: {availability.currentCount}</span>
                      <span>Max: {availability.maxCap}</span>
                    </div>
                  </div>
                )}
              </div>

              <div className="form-group" style={{ marginBottom: '1.5rem' }}>
                <label>Notes (Optional)</label>
                <textarea rows={3} value={formData.notes} onChange={e => setFormData({...formData, notes: e.target.value})} placeholder="Any special requirements or reasons..." />
              </div>

              <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem', paddingTop: '1.5rem', borderTop: '1px solid var(--border-color)' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={bookMutation.isPending || updateMutation.isPending || !formData.patientId || !formData.doctorId || !isValidDate}>
                  {bookMutation.isPending || updateMutation.isPending ? 'Saving...' : (editingId ? 'Update Appointment' : 'Confirm Booking')}
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={() => setShowModal(false)}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {appointmentToCancel && (
        <ConfirmModal
          title="Cancel Appointment"
          message={`Are you sure you want to cancel this appointment for ${appointmentToCancel.patientName}?`}
          confirmText="Cancel Appointment"
          onConfirm={() => cancelMutation.mutate(appointmentToCancel.id)}
          onCancel={() => setAppointmentToCancel(null)}
        />
      )}

      {appointmentToDelete && (
        <ConfirmModal
          title="Delete Appointment"
          message={`Are you sure you want to permanently delete this appointment for ${appointmentToDelete.patientName}? This cannot be undone.`}
          confirmText="Delete Appointment"
          onConfirm={() => deleteMutation.mutate(appointmentToDelete.id)}
          onCancel={() => setAppointmentToDelete(null)}
        />
      )}
    </div>
  );
}
