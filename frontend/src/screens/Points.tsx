import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router'
import { ArrowDownRight, ArrowUpRight, Coins, Wallet } from 'lucide-react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { api } from '../lib/api'
import { formatNumber } from '../lib/format'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'

export function Points() {
  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['points'],
    queryFn: () => api.points(6),
  })

  if (isPending) return <PageLoading label="Loading the points economy" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const rules = [
    { label: 'Daily goal reached', value: data.rules.dailyGoalReward, live: true },
    { label: 'Rewarded ad watched', value: data.rules.adReward, live: false },
    { label: 'Streak day', value: data.rules.streakReward, live: false },
    { label: 'Spin wheel maximum', value: data.rules.spinMaxReward, live: false },
  ]

  return (
    <>
      <PageHeader
        title="Points wallet"
        subtitle="What the platform is issuing, what users are spending, and what it still owes them."
      />

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard title="Issued today" value={formatNumber(data.issuedToday)} icon={ArrowUpRight} />
        <KPICard
          title="Spent today"
          value={formatNumber(data.spentToday)}
          icon={ArrowDownRight}
          colorClass="from-[#8338EC] to-[#E74C3C]"
        />
        <KPICard
          title="Lifetime earned"
          value={formatNumber(data.lifetimeEarned)}
          hint={`${formatNumber(data.lifetimeSpent)} spent all time`}
          icon={Coins}
          colorClass="from-[#f59e0b] to-[#E74C3C]"
        />
        <KPICard
          title="Outstanding balance"
          value={formatNumber(data.outstandingBalance)}
          hint="points users can still redeem"
          icon={Wallet}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Panel title="Earned against spent" description="Ledger totals by month.">
            <ResponsiveContainer width="100%" height={300}>
              <BarChart data={data.flow}>
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
                <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
                <YAxis stroke="#a3a3a3" fontSize={12} />
                <Tooltip />
                <Bar dataKey="earned" name="Earned" fill="#00C9A7" radius={[8, 8, 0, 0]} />
                <Bar dataKey="spent" name="Spent" fill="#8338EC" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </Panel>
        </div>

        <Panel
          title="Reward rules"
          description="Edit these on the settings screen."
          actions={
            <Link
              to="/admin/settings"
              className="rounded-lg border border-neutral-200 px-3 py-1.5 text-sm font-medium text-neutral-700 transition-colors hover:bg-neutral-50"
            >
              Edit
            </Link>
          }
        >
          <ul className="divide-y divide-neutral-100">
            {rules.map((rule) => (
              <li key={rule.label} className="flex items-center justify-between gap-3 py-3">
                <div>
                  <p className="text-sm font-medium text-neutral-900">{rule.label}</p>
                  {!rule.live && (
                    <p className="text-xs text-neutral-400">
                      Saved, but the earning feature is not built yet
                    </p>
                  )}
                </div>
                <span className="shrink-0 font-semibold text-[#00C9A7]">
                  {formatNumber(rule.value)} pts
                </span>
              </li>
            ))}
          </ul>
        </Panel>
      </div>
    </>
  )
}
