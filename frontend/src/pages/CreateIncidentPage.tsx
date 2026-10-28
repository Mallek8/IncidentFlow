import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useToast } from '../components/Toast'
import { api, unwrap } from '../lib/api'
import type { ApiResponse, IncidentCategory, IncidentDetail, IncidentPriority } from '../types/api'

export function CreateIncidentPage() {
  const toast = useToast()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [priority, setPriority] = useState<IncidentPriority>('Medium')

  const categories = useQuery({
    queryKey: ['categories'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<IncidentCategory[]>>('/api/categories')
      return unwrap(data)
    },
  })

  const mutation = useMutation({
    mutationFn: async () => {
      const { data } = await api.post<ApiResponse<IncidentDetail>>('/api/incidents', {
        title,
        description,
        categoryId,
        priority,
      })
      return unwrap(data)
    },
    onSuccess: async (incident) => {
      await queryClient.invalidateQueries({ queryKey: ['incidents'] })
      toast.push('Incident created', 'success')
      navigate(`/incidents/${incident.id}`)
    },
    onError: (err: Error) => toast.push(err.message, 'error'),
  })

  const onSubmit = (e: FormEvent) => {
    e.preventDefault()
    if (!categoryId) {
      toast.push('Select a category', 'error')
      return
    }
    mutation.mutate()
  }

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <div>
        <h1 className="font-display text-3xl text-navy-900">Create incident</h1>
        <p className="mt-1 text-muted">Describe the technical issue for IT support</p>
      </div>
      <form onSubmit={onSubmit} className="space-y-4 rounded-2xl border border-line bg-white p-6 shadow-sm">
        <label className="block space-y-1 text-sm">
          <span className="font-medium">Title</span>
          <input
            className="w-full rounded-xl border border-line px-3 py-2.5 outline-none ring-electric-500 focus:ring-2"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            required
            maxLength={200}
          />
        </label>
        <label className="block space-y-1 text-sm">
          <span className="font-medium">Description</span>
          <textarea
            className="min-h-36 w-full rounded-xl border border-line px-3 py-2.5 outline-none ring-electric-500 focus:ring-2"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
            maxLength={4000}
          />
        </label>
        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block space-y-1 text-sm">
            <span className="font-medium">Category</span>
            <select
              className="w-full rounded-xl border border-line px-3 py-2.5"
              value={categoryId}
              onChange={(e) => setCategoryId(e.target.value)}
              required
            >
              <option value="">Select category</option>
              {categories.data?.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </label>
          <label className="block space-y-1 text-sm">
            <span className="font-medium">Priority</span>
            <select
              className="w-full rounded-xl border border-line px-3 py-2.5"
              value={priority}
              onChange={(e) => setPriority(e.target.value as IncidentPriority)}
            >
              {['Low', 'Medium', 'High', 'Critical'].map((p) => (
                <option key={p} value={p}>{p}</option>
              ))}
            </select>
          </label>
        </div>
        <button
          type="submit"
          disabled={mutation.isPending}
          className="rounded-xl bg-electric-500 px-4 py-2.5 font-medium text-white hover:bg-electric-600 disabled:opacity-60"
        >
          {mutation.isPending ? 'Submitting…' : 'Submit incident'}
        </button>
      </form>
    </div>
  )
}
