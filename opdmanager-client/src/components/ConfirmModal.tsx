import { X, AlertTriangle } from 'lucide-react';

interface ConfirmModalProps {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  isDestructive?: boolean;
  onConfirm: () => void;
  onCancel: () => void;
}

export default function ConfirmModal({
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  isDestructive = true,
  onConfirm,
  onCancel
}: ConfirmModalProps) {
  return (
    <div style={{
      position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
      background: 'rgba(11, 15, 25, 0.8)', backdropFilter: 'blur(4px)',
      display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 1000
    }} onClick={onCancel}>
      <div className="card animate-fade-in" style={{ width: '100%', maxWidth: '400px' }} onClick={e => e.stopPropagation()}>
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '1.5rem', alignItems: 'flex-start' }}>
          <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center' }}>
            {isDestructive && (
              <div style={{ padding: '0.5rem', background: 'rgba(239, 68, 68, 0.1)', color: 'var(--danger)', borderRadius: 'var(--radius-full)' }}>
                <AlertTriangle size={24} />
              </div>
            )}
            <h2 style={{ fontSize: '1.25rem', margin: 0 }}>{title}</h2>
          </div>
          <button onClick={onCancel} style={{ background: 'none', border: 'none', color: 'var(--text-muted)', cursor: 'pointer' }}>
            <X size={20} />
          </button>
        </div>

        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem', lineHeight: 1.5 }}>
          {message}
        </p>

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'flex-end' }}>
          <button className="btn btn-secondary" onClick={onCancel} style={{ padding: '0.5rem 1.5rem' }}>
            {cancelText}
          </button>
          <button 
            className={`btn ${isDestructive ? 'btn-danger' : 'btn-primary'}`} 
            onClick={() => { onConfirm(); onCancel(); }}
            style={{ padding: '0.5rem 1.5rem', border: 'none' }}
          >
            {confirmText}
          </button>
        </div>
      </div>
    </div>
  );
}
