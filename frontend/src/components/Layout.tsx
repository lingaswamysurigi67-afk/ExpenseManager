import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

interface NavItem {
  to: string
  label: string
  icon: string
  end?: boolean
}

const links: NavItem[] = [
  { to: '/', label: 'Dashboard', icon: '📊', end: true },
  { to: '/expenses', label: 'Expenses', icon: '🧾' },
  { to: '/income', label: 'Income', icon: '💸' },
  { to: '/categories', label: 'Categories', icon: '🏷️' },
  { to: '/reports', label: 'Reports', icon: '📈' },
]

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const initials = (user?.userName || '?').slice(0, 2).toUpperCase()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="logo">💰</div>
          <div>
            <h1>ExpenseFlow</h1>
            <small>Daily expense manager</small>
          </div>
        </div>

        {links.map((l) => (
          <NavLink
            key={l.to}
            to={l.to}
            end={l.end}
            className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
          >
            <span>{l.icon}</span>
            <span>{l.label}</span>
          </NavLink>
        ))}

        <div style={{ marginTop: 'auto' }}>
          <button className="btn ghost" style={{ width: '100%' }} onClick={handleLogout}>
            ⎋ Sign out
          </button>
        </div>
      </aside>

      <main className="main">
        <div className="topbar">
          <div>
            <h2>Hi, {user?.userName} 👋</h2>
            <p>Here's your money at a glance.</p>
          </div>
          <div className="user-chip">
            <span style={{ fontSize: 13, color: 'var(--text-dim)' }}>{user?.email}</span>
            <div className="avatar">{initials}</div>
          </div>
        </div>
        <Outlet />
      </main>
    </div>
  )
}
