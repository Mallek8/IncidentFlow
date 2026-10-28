import type { IncidentPriority, IncidentStatus } from '../types/api'

const statusStyles: Record<IncidentStatus, string> = {
  New: 'bg-sky-50 text-sky-700 ring-sky-200',
  Assigned: 'bg-indigo-50 text-indigo-700 ring-indigo-200',
  InProgress: 'bg-amber-50 text-amber-800 ring-amber-200',
  Resolved: 'bg-emerald-50 text-emerald-700 ring-emerald-200',
  Closed: 'bg-slate-100 text-slate-600 ring-slate-200',
  Reopened: 'bg-orange-50 text-orange-700 ring-orange-200',
}

const priorityStyles: Record<IncidentPriority, string> = {
  Low: 'bg-slate-100 text-slate-600 ring-slate-200',
  Medium: 'bg-blue-50 text-blue-700 ring-blue-200',
  High: 'bg-orange-50 text-orange-700 ring-orange-200',
  Critical: 'bg-rose-50 text-rose-700 ring-rose-200',
}

export function StatusBadge({ status }: { status: IncidentStatus }) {
  return (
    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${statusStyles[status]}`}>
      {status}
    </span>
  )
}

export function PriorityBadge({ priority }: { priority: IncidentPriority }) {
  return (
    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ring-1 ring-inset ${priorityStyles[priority]}`}>
      {priority}
    </span>
  )
}
