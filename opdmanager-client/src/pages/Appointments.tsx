import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import api from '../api';
import toast from 'react-hot-toast';
import { Search } from 'lucide-react';

export default function Appointments() {
  const queryClient = useQueryClient();
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['appointments', date, page],
    queryFn: async () => {
      const res = await api.get(`/appointments/by-date?date=${date}&page=${page}&pageSize=15`);
      return res.data;
    }
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

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Appointments</h1>
        
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <input 
            type="date" 
            value={date} 
            onChange={e => { setDate(e.target.value); setPage(1); }}
            style={{ width: 'auto' }}
          />
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Patient</th>
              <th>Doctor</th>
              <th>Status</th>
              <th>Queue #</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={5} style={{ textAlign: 'center' }}>Loading...</td></tr>
            ) : data?.items?.length === 0 ? (
              <tr><td colSpan={5} style={{ textAlign: 'center' }}>No appointments found</td></tr>
            ) : (
              data?.items?.map((a: any) => (
                <tr key={a.id}>
                  <td style={{ fontWeight: 500 }}>{a.patientName}</td>
                  <td>Dr. {a.doctorName} <span style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>({a.doctorSpecialty})</span></td>
                  <td>
                    <span className={`badge badge-${a.status === 'Scheduled' ? 'info' : a.status === 'Completed' ? 'success' : a.status === 'Cancelled' ? 'danger' : 'warning'}`}>
                      {a.status}
                    </span>
                  </td>
                  <td>{a.queueNumber ? `#${a.queueNumber}` : '-'}</td>
                  <td>
                    <div style={{ display: 'flex', gap: '0.5rem' }}>
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
                            className="btn btn-danger" 
                            style={{ padding: '0.25rem 0.75rem', fontSize: '0.85rem' }}
                            onClick={() => {
                              if (confirm('Are you sure you want to cancel this appointment?')) {
                                cancelMutation.mutate(a.id);
                              }
                            }}
                          >
                            Cancel
                          </button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
      
      {/* Pagination controls can be added here using data.page, data.totalPages */}
    </div>
  );
}
