import { useQuery } from '@tanstack/react-query'
import { api } from '../lib/api'
import { formatDateTime, humanizeEnum } from '../lib/format'
import type { AdminAuditEntry } from '../lib/types'
import { Badge } from '../components/StatusBadge'
import { DataTable, type Column } from '../components/DataTable'
import { PageError, PageHeader, PageLoading } from '../components/PageState'

const actionStyles: Record<string, string> = {
  AdminLogin: 'bg-neutral-100 text-neutral-600',
  PointsAdjusted: 'bg-amber-100 text-amber-700',
  UserStatusChanged: 'bg-red-100 text-red-700',
  SettingsUpdated: 'bg-blue-100 text-blue-700',
  DrawModified: 'bg-purple-100 text-purple-700',
  DrawWinnerSelected: 'bg-purple-100 text-purple-700',
  PrizeFulfillmentUpdated: 'bg-blue-100 text-blue-700',
  SponsorModified: 'bg-teal-100 text-teal-700',
  RewardModified: 'bg-teal-100 text-teal-700',
  AdminUserModified: 'bg-purple-100 text-purple-700',
}

export function Audit() {
  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['audit'],
    queryFn: () => api.audit(100),
  })

  if (isPending) return <PageLoading label="Loading the audit trail" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const columns: Column<AdminAuditEntry>[] = [
    {
      key: 'occurredAt',
      label: 'When',
      sortable: true,
      sortValue: (entry) => entry.occurredAt,
      render: (entry) => formatDateTime(entry.occurredAt),
    },
    {
      key: 'operator',
      label: 'Operator',
      sortable: true,
      sortValue: (entry) => entry.operator.toLowerCase(),
      render: (entry) => <span className="font-medium text-neutral-900">{entry.operator}</span>,
    },
    {
      key: 'action',
      label: 'Action',
      sortable: true,
      sortValue: (entry) => entry.action,
      render: (entry) => (
        <Badge className={actionStyles[entry.action] ?? 'bg-neutral-100 text-neutral-600'}>
          {humanizeEnum(entry.action)}
        </Badge>
      ),
    },
    { key: 'entityName', label: 'Entity', render: (entry) => entry.entityName },
    {
      key: 'change',
      label: 'Change',
      render: (entry) => (
        <div className="max-w-md">
          {entry.oldValue && (
            <p className="truncate text-xs text-neutral-400 line-through">{entry.oldValue}</p>
          )}
          <p className="truncate text-neutral-700">{entry.newValue ?? '—'}</p>
        </div>
      ),
    },
    { key: 'ipAddress', label: 'From', render: (entry) => entry.ipAddress ?? '—' },
  ]

  return (
    <>
      <PageHeader
        title="Audit trail"
        subtitle="Every privileged back-office action, append-only and attributed to an operator."
      />

      <DataTable
        data={data}
        columns={columns}
        rowKey={(entry) => entry.id}
        emptyMessage="No back-office actions recorded yet."
      />
    </>
  )
}
