import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { useToast } from '../components/Toast'
import { getErrorMessage } from '../lib/api'
import { useAuth } from '../lib/auth'

export function RegisterPage() {
  const { register, isAuthenticated } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  const [form, setForm] = useState({
    firstName: '',
    lastName: '',
    email: '',
    password: '',
  })
  const [loading, setLoading] = useState(false)

  if (isAuthenticated) return <Navigate to="/" replace />

  const onSubmit = async (e: FormEvent) => {
    e.preventDefault()
    setLoading(true)
    try {
      await register(form)
      toast.push('Account created', 'success')
      navigate('/')
    } catch (err) {
      toast.push(getErrorMessage(err), 'error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-lg items-center px-4 py-10">
      <form onSubmit={onSubmit} className="w-full space-y-4 rounded-3xl border border-line bg-white p-8 shadow-xl">
        <h1 className="font-display text-3xl text-navy-900">Create account</h1>
        <p className="text-sm text-muted">Employee self-registration · Created by Mallek Hannachi</p>
        {(['firstName', 'lastName', 'email', 'password'] as const).map((field) => (
          <label key={field} className="block space-y-1 text-sm capitalize">
            <span className="font-medium text-slate-700">{field.replace(/([A-Z])/g, ' $1')}</span>
            <input
              className="w-full rounded-xl border border-line px-3 py-2.5 outline-none ring-electric-500 focus:ring-2"
              type={field === 'password' ? 'password' : field === 'email' ? 'email' : 'text'}
              value={form[field]}
              onChange={(e) => setForm((prev) => ({ ...prev, [field]: e.target.value }))}
              required
            />
          </label>
        ))}
        <button
          type="submit"
          disabled={loading}
          className="w-full rounded-xl bg-electric-500 px-4 py-2.5 font-medium text-white hover:bg-electric-600 disabled:opacity-60"
        >
          {loading ? 'Creating…' : 'Register'}
        </button>
        <p className="text-sm text-muted">
          Already registered?{' '}
          <Link className="font-medium text-electric-600" to="/login">
            Sign in
          </Link>
        </p>
      </form>
    </div>
  )
}
