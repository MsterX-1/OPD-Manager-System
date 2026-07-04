import api from '../api';
import { useState } from 'react';
import toast from 'react-hot-toast';

export default function Reports() {
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [fromDate, setFromDate] = useState(new Date().toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);

  const downloadReport = async (type: string, format: string) => {
    try {
      let url = '';
      let filename = '';
      
      if (type === 'appointments') {
        url = `/reports/appointments/${format}?date=${date}`;
        filename = `appointments_${date}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      } else {
        url = `/reports/performance/${format}?from=${fromDate}&to=${toDate}`;
        filename = `performance_${fromDate}_to_${toDate}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
      }

      const response = await api.get(url, { responseType: 'blob' });
      const blob = new Blob([response.data]);
      const downloadUrl = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = downloadUrl;
      link.download = filename;
      document.body.appendChild(link);
      link.click();
      link.remove();
      toast.success('Report downloaded successfully');
    } catch (err) {
      toast.error('Failed to download report');
    }
  };

  return (
    <div>
      <div className="page-header">
        <h1 className="page-title">Reports</h1>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1.5rem' }}>
        <div className="card">
          <h3 style={{ marginBottom: '1rem' }}>Appointments Report</h3>
          <div className="form-group">
            <label>Date</label>
            <input type="date" value={date} onChange={e => setDate(e.target.value)} />
          </div>
          <div style={{ display: 'flex', gap: '1rem', marginTop: '1.5rem' }}>
            <button className="btn btn-primary" onClick={() => downloadReport('appointments', 'pdf')}>Download PDF</button>
            <button className="btn btn-secondary" onClick={() => downloadReport('appointments', 'excel')}>Download Excel</button>
          </div>
        </div>

        <div className="card">
          <h3 style={{ marginBottom: '1rem' }}>Doctor Performance</h3>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
            <div className="form-group">
              <label>From Date</label>
              <input type="date" value={fromDate} onChange={e => setFromDate(e.target.value)} />
            </div>
            <div className="form-group">
              <label>To Date</label>
              <input type="date" value={toDate} onChange={e => setToDate(e.target.value)} />
            </div>
          </div>
          <div style={{ display: 'flex', gap: '1rem', marginTop: '0.5rem' }}>
            <button className="btn btn-primary" onClick={() => downloadReport('performance', 'pdf')}>Download PDF</button>
            <button className="btn btn-secondary" onClick={() => downloadReport('performance', 'excel')}>Download Excel</button>
          </div>
        </div>
      </div>
    </div>
  );
}
