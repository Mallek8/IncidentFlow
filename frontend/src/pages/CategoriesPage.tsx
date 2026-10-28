import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState, type FormEvent } from 'react'
import { useToast } from '../components/Toast'
import { api, unwrap } from '../lib/api'
import type { ApiResponse, IncidentCategory } from '../types/api'

export function CategoriesPage() {
  const toast = useToast()
  const queryClient = useQueryClient()
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')

  const categories = useQuery({
    queryKey: ['categories'],
    queryFn: async () => {
      const { data } = await api.get<ApiResponse<IncidentCategory[]>>('/api/categories')
      return unwrap(data)
    },
  })

  const create = useMutation({
    mutationFn: async () => {
      const { data } = await api.post<ApiResponse<IncidentCategory>>('/api/categories', { name, description })
      return unwrap(data)
    },
    onSuccess: async () => {
      setName('')
      setDescription('')
      toast.push('Category created', 'success')
      await queryClient.invalidateQueries({ queryKey: ['categories'] })
    },
    onError: (err: Error) => toast.push(err.message, 'error'),
  })

  const onSubmit = (e: FormEvent) => {
    e.preventDefault()
    create.mutate()
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="font-display text-3xl text-navy-900">Incident categories</h1>
        <p className="mt-1 text-muted">Admin configuration</p>
      </div>
      <form onSubmit={onSubmit} className="grid gap-3 rounded-2xl border border-line bg-white p-6 shadow-sm md:grid-cols-[1fr_1fr_auto]">
        <input
          className="rounded-xl border border-line px-3 py-2.5"
          placeholder="Name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          required
        />
        <input
          className="rounded-xl border border-line px-3 py-2.5"
          placeholder="Description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />
        <button type="submit" className="rounded-xl bg-electric-500 px-4 py-2.5 font-medium text-white">
          Add
        </button>
      </form>
      <div className="overflow-hidden rounded-2xl border border-line bg-white shadow-sm">
        {categories.isLoading && <div className="p-6 text-muted">Loading…</div>}
        <ul className="divide-y divide-line">
          {categories.data?.map((c) => (
            <li key={c.id} className="px-5 py-4">
              <p className="font-medium text-navy-900">{c.name}</p>
              <p className="text-sm text-muted">{c.description || 'No description'}</p>
            </li>
          ))}
        </ul>
      </div>
    </div>
  )
}
