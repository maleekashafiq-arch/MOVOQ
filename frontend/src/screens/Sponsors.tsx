import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, Sparkles, Store, TrendingUp } from 'lucide-react'
import { api, ApiError } from '../lib/api'
import { formatNumber } from '../lib/format'
import type { AdminSponsor, SaveSponsorPayload } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { Badge } from '../components/StatusBadge'

const blank = (): SaveSponsorPayload => ({
  name: '',
  contactPersonName: null,
  contactEmail: null,
  contactPhone: null,
  websiteUrl: null,
  isActive: true,
})

export function Sponsors() {
  const queryClient = useQueryClient()
  const [form, setForm] = useState<(SaveSponsorPayload & { id?: string }) | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data, isPending, error: loadError, refetch } = useQuery({
    queryKey: ['sponsors'],
    queryFn: api.sponsors,
  })

  const save = useMutation({
    mutationFn: (payload: SaveSponsorPayload & { id?: string }) =>
      payload.id ? api.updateSponsor(payload.id, payload) : api.createSponsor(payload),
    onSuccess: async () => {
      setForm(null)
      setError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['sponsors'] }),
        queryClient.invalidateQueries({ queryKey: ['rewards'] }),
      ])
    },
    onError: (cause) => setError(cause instanceof ApiError ? cause.message : 'The sponsor could not be saved.'),
  })

  if (isPending) return <PageLoading label="Loading sponsors" />
  if (loadError) return <PageError error={loadError} onRetry={() => void refetch()} />

  const columns: Column<AdminSponsor>[] = [
    { key: 'name', label: 'Sponsor', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
    { key: 'contactEmail', label: 'Contact', render: (row) => row.contactEmail ?? row.contactPersonName ?? '—' },
    { key: 'offers', label: 'Offers', render: (row) => formatNumber(row.offers) },
    { key: 'redemptions', label: 'Redemptions', render: (row) => formatNumber(row.redemptions) },
    {
      key: 'isActive',
      label: 'Status',
      render: (row) => (
        <Badge className={row.isActive ? 'bg-green-100 text-green-700' : 'bg-amber-100 text-amber-700'}>
          {row.isActive ? 'Live' : 'Paused'}
        </Badge>
      ),
    },
  ]

  return (
    <>
      <PageHeader
        title="Sponsors & marketplace"
        subtitle="Brands that place rewards in the catalogue. Add a sponsor before you can list a reward."
        actions={
          <button
            type="button"
            onClick={() => setForm(blank())}
            className="flex items-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2 text-sm font-semibold text-white"
          >
            <Plus className="h-4 w-4" />
            Add sponsor
          </button>
        }
      />

      {error && <p className="mb-6 rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        <KPICard title="Active sponsors" value={data.activeSponsors} icon={Store} />
        <KPICard title="Live offers" value={data.liveOffers} icon={Sparkles} colorClass="from-[#3A86FF] to-[#8338EC]" />
        <KPICard
          title="Offer redemptions"
          value={formatNumber(data.redemptionsThisMonth)}
          hint="this month"
          icon={TrendingUp}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
      </div>

      <Panel title="Sponsors" description="Click a row to edit contact details or pause the brand.">
        <DataTable
          data={data.items}
          columns={columns}
          rowKey={(row) => row.id}
          onRowClick={(row) =>
            setForm({
              id: row.id,
              name: row.name,
              contactPersonName: row.contactPersonName,
              contactEmail: row.contactEmail,
              contactPhone: row.contactPhone,
              websiteUrl: row.websiteUrl,
              isActive: row.isActive,
            })
          }
          emptyMessage="No sponsors yet."
        />
      </Panel>

      {form && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
          <form
            className="w-full max-w-lg space-y-3 rounded-2xl bg-white p-6 shadow-xl"
            onSubmit={(event) => {
              event.preventDefault()
              save.mutate(form)
            }}
          >
            <h2 className="text-lg font-semibold">{form.id ? 'Edit sponsor' : 'Add sponsor'}</h2>
            <input
              required
              placeholder="Brand name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className={inputClass}
            />
            <input
              placeholder="Contact name"
              value={form.contactPersonName ?? ''}
              onChange={(e) => setForm({ ...form, contactPersonName: e.target.value || null })}
              className={inputClass}
            />
            <input
              type="email"
              placeholder="Contact email"
              value={form.contactEmail ?? ''}
              onChange={(e) => setForm({ ...form, contactEmail: e.target.value || null })}
              className={inputClass}
            />
            <input
              placeholder="Website"
              value={form.websiteUrl ?? ''}
              onChange={(e) => setForm({ ...form, websiteUrl: e.target.value || null })}
              className={inputClass}
            />
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={form.isActive}
                onChange={(e) => setForm({ ...form, isActive: e.target.checked })}
              />
              Active
            </label>
            <div className="flex justify-end gap-2">
              <button type="button" onClick={() => setForm(null)} className="rounded-lg px-3 py-2 text-sm">
                Cancel
              </button>
              <button
                type="submit"
                disabled={save.isPending}
                className="rounded-lg bg-[#3A86FF] px-4 py-2 text-sm font-semibold text-white disabled:opacity-40"
              >
                Save
              </button>
            </div>
          </form>
        </div>
      )}
    </>
  )
}

const inputClass = 'w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none focus:border-[#3A86FF]'
