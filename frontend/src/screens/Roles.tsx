import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Plus, Shield, ShieldCheck, Timer, UsersRound } from 'lucide-react'
import { api, ApiError } from '../lib/api'
import { formatDateTime } from '../lib/format'
import { useAdminSession } from '../lib/auth'
import type { AdminOperator, AdminRole, AdminStatus, SaveOperatorPayload } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { OperatorStatusBadge, RoleBadge } from '../components/StatusBadge'

const roles: AdminRole[] = ['SuperAdmin', 'Admin', 'Support', 'ReadOnly']
const statuses: AdminStatus[] = ['Active', 'PendingActivation', 'Suspended', 'Deactivated']

const roleGrants: { role: AdminRole; description: string }[] = [
  { role: 'SuperAdmin', description: 'Full access, including operator management.' },
  { role: 'Admin', description: 'Manages users, draws, rewards, and settings.' },
  { role: 'Support', description: 'Reads the back office. Cannot change records.' },
  { role: 'ReadOnly', description: 'Sees dashboards and reports only.' },
]

const blank = (): SaveOperatorPayload => ({
  email: '',
  displayName: null,
  role: 'Admin',
  status: 'Active',
})

export function Roles() {
  const session = useAdminSession()
  const canManage = session.role === 'SuperAdmin'
  const queryClient = useQueryClient()
  const [form, setForm] = useState<(SaveOperatorPayload & { id?: string }) | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { data, isPending, error: loadError, refetch } = useQuery({
    queryKey: ['operators'],
    queryFn: api.operators,
  })

  const save = useMutation({
    mutationFn: (payload: SaveOperatorPayload & { id?: string }) =>
      payload.id ? api.updateOperator(payload.id, payload) : api.createOperator(payload),
    onSuccess: async () => {
      setForm(null)
      setError(null)
      await queryClient.invalidateQueries({ queryKey: ['operators'] })
    },
    onError: (cause) =>
      setError(cause instanceof ApiError ? cause.message : 'The operator could not be saved.'),
  })

  if (isPending) return <PageLoading label="Loading operators" />
  if (loadError) return <PageError error={loadError} onRetry={() => void refetch()} />

  const columns: Column<AdminOperator>[] = [
    {
      key: 'email',
      label: 'Operator',
      render: (row) => (
        <div>
          <p className="font-medium text-neutral-900">{row.displayName ?? row.email}</p>
          <p className="text-xs text-neutral-500">{row.email}</p>
        </div>
      ),
    },
    { key: 'role', label: 'Role', render: (row) => <RoleBadge role={row.role} /> },
    { key: 'status', label: 'Status', render: (row) => <OperatorStatusBadge status={row.status} /> },
    {
      key: 'lastLoginAt',
      label: 'Last sign-in',
      render: (row) => (row.lastLoginAt ? formatDateTime(row.lastLoginAt) : 'Never'),
    },
  ]

  return (
    <>
      <PageHeader
        title="Roles & permissions"
        subtitle="Firebase proves who they are. This list is what proves they may open the back office."
        actions={
          canManage ? (
            <button
              type="button"
              onClick={() => {
                setForm(blank())
                setError(null)
              }}
              className="flex items-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2 text-sm font-semibold text-white"
            >
              <Plus className="h-4 w-4" />
              Add operator
            </button>
          ) : undefined
        }
      />

      {error && <p className="mb-6 rounded-lg bg-red-50 p-3 text-sm text-red-700">{error}</p>}

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard title="Operators" value={data.operatorCount} icon={Shield} />
        <KPICard
          title="SuperAdmins"
          value={data.superAdminCount}
          icon={ShieldCheck}
          colorClass="from-[#3A86FF] to-[#8338EC]"
        />
        <KPICard
          title="Signed in today"
          value={data.signedInToday}
          icon={Timer}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
        <KPICard title="Roles" value={4} icon={UsersRound} colorClass="from-[#f59e0b] to-[#E74C3C]" />
      </div>

      <div className="mb-6 grid gap-4 lg:grid-cols-2">
        {roleGrants.map((item) => (
          <div key={item.role} className="rounded-xl border border-neutral-200 bg-white p-4">
            <RoleBadge role={item.role} />
            <p className="mt-2 text-sm text-neutral-600">{item.description}</p>
          </div>
        ))}
      </div>

      <Panel
        title="Allowlist"
        description={
          canManage
            ? 'Click a row to change role or suspend access. They must already have a Firebase account with this email.'
            : 'Only a SuperAdmin can add or change operators.'
        }
      >
        <DataTable
          data={data.items}
          columns={columns}
          rowKey={(row) => row.id}
          onRowClick={
            canManage
              ? (row) =>
                  setForm({
                    id: row.id,
                    email: row.email,
                    displayName: row.displayName,
                    role: row.role,
                    status: row.status,
                  })
              : undefined
          }
          emptyMessage="No operators yet."
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
            <h2 className="text-lg font-semibold">{form.id ? 'Edit operator' : 'Add operator'}</h2>
            <p className="text-sm text-neutral-500">
              Sign-in still happens in Firebase. Adding someone here only lets that verified email into
              this app.
            </p>
            <input
              required
              type="email"
              placeholder="Email"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              className={inputClass}
            />
            <input
              placeholder="Display name"
              value={form.displayName ?? ''}
              onChange={(e) => setForm({ ...form, displayName: e.target.value || null })}
              className={inputClass}
            />
            <select
              value={form.role}
              onChange={(e) => setForm({ ...form, role: e.target.value as AdminRole })}
              className={inputClass}
            >
              {roles.map((role) => (
                <option key={role} value={role}>
                  {role}
                </option>
              ))}
            </select>
            <select
              value={form.status}
              onChange={(e) => setForm({ ...form, status: e.target.value as AdminStatus })}
              className={inputClass}
            >
              {statuses.map((status) => (
                <option key={status} value={status}>
                  {status === 'PendingActivation' ? 'Pending activation' : status}
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
