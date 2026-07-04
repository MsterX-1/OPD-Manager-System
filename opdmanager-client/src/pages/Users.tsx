import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import api from '../api';
import toast from 'react-hot-toast';
import { Plus, Trash2, Edit2, X } from 'lucide-react';
import { useAuthStore } from '../store';

interface User {
  id: string;
  email: string;
  fullName: string;
  role: string;
}

export default function Users() {
  const queryClient = useQueryClient();
  const currentUser = useAuthStore(state => state.user);
  const [showModal, setShowModal] = useState(false);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  
  const [formData, setFormData] = useState({
    email: '',
    fullName: '',
    password: '',
    role: 'Staff'
  });

  const { data: users, isLoading } = useQuery({
    queryKey: ['users'],
    queryFn: async () => {
      const res = await api.get('/users');
      return res.data as User[];
    }
  });

  const createMutation = useMutation({
    mutationFn: async (data: any) => await api.post('/users', data),
    onSuccess: () => {
      toast.success('User created successfully');
      queryClient.invalidateQueries({ queryKey: ['users'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to create user')
  });

  const updateMutation = useMutation({
    mutationFn: async ({ id, data }: { id: string, data: any }) => await api.put(`/users/${id}`, data),
    onSuccess: () => {
      toast.success('User updated successfully');
      queryClient.invalidateQueries({ queryKey: ['users'] });
      closeModal();
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to update user')
  });

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => await api.delete(`/users/${id}`),
    onSuccess: () => {
      toast.success('User deleted');
      queryClient.invalidateQueries({ queryKey: ['users'] });
    },
    onError: (err: any) => toast.error(err.response?.data?.message || 'Failed to delete user')
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (editingUser) {
      updateMutation.mutate({
        id: editingUser.id,
        data: {
          fullName: formData.fullName,
          newPassword: formData.password || null,
          role: formData.role
        }
      });
    } else {
      createMutation.mutate(formData);
    }
  };

  const openNewUser = () => {
    setEditingUser(null);
    setFormData({ email: '', fullName: '', password: '', role: 'Staff' });
    setShowModal(true);
  };

  const openEdit = (user: User) => {
    setEditingUser(user);
    setFormData({ email: user.email, fullName: user.fullName, password: '', role: user.role });
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setEditingUser(null);
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">User Management</h1>
        <button className="btn btn-primary" onClick={openNewUser}>
          <Plus size={18} /> Add User
        </button>
      </div>

      <div className="table-container">
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>Email</th>
              <th>Role</th>
              <th style={{ textAlign: 'right' }}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              <tr><td colSpan={4} style={{ textAlign: 'center' }}>Loading...</td></tr>
            ) : users?.map((u) => (
              <tr key={u.id}>
                <td style={{ fontWeight: 500 }}>{u.fullName}</td>
                <td>{u.email}</td>
                <td>
                  <span className={`badge badge-${u.role === 'Admin' ? 'accent-primary' : 'neutral'}`} 
                        style={u.role === 'Admin' ? { background: 'rgba(59, 130, 246, 0.1)', color: 'var(--accent-primary)', border: '1px solid rgba(59, 130, 246, 0.2)' } : {}}>
                    {u.role}
                  </span>
                </td>
                <td style={{ textAlign: 'right' }}>
                  <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                    <button className="btn btn-secondary" style={{ padding: '0.35rem 0.5rem' }} onClick={() => openEdit(u)}>
                      <Edit2 size={16} />
                    </button>
                    {u.id !== currentUser?.id && (
                      <button className="btn btn-danger" style={{ padding: '0.35rem 0.5rem' }} onClick={() => {
                        if (confirm(`Are you sure you want to delete ${u.fullName}?`)) deleteMutation.mutate(u.id);
                      }}>
                        <Trash2 size={16} />
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {showModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50
        }}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '400px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>{editingUser ? 'Edit User' : 'New User'}</h2>
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
                <label>Email</label>
                <input required type="email" disabled={!!editingUser} value={formData.email} onChange={e => setFormData({...formData, email: e.target.value})} />
              </div>
              <div className="form-group">
                <label>{editingUser ? 'New Password (leave blank to keep current)' : 'Password'}</label>
                <input required={!editingUser} type="password" value={formData.password} onChange={e => setFormData({...formData, password: e.target.value})} />
              </div>
              <div className="form-group">
                <label>Role</label>
                <select value={formData.role} onChange={e => setFormData({...formData, role: e.target.value})}>
                  <option value="Staff">Staff</option>
                  <option value="Admin">Admin</option>
                </select>
              </div>
              <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={createMutation.isPending || updateMutation.isPending}>
                  Save User
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
