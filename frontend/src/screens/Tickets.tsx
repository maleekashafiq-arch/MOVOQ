import { useQuery } from '@tanstack/react-query'
import { Coins, Ticket, Timer, Users } from 'lucide-react'
import { api } from '../lib/api'
import { formatDateTime, formatNumber } from '../lib/format'
import type { AdminTicketRow } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'
import { Badge } from '../components/StatusBadge'

const ticketStyles: Record<AdminTicketRow['status'], string> = {
  Active: 'bg-green-100 text-green-700',
  Used: 'bg-purple-100 text-purple-700',
  Cancelled: 'bg-red-100 text-red-700',
  Expired: 'bg-neutral-100 text-neutral-600',
}

export function Tickets() {
  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['tickets'],
    queryFn: () => api.tickets(),
  })

  if (isPending) return <PageLoading label="Loading tickets" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const columns: Column<AdminTicketRow>[] = [
    { key: 'userName', label: 'User', render: (row) => <span className="font-medium text-neutral-900">{row.userName}</span> },
    { key: 'drawName', label: 'Draw', render: (row) => row.drawName },
    { key: 'ticketNumber', label: 'Ticket', render: (row) => row.ticketNumber },
    {
      key: 'pointsCost',
      label: 'Points spent',
      render: (row) => <span className="text-[#8338EC]">{formatNumber(row.pointsCost)}</span>,
    },
    {
      key: 'status',
      label: 'Status',
      render: (row) => <Badge className={ticketStyles[row.status]}>{row.status}</Badge>,
    },
    { key: 'purchasedAt', label: 'Purchased', render: (row) => formatDateTime(row.purchasedAt) },
  ]

  return (
    <>
      <PageHeader
        title="Tickets"
        subtitle="Points converted into entries. Tickets are created by the mobile app, not by this screen."
      />

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard title="Tickets issued today" value={formatNumber(data.ticketsToday)} icon={Ticket} />
        <KPICard
          title="Active tickets"
          value={formatNumber(data.activeTickets)}
          hint="not yet drawn"
          icon={Timer}
          colorClass="from-[#3A86FF] to-[#8338EC]"
        />
        <KPICard
          title="Points converted"
          value={formatNumber(data.pointsSpentToday)}
          hint="today"
          icon={Coins}
          colorClass="from-[#f59e0b] to-[#E74C3C]"
        />
        <KPICard
          title="Buyers today"
          value={formatNumber(data.buyersToday)}
          icon={Users}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
      </div>

      <Panel title="Recent purchases" description={`${formatNumber(data.ticketsAllTime)} tickets issued in total.`}>
        <DataTable data={data.recent} columns={columns} rowKey={(row) => row.id} emptyMessage="No tickets have been bought yet." />
      </Panel>
    </>
  )
}
