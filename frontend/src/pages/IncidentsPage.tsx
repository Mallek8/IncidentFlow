import { useQuery } from '@tanstack/react-query'
import { Search } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { PriorityBadge, StatusBadge } from '../components/StatusBadge'
import { api, unwrap } from '../lib/api'
import type { ApiResponse, IncidentListItem, IncidentPriority, IncidentStatus, PagedResult } from '../types/api'

export function IncidentsPage({ assignedToMe = false }: { assignedToMe?: boolean }) {
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState<IncidentStatus | ''>('')
  const [priority, setPriority] = useState<IncidentPriority | ''>('')
  const [page, setPage] = useState(1)

  const queryKey = useMemo(
    () => ['incidents', { search, status, priority, page, assignedToMe }],
    [search, status, priority, page, assignedToMe],
  )

  const { data, isLoading, error } = useQuery({
    queryKey,
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<PagedResult<IncidentListItem>>>('/api/incidents', {
        params: {
          search: search || undefined,
          status: status || undefined,
          priority: priority || undefined,
          page,
          pageSize: 10,
          assignedToMe: assignedToMe || undefined,
          sortBy: 'created',
          sortDescending: true,
        },
      })
      return unwrap(data)
    },
  })

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="font-display text-3xl text-navy-900">
            {assignedToMe ? 'My assigned incidents' : 'Incidents'}
          </h1>
          <p className="mt-1 text-muted">Search, filter, and open tickets</p>
        </div>
        <Link
          to="/incidents/new"
          className="rounded-xl bg-electric-500 px-4 py-2.5 text-sm font-medium text-white hover:bg-electric-600"
        >
          New incident
        </Link>
      </div>

      <div className="grid gap-3 rounded-2xl border border-line bg-white p-4 shadow-sm md:grid-cols-4">
        <label className="relative md:col-span-2">
          <Search className="pointer-events-none absolute left-3 top-2.5 text-slate-400" size={16} />
          <input
            value={search}
            onChange={(e) => {
              setPage(1)
              setSearch(e.target.value)
            }}
            placeholder="Search title or ticket"
            className="w-full rounded-xl border border-line py-2 pl-9 pr-3 outline-none ring-electric-500 focus:ring-2"
          />
        </label>
        <select
          value={status}
          onChange={(e) => {
            setPage(1)
            setStatus(e.target.value as IncidentStatus | '')
          }}
          className="rounded-xl border border-line px-3 py-2"
        >
          <option value="">All statuses</option>
          {['New', 'Assigned', 'InProgress', 'Resolved', 'Closed', 'Reopened'].map((s) => (
            <option key={s} value={s}>{s}</option>
          ))}
        </select>
        <select
          value={priority}
          onChange={(e) => {
            setPage(1)
            setPriority(e.target.value as IncidentPriority | '')
          }}
          className="rounded-xl border border-line px-3 py-2"
        >
          <option value="">All priorities</option>
          {['Low', 'Medium', 'High', 'Critical'].map((p) => (
            <option key={p} value={p}>{p}</option>
          ))}
        </select>
      </div>

      <div className="overflow-hidden rounded-2xl border border-line bg-white shadow-sm">
        {isLoading && <div className="p-8 text-muted">Loading incidents…</div>}
        {error && (
          <div className="p-8 text-rose-700">
            {error instanceof Error ? error.message : 'Failed to load incidents'}
          </div>
        )}
        {!isLoading && data && data.items.length === 0 && (
          <div className="p-10 text-center text-muted">No incidents match your filters.</div>
        )}
        {data && data.items.length > 0 && (
          <table className="min-w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-3 font-medium">Ticket</th>
                <th className="px-4 py-3 font-medium">Title</th>
                <th className="px-4 py-3 font-medium">Priority</th>
                <th className="px-4 py-3 font-medium">Status</th>
                <th className="px-4 py-3 font-medium">SLA</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => (
                <tr key={item.id} className="border-t border-line hover:bg-slate-50/80">
                  <td className="px-4 py-3 font-medium text-electric-600">
                    <Link to={`/incidents/${item.id}`}>{item.ticketNumber}</Link>
                  </td>
                  <td className="px-4 py-3">
                    <div className="font-medium text-navy-900">{item.title}</div>
                    <div className="text-xs text-muted">{item.categoryName}</div>
                  </td>
                  <td className="px-4 py-3"><PriorityBadge priority={item.priority} /></td>
                  <td className="px-4 py-3"><StatusBadge status={item.status} /></td>
                  <td className="px-4 py-3">
                    <span className={item.isOverdue ? 'font-medium text-rose-600' : 'text-slate-600'}>
                      {new Date(item.slaDeadlineUtc).toLocaleString()}
                      {item.isOverdue ? ' · Overdue' : ''}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted">
            Page {data.page} of {data.totalPages} · {data.totalCount} total
          </p>
          <div className="flex gap-2">
            <button
              type="button"
              disabled={!data.hasPreviousPage}
              onClick={() => setPage((p) => p - 1)}
              className="rounded-xl border border-line bg-white px-3 py-2 text-sm disabled:opacity-40"
            >
              Previous
            </button>
            <button
              type="button"
              disabled={!data.hasNextPage}
              onClick={() => setPage((p) => p + 1)}
              className="rounded-xl border border-line bg-white px-3 py-2 text-sm disabled:opacity-40"
            >
              Next
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
