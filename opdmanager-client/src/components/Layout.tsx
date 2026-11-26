import { useState, useEffect } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuthStore } from '../store';
import { useTranslation } from 'react-i18next';
import { LayoutDashboard, Users, UserSquare2, CalendarDays, Activity, AlertCircle, FileBarChart, Settings, LogOut, UserCog, PieChart, Stethoscope, Building2, ClipboardList, Menu, X, Globe } from 'lucide-react';

export default function Layout() {
  const user = useAuthStore(state => state.user);
  const logout = useAuthStore(state => state.logout);
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);

  useEffect(() => {
    document.dir = i18n.language === 'ar' ? 'rtl' : 'ltr';
    document.documentElement.lang = i18n.language;
  }, [i18n.language]);

  const toggleLanguage = () => {
    const nextLang = i18n.language === 'en' ? 'ar' : 'en';
    i18n.changeLanguage(nextLang);
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const navItems = [
    { to: "/", icon: LayoutDashboard, label: "Dashboard" },
    { to: "/queue", icon: Activity, label: "Queue Monitor", hideForRole: "Doctor" },
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
      {/* Mobile Top Bar */}
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '1rem',
        background: 'var(--bg-secondary)',
        borderBottom: '1px solid var(--border-color)',
        position: 'absolute',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 40,
      }} className="md-hidden">
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
          <button onClick={() => setIsMobileMenuOpen(true)} style={{ background: 'transparent', border: 'none', color: 'var(--text-primary)', cursor: 'pointer' }}>
            <Menu size={24} />
          </button>
          <h1 style={{ fontSize: '1.25rem', fontWeight: 600, color: 'var(--text-primary)' }}>OPD</h1>
        </div>
        <button onClick={toggleLanguage} style={{ background: 'transparent', border: 'none', color: 'var(--text-primary)', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <Globe size={20} />
          <span style={{ fontSize: '0.85rem', fontWeight: 500 }}>{i18n.language === 'en' ? 'ع' : 'En'}</span>
        </button>
      </div>

      {/* Overlay for mobile */}
      {isMobileMenuOpen && (
        <div 
          onClick={() => setIsMobileMenuOpen(false)}
          style={{
            position: 'fixed',
            inset: 0,
            background: 'rgba(0,0,0,0.5)',
            backdropFilter: 'blur(4px)',
            zIndex: 45,
            animation: 'fadeIn 0.3s ease forwards'
          }}
          className="md-hidden"
        />
      )}

      {/* Sidebar */}
      <aside 
        style={{
          width: 'var(--sidebar-width)',
          background: 'var(--bg-secondary)',
          borderInlineEnd: '1px solid var(--border-color)',
          display: 'flex',
          flexDirection: 'column',
          padding: '1.5rem 1rem',
          position: 'fixed',
          top: 0,
          bottom: 0,
          zIndex: 50,
          transition: 'transform 0.3s cubic-bezier(0.23, 1, 0.32, 1)',
        }}
        className={`sidebar ${isMobileMenuOpen ? 'open' : ''}`}
      >
        <div style={{ padding: '0 0.5rem', marginBottom: '2.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
          <div>
            <h1 style={{ 
              fontSize: '1.5rem', 
              fontWeight: 700, 
              color: 'var(--accent-primary)',
              letterSpacing: '-0.5px'
            }}>
              OPD Manager
            </h1>
            <div style={{ marginTop: '0.25rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
              {t('Welcome')}, {user?.fullName?.split(' ')[0] ?? 'User'}
            </div>
          </div>
          <button 
            className="md-hidden"
            onClick={() => setIsMobileMenuOpen(false)} 
            style={{ background: 'transparent', border: 'none', color: 'var(--text-secondary)', cursor: 'pointer' }}
          >
            <X size={24} />
          </button>
        </div>

        <nav style={{ flex: 1, display: 'flex', flexDirection: 'column', gap: '0.25rem', overflowY: 'auto' }}>
          {navItems.map(item => {
            if (item.role && item.role !== user?.role) return null;
            if ((item as any).hideForRole && (item as any).hideForRole === user?.role) return null;
            const Icon = item.icon;
            
            return (
              <NavLink
                key={item.to}
                to={item.to}
                onClick={() => setIsMobileMenuOpen(false)}
                className="sidebar-link"
                style={({ isActive }) => ({
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.75rem',
                  padding: '0.75rem 1rem',
                  borderRadius: 'var(--radius-md)',
                  color: isActive ? 'var(--accent-primary)' : 'var(--text-secondary)',
                  background: isActive ? 'linear-gradient(90deg, rgba(13,148,136,0.15) 0%, transparent 100%)' : 'transparent',
                  borderInlineStart: isActive ? '3px solid var(--accent-primary)' : '3px solid transparent',
                  textDecoration: 'none',
                  fontSize: '0.95rem',
                  fontWeight: isActive ? 600 : 500,
                  transition: 'all 0.2s cubic-bezier(0.16, 1, 0.3, 1)',
                })}
              >
                <div className="icon-container" style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', transition: 'transform 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275)' }}>
                  <Icon size={20} opacity={0.9} />
                </div>
                {t(item.label)}
              </NavLink>
            )
          })}
        </nav>

        <div style={{ marginTop: 'auto', paddingTop: '1rem', borderTop: '1px solid var(--border-color)', display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
          <button 
            onClick={toggleLanguage}
            className="md-visible"
            style={{
              display: 'flex',
              alignItems: 'center',
              gap: '0.75rem',
              padding: '0.75rem 1rem',
              width: '100%',
              background: 'transparent',
              border: 'none',
              color: 'var(--text-secondary)',
              cursor: 'pointer',
              fontSize: '0.95rem',
              fontWeight: 500,
              borderRadius: 'var(--radius-md)',
              textAlign: 'start'
            }}
          >
            <Globe size={20} />
            {i18n.language === 'en' ? 'عربي' : 'English'}
          </button>

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
              textAlign: 'start'
            }}
          >
            <LogOut size={20} />
            {t('Logout')}
          </button>
        </div>
      </aside>

      {/* Main Content Area */}
      <main className="main-content" style={{ marginInlineStart: 'var(--sidebar-width)', paddingTop: '0' }}>
        <div className="page-container animate-fade-in">
          <Outlet />
        </div>
      </main>

      <style>{`
        .md-hidden { display: none !important; }
        .md-visible { display: flex !important; }
        
        .sidebar-link:hover {
          background: var(--bg-tertiary) !important;
          color: var(--text-primary) !important;
        }
        
        .sidebar-link:hover .icon-container {
          transform: scale(1.15) rotate(-5deg);
          color: var(--accent-primary);
        }

        @media (max-width: 768px) {
          .md-hidden { display: flex !important; }
          .md-visible { display: none !important; }
          
          .main-content {
            margin-inline-start: 0 !important;
            padding-top: 4rem !important; /* Space for mobile top bar */
          }
          
          .sidebar {
            transform: translateX(-100%);
          }
          [dir="rtl"] .sidebar {
            transform: translateX(100%);
          }
          
          .sidebar.open {
            transform: translateX(0) !important;
          }
        }
      `}</style>
    </div>
  );
}
