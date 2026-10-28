import axios, { AxiosError } from 'axios'
import type { ApiResponse } from '../types/api'

// Empty base URL uses the Vite dev-server proxy (/api -> http://localhost:5088),
// which avoids CORS issues during local development.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? ''

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 20000,
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('if_access_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

export function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data === undefined || response.data === null) {
    throw new Error(response.message ?? response.errors?.[0] ?? 'Request failed')
  }
  return response.data
}

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<ApiResponse<unknown>>
    if (axiosError.code === 'ERR_NETWORK' || axiosError.message === 'Network Error') {
      return 'Network Error: API unreachable. Start the backend with: dotnet run --project src/IncidentFlow.API --launch-profile http (http://localhost:5088)'
    }
    const payload = axiosError.response?.data
    if (payload?.message) return payload.message
    if (payload?.errors?.length) return payload.errors.join(', ')
    if (axiosError.response?.status) return `Request failed (${axiosError.response.status})`
    return axiosError.message
  }
  if (error instanceof Error) return error.message
  return 'Unexpected error'
}
