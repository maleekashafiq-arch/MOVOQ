import { useQuery } from '@tanstack/react-query'
import { AlertTriangle, ShieldAlert, ShieldCheck } from 'lucide-react'
import { api } from '../lib/api'
import { formatDateTime, humanizeEnum } from '../lib/format'
import type { AdminFraudSignal } from '../lib/types'
import { DataTable, type Column } from '../components/DataTable'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading } from '../components/PageState'
import { SeverityBadge } from '../components/StatusBadge'

export function Fraud() {
  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['fraud'],
    queryFn: () => api.fraud(100),
  })

  if (isPending) return <PageLoading label="Loading fraud signals" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const columns: Column<AdminFraudSignal>[] = [
    {
      key: 'userName',
      label: 'User',
      sortable: true,
      sortValue: (signal) => signal.userName.toLowerCase(),
      render: (signal) => <span className="font-medium text-neutral-900">{signal.userName}</span>,
    },
    {
      key: 'signalType',
      label: 'Signal',
      sortable: true,
      sortValue: (signal) => signal.signalType,
      render: (signal) => humanizeEnum(signal.signalType),
    },
    { key: 'description', label: 'Detail', render: (signal) => signal.description },
    {
      key: 'severity',
      label: 'Severity',
      sortable: true,
      sortValue: (signal) => signal.severity,
      render: (signal) => <SeverityBadge severity={signal.severity} />,
    },
    {
      key: 'detectedAt',
      label: 'Detected',
      sortable: true,
      sortValue: (signal) => signal.detectedAt,
      render: (signal) => formatDateTime(signal.detectedAt),
    },
  ]

  const count = (severity: AdminFraudSignal['severity']) =>
    data.filter((signal) => signal.severity === severity).length

  return (
    <>
      <PageHeader
        title="Fraud monitoring"
        subtitle="Signals the server raised while validating incoming activity."
      />

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard
          title="Critical"
          value={count('Critical')}
          icon={ShieldAlert}
          colorClass="from-[#E74C3C] to-[#8338EC]"
        />
        <KPICard
          title="High"
          value={count('High')}
          icon={AlertTriangle}
          colorClass="from-[#f59e0b] to-[#E74C3C]"
        />
        <KPICard title="Medium" value={count('Medium')} icon={AlertTriangle} />
        <KPICard
          title="Low"
          value={count('Low')}
          icon={ShieldCheck}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
      </div>

      <div className="mb-4 rounded-xl border border-neutral-200 bg-white p-4 text-sm text-neutral-600">
        Today the server raises a signal when a device claims an implausible number of steps in one
        day; that day is rejected and earns nothing. More detection rules will appear here as the ads
        and redemption features land.
      </div>

      <DataTable
        data={data}
        columns={columns}
        rowKey={(signal) => signal.id}
        emptyMessage="No fraud signals have been raised."
      />
    </>
  )
}
