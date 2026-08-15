import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Gift, Package, Timer } from 'lucide-react'
import { api, ApiError } from '../lib/api'
import { formatDate, formatNumber } from '../lib/format'
import type { AdminPrize, PrizeFulfillmentStatus } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { FulfillmentBadge } from '../components/StatusBadge'

const statuses: PrizeFulfillmentStatus[] = ['AwaitingDetails', 'Packed', 'Shipped', 'Delivered']

export function Prizes() {
  const queryClient = useQueryClient()
  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['prizes'],
    queryFn: () => api.prizes(),
  })

  const update = useMutation({
    mutationFn: ({ id, status }: { id: string; status: PrizeFulfillmentStatus }) =>
      api.updateFulfillment(id, status),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['prizes'] }),
  })

  if (isPending) return <PageLoading label="Loading prizes" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const columns: Column<AdminPrize>[] = [
    { key: 'userName', label: 'Winner', render: (row) => <span className="font-medium text-neutral-900">{row.userName}</span> },
    { key: 'prizeDescription', label: 'Prize', render: (row) => row.prizeDescription },
    { key: 'drawName', label: 'Draw', render: (row) => row.drawName },
    { key: 'selectedAt', label: 'Won', render: (row) => formatDate(row.selectedAt) },
    {
      key: 'fulfillmentStatus',
      label: 'Fulfillment',
      render: (row) => (
        <div className="flex items-center gap-2">
          <FulfillmentBadge status={row.fulfillmentStatus} />
          <select
            value={row.fulfillmentStatus}
            disabled={update.isPending}
            onChange={(event) =>
              update.mutate({ id: row.id, status: event.target.value as PrizeFulfillmentStatus })
            }
            className="rounded-md border border-neutral-200 bg-white px-2 py-1 text-xs"
          >
            {statuses.map((status) => (
              <option key={status} value={status}>
                {status === 'AwaitingDetails' ? 'Awaiting details' : status}
              </option>
            ))}
          </select>
        </div>
      ),
    },
  ]

  return (
    <>
      <PageHeader
        title="Prizes & fulfillment"
        subtitle="After a winner is picked, track the prize from that result through to delivery."
      />

      {update.error && (
        <p className="mb-6 rounded-lg bg-red-50 p-3 text-sm text-red-700">
          {update.error instanceof ApiError ? update.error.message : 'Fulfillment could not be updated.'}
        </p>
      )}

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard title="Awaiting details" value={data.awaitingDetails} icon={Timer} colorClass="from-[#f59e0b] to-[#E74C3C]" />
        <KPICard title="In transit" value={data.inTransit} icon={Package} colorClass="from-[#3A86FF] to-[#8338EC]" />
        <KPICard title="Delivered" value={data.delivered} icon={Gift} />
        <KPICard
          title="Prizes awarded"
          value={formatNumber(data.awardedAllTime)}
          hint="all time"
          icon={Gift}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
      </div>

      <Panel title="Open fulfillments" description="Each winner is tracked from the draw result to delivery.">
        <DataTable
          data={data.items}
          columns={columns}
          rowKey={(row) => row.id}
          emptyMessage="No winners yet. Close a draw and pick a winner first."
        />
      </Panel>
    </>
  )
}
