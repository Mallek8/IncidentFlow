import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useParams } from 'react-router-dom'
import { PriorityBadge, StatusBadge } from '../components/StatusBadge'
import { useToast } from '../components/Toast'
import { useAuth } from '../lib/auth'
import { api, unwrap } from '../lib/api'
import type { ApiResponse, IncidentDetail, IncidentStatus } from '../types/api'

export function IncidentDetailPage() {
  const { id } = useParams()
  const { hasRole } = useAuth()
  const toast = useToast()
  const queryClient = useQueryClient()
  const [comment, setComment] = useState('')
  const [isInternal, setIsInternal] = useState(false)
  const [status, setStatus] = useState<IncidentStatus | ''>('')
  const [resolutionNotes, setResolutionNotes] = useState('')

  const detail = useQuery({
    queryKey: ['incident', id],
    enabled: Boolean(id),
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<IncidentDetail>>(`/api/incidents/${id}`)
      return unwrap(data)
    },
  })

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: ['incident', id] })
    await queryClient.invalidateQueries({ queryKey: ['incidents'] })
  }

  const addComment = useMutation({
    mutationFn: async () => {
      const { data } = await api.post(`/api/incidents/${id}/comments`, { content: comment, isInternal })
      return unwrap(data)
    },
    onSuccess: async () => {
      setComment('')
      toast.push('Comment added', 'success')
      await refresh()
    },
    onError: (err: Error) => toast.push(err.message, 'error'),
  })

  const updateStatus = useMutation({
    mutationFn: async () => {
      const { data } = await api.patch<ApiResponse<IncidentDetail>>(`/api/incidents/${id}/status`, {
        status,
        resolutionNotes: resolutionNotes || null,
      })
      return unwrap(data)
    },
    onSuccess: async () => {
      toast.push('Status updated', 'success')
      await refresh()
    },
    onError: (err: Error) => toast.push(err.message, 'error'),
  })

  const reopen = useMutation({
    mutationFn: async () => {
      const { data } = await api.post<ApiResponse<IncidentDetail>>(`/api/incidents/${id}/reopen`)
      return unwrap(data)
    },
    onSuccess: async () => {
      toast.push('Incident reopened', 'success')
      await refresh()
    },
    onError: (err: Error) => toast.push(err.message, 'error'),
  })

  if (detail.isLoading) return <div className="rounded-2xl border border-line bg-white p-8 text-muted">Loading…</div>
  if (detail.error || !detail.data) {
    return (
      <div className="rounded-2xl border border-rose-200 bg-rose-50 p-8 text-rose-800">
        {detail.error instanceof Error ? detail.error.message : 'Incident not found'}
      </div>
    )
  }

  const incident = detail.data
  const canManageStatus = hasRole('Admin', 'Technician')
  const canReopen = incident.allowedNextStatuses.includes('Reopened')

  const onComment = (e: FormEvent) => {
    e.preventDefault()
    addComment.mutate()
  }

  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-line bg-white p-6 shadow-sm">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="text-sm font-medium text-electric-600">{incident.ticketNumber}</p>
            <h1 className="mt-1 font-display text-3xl text-navy-900">{incident.title}</h1>
            <p className="mt-3 max-w-3xl text-slate-600">{incident.description}</p>
          </div>
          <div className="flex flex-wrap gap-2">
            <PriorityBadge priority={incident.priority} />
            <StatusBadge status={incident.status} />
            {incident.isOverdue && (
              <span className="rounded-full bg-rose-50 px-2.5 py-1 text-xs font-medium text-rose-700 ring-1 ring-rose-200">
                SLA overdue
              </span>
            )}
          </div>
        </div>
        <dl className="mt-6 grid gap-4 text-sm sm:grid-cols-2 lg:grid-cols-4">
          <div><dt className="text-muted">Category</dt><dd className="font-medium">{incident.categoryName}</dd></div>
          <div><dt className="text-muted">Created</dt><dd className="font-medium">{new Date(incident.createdAtUtc).toLocaleString()}</dd></div>
          <div><dt className="text-muted">SLA deadline</dt><dd className="font-medium">{new Date(incident.slaDeadlineUtc).toLocaleString()}</dd></div>
          <div><dt className="text-muted">Resolution</dt><dd className="font-medium">{incident.resolutionDateUtc ? new Date(incident.resolutionDateUtc).toLocaleString() : '—'}</dd></div>
        </dl>
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <section className="rounded-2xl border border-line bg-white p-6 shadow-sm">
          <h2 className="font-semibold text-navy-900">Comments</h2>
          <div className="mt-4 space-y-3">
            {incident.comments.length === 0 && <p className="text-sm text-muted">No comments yet.</p>}
            {incident.comments.map((c) => (
              <div key={c.id} className="rounded-xl border border-line bg-slate-50 px-4 py-3">
                <div className="flex items-center justify-between text-xs text-muted">
                  <span>{c.isInternal ? 'Internal note' : 'Public comment'}</span>
                  <span>{new Date(c.createdAtUtc).toLocaleString()}</span>
                </div>
                <p className="mt-2 text-sm text-slate-700">{c.content}</p>
              </div>
            ))}
          </div>
          <form onSubmit={onComment} className="mt-4 space-y-3">
            <textarea
              className="min-h-24 w-full rounded-xl border border-line px-3 py-2.5 outline-none ring-electric-500 focus:ring-2"
              value={comment}
              onChange={(e) => setComment(e.target.value)}
              required
              placeholder="Add a comment"
            />
            {canManageStatus && (
              <label className="flex items-center gap-2 text-sm text-slate-600">
                <input type="checkbox" checked={isInternal} onChange={(e) => setIsInternal(e.target.checked)} />
                Internal note (staff only)
              </label>
            )}
            <button type="submit" className="rounded-xl bg-navy-900 px-4 py-2 text-sm font-medium text-white">
              Add comment
            </button>
          </form>
        </section>

        <section className="space-y-6">
          {canManageStatus && (
            <div className="rounded-2xl border border-line bg-white p-6 shadow-sm">
              <h2 className="font-semibold text-navy-900">Update status</h2>
              <div className="mt-4 space-y-3">
                <select
                  className="w-full rounded-xl border border-line px-3 py-2.5"
                  value={status}
                  onChange={(e) => setStatus(e.target.value as IncidentStatus | '')}
                >
                  <option value="">Select next status</option>
                  {incident.allowedNextStatuses.map((s) => (
                    <option key={s} value={s}>{s}</option>
                  ))}
                </select>
                {status === 'Resolved' && (
                  <textarea
                    className="min-h-24 w-full rounded-xl border border-line px-3 py-2.5"
                    placeholder="Resolution notes"
                    value={resolutionNotes}
                    onChange={(e) => setResolutionNotes(e.target.value)}
                    required
                  />
                )}
                <button
                  type="button"
                  disabled={!status || updateStatus.isPending}
                  onClick={() => updateStatus.mutate()}
                  className="rounded-xl bg-electric-500 px-4 py-2 text-sm font-medium text-white disabled:opacity-50"
                >
                  Save status
                </button>
              </div>
            </div>
          )}

          {canReopen && (
            <div className="rounded-2xl border border-line bg-white p-6 shadow-sm">
              <h2 className="font-semibold text-navy-900">Reopen</h2>
              <p className="mt-2 text-sm text-muted">Reopen if the issue persists after resolution or closure.</p>
              <button
                type="button"
                onClick={() => {
                  if (window.confirm('Reopen this incident?')) reopen.mutate()
                }}
                className="mt-4 rounded-xl border border-line px-4 py-2 text-sm font-medium"
              >
                Reopen incident
              </button>
            </div>
          )}

          <div className="rounded-2xl border border-line bg-white p-6 shadow-sm">
            <h2 className="font-semibold text-navy-900">History</h2>
            <ul className="mt-4 space-y-3">
              {incident.history.map((h) => (
                <li key={h.id} className="border-l-2 border-electric-500 pl-3 text-sm">
                  <p className="font-medium text-navy-900">{h.changeType}</p>
                  <p className="text-muted">
                    {h.oldStatus && h.newStatus ? `${h.oldStatus} → ${h.newStatus}` : h.notes || '—'}
                  </p>
                  <p className="text-xs text-slate-400">{new Date(h.changedAtUtc).toLocaleString()}</p>
                </li>
              ))}
            </ul>
          </div>
        </section>
      </div>
    </div>
  )
}
