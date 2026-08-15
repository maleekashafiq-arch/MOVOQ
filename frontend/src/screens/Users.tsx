import { useState } from 'react'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Ban, Coins, Search, ShieldCheck, X } from 'lucide-react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api, ApiError } from '../lib/api'
import { formatDate, formatDateTime, formatNumber, formatShortDay, formatSigned } from '../lib/format'
import type { AdminUser, UserStatus } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { UserStatusBadge } from '../components/StatusBadge'

const PAGE_SIZE = 25

const statusFilters: { label: string; value: UserStatus | '' }[] = [
  { label: 'All', value: '' },
  { label: 'Active', value: 'Active' },
  { label: 'Suspended', value: 'Suspended' },
  { label: 'Pending', value: 'PendingVerification' },
]

export function Users() {
  const [search, setSearch] = useState('')
  const [applied, setApplied] = useState('')
  const [status, setStatus] = useState<UserStatus | ''>('')
  const [page, setPage] = useState(1)
  const [selectedId, setSelectedId] = useState<string | null>(null)

  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['users', applied, status, page],
    queryFn: () => api.users({ query: applied, status, page, pageSize: PAGE_SIZE }),
    placeholderData: keepPreviousData,
  })

  const columns: Column<AdminUser>[] = [
    {
      key: 'name',
      label: 'User',
      sortable: true,
      sortValue: (user) => user.name.toLowerCase(),
      render: (user) => (
        <div>
          <p className="font-medium text-neutral-900">{user.name}</p>
          <p className="text-xs text-neutral-500">{user.email}</p>
        </div>
      ),
    },
    {
      key: 'location',
      label: 'Location',
      render: (user) => [user.city, user.country].filter(Boolean).join(', ') || '—',
    },
    {
      key: 'dailyTargetSteps',
      label: 'Daily goal',
      sortable: true,
      sortValue: (user) => user.dailyTargetSteps,
      render: (user) => formatNumber(user.dailyTargetSteps),
    },
    {
      key: 'steps',
      label: 'Lifetime steps',
      sortable: true,
      sortValue: (user) => user.steps,
      render: (user) => formatNumber(user.steps),
    },
    {
      key: 'points',
      label: 'Points',
      sortable: true,
      sortValue: (user) => user.points,
      render: (user) => (
        <span className="font-medium text-[#00C9A7]">{formatNumber(user.points)}</span>
      ),
    },
    {
      key: 'status',
      label: 'Status',
      sortable: true,
      sortValue: (user) => user.status,
      render: (user) => <UserStatusBadge status={user.status} />,
    },
    { key: 'createdAt', label: 'Joined', render: (user) => formatDate(user.createdAt) },
  ]

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1

  return (
    <>
      <PageHeader
        title="Users"
        subtitle="Every registered account, with the balances and standing the app sees."
      />

      <div className="mb-4 flex flex-wrap items-center gap-3">
        <form
          onSubmit={(event) => {
            event.preventDefault()
            setPage(1)
            setApplied(search.trim())
          }}
          className="relative min-w-[260px] flex-1"
        >
          <Search className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-neutral-400" />
          <input
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            placeholder="Search by name, email, phone, or city"
            className="w-full rounded-lg border border-neutral-200 bg-white py-2 pl-10 pr-4 text-sm outline-none transition-colors focus:border-[#3A86FF]"
          />
        </form>

        <div className="flex gap-1 rounded-lg border border-neutral-200 bg-white p-1">
          {statusFilters.map((filter) => (
            <button
              key={filter.label}
              type="button"
              onClick={() => {
                setPage(1)
                setStatus(filter.value)
              }}
              className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                status === filter.value
                  ? 'bg-neutral-900 text-white'
                  : 'text-neutral-600 hover:bg-neutral-100'
              }`}
            >
              {filter.label}
            </button>
          ))}
        </div>
      </div>

      {data && (
        <div className="mb-4 flex flex-wrap gap-4 text-sm text-neutral-500">
          <span>{formatNumber(data.totalCount)} matching</span>
          <span>{formatNumber(data.activeCount)} active</span>
          <span>{formatNumber(data.suspendedCount)} suspended</span>
          <span>{formatNumber(data.pendingCount)} pending verification</span>
        </div>
      )}

      {isPending ? (
        <PageLoading label="Loading users" />
      ) : error ? (
        <PageError error={error} onRetry={() => void refetch()} />
      ) : (
        <>
          <DataTable
            data={data.items}
            columns={columns}
            rowKey={(user) => user.id}
            onRowClick={(user) => setSelectedId(user.id)}
            emptyMessage="No users match this filter."
          />

          {totalPages > 1 && (
            <div className="mt-4 flex items-center justify-between">
              <p className="text-sm text-neutral-500">
                Page {data.page} of {totalPages}
              </p>
              <div className="flex gap-2">
                <PageButton disabled={page <= 1} onClick={() => setPage(page - 1)}>
                  Previous
                </PageButton>
                <PageButton disabled={page >= totalPages} onClick={() => setPage(page + 1)}>
                  Next
                </PageButton>
              </div>
            </div>
          )}
        </>
      )}

      {selectedId && <UserDrawer userId={selectedId} onClose={() => setSelectedId(null)} />}
    </>
  )
}

function PageButton({
  disabled,
  onClick,
  children,
}: {
  disabled: boolean
  onClick: () => void
  children: React.ReactNode
}) {
  return (
    <button
      type="button"
      disabled={disabled}
      onClick={onClick}
      className="rounded-lg border border-neutral-200 bg-white px-3 py-1.5 text-sm font-medium text-neutral-700 transition-colors hover:bg-neutral-50 disabled:opacity-40"
    >
      {children}
    </button>
  )
}

function UserDrawer({ userId, onClose }: { userId: string; onClose: () => void }) {
  const queryClient = useQueryClient()
  const [amount, setAmount] = useState('')
  const [reason, setReason] = useState('')
  const [feedback, setFeedback] = useState<{ tone: 'ok' | 'error'; message: string } | null>(null)

  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['user', userId],
    queryFn: () => api.user(userId, 7),
  })

  const invalidate = async () => {
    await Promise.all([
      queryClient.invalidateQueries({ queryKey: ['users'] }),
      queryClient.invalidateQueries({ queryKey: ['user', userId] }),
      queryClient.invalidateQueries({ queryKey: ['dashboard'] }),
    ])
  }

  const adjust = useMutation({
    mutationFn: (delta: number) => api.adjustPoints(userId, delta, reason.trim()),
    onSuccess: async (result) => {
      setAmount('')
      setReason('')
      setFeedback({
        tone: 'ok',
        message: `${formatSigned(result.amount)} points applied. New balance ${formatNumber(result.balance)}.`,
      })
      await invalidate()
    },
    onError: (cause) => setFeedback({ tone: 'error', message: describe(cause) }),
  })

  const changeStatus = useMutation({
    mutationFn: (next: UserStatus) =>
      api.changeStatus(userId, next, reason.trim() || undefined),
    onSuccess: async (user) => {
      setReason('')
      setFeedback({ tone: 'ok', message: `Account is now ${user.status}.` })
      await invalidate()
    },
    onError: (cause) => setFeedback({ tone: 'error', message: describe(cause) }),
  })

  const parsed = Number.parseInt(amount, 10)
  const canAdjust =
    Number.isFinite(parsed) && parsed > 0 && reason.trim().length > 0 && !adjust.isPending

  return (
    <div className="fixed inset-0 z-50 flex justify-end">
      <button
        type="button"
        aria-label="Close panel"
        className="flex-1 bg-black/40"
        onClick={onClose}
      />
      <aside className="w-full max-w-xl overflow-y-auto bg-neutral-50 shadow-xl">
        <div className="sticky top-0 flex items-center justify-between border-b border-neutral-200 bg-white px-6 py-4">
          <h2 className="font-semibold text-neutral-900">User detail</h2>
          <button
            type="button"
            onClick={onClose}
            className="rounded-lg p-1.5 text-neutral-500 transition-colors hover:bg-neutral-100"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        <div className="space-y-6 p-6">
          {isPending ? (
            <PageLoading label="Loading user" />
          ) : error ? (
            <PageError error={error} onRetry={() => void refetch()} />
          ) : (
            <>
              <Panel title={data.user.name} description={data.user.email}>
                <dl className="grid grid-cols-2 gap-4 text-sm">
                  <Detail label="Status" value={<UserStatusBadge status={data.user.status} />} />
                  <Detail label="Points" value={formatNumber(data.user.points)} />
                  <Detail label="Lifetime steps" value={formatNumber(data.user.steps)} />
                  <Detail label="Daily goal" value={formatNumber(data.user.dailyTargetSteps)} />
                  <Detail
                    label="Location"
                    value={[data.user.city, data.user.country].filter(Boolean).join(', ') || '—'}
                  />
                  <Detail label="Phone" value={data.user.phoneNumber ?? '—'} />
                  <Detail label="Joined" value={formatDate(data.user.createdAt)} />
                  <Detail
                    label="Last login"
                    value={data.user.lastLoginAt ? formatDateTime(data.user.lastLoginAt) : 'Never'}
                  />
                </dl>
              </Panel>

              <Panel title="Last seven days" description="Validated steps against the daily goal.">
                <ResponsiveContainer width="100%" height={200}>
                  <BarChart
                    data={data.stepHistory.map((day) => ({
                      label: formatShortDay(day.date),
                      steps: day.steps,
                      goal: day.goal,
                    }))}
                  >
                    <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
                    <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
                    <YAxis stroke="#a3a3a3" fontSize={12} />
                    <Tooltip />
                    <Bar dataKey="steps" name="Steps" fill="#00C9A7" radius={[8, 8, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </Panel>

              <Panel
                title="Adjust points"
                description="Posts to the ledger and the audit trail. A reason is required."
              >
                <div className="space-y-3">
                  <div className="grid gap-3 sm:grid-cols-2">
                    <label className="block">
                      <span className="mb-1.5 block text-sm font-medium text-neutral-700">
                        Amount
                      </span>
                      <input
                        type="number"
                        min={1}
                        value={amount}
                        onChange={(event) => setAmount(event.target.value)}
                        placeholder="100"
                        className="w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none focus:border-[#3A86FF]"
                      />
                    </label>
                    <label className="block">
                      <span className="mb-1.5 block text-sm font-medium text-neutral-700">
                        Reason
                      </span>
                      <input
                        value={reason}
                        onChange={(event) => setReason(event.target.value)}
                        placeholder="Goodwill for a failed redemption"
                        className="w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none focus:border-[#3A86FF]"
                      />
                    </label>
                  </div>

                  <div className="flex flex-wrap gap-2">
                    <button
                      type="button"
                      disabled={!canAdjust}
                      onClick={() => adjust.mutate(parsed)}
                      className="flex items-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2 text-sm font-semibold text-white transition-opacity hover:opacity-90 disabled:opacity-40"
                    >
                      <Coins className="h-4 w-4" />
                      Credit points
                    </button>
                    <button
                      type="button"
                      disabled={!canAdjust}
                      onClick={() => adjust.mutate(-parsed)}
                      className="rounded-lg border border-neutral-200 px-4 py-2 text-sm font-medium text-neutral-700 transition-colors hover:bg-neutral-100 disabled:opacity-40"
                    >
                      Deduct points
                    </button>
                  </div>
                </div>
              </Panel>

              <Panel
                title="Account standing"
                description="Suspending blocks the account from earning. The reason above is recorded."
              >
                <div className="flex flex-wrap gap-2">
                  {data.user.status === 'Suspended' ? (
                    <button
                      type="button"
                      disabled={changeStatus.isPending}
                      onClick={() => changeStatus.mutate('Active')}
                      className="flex items-center gap-2 rounded-lg border border-green-200 bg-green-50 px-4 py-2 text-sm font-medium text-green-700 transition-colors hover:bg-green-100 disabled:opacity-40"
                    >
                      <ShieldCheck className="h-4 w-4" />
                      Reinstate account
                    </button>
                  ) : (
                    <button
                      type="button"
                      disabled={changeStatus.isPending}
                      onClick={() => changeStatus.mutate('Suspended')}
                      className="flex items-center gap-2 rounded-lg border border-red-200 bg-red-50 px-4 py-2 text-sm font-medium text-red-700 transition-colors hover:bg-red-100 disabled:opacity-40"
                    >
                      <Ban className="h-4 w-4" />
                      Suspend account
                    </button>
                  )}
                </div>
              </Panel>

              {feedback && (
                <p
                  className={`rounded-lg p-3 text-sm ${
                    feedback.tone === 'ok'
                      ? 'bg-green-50 text-green-800'
                      : 'bg-red-50 text-red-700'
                  }`}
                >
                  {feedback.message}
                </p>
              )}

              <Panel title="Recent ledger entries">
                {data.recentLedger.length === 0 ? (
                  <p className="py-6 text-center text-sm text-neutral-500">
                    This account has no ledger history.
                  </p>
                ) : (
                  <ul className="divide-y divide-neutral-100">
                    {data.recentLedger.map((entry) => (
                      <li key={entry.id} className="flex items-start justify-between gap-4 py-3">
                        <div className="min-w-0">
                          <p className="text-sm font-medium text-neutral-900">
                            {entry.transactionType}
                          </p>
                          <p className="truncate text-xs text-neutral-500">
                            {entry.description ?? '—'}
                          </p>
                        </div>
                        <div className="shrink-0 text-right">
                          <p
                            className={`text-sm font-semibold ${
                              entry.amount >= 0 ? 'text-green-600' : 'text-red-600'
                            }`}
                          >
                            {formatSigned(entry.amount)}
                          </p>
                          <p className="text-xs text-neutral-400">
                            {formatDateTime(entry.occurredAt)}
                          </p>
                        </div>
                      </li>
                    ))}
                  </ul>
                )}
              </Panel>
            </>
          )}
        </div>
      </aside>
    </div>
  )
}

function Detail({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-wide text-neutral-400">{label}</dt>
      <dd className="mt-1 font-medium text-neutral-900">{value}</dd>
    </div>
  )
}

function describe(cause: unknown) {
  return cause instanceof ApiError
    ? cause.message
    : cause instanceof Error
      ? cause.message
      : 'The change could not be saved.'
}
