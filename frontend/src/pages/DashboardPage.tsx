import { useQuery } from '@tanstack/react-query'
import {
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { api, unwrap } from '../lib/api'
import type { ApiResponse, DashboardSummary } from '../types/api'

export function DashboardPage() {
  const { data, isLoading, error } = useQuery({
    queryKey: ['dashboard'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<DashboardSummary>>('/api/dashboard/summary')
      return unwrap(data)
    },
  })

  if (isLoading) {
    return <div className="rounded-2xl border border-line bg-white p-8 text-muted">Loading dashboard…</div>
  }

  if (error || !data) {
    return (
      <div className="rounded-2xl border border-rose-200 bg-rose-50 p-8 text-rose-800">
        {error instanceof Error ? error.message : 'Unable to load dashboard. Ensure the API and SQL Server are running.'}
      </div>
    )
  }

  const cards = [
    { label: 'Total incidents', value: data.totalIncidents },
    { label: 'Open', value: data.openIncidents },
    { label: 'Critical open', value: data.criticalIncidents },
    { label: 'Resolved', value: data.resolvedIncidents },
    {
      label: 'Avg resolution (h)',
      value: data.averageResolutionHours == null ? '—' : data.averageResolutionHours.toFixed(1),
    },
    { label: 'SLA compliance', value: `${data.slaComplianceRate}%` },
  ]

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-3xl text-navy-900">Operations dashboard</h1>
        <p className="mt-1 text-muted">Live metrics from the IncidentFlow database</p>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {cards.map((card) => (
          <div key={card.label} className="rounded-2xl border border-line bg-white p-5 shadow-sm">
            <p className="text-sm text-muted">{card.label}</p>
            <p className="mt-2 text-3xl font-semibold text-navy-900">{card.value}</p>
          </div>
        ))}
      </div>

      <div className="grid gap-6 xl:grid-cols-2">
        <div className="rounded-2xl border border-line bg-white p-5 shadow-sm">
          <h2 className="mb-4 font-semibold text-navy-900">Incidents by category</h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <BarChart data={data.byCategory}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="name" tick={{ fontSize: 12 }} />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="count" fill="#2563eb" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </div>
        </div>
        <div className="rounded-2xl border border-line bg-white p-5 shadow-sm">
          <h2 className="mb-4 font-semibold text-navy-900">Monthly trends</h2>
          <div className="h-72">
            <ResponsiveContainer width="100%" height="100%">
              <LineChart data={data.monthlyTrends}>
                <CartesianGrid strokeDasharray="3 3" vertical={false} />
                <XAxis dataKey="month" tick={{ fontSize: 12 }} />
                <YAxis allowDecimals={false} />
                <Tooltip />
                <Line type="monotone" dataKey="count" stroke="#102a4c" strokeWidth={3} dot />
              </LineChart>
            </ResponsiveContainer>
          </div>
        </div>
      </div>
    </div>
  )
}