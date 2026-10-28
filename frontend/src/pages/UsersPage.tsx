import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getErrorMessage, api, unwrap } from '../lib/api'
import { useToast } from '../components/Toast'
import type { ApiResponse, UserProfile } from '../types/api'

const allRoles = ['Admin', 'Technician', 'Employee'] as const

export function UsersPage() {
  const toast = useToast()
  const queryClient = useQueryClient()

  const users = useQuery({
    queryKey: ['users'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<UserProfile[]>>('/api/users')
      return unwrap(data)
    },
  })

  const updateRoles = useMutation({
    mutationFn: async ({ id, roles }: { id: string; roles: string[] }) => {
      const { data } = await api.put<ApiResponse<UserProfile>>(`/api/users/${id}/roles`, { roles })
      return unwrap(data)
    },
    onSuccess: async () => {
      toast.push('Roles updated', 'success')
      await queryClient.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err) => toast.push(getErrorMessage(err), 'error'),
  })

  const setActive = useMutation({
    mutationFn: async ({ id, isActive }: { id: string; isActive: boolean }) => {
      const { data } = await api.put<ApiResponse<UserProfile>>(`/api/users/${id}/active`, { isActive })
      return unwrap(data)
    },
    onSuccess: async () => {
      toast.push('User status updated', 'success')
      await queryClient.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (err) => toast.push(getErrorMessage(err), 'error'),
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-3xl text-navy-900">User management</h1>
        <p className="mt-1 text-muted">Manage roles and account status</p>
      </div>

      <div className="overflow-hidden rounded-2xl border border-line bg-white shadow-sm">
        {users.isLoading && <div className="p-8 text-muted">Loading users…</div>}
        {users.error && (
          <div className="p-8 text-rose-700">{getErrorMessage(users.error)}</div>
        )}
        {users.data && (
          <table className="min-w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Email</th>
                <th className="px-4 py-3">Roles</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Actions</th>
              </tr>
            </thead>
            <tbody>
              {users.data.map((user) => (
                <tr key={user.id} className="border-t border-line align-top">
                  <td className="px-4 py-3 font-medium text-navy-900">{user.fullName}</td>
                  <td className="px-4 py-3">{user.email}</td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap gap-2">
                      {allRoles.map((role) => {
                        const checked = user.roles.includes(role)
                        return (
                          <label key={role} className="inline-flex items-center gap-1 rounded-lg bg-slate-50 px-2 py-1 text-xs">
                            <input
                              type="checkbox"
                              checked={checked}
                              onChange={(e) => {
                                const next = e.target.checked
                                  ? [...user.roles, role]
                                  : user.roles.filter((r) => r !== role)
                                if (next.length === 0) {
                                  toast.push('A user must keep at least one role', 'error')
                                  return
                                }
                                updateRoles.mutate({ id: user.id, roles: next })
                              }}
                            />
                            {role}
                          </label>
                        )
                      })}
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <span className={user.isActive ? 'text-emerald-700' : 'text-rose-700'}>
                      {user.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-4 py-3">
                    <button
                      type="button"
                      className="rounded-lg border border-line px-3 py-1.5 text-xs font-medium"
                      onClick={() =>
                        setActive.mutate({ id: user.id, isActive: !user.isActive })
                      }
                    >
                      {user.isActive ? 'Deactivate' : 'Activate'}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
      <p className="text-xs text-muted">Created by Mallek Hannachi</p>
    </div>
  )
}
