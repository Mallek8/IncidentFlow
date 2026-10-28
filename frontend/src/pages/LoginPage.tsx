import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useAuth } from '../lib/auth'
import { useToast } from '../components/Toast'

export function LoginPage() {
  const { login, isAuthenticated } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const [email, setEmail] = useState('employee@incidentflow.local')
  const [password, setPassword] = useState('Employee123!')
  const [loading, setLoading] = useState(false)

  if (isAuthenticated) return <Navigate to="/" replace />

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      await login(email, password)
      toast.push('Welcome back', 'success')
      navigate('/')
    } catch (err) {
      toast.push(err instanceof Error ? err.message : 'Login failed', 'error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-6xl items-center px-4 py-10">
      <div className="grid w-full overflow-hidden rounded-3xl border border-line bg-white shadow-xl lg:grid-cols-2">
        <div className="bg-gradient-to-br from-navy-950 via-navy-900 to-electric-600 p-10 text-white">
          <p className="font-display text-4xl">IncidentFlow</p>
          <p className="mt-4 max-w-md text-slate-200">
            Report, assign, and resolve IT incidents with role-based workflows and SLA tracking.
          </p>
        </div>
        <form onSubmit={onSubmit} className="space-y-4 p-8 sm:p-10">
          <h1 className="text-2xl font-semibold text-navy-900">Sign in</h1>
          <label className="block space-y-1 text-sm">
            <span className="font-medium text-slate-700">Email</span>
            <input
              className="w-full rounded-xl border border-line px-3 py-2.5 outline-none ring-electric-500 focus:ring-2"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              type="email"
              required
            />
          </label>
          <label className="block space-y-1 text-sm">
            <span className="font-medium text-slate-700">Password</span>
            <input
              className="w-full rounded-xl border border-line px-3 py-2.5 outline-none ring-electric-500 focus:ring-2"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              type="password"
              required
            />
          </label>
          <button
            type="submit"
            disabled={loading}
            className="w-full rounded-xl bg-electric-500 px-4 py-2.5 font-medium text-white hover:bg-electric-600 disabled:opacity-60"
          >
            {loading ? 'Signing in…' : 'Sign in'}
          </button>
          <p className="text-sm text-muted">
            No account?{' '}
            <Link className="font-medium text-electric-600" to="/register">
              Register
            </Link>
          </p>
        </form>
      </div>
    </div>
  )
}
