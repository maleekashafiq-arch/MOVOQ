import { useState, type ReactNode } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Gift, Loader2, Plus, Ticket, Timer, Trophy } from 'lucide-react'
import { api, ApiError } from '../lib/api'
import { daysFromNow, fromLocalInput, toLocalInput } from '../lib/datetime'
import { formatDateTime, formatNumber } from '../lib/format'
import type { AdminDraw, LuckyDrawStatus, SaveDrawPayload } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { DrawStatusBadge } from '../components/StatusBadge'

const emptyForm = (): SaveDrawPayload => ({
  name: '',
  description: null,
  prizeDescription: '',
  emoji: '🎁',
  imageUrl: null,
  ticketCostPoints: 100,
  startAt: fromLocalInput(daysFromNow(0)),
  endAt: fromLocalInput(daysFromNow(14)),
  maxTickets: null,
  isFeatured: false,
})

export function Draws() {
  const queryClient = useQueryClient()
  const [form, setForm] = useState<SaveDrawPayload | null>(null)
  const [editingId, setEditingId] = useState<string | null>(null)
  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data, isPending, error: loadError, refetch } = useQuery({
    queryKey: ['draws'],
    queryFn: api.draws,
  })

  const detail = useQuery({
    queryKey: ['draw', selectedId],
    queryFn: () => api.draw(selectedId!),
    enabled: selectedId !== null,
  })

  const invalidate = async () => {
    await queryClient.invalidateQueries({ queryKey: ['draws'] })
    if (selectedId) await queryClient.invalidateQueries({ queryKey: ['draw', selectedId] })
    await queryClient.invalidateQueries({ queryKey: ['tickets'] })
    await queryClient.invalidateQueries({ queryKey: ['prizes'] })
  }

  const create = useMutation({
    mutationFn: api.createDraw,
    onSuccess: async () => {
      setForm(null)
      setEditingId(null)
      setError(null)
      await invalidate()
    },
    onError: (cause) => setError(cause instanceof ApiError ? cause.message : 'The draw could not be saved.'),
  })

  const update = useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: SaveDrawPayload }) => api.updateDraw(id, payload),
    onSuccess: async () => {
      setForm(null)
      setEditingId(null)
      setError(null)
      await invalidate()
    },
    onError: (cause) => setError(cause instanceof ApiError ? cause.message : 'The draw could not be updated.'),
  })

  const changeStatus = useMutation({
    mutationFn: ({ id, status }: { id: string; status: LuckyDrawStatus }) => api.changeDrawStatus(id, status),
    onSuccess: async () => {
      setError(null)
      await invalidate()
    },
    onError: (cause) => setError(cause instanceof ApiError ? cause.message : 'The status could not be changed.'),
  })

  const pickWinner = useMutation({
    mutationFn: api.selectWinner,
    onSuccess: async () => {
      setError(null)
      await invalidate()
    },
    onError: (cause) => setError(cause instanceof ApiError ? cause.message : 'A winner could not be picked.'),
  })

  if (isPending) return <PageLoading label="Loading lucky draws" />
  if (loadError) return <PageError error={loadError} onRetry={() => void refetch()} />

  const open = data.filter((draw) => draw.status === 'Active').length
  const entries = data.reduce((sum, draw) => sum + draw.tickets, 0)
  const next = data
    .filter((draw) => draw.status === 'Active')
    .slice()
    .sort((a, b) => a.endAt.localeCompare(b.endAt))[0]
  const awarded = data.filter((draw) => draw.isWinnerSelected).length

  const columns: Column<AdminDraw>[] = [
    {
      key: 'name',
      label: 'Draw',
      render: (draw) => (
        <div>
          <p className="font-medium text-neutral-900">
            {draw.emoji ? `${draw.emoji} ` : ''}
            {draw.name}
          </p>
          <p className="text-xs text-neutral-500">{draw.prizeDescription}</p>
        </div>
      ),
    },
    { key: 'tickets', label: 'Entries', render: (draw) => formatNumber(draw.tickets) },
    { key: 'endAt', label: 'Ends', render: (draw) => formatDateTime(draw.endAt) },
    { key: 'status', label: 'Status', render: (draw) => <DrawStatusBadge status={draw.status} /> },
    {
      key: 'actions',
      label: '',
      render: (draw) => (
        <div className="flex flex-wrap justify-end gap-1">
          {(draw.status === 'Draft' || draw.status === 'Active') && (
            <ActionButton
              label="Edit"
              onClick={() => {
                setEditingId(draw.id)
                setForm(toForm(draw))
                setError(null)
              }}
            />
          )}
          {draw.status === 'Draft' && (
            <ActionButton label="Activate" onClick={() => changeStatus.mutate({ id: draw.id, status: 'Active' })} />
          )}
          {draw.status === 'Active' && (
            <ActionButton label="Close" onClick={() => changeStatus.mutate({ id: draw.id, status: 'Closed' })} />
          )}
          {draw.status === 'Closed' && (
            <ActionButton label="Pick winner" onClick={() => pickWinner.mutate(draw.id)} />
          )}
          {(draw.status === 'Draft' || draw.status === 'Active' || draw.status === 'Closed') && (
            <ActionButton
              label="Cancel"
              tone="danger"
              onClick={() => changeStatus.mutate({ id: draw.id, status: 'Cancelled' })}
            />
          )}
        </div>
      ),
    },
  ]

  return (
    <>
      <PageHeader
        title="Lucky draws"
        subtitle="Create a draw, sell tickets from the app, then close it and pick a winner."
        actions={
          <button
            type="button"
            onClick={() => {
              setEditingId(null)
              setForm(emptyForm())
              setError(null)
            }}
            className="flex items-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2 text-sm font-semibold text-white"
          >
            <Plus className="h-4 w-4" />
            New draw
          </button>
        }
      />

      {error && <p className="mb-6 rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard title="Open draws" value={open} icon={Trophy} />
        <KPICard title="Total entries" value={formatNumber(entries)} icon={Ticket} colorClass="from-[#3A86FF] to-[#8338EC]" />
        <KPICard
          title="Next draw"
          value={next ? formatDateTime(next.endAt) : '—'}
          hint={next?.name}
          icon={Timer}
          colorClass="from-[#f59e0b] to-[#E74C3C]"
        />
        <KPICard title="Prizes awarded" value={awarded} icon={Gift} colorClass="from-[#00C9A7] to-[#3A86FF]" />
      </div>

      <Panel title="Draw schedule" description="Click a row to see who entered. Activate a draft before the app can sell tickets.">
        <DataTable data={data} columns={columns} rowKey={(draw) => draw.id} onRowClick={(draw) => setSelectedId(draw.id)} />
      </Panel>

      {form && (
        <DrawForm
          title={editingId ? 'Edit draw' : 'New draw'}
          saveLabel={editingId ? 'Save changes' : 'Create draft'}
          busy={create.isPending || update.isPending}
          form={form}
          onChange={setForm}
          onCancel={() => {
            setForm(null)
            setEditingId(null)
          }}
          onSave={() => (editingId ? update.mutate({ id: editingId, payload: form }) : create.mutate(form))}
        />
      )}

      {selectedId && (
        <aside className="fixed inset-y-0 right-0 z-40 w-full max-w-md overflow-y-auto border-l border-neutral-200 bg-white p-6 shadow-xl">
          <div className="mb-4 flex items-start justify-between gap-3">
            <h2 className="text-lg font-semibold text-neutral-900">Draw entries</h2>
            <button type="button" onClick={() => setSelectedId(null)} className="text-sm text-neutral-500">
              Close
            </button>
          </div>
          {detail.isPending && (
            <p className="flex items-center gap-2 text-sm text-neutral-500">
              <Loader2 className="h-4 w-4 animate-spin" /> Loading...
            </p>
          )}
          {detail.data && (
            <>
              <p className="mb-2 text-sm text-neutral-600">
                {detail.data.draw.name} · {formatNumber(detail.data.draw.tickets)} tickets ·{' '}
                {formatNumber(detail.data.draw.participants)} people
              </p>
              {detail.data.winner && (
                <p className="mb-4 rounded-lg bg-purple-50 p-3 text-sm text-purple-900">
                  Winner: {detail.data.winner.userName} with ticket {detail.data.winner.ticketNumber}
                </p>
              )}
              <ul className="space-y-2 text-sm">
                {detail.data.entries.length === 0 && <li className="text-neutral-500">No tickets yet.</li>}
                {detail.data.entries.map((entry) => (
                  <li key={entry.ticketId} className="flex justify-between gap-3 rounded-lg border border-neutral-100 px-3 py-2">
                    <span>
                      <span className="font-medium text-neutral-900">{entry.userName}</span>
                      <span className="text-neutral-500"> · {entry.ticketNumber}</span>
                    </span>
                    <span className="text-neutral-500">{formatNumber(entry.pointsCost)} pts</span>
                  </li>
                ))}
              </ul>
            </>
          )}
        </aside>
      )}
    </>
  )
}

function ActionButton({
  label,
  onClick,
  tone = 'default',
}: {
  label: string
  onClick: () => void
  tone?: 'default' | 'danger'
}) {
  return (
    <button
      type="button"
      onClick={(event) => {
        event.stopPropagation()
        onClick()
      }}
      className={`rounded-md px-2 py-1 text-xs font-semibold ${
        tone === 'danger' ? 'text-red-700 hover:bg-red-50' : 'text-[#3A86FF] hover:bg-blue-50'
      }`}
    >
      {label}
    </button>
  )
}

function toForm(draw: AdminDraw): SaveDrawPayload {
  return {
    name: draw.name,
    description: draw.description,
    prizeDescription: draw.prizeDescription,
    emoji: draw.emoji,
    imageUrl: draw.imageUrl,
    ticketCostPoints: draw.ticketCostPoints,
    startAt: draw.startAt,
    endAt: draw.endAt,
    maxTickets: draw.maxTickets,
    isFeatured: draw.isFeatured,
  }
}

function DrawForm({
  form,
  onChange,
  onCancel,
  onSave,
  busy,
  title,
  saveLabel,
}: {
  form: SaveDrawPayload
  onChange: (form: SaveDrawPayload) => void
  onCancel: () => void
  onSave: () => void
  busy: boolean
  title: string
  saveLabel: string
}) {
  const set = <K extends keyof SaveDrawPayload>(key: K, value: SaveDrawPayload[K]) => onChange({ ...form, [key]: value })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form
        className="w-full max-w-lg space-y-3 rounded-2xl bg-white p-6 shadow-xl"
        onSubmit={(event) => {
          event.preventDefault()
          onSave()
        }}
      >
        <h2 className="text-lg font-semibold text-neutral-900">{title}</h2>
        <Field label="Name">
          <input required value={form.name} onChange={(e) => set('name', e.target.value)} className={inputClass} />
        </Field>
        <Field label="Prize">
          <input
            required
            value={form.prizeDescription}
            onChange={(e) => set('prizeDescription', e.target.value)}
            className={inputClass}
          />
        </Field>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Ticket cost (pts)">
            <input
              type="number"
              min={0}
              value={form.ticketCostPoints}
              onChange={(e) => set('ticketCostPoints', Number(e.target.value))}
              className={inputClass}
            />
          </Field>
          <Field label="Emoji">
            <input value={form.emoji ?? ''} onChange={(e) => set('emoji', e.target.value || null)} className={inputClass} />
          </Field>
        </div>
        <div className="grid grid-cols-2 gap-3">
          <Field label="Starts">
            <input
              type="datetime-local"
              value={toLocalInput(form.startAt)}
              onChange={(e) => set('startAt', fromLocalInput(e.target.value))}
              className={inputClass}
            />
          </Field>
          <Field label="Ends">
            <input
              type="datetime-local"
              value={toLocalInput(form.endAt)}
              onChange={(e) => set('endAt', fromLocalInput(e.target.value))}
              className={inputClass}
            />
          </Field>
        </div>
        <label className="flex items-center gap-2 text-sm text-neutral-700">
          <input
            type="checkbox"
            checked={form.isFeatured}
            onChange={(e) => set('isFeatured', e.target.checked)}
          />
          Feature this draw in the app
        </label>
        <div className="flex justify-end gap-2 pt-2">
          <button type="button" onClick={onCancel} className="rounded-lg px-3 py-2 text-sm text-neutral-600">
            Cancel
          </button>
          <button
            type="submit"
            disabled={busy}
            className="rounded-lg bg-[#3A86FF] px-4 py-2 text-sm font-semibold text-white disabled:opacity-40"
          >
            {busy ? 'Saving...' : saveLabel}
          </button>
        </div>
      </form>
    </div>
  )
}

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <label className="block text-sm">
      <span className="mb-1 block font-medium text-neutral-700">{label}</span>
      {children}
    </label>
  )
}

const inputClass =
  'w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none focus:border-[#3A86FF]'
