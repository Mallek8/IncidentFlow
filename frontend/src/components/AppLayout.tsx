import {
  AlertTriangle,
  ClipboardList,
  Gauge,
  LayoutDashboard,
  LogOut,
  PlusCircle,
  Settings,
  Shield,
  Ticket,
  Users,
} from 'lucide-react'
import { NavLink, Outlet } from 'react-router-dom'
import { useAuth } from '../lib/auth'

const linkClass = ({ isActive }: { isActive: boolean }) =>
  `flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium transition ${
    isActive
      ? 'bg-electric-500 text-white shadow-md shadow-blue-500/20'
      : 'text-slate-300 hover:bg-white/10 hover:text-white'
  }`

export function AppLayout() {
  const { user, logout, hasRole } = useAuth()

  return (
    <div className="min-h-screen lg:grid lg:grid-cols-[260px_1fr]">
      <aside className="bg-navy-950 text-white">
        <div className="border-b border-white/10 px-6 py-6">
          <div className="font-display text-2xl tracking-tight">IncidentFlow</div>
          <p className="mt-1 text-sm text-slate-400">IT service operations</p>
        </div>
        <nav className="space-y-1 px-3 py-4">
          <NavLink to="/" end className={linkClass}>
            <LayoutDashboard size={18} /> Dashboard
          </NavLink>
          <NavLink to="/incidents" className={linkClass}>
            <Ticket size={18} /> Incidents
          </NavLink>
          <NavLink to="/incidents/new" className={linkClass}>
            <PlusCircle size={18} /> Create incident
          </NavLink>
          {(hasRole('Technician') || hasRole('Admin')) && (
            <NavLink to="/assigned" className={linkClass}>
              <ClipboardList size={18} /> My assignments
            </NavLink>
          )}
          <NavLink to="/sla" className={linkClass}>
            <AlertTriangle size={18} /> SLA monitoring
          </NavLink>
          {hasRole('Admin') && (
            <>
              <NavLink to="/users" className={linkClass}>
                <Users size={18} /> Users
              </NavLink>
              <NavLink to="/categories" className={linkClass}>
                <Shield size={18} /> Categories
              </NavLink>
            </>
          )}
          <NavLink to="/profile" className={linkClass}>
            <Settings size={18} /> Profile
          </NavLink>
        </nav>
      </aside>

      <div className="flex min-h-screen flex-col">
        <header className="sticky top-0 z-20 border-b border-line bg-white/80 px-6 py-4 backdrop-blur">
          <div className="flex items-center justify-between gap-4">
            <div>
              <p className="text-sm text-muted">Signed in as</p>
              <p className="font-semibold text-navy-900">
                {user?.fullName}{' '}
                <span className="text-sm font-normal text-muted">
                  ({user?.roles.join(', ')})
                </span>
              </p>
            </div>
            <div className="flex items-center gap-3">
              <div className="hidden items-center gap-2 rounded-full bg-slate-100 px-3 py-1.5 text-xs font-medium text-slate-600 sm:flex">
                <Gauge size={14} /> Live operations
              </div>
              <button
                type="button"
                onClick={() => void logout()}
                className="inline-flex items-center gap-2 rounded-xl border border-line bg-white px-3 py-2 text-sm font-medium text-slate-700 shadow-sm hover:bg-slate-50"
              >
                <LogOut size={16} /> Logout
              </button>
            </div>
          </div>
        </header>
        <main className="flex-1 px-4 py-6 sm:px-6 lg:px-8">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
