import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Coins, Gift, Package, Plus, TrendingUp } from 'lucide-react'
import { api, ApiError } from '../lib/api'
import { formatNumber } from '../lib/format'
import type { AdminReward, RewardStatus, SaveRewardPayload } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { RewardStatusBadge } from '../components/StatusBadge'

const statuses: RewardStatus[] = ['Draft', 'Active', 'Inactive', 'OutOfStock', 'Expired', 'Discontinued']

const blank = (sponsorId: string): SaveRewardPayload => ({
  sponsorId,
  name: '',
  description: null,
  pointsCost: 1000,
  stockQuantity: 100,
  status: 'Active',
  validFrom: null,
  validUntil: null,
  termsAndConditions: null,
})

export function Rewards() {
  const queryClient = useQueryClient()
  const [form, setForm] = useState<(SaveRewardPayload & { id?: string }) | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data, isPending, error: loadError, refetch } = useQuery({
    queryKey: ['rewards'],
    queryFn: api.rewards,
  })

  const save = useMutation({
    mutationFn: (payload: SaveRewardPayload & { id?: string }) =>
      payload.id ? api.updateReward(payload.id, payload) : api.createReward(payload),
    onSuccess: async () => {
      setForm(null)
      setError(null)
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['rewards'] }),
        queryClient.invalidateQueries({ queryKey: ['sponsors'] }),
      ])
    },
    onError: (cause) => setError(cause instanceof ApiError ? cause.message : 'The reward could not be saved.'),
  })

  if (isPending) return <PageLoading label="Loading rewards" />
  if (loadError) return <PageError error={loadError} onRetry={() => void refetch()} />

  const defaultSponsor = data.sponsors[0]?.id ?? ''

  const columns: Column<AdminReward>[] = [
    { key: 'name', label: 'Reward', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
    { key: 'sponsorName', label: 'Sponsor', render: (row) => row.sponsorName },
    {
      key: 'pointsCost',
      label: 'Cost',
      render: (row) => <span className="font-medium text-[#00C9A7]">{formatNumber(row.pointsCost)} pts</span>,
    },
    { key: 'stockQuantity', label: 'Stock', render: (row) => (row.stockQuantity == null ? 'Unlimited' : formatNumber(row.stockQuantity)) },
    { key: 'redeemed', label: 'Redeemed', render: (row) => formatNumber(row.redeemed) },
    { key: 'status', label: 'Status', render: (row) => <RewardStatusBadge status={row.status} /> },
  ]

  return (
    <>
      <PageHeader
        title="Rewards"
        subtitle="The catalogue users spend points on. Each reward belongs to a sponsor."
        actions={
          <button
            type="button"
            disabled={data.sponsors.length === 0}
            onClick={() => setForm(blank(defaultSponsor))}
            className="flex items-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2 text-sm font-semibold text-white disabled:opacity-40"
          >
            <Plus className="h-4 w-4" />
            Add reward
          </button>
        }
      />

      {data.sponsors.length === 0 && (
        <p className="mb-6 rounded-lg bg-amber-50 p-3 text-sm text-amber-800">
          Add a sponsor first. Rewards cannot exist without a brand behind them.
        </p>
      )}
      {error && <p className="mb-6 rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard title="Live rewards" value={data.liveRewards} icon={Gift} />
        <KPICard title="Redemptions today" value={data.redemptionsToday} icon={TrendingUp} colorClass="from-[#3A86FF] to-[#8338EC]" />
        <KPICard
          title="Points redeemed"
          value={formatNumber(data.pointsRedeemedThisMonth)}
          hint="this month"
          icon={Coins}
          colorClass="from-[#f59e0b] to-[#E74C3C]"
        />
        <KPICard title="Out of stock" value={data.outOfStock} icon={Package} colorClass="from-[#E74C3C] to-[#8338EC]" />
      </div>

      <Panel title="Catalogue" description="Cost, stock, and lifetime redemptions per reward. Click a row to edit.">
        <DataTable
          data={data.items}
          columns={columns}
          rowKey={(row) => row.id}
          onRowClick={(row) =>
            setForm({
              id: row.id,
              sponsorId: row.sponsorId,
              name: row.name,
              description: row.description,
              pointsCost: row.pointsCost,
              stockQuantity: row.stockQuantity,
              status: row.status,
              validFrom: row.validFrom,
              validUntil: row.validUntil,
              termsAndConditions: null,
            })
          }
          emptyMessage="No rewards yet."
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
            <h2 className="text-lg font-semibold">{form.id ? 'Edit reward' : 'Add reward'}</h2>
            <select
              required
              value={form.sponsorId}
              onChange={(e) => setForm({ ...form, sponsorId: e.target.value })}
              className={inputClass}
            >
              {data.sponsors.map((sponsor) => (
                <option key={sponsor.id} value={sponsor.id}>
                  {sponsor.name}
                  {sponsor.isActive ? '' : ' (paused)'}
                </option>
              ))}
            </select>
            <input
              required
              placeholder="Reward name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              className={inputClass}
            />
            <textarea
              placeholder="Description"
              value={form.description ?? ''}
              onChange={(e) => setForm({ ...form, description: e.target.value || null })}
              className={inputClass}
              rows={3}
            />
            <div className="grid grid-cols-2 gap-3">
              <input
                type="number"
                min={1}
                value={form.pointsCost}
                onChange={(e) => setForm({ ...form, pointsCost: Number(e.target.value) })}
                className={inputClass}
              />
              <input
                type="number"
                min={0}
                placeholder="Stock"
                value={form.stockQuantity ?? ''}
                onChange={(e) =>
                  setForm({ ...form, stockQuantity: e.target.value === '' ? null : Number(e.target.value) })
                }
                className={inputClass}
              />
            </div>
            <select
              value={form.status}
              onChange={(e) => setForm({ ...form, status: e.target.value as RewardStatus })}
              className={inputClass}
            >
              {statuses.map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
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
