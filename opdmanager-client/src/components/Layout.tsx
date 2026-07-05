import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store';
import { LayoutDashboard, Users, UserSquare2, CalendarDays, Activity, AlertCircle, FileBarChart, Settings, LogOut, UserCog, PieChart, Stethoscope, Building2, ClipboardList } from 'lucide-react';

export default function Layout() {
  const user = useAuthStore(state => state.user);
  const logout = useAuthStore(state => state.logout);
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const navItems = [
    { to: "/", icon: LayoutDashboard, label: "Dashboard" },
    { to: "/queue", icon: Activity, label: "Queue Monitor" },
    { to: "/appointments", icon: CalendarDays, label: "Appointments" },
    { to: "/patients", icon: Users, label: "Patients" },
    { to: "/doctor-portal", icon: Stethoscope, label: "My Queue", role: "Doctor" },
    { to: "/department-portal", icon: ClipboardList, label: "Dept Portal", role: "DepartmentStaff" },
    { to: "/doctors", icon: UserSquare2, label: "Doctors", role: "Admin" },
    { to: "/departments", icon: Building2, label: "Departments", role: "Admin" },
    { to: "/issues", icon: AlertCircle, label: "Issues" },
    { to: "/reports", icon: FileBarChart, label: "Reports" },
    { to: "/analytics", icon: PieChart, label: "Analytics", role: "Admin" },
    { to: "/users", icon: UserCog, label: "Users", role: "Admin" },
    { to: "/settings", icon: Settings, label: "Settings", role: "Admin" },
  ];

  return (
    <div className="app-container">
      {/* Sidebar */}
      <aside style={{
        width: '260px',
        background: 'var(--bg-secondary)',
        borderRight: '1px solid var(--border-color)',
        display: 'flex',
        flexDirection: 'column',
        padding: '1.5rem 1rem'
      }}>
        <div style={{ padding: '0 0.5rem', marginBottom: '2.5rem' }}>
          <h1 style={{ 
            fontSize: '1.5rem', 
            fontWeight: 700, 
            background: 'linear-gradient(to right, var(--accent-primary), var(--accent-secondary))',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            letterSpacing: '-0.5px'
          }}>
            OPD Manager
          </h1>
          <div style={{ marginTop: '0.25rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
            Welcome, {user?.fullName?.split(' ')[0] ?? 'User'}
          </div>
          {user?.role && user.role !== 'Staff' && (
            <div style={{ 
              marginTop: '0.25rem', 
              fontSize: '0.75rem', 
              color: 'var(--accent-primary)',
              opacity: 0.8
            }}>
              {user.role === 'DepartmentStaff' ? 'Department Staff' : user.role}
            </div>
          )}
        </div>

        <nav style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
          {navItems.map(item => {
            if (item.role && item.role !== user?.role) return null;
            const Icon = item.icon;
            
            return (
              <NavLink
                key={item.to}
                to={item.to}
                style={({ isActive }) => ({
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.75rem',
                  padding: '0.75rem 1rem',
                  borderRadius: 'var(--radius-md)',
                  color: isActive ? 'white' : 'var(--text-secondary)',
                  background: isActive ? 'linear-gradient(135deg, var(--accent-primary), var(--accent-secondary))' : 'transparent',
                  textDecoration: 'none',
                  fontSize: '0.95rem',
                  fontWeight: 500,
                  transition: 'all 0.2s ease',
                  boxShadow: isActive ? 'var(--shadow-glow)' : 'none'
                })}
              >
                <Icon size={20} opacity={0.9} />
                {item.label}
              </NavLink>
            )
          })}
        </nav>

        <div style={{ marginTop: 'auto', paddingTop: '1rem', borderTop: '1px solid var(--border-color)' }}>
          <button 
            onClick={handleLogout}
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.75rem',
              padding: '0.75rem 1rem',
              width: '100%',
              background: 'transparent',
              border: 'none',
              color: 'var(--danger)',
              cursor: 'pointer',
              fontSize: '0.95rem',
              fontWeight: 500,
              borderRadius: 'var(--radius-md)',
              transition: 'background 0.2s',
              textAlign: 'left'
            }}
            onMouseOver={(e) => e.currentTarget.style.background = 'rgba(239, 68, 68, 0.1)'}
            onMouseOut={(e) => e.currentTarget.style.background = 'transparent'}
          >
            <LogOut size={20} />
            Logout
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="main-content">
        <div className="page-container animate-fade-in">
          <Outlet />
        </div>
      </main>
    </div>
  );
}
