import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Plus, Trash2, Building2, Edit2 } from 'lucide-react';
import api from '../api';
import toast from 'react-hot-toast';
import ConfirmModal from '../components/ConfirmModal';

export default function DepartmentManagement() {
  const queryClient = useQueryClient();
  const [selectedDeptId, setSelectedDeptId] = useState<number | null>(null);

  // Modal states
  const [showDeptModal, setShowDeptModal] = useState(false);
  const [editingDeptId, setEditingDeptId] = useState<number | null>(null);
  const [newDeptName, setNewDeptName] = useState('');
  const [newDeptType, setNewDeptType] = useState('Lab');
  
  const [deptToDelete, setDeptToDelete] = useState<any | null>(null);

  const [showServiceModal, setShowServiceModal] = useState(false);
  const [editingServiceId, setEditingServiceId] = useState<number | null>(null);
  const [newServiceName, setNewServiceName] = useState('');
  const [newServiceFee, setNewServiceFee] = useState('');

  const { data: departments = [], isLoading } = useQuery({
    queryKey: ['allDepartments'],
    queryFn: async () => {
      const res = await api.get('/departments/all');
      return res.data;
    }
  });

  const selectedDept = departments.find((d: any) => d.id === selectedDeptId);

  const createDeptMutation = useMutation({
    mutationFn: async (dept: { name: string, type: string }) => {
      await api.post('/departments', dept);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allDepartments'] });
      toast.success('Department created');
      closeDeptModal();
    }
  });

  const updateDeptMutation = useMutation({
    mutationFn: async ({ id, dept }: { id: number, dept: { name: string, type: string, isActive: boolean } }) => {
      await api.put(`/departments/${id}`, dept);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allDepartments'] });
      toast.success('Department updated');
      closeDeptModal();
    }
  });

  const deleteDeptMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/departments/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allDepartments'] });
      toast.success('Department deleted');
      setDeptToDelete(null);
      if (selectedDeptId === deptToDelete?.id) {
        setSelectedDeptId(null);
      }
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || 'Failed to delete department. It may have associated records.');
    }
  });

  const addServiceMutation = useMutation({
    mutationFn: async ({ deptId, service }: { deptId: number, service: { name: string, fee: number } }) => {
      await api.post(`/departments/${deptId}/services`, service);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allDepartments'] });
      toast.success('Service added');
      closeServiceModal();
    }
  });

  const updateServiceMutation = useMutation({
    mutationFn: async ({ id, service }: { id: number, service: { name: string, fee: number } }) => {
      await api.put(`/departments/services/${id}`, service);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allDepartments'] });
      toast.success('Service updated');
      closeServiceModal();
    }
  });

  const deleteServiceMutation = useMutation({
    mutationFn: async (id: number) => {
      await api.delete(`/departments/services/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['allDepartments'] });
      toast.success('Service removed');
    }
  });

  const handleCreateDept = (e: React.FormEvent) => {
    e.preventDefault();
    if (!newDeptName) return;
    
    if (editingDeptId) {
      updateDeptMutation.mutate({
        id: editingDeptId,
        dept: { name: newDeptName, type: newDeptType, isActive: true }
      });
    } else {
      createDeptMutation.mutate({ name: newDeptName, type: newDeptType });
    }
  };

  const openNewDept = () => {
    setEditingDeptId(null);
    setNewDeptName('');
    setNewDeptType('Lab');
    setShowDeptModal(true);
  };

  const openEditDept = (e: React.MouseEvent, dept: any) => {
    e.stopPropagation(); // Prevent row selection
    setEditingDeptId(dept.id);
    setNewDeptName(dept.name);
    setNewDeptType(dept.type);
    setShowDeptModal(true);
  };

  const closeDeptModal = () => {
    setShowDeptModal(false);
    setEditingDeptId(null);
    setNewDeptName('');
    setNewDeptType('Lab');
  };

  const handleAddService = (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedDeptId || !newServiceName || !newServiceFee) return;
    
    if (editingServiceId) {
      updateServiceMutation.mutate({
        id: editingServiceId,
        service: { name: newServiceName, fee: Number(newServiceFee) }
      });
    } else {
      addServiceMutation.mutate({ 
        deptId: selectedDeptId, 
        service: { name: newServiceName, fee: Number(newServiceFee) } 
      });
    }
  };

  const openNewService = () => {
    setEditingServiceId(null);
    setNewServiceName('');
    setNewServiceFee('');
    setShowServiceModal(true);
  };

  const openEditService = (svc: any) => {
    setEditingServiceId(svc.id);
    setNewServiceName(svc.name);
    setNewServiceFee(svc.fee.toString());
    setShowServiceModal(true);
  };

  const closeServiceModal = () => {
    setShowServiceModal(false);
    setEditingServiceId(null);
    setNewServiceName('');
    setNewServiceFee('');
  };

  if (isLoading) return <div>Loading...</div>;

  return (
    <div style={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div className="page-header" style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h1 className="page-title" style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <Building2 size={28} />
          Departments & Services
        </h1>
        <button className="btn btn-primary" onClick={openNewDept}>
          <Plus size={18} /> New Department
        </button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 2fr', gap: '1.5rem', flex: 1, minHeight: 0 }}>
        {/* Departments List */}
        <div className="glass-panel" style={{ display: 'flex', flexDirection: 'column' }}>
          <div style={{ padding: '1rem', borderBottom: '1px solid var(--border-color)', fontWeight: 600 }}>
            Departments
          </div>
          <div style={{ padding: '0.5rem', overflowY: 'auto', flex: 1, display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
            {departments.map((dept: any) => (
              <button
                key={dept.id}
                onClick={() => setSelectedDeptId(dept.id)}
                style={{
                  padding: '1rem',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  background: selectedDeptId === dept.id ? 'var(--bg-tertiary)' : 'transparent',
                  border: 'none',
                  borderRadius: 'var(--radius-sm)',
                  cursor: 'pointer',
                  textAlign: 'left',
                  borderLeft: selectedDeptId === dept.id ? '4px solid var(--accent-primary)' : '4px solid transparent'
                }}
              >
                <div>
                  <div style={{ fontWeight: 600, color: 'var(--text-primary)' }}>{dept.name}</div>
                  <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{dept.type}</div>
                </div>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  <div style={{ fontSize: '0.8rem', background: 'var(--bg-secondary)', padding: '0.2rem 0.5rem', borderRadius: '1rem', marginRight: '0.5rem' }}>
                    {dept.services?.length || 0} services
                  </div>
                  {selectedDeptId === dept.id && (
                    <div style={{ display: 'flex', gap: '0.25rem' }}>
                      <button className="btn btn-secondary" onClick={(e) => openEditDept(e, dept)}
                        style={{ padding: '0.3rem', border: 'none', background: 'transparent' }} title="Edit Department">
                        <Edit2 size={14} />
                      </button>
                      <button className="btn" onClick={(e) => { e.stopPropagation(); setDeptToDelete(dept); }}
                        style={{ padding: '0.3rem', color: 'var(--danger)', background: 'transparent', border: 'none' }} title="Delete Department">
                        <Trash2 size={14} />
                      </button>
                    </div>
                  )}
                </div>
              </button>
            ))}
          </div>
        </div>

        {/* Services List */}
        <div className="glass-panel" style={{ display: 'flex', flexDirection: 'column' }}>
          {selectedDept ? (
            <>
              <div style={{ padding: '1rem', borderBottom: '1px solid var(--border-color)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div>
                  <h3 style={{ margin: 0 }}>{selectedDept.name} Services</h3>
                  <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Price List / Catalog</div>
                </div>
                <button className="btn btn-primary" onClick={openNewService} style={{ padding: '0.5rem 1rem' }}>
                  <Plus size={16} /> Add Service
                </button>
              </div>
              <div style={{ padding: '1rem', overflowY: 'auto', flex: 1 }}>
                {selectedDept.services?.length === 0 ? (
                  <div style={{ textAlign: 'center', color: 'var(--text-muted)', marginTop: '2rem' }}>
                    No services defined for this department.
                  </div>
                ) : (
                  <table className="table">
                    <thead>
                      <tr>
                        <th>Service Name</th>
                        <th style={{ width: '150px' }}>Fee (EGP)</th>
                        <th style={{ width: '80px', textAlign: 'right' }}>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {selectedDept.services?.map((svc: any) => (
                        <tr key={svc.id}>
                          <td style={{ fontWeight: 500 }}>{svc.name}</td>
                          <td>{svc.fee.toFixed(2)}</td>
                          <td style={{ textAlign: 'right' }}>
                            <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end' }}>
                              <button className="btn btn-secondary" onClick={() => openEditService(svc)}
                                style={{ padding: '0.4rem', border: 'none', background: 'transparent' }}>
                                <Edit2 size={16} />
                              </button>
                              <button className="btn" onClick={() => deleteServiceMutation.mutate(svc.id)}
                                style={{ padding: '0.4rem', color: 'var(--danger)', background: 'transparent', border: 'none' }}>
                                <Trash2 size={16} />
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                )}
              </div>
            </>
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', flex: 1, color: 'var(--text-muted)' }}>
              Select a department to manage its services.
            </div>
          )}
        </div>
      </div>

      {/* New Department Modal */}
      {showDeptModal && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50
        }} onClick={closeDeptModal}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '400px' }} onClick={e => e.stopPropagation()}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>
                {editingDeptId ? 'Edit Department' : 'New Department'}
              </h2>
            </div>
            <form onSubmit={handleCreateDept}>
              <div className="form-group">
                <label>Department Name</label>
                <input required type="text" value={newDeptName} onChange={e => setNewDeptName(e.target.value)} placeholder="e.g. Main Laboratory" />
              </div>
              <div className="form-group">
                <label>Department Type</label>
                <select value={newDeptType} onChange={e => setNewDeptType(e.target.value)}>
                  <option value="Lab">Lab</option>
                  <option value="Radiology">Radiology</option>
                  <option value="Pharmacy">Pharmacy</option>
                  <option value="Clinic">Clinic</option>
                  <option value="Other">Other</option>
                </select>
              </div>
              <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={createDeptMutation.isPending || updateDeptMutation.isPending}>
                  {editingDeptId ? 'Save Changes' : 'Create'}
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeDeptModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* New Service Modal */}
      {showServiceModal && selectedDept && (
        <div style={{
          position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
          background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
          display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50
        }} onClick={closeServiceModal}>
          <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '400px' }} onClick={e => e.stopPropagation()}>
            <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem' }}>
                {editingServiceId ? 'Edit Service' : `Add Service to ${selectedDept.name}`}
              </h2>
            </div>
            <form onSubmit={handleAddService}>
              <div className="form-group">
                <label>Service Name</label>
                <input required type="text" value={newServiceName} onChange={e => setNewServiceName(e.target.value)} placeholder="e.g. CBC, Chest X-Ray" />
              </div>
              <div className="form-group">
                <label>Fee (EGP)</label>
                <input required type="number" step="0.01" min="0" value={newServiceFee} onChange={e => setNewServiceFee(e.target.value)} placeholder="0.00" />
              </div>
              <div style={{ marginTop: '2rem', display: 'flex', gap: '1rem' }}>
                <button type="submit" className="btn btn-primary" style={{ flex: 1 }} disabled={addServiceMutation.isPending || updateServiceMutation.isPending}>
                  {editingServiceId ? 'Save Changes' : 'Add'}
                </button>
                <button type="button" className="btn btn-secondary" style={{ flex: 1 }} onClick={closeServiceModal}>Cancel</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {deptToDelete && (
        <ConfirmModal
          title="Delete Department"
          message={`Are you sure you want to permanently delete the department "${deptToDelete.name}"? This action cannot be undone and will fail if the department has active records.`}
          confirmText="Delete Department"
          onConfirm={() => deleteDeptMutation.mutate(deptToDelete.id)}
          onCancel={() => setDeptToDelete(null)}
        />
      )}
    </div>
  );
}
