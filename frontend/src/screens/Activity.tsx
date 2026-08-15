import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { Activity as ActivityIcon, Footprints, Target, Users } from 'lucide-react'
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
import { formatNumber, formatShortDay } from '../lib/format'
import { KPICard } from '../components/KPICard'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'

const ranges = [7, 14, 30, 90]

export function Activity() {
  const [days, setDays] = useState(14)

  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['activity', days],
    queryFn: () => api.activity(days),
  })

  return (
    <>
      <PageHeader
        title="Activity"
        subtitle="How much the platform is walking, and when devices report it."
        actions={
          <div className="flex gap-1 rounded-lg border border-neutral-200 bg-white p-1">
            {ranges.map((range) => (
              <button
                key={range}
                type="button"
                onClick={() => setDays(range)}
                className={`rounded-md px-3 py-1.5 text-sm font-medium transition-colors ${
                  days === range
                    ? 'bg-neutral-900 text-white'
                    : 'text-neutral-600 hover:bg-neutral-100'
                }`}
              >
                {range}d
              </button>
            ))}
          </div>
        }
      />

      {isPending ? (
        <PageLoading label="Loading activity" />
      ) : error ? (
        <PageError error={error} onRetry={() => void refetch()} />
      ) : (
        <>
          <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <KPICard title="Steps today" value={formatNumber(data.stepsToday)} icon={Footprints} />
            <KPICard
              title="Active users today"
              value={formatNumber(data.activeUsersToday)}
              icon={Users}
              colorClass="from-[#3A86FF] to-[#8338EC]"
            />
            <KPICard
              title="Average steps"
              value={formatNumber(data.averageStepsPerActiveUser)}
              hint="per active user"
              icon={ActivityIcon}
              colorClass="from-[#00C9A7] to-[#3A86FF]"
            />
            <KPICard
              title="Goals reached"
              value={formatNumber(data.goalsCompletedToday)}
              hint="today"
              icon={Target}
              colorClass="from-[#f59e0b] to-[#E74C3C]"
            />
          </div>

          <div className="mb-6 grid gap-6 lg:grid-cols-2">
            <Panel title="Daily steps" description={`Validated steps over the last ${days} days.`}>
              <ResponsiveContainer width="100%" height={280}>
                <AreaChart data={data.daily.map(toPoint)}>
                  <defs>
                    <linearGradient id="dailyStepsFill" x1="0" y1="0" x2="0" y2="1">
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
                    fill="url(#dailyStepsFill)"
                  />
                </AreaChart>
              </ResponsiveContainer>
            </Panel>

            <Panel title="Active users" description="Accounts that synced at least one step.">
              <ResponsiveContainer width="100%" height={280}>
                <LineChart data={data.daily.map(toPoint)}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
                  <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
                  <YAxis stroke="#a3a3a3" fontSize={12} />
                  <Tooltip />
                  <Line
                    type="monotone"
                    dataKey="activeUsers"
                    name="Active users"
                    stroke="#3A86FF"
                    strokeWidth={2}
                    dot={false}
                  />
                </LineChart>
              </ResponsiveContainer>
            </Panel>
          </div>

          <Panel
            title="Sync traffic by hour"
            description="When today's devices reported in, in UTC. Devices send a whole day at a time, so this shows sync timing rather than when people walked."
          >
            <ResponsiveContainer width="100%" height={260}>
              <BarChart
                data={data.syncsByHour.map((bucket) => ({
                  label: `${String(bucket.hour).padStart(2, '0')}:00`,
                  syncs: bucket.syncs,
                }))}
              >
                <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
                <XAxis dataKey="label" stroke="#a3a3a3" fontSize={11} interval={1} />
                <YAxis stroke="#a3a3a3" fontSize={12} allowDecimals={false} />
                <Tooltip />
                <Bar dataKey="syncs" name="Syncs" fill="#8338EC" radius={[8, 8, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </Panel>
        </>
      )}
    </>
  )
}

function toPoint(day: { date: string; steps: number; activeUsers: number }) {
  return { label: formatShortDay(day.date), steps: day.steps, activeUsers: day.activeUsers }
}
