import { useAuth } from '../lib/auth'

export function ProfilePage() {
  const { user } = useAuth()

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div>
        <h1 className="font-display text-3xl text-navy-900">Profile & settings</h1>
        <p className="mt-1 text-muted">Account details from Identity</p>
      </div>
      <div className="rounded-2xl border border-line bg-white p-6 shadow-sm">
        <dl className="grid gap-4 sm:grid-cols-2">
          <div><dt className="text-sm text-muted">Full name</dt><dd className="font-medium">{user?.fullName}</dd></div>
          <div><dt className="text-sm text-muted">Email</dt><dd className="font-medium">{user?.email}</dd></div>
          <div><dt className="text-sm text-muted">Roles</dt><dd className="font-medium">{user?.roles.join(', ')}</dd></div>
          <div><dt className="text-sm text-muted">Status</dt><dd className="font-medium">{user?.isActive ? 'Active' : 'Inactive'}</dd></div>
        </dl>
      </div>
    </div>
  )
}
