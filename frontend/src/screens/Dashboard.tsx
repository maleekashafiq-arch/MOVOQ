import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router'
import {
  Activity,
  Coins,
  Footprints,
  ShieldAlert,
  Target,
  TrendingUp,
  Users,
  Wallet,
} from 'lucide-react'
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { api } from '../lib/api'
import { formatNumber, formatRelative, formatShortDay } from '../lib/format'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'

export function Dashboard() {
  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['dashboard'],
    queryFn: api.dashboard,
  })

  if (isPending) return <PageLoading label="Loading the dashboard" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const stepTrend = data.stepActivity.map((day) => ({
    label: formatShortDay(day.date),
    steps: day.steps,
    activeUsers: day.activeUsers,
  }))

  return (
    <>
      <PageHeader
        title="Dashboard"
        subtitle="Live platform activity, refreshed every time you open this page."
      />

      {data.openFraudSignals > 0 && (
        <Link
          to="/admin/fraud"
          className="mb-6 flex items-center gap-3 rounded-xl border border-red-200 bg-red-50 p-4 transition-colors hover:bg-red-100"
        >
          <ShieldAlert className="h-5 w-5 shrink-0 text-red-600" />
          <div className="flex-1">
            <p className="font-medium text-red-900">
              {data.openFraudSignals} fraud {data.openFraudSignals === 1 ? 'signal' : 'signals'} in
              the last seven days
            </p>
            <p className="text-sm text-red-700">Open fraud monitoring to review them.</p>
          </div>
        </Link>
      )}

      <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <KPICard
          title="Total users"
          value={formatNumber(data.totalUsers)}
          hint={`${formatNumber(data.newUsersThisMonth)} joined this month`}
          icon={Users}
        />
        <KPICard
          title="Active today"
          value={formatNumber(data.activeUsersToday)}
          hint={`${formatNumber(data.goalsCompletedToday)} reached their goal`}
          icon={Activity}
          colorClass="from-[#3A86FF] to-[#8338EC]"
        />
        <KPICard
          title="Steps today"
          value={formatNumber(data.stepsToday)}
          icon={Footprints}
          colorClass="from-[#00C9A7] to-[#3A86FF]"
        />
        <KPICard
          title="Points issued today"
          value={formatNumber(data.pointsIssuedToday)}
          hint={`${formatNumber(data.pointsSpentToday)} spent`}
          icon={Coins}
          colorClass="from-[#f59e0b] to-[#E74C3C]"
        />
      </div>

      <div className="mb-6 grid gap-6 lg:grid-cols-2">
        <Panel title="User growth" description="Cumulative registered accounts by month.">
          <ResponsiveContainer width="100%" height={260}>
            <LineChart data={data.userGrowth}>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
              <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
              <YAxis stroke="#a3a3a3" fontSize={12} />
              <Tooltip />
              <Line
                type="monotone"
                dataKey="value"
                name="Users"
                stroke="#3A86FF"
                strokeWidth={2}
                dot={false}
              />
            </LineChart>
          </ResponsiveContainer>
        </Panel>

        <Panel title="Step activity" description="Validated steps across the last seven days.">
          <ResponsiveContainer width="100%" height={260}>
            <AreaChart data={stepTrend}>
              <defs>
                <linearGradient id="stepsFill" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="5%" stopColor="#00C9A7" stopOpacity={0.35} />
                  <stop offset="95%" stopColor="#00C9A7" stopOpacity={0} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
              <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
              <YAxis stroke="#a3a3a3" fontSize={12} />
              <Tooltip />
              <Area
                type="monotone"
                dataKey="steps"
                name="Steps"
                stroke="#00C9A7"
                strokeWidth={2}
                fill="url(#stepsFill)"
              />
            </AreaChart>
          </ResponsiveContainer>
        </Panel>
      </div>

      <div className="mb-6 grid gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Panel title="Points earned and spent" description="Ledger totals by month.">
            <ResponsiveContainer width="100%" height={260}>
              <BarChart data={data.pointsFlow}>
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

        <Panel title="Outstanding balance" description="Points held across every wallet.">
          <div className="flex h-full flex-col justify-center gap-4">
            <div className="flex items-center gap-3">
              <div className="flex h-12 w-12 items-center justify-center rounded-lg bg-gradient-to-br from-[#00C9A7] to-[#3A86FF]">
                <Wallet className="h-6 w-6 text-white" />
              </div>
              <div>
                <p className="text-3xl font-bold text-neutral-900">
                  {formatNumber(data.outstandingBalance)}
                </p>
                <p className="text-sm text-neutral-500">points liability</p>
              </div>
            </div>
            <p className="text-sm text-neutral-500">
              This is what users could redeem right now. It moves whenever points are earned, spent,
              or adjusted by an operator.
            </p>
          </div>
        </Panel>
      </div>

      <div className="grid gap-6 lg:grid-cols-2">
        <Panel title="Recent activity" description="The latest entries in the points ledger.">
          {data.recentActivity.length === 0 ? (
            <Empty message="No ledger activity yet." />
          ) : (
            <ul className="divide-y divide-neutral-100">
              {data.recentActivity.map((item) => (
                <li key={item.id} className="flex items-start justify-between gap-4 py-3">
                  <div className="min-w-0">
                    <p className="truncate font-medium text-neutral-900">{item.user}</p>
                    <p className="text-sm text-neutral-500">{item.action}</p>
                    <p className="truncate text-xs text-neutral-400">{item.details}</p>
                  </div>
                  <span className="shrink-0 text-xs text-neutral-400">
                    {formatRelative(item.occurredAt)}
                  </span>
                </li>
              ))}
            </ul>
          )}
        </Panel>

        <Panel title="Top walkers today" description="Ranked by validated steps.">
          {data.topUsers.length === 0 ? (
            <Empty message="Nobody has synced steps today." />
          ) : (
            <ul className="divide-y divide-neutral-100">
              {data.topUsers.map((user) => (
                <li key={user.userId} className="flex items-center gap-4 py-3">
                  <span className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-neutral-100 text-sm font-semibold text-neutral-600">
                    {user.rank}
                  </span>
                  <div className="min-w-0 flex-1">
                    <p className="truncate font-medium text-neutral-900">{user.name}</p>
                    <p className="text-sm text-neutral-500">
                      {formatNumber(user.steps)} steps · {formatNumber(user.points)} points
                    </p>
                  </div>
                  <Target className="h-4 w-4 shrink-0 text-neutral-300" />
                </li>
              ))}
            </ul>
          )}
        </Panel>
      </div>
    </>
  )
}

function Empty({ message }: { message: string }) {
  return (
    <div className="flex flex-col items-center gap-2 py-10 text-center">
      <TrendingUp className="h-6 w-6 text-neutral-300" />
      <p className="text-sm text-neutral-500">{message}</p>
    </div>
  )
}
