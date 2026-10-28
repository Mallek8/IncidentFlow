export type IncidentStatus =
  | 'New'
  | 'Assigned'
  | 'InProgress'
  | 'Resolved'
  | 'Closed'
  | 'Reopened'

export type IncidentPriority = 'Low' | 'Medium' | 'High' | 'Critical'

export interface ApiResponse<T> {
  success: boolean
  message?: string | null
  data?: T | null
  errors?: string[] | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface UserProfile {
  id: string
  email: string
  firstName: string
  lastName: string
  fullName: string
  roles: string[]
  isActive: boolean
}

export interface AuthResult {
  accessToken: string
  refreshToken: string
  accessTokenExpiresAtUtc: string
  user: UserProfile
}

export interface IncidentListItem {
  id: string
  ticketNumber: string
  title: string
  categoryName: string
  priority: IncidentPriority
  status: IncidentStatus
  createdByUserId: string
  assignedTechnicianId?: string | null
  createdAtUtc: string
  updatedAtUtc?: string | null
  slaDeadlineUtc: string
  isOverdue: boolean
}

export interface IncidentDetail extends IncidentListItem {
  description: string
  categoryId: string
  resolutionDateUtc?: string | null
  resolutionNotes?: string | null
  allowedNextStatuses: IncidentStatus[]
  comments: IncidentComment[]
  history: IncidentHistoryItem[]
}

export interface IncidentComment {
  id: string
  authorUserId: string
  content: string
  isInternal: boolean
  createdAtUtc: string
}

export interface IncidentHistoryItem {
  id: string
  changedByUserId: string
  changeType: string
  fieldName?: string | null
  oldValue?: string | null
  newValue?: string | null
  oldStatus?: IncidentStatus | null
  newStatus?: IncidentStatus | null
  changedAtUtc: string
  notes?: string | null
}

export interface IncidentCategory {
  id: string
  name: string
  description?: string | null
  isActive: boolean
}

export interface DashboardSummary {
  totalIncidents: number
  openIncidents: number
  criticalIncidents: number
  resolvedIncidents: number
  averageResolutionHours?: number | null
  slaComplianceRate: number
  byCategory: NamedCount[]
  byPriority: NamedCount[]
  monthlyTrends: MonthlyTrend[]
}

export interface NamedCount {
  name: string
  count: number
}

export interface MonthlyTrend {
  month: string
  count: number
}
