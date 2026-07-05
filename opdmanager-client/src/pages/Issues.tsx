import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../api';
import toast from 'react-hot-toast';
import { Plus, Trash2, Edit2, X, Search, ChevronLeft, ChevronRight, CheckCircle, Clock } from 'lucide-react';
import ConfirmModal from '../components/ConfirmModal';

interface Issue {
  id: number;
  title: string;
  description: string;
  category: string;
  status: 'Open' | 'InReview' | 'Closed';
  reportedAt: string;
  resolvedAt: string | null;
  resolution: string | null;
  appointmentId: number | null;
}

export default function Issues() {
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [page, setPage] = useState(1);
  const [showModal, setShowModal] = useState(false);
  const [showResolutionModal, setShowResolutionModal] = useState(false);
  const [selectedIssue, setSelectedIssue] = useState<Issue | null>(null);
  const [issueToDelete, setIssueToDelete] = useState<Issue | null>(null);
  const [resolutionText, setResolutionText] = useState('');
  
  const [formData, setFormData] = useState({
    title: '',
    description: '',
    category: ''
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
    queryKey: ['issues', debouncedSearch, statusFilter, page],
    queryFn: async () => {
      let url = `/issues?page=${page}`;
      if (debouncedSearch) url += `&title=${encodeURIComponent(debouncedSearch)}`;
      if (statusFilter) url += `&status=${statusFilter}`;
      const res = await api.get(url);
      return res.data;
    }
  });

  const createMutation = useMutation({
    mutationFn: async (data: any) => await api.post('/issues', data),
    onSuccess: () => {
      toast.success('Issue reported successfully');
      queryClient.invalidateQueries({ queryKey: ['issues'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to create issue')
  });

  const updateMutation = useMutation({
    mutationFn: async ({ id, data }: { id: number, data: any }) => await api.put(`/issues/${id}`, data),
    onSuccess: () => {
      toast.success('Issue updated successfully');
      queryClient.invalidateQueries({ queryKey: ['issues'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to update issue')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: number) => await api.delete(`/issues/${id}`),
    onSuccess: () => {
      toast.success('Issue deleted');
      queryClient.invalidateQueries({ queryKey: ['issues'] });
    },
    onError: () => toast.error('Failed to delete issue.')
  });

  const reviewMutation = useMutation({
    mutationFn: async (id: number) => await api.patch(`/issues/${id}/in-review`),
    onSuccess: () => {
      toast.success('Issue marked as In Review');
      queryClient.invalidateQueries({ queryKey: ['issues'] });
    }
  });

  const closeIssueMutation = useMutation({
    mutationFn: async ({ id, resolution }: { id: number, resolution: string }) => 
      await api.patch(`/issues/${id}/close`, { resolution }),
    onSuccess: () => {
      toast.success('Issue closed');
      queryClient.invalidateQueries({ queryKey: ['issues'] });
      closeModal();
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedIssue) {
      updateMutation.mutate({ id: selectedIssue.id, data: formData });
    } else {
      createMutation.mutate(formData);
    }
  };

  const handleCloseIssue = (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedIssue && resolutionText.trim()) {
      closeIssueMutation.mutate({ id: selectedIssue.id, resolution: resolutionText });
    }
  };

  const openNew = () => {
    setSelectedIssue(null);
    setFormData({ title: '', description: '', category: '' });
    setShowModal(true);
  };

  const openEdit = (issue: Issue) => {
    setSelectedIssue(issue);
    setFormData({ title: issue.title, description: issue.description, category: issue.category });
    setShowModal(true);
  };

  const openClose = (issue: Issue) => {
    setSelectedIssue(issue);
    setResolutionText('');
    setShowResolutionModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setShowResolutionModal(false);
    setSelectedIssue(null);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Open': return <span className="badge badge-danger">Open</span>;
      case 'InReview': return <span className="badge badge-warning">In Review</span>;
      case 'Closed': return <span className="badge badge-success">Closed</span>;
      default: return <span className="badge badge-neutral">{status}</span>;
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Issue Tracking</h1>
        <div style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
          <div style={{ position: 'relative' }}>
            <Search size={18} style={{ position: 'absolute', left: '10px', top: '10px', color: 'var(--text-muted)' }} />
            <input 
              type="text" 
              placeholder="Search issues..." 
              value={searchTerm}
              onChange={(e) => { setSearchTerm(e.target.value); setPage(1); }}
              style={{ paddingLeft: '2.5rem', width: '250px' }}
            />
          </div>
          <select 
            value={statusFilter} 
            onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
            style={{ width: '150px' }}
          >
            <option value="">All Statuses</option>
            <option value="Open">Open</option>
            <option value="InReview">In Review</option>
            <option value="Closed">Closed</option>
          </select>
          <button className="btn btn-primary" onClick={openNew}>
            <Plus size={18} /> Report Issue
          </button>
        </div>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Title & Category</th>
              <th>Status</th>
              <th>Reported</th>
              <th>Resolution</th>
              <th style={{ textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={5} style={{ textAlign: 'center' }}>Loading...</td></tr>
            ) : data?.items?.length === 0 ? (
              <tr><td colSpan={5} style={{ textAlign: 'center' }}>No issues found.</td></tr>
            ) : data?.items?.map((issue: Issue) => (
              <tr key={issue.id}>
                <td>
                  <div style={{ fontWeight: 500, marginBottom: '0.25rem' }}>{issue.title}</div>
                  <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{issue.category}</div>
                </td>
                <td>{getStatusBadge(issue.status)}</td>
                <td>{new Date(issue.reportedAt).toLocaleDateString()}</td>
                <td>
                  {issue.status === 'Closed' ? (
                    <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', maxWidth: '250px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {issue.resolution}
                    </div>
                  ) : '-'}
                </td>
                <td style={{ textAlign: 'right' }}>
                  <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                    {issue.status === 'Open' && (
                      <button className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem' }} onClick={() => reviewMutation.mutate(issue.id)} title="Mark In Review">
                        <Clock size={16} />
                      </button>
                    )}
                    {issue.status !== 'Closed' && (
                      <>
                        <button className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem', color: 'var(--success)', borderColor: 'var(--success)' }} onClick={() => openClose(issue)} title="Close Issue">
                          <CheckCircle size={16} />
                        </button>
                        <button className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem' }} onClick={() => openEdit(issue)} title="Edit">
                          <Edit2 size={16} />
                        </button>
                      </>
                    )}
                    <button className="btn btn-danger" style={{ padding: '0.35rem 0.5rem' }} onClick={() => setIssueToDelete(issue)} title="Delete">
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
              <button className="btn btn-secondary" disabled={!data.hasPrevious} onClick={() => setPage(p => p - 1)} style={{ padding: '0.5rem' }}><ChevronLeft size={18} /></button>
              <button className="btn btn-secondary" disabled={!data.hasNext} onClick={() => setPage(p => p + 1)} style={{ padding: '0.5rem' }}><ChevronRight size={18} /></button>
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
              <h2 style={{ fontSize: '1.25rem' }}>{selectedIssue ? 'Edit Issue' : 'Report Issue'}</h2>
              <button onClick={closeModal} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleSubmit}>
              <div className="form-group">
                <label>Title</label>
                <input required type="text" value={formData.title} onChange={e => setFormData({...formData, title: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Category</label>
                <input required type="text" value={formData.category} onChange={e => setFormData({...formData, category: e.target.value})} placeholder="e.g. Facilities, IT, Staffing" />
              </div>
              <div className="form-group">
                <label>Description</label>
                <textarea required rows={4} value={formData.description} onChange={e => setFormData({...formData, description: e.target.value})} />
              </div>

              <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={createMutation.isPending || updateMutation.isPending}>
                  Save Issue
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {showResolutionModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
          padding: '2rem'
        }}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '500px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>Close Issue</h2>
              <button onClick={closeModal} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleCloseIssue}>
              <div className="form-group">
                <label>Resolution Details</label>
                <textarea 
                  required 
                  rows={4} 
                  value={resolutionText} 
                  onChange={e => setResolutionText(e.target.value)} 
                  placeholder="Explain how this issue was resolved..."
                />
              </div>

              <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1, background: 'var(--success)' }} disabled={closeIssueMutation.isPending}>
                  Confirm & Close
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {issueToDelete && (
        <ConfirmModal
          title="Delete Issue"
          message={`Are you sure you want to delete this issue? This action cannot be undone.`}
          confirmText="Delete Issue"
          onConfirm={() => deleteMutation.mutate(issueToDelete.id)}
          onCancel={() => setIssueToDelete(null)}
        />
      )}
    </div>
  );
}
