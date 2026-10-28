import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { PriorityBadge, StatusBadge } from '../components/StatusBadge'
import { api, unwrap } from '../lib/api'
import type { ApiResponse, IncidentListItem, PagedResult } from '../types/api'

export function SlaPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['sla-overdue'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<PagedResult<IncidentListItem>>>('/api/incidents', {
        params: { overdueOnly: true, page: 1, pageSize: 50, sortBy: 'sla', sortDescending: false },
      })
      return unwrap(data)
    },
  })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-3xl text-navy-900">SLA monitoring</h1>
        <p className="mt-1 text-muted">
          Overdue open incidents based on priority windows (Critical 4h, High 8h, Medium 24h, Low 72h calendar hours).
        </p>
      </div>
      <div className="overflow-hidden rounded-2xl border border-line bg-white shadow-sm">
        {isLoading && <div className="p-8 text-muted">Loading SLA breaches…</div>}
        {error && <div className="p-8 text-rose-700">{error instanceof Error ? error.message : 'Failed to load'}</div>}
        {data && data.items.length === 0 && (
          <div className="p-10 text-center text-muted">No overdue incidents right now.</div>
        )}
        {data && data.items.length > 0 && (
          <table className="min-w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr>
                <th className="px-4 py-3">Ticket</th>
                <th className="px-4 py-3">Title</th>
                <th className="px-4 py-3">Priority</th>
                <th className="px-4 py-3">Status</th>
                <th className="px-4 py-3">Deadline</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => (
                <tr key={item.id} className="border-t border-line">
                  <td className="px-4 py-3">
                    <Link className="font-medium text-electric-600" to={`/incidents/${item.id}`}>
                      {item.ticketNumber}
                    </Link>
                  </td>
                  <td className="px-4 py-3">{item.title}</td>
                  <td className="px-4 py-3"><PriorityBadge priority={item.priority} /></td>
                  <td className="px-4 py-3"><StatusBadge status={item.status} /></td>
                  <td className="px-4 py-3 text-rose-600">{new Date(item.slaDeadlineUtc).toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
