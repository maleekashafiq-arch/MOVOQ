import {
  BarChart3,
  Bell,
  CheckCircle2,
  Clock,
  FileText,
  LifeBuoy,
  MessageSquare,
  Shield,
  Timer,
  TrendingUp,
  Trophy,
  UsersRound,
} from 'lucide-react'
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import { DataTable, type Column } from '../../components/DataTable'
import { Panel } from '../../components/PageState'
import { Badge } from '../../components/StatusBadge'
import { formatNumber } from '../../lib/format'
import { PreviewScreen } from './PreviewScreen'

interface TeamRow {
  id: string
  name: string
  coach: string
  members: number
  weeklySteps: number
  rank: number
}

const teamRows: TeamRow[] = [
  { id: 'tm1', name: 'Karachi Striders', coach: 'Imran Sadiq', members: 42, weeklySteps: 3120000, rank: 1 },
  { id: 'tm2', name: 'Lahore Pacers', coach: 'Nadia Rehman', members: 38, weeklySteps: 2840000, rank: 2 },
  { id: 'tm3', name: 'Islamabad Trailblazers', coach: 'Faisal Khan', members: 29, weeklySteps: 2210000, rank: 3 },
  { id: 'tm4', name: 'Multan Movers', coach: 'Unassigned', members: 18, weeklySteps: 1080000, rank: 4 },
]

const teamColumns: Column<TeamRow>[] = [
  { key: 'rank', label: '#', render: (row) => <span className="font-semibold text-neutral-900">{row.rank}</span> },
  { key: 'name', label: 'Team', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
  {
    key: 'coach',
    label: 'Coach',
    render: (row) =>
      row.coach === 'Unassigned' ? <span className="text-neutral-400">Unassigned</span> : row.coach,
  },
  { key: 'members', label: 'Members', render: (row) => formatNumber(row.members) },
  {
    key: 'weeklySteps',
    label: 'Steps this week',
    render: (row) => <span className="text-[#00C9A7]">{formatNumber(row.weeklySteps)}</span>,
  },
]

export function TeamsPreview() {
  return (
    <PreviewScreen
      title="Teams & coaches"
      subtitle="Group competition and the coaches who run it."
      feature="Teams and coaching"
      kpis={[
        { title: 'Active teams', value: 24, icon: UsersRound },
        { title: 'Members in teams', value: '1,180', icon: Trophy, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Team steps', value: '9.2M', hint: 'this week', icon: TrendingUp, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
        { title: 'Coaches', value: 14, hint: '3 teams unassigned', icon: Shield, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
      ]}
    >
      <Panel title="Leaderboard" description="Teams ranked by validated steps over the current week.">
        <DataTable data={teamRows} columns={teamColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface AnnouncementRow {
  id: string
  title: string
  audience: string
  sentAt: string
  opens: string
  status: 'Sent' | 'Scheduled' | 'Draft'
}

const announcementRows: AnnouncementRow[] = [
  { id: 'an1', title: 'August grand draw is open', audience: 'All users', sentAt: '1 Aug 2026', opens: '62%', status: 'Sent' },
  { id: 'an2', title: 'Maintenance window Sunday', audience: 'All users', sentAt: '18 Aug 2026', opens: '—', status: 'Scheduled' },
  { id: 'an3', title: 'New rewards from GreenMart', audience: 'Marketplace users', sentAt: '28 Jul 2026', opens: '48%', status: 'Sent' },
  { id: 'an4', title: 'Streak rules are changing', audience: 'Streak holders', sentAt: '—', opens: '—', status: 'Draft' },
]

const announcementStatusStyles: Record<AnnouncementRow['status'], string> = {
  Sent: 'bg-green-100 text-green-700',
  Scheduled: 'bg-blue-100 text-blue-700',
  Draft: 'bg-neutral-100 text-neutral-600',
}

const announcementColumns: Column<AnnouncementRow>[] = [
  { key: 'title', label: 'Announcement', render: (row) => <span className="font-medium text-neutral-900">{row.title}</span> },
  { key: 'audience', label: 'Audience', render: (row) => row.audience },
  { key: 'sentAt', label: 'Send date', render: (row) => row.sentAt },
  { key: 'opens', label: 'Open rate', render: (row) => row.opens },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={announcementStatusStyles[row.status]}>{row.status}</Badge>,
  },
]

export function AnnouncementsPreview() {
  return (
    <PreviewScreen
      title="Announcements"
      subtitle="Broadcast messages that land in the app's notification centre."
      feature="Announcements"
      kpis={[
        { title: 'Sent this month', value: 6, icon: Bell },
        { title: 'Scheduled', value: 1, icon: Timer, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Average open rate', value: '55%', icon: TrendingUp, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
        { title: 'Drafts', value: 1, icon: FileText, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
      ]}
    >
      <Panel title="All announcements" description="Sent, scheduled, and draft broadcasts.">
        <DataTable data={announcementRows} columns={announcementColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface TicketRow {
  id: string
  reference: string
  user: string
  subject: string
  age: string
  priority: 'Urgent' | 'Normal' | 'Low'
  status: 'Open' | 'Waiting on user' | 'Resolved'
}

const supportRows: TicketRow[] = [
  { id: 'sp1', reference: 'SUP-1042', user: 'Ayesha Khan', subject: 'Steps not syncing since update', age: '2h', priority: 'Urgent', status: 'Open' },
  { id: 'sp2', reference: 'SUP-1041', user: 'Bilal Ahmed', subject: 'Points missing after a draw entry', age: '6h', priority: 'Normal', status: 'Open' },
  { id: 'sp3', reference: 'SUP-1039', user: 'Sana Malik', subject: 'Cannot verify phone number', age: '1d', priority: 'Normal', status: 'Waiting on user' },
  { id: 'sp4', reference: 'SUP-1035', user: 'Hamza Iqbal', subject: 'Reward never arrived', age: '3d', priority: 'Low', status: 'Resolved' },
]

const priorityStyles: Record<TicketRow['priority'], string> = {
  Urgent: 'bg-red-100 text-red-700',
  Normal: 'bg-blue-100 text-blue-700',
  Low: 'bg-neutral-100 text-neutral-600',
}

const supportStatusStyles: Record<TicketRow['status'], string> = {
  Open: 'bg-amber-100 text-amber-700',
  'Waiting on user': 'bg-purple-100 text-purple-700',
  Resolved: 'bg-green-100 text-green-700',
}

const supportColumns: Column<TicketRow>[] = [
  { key: 'reference', label: 'Ref', render: (row) => <span className="font-mono text-xs text-neutral-500">{row.reference}</span> },
  { key: 'user', label: 'User', render: (row) => <span className="font-medium text-neutral-900">{row.user}</span> },
  { key: 'subject', label: 'Subject', render: (row) => row.subject },
  { key: 'age', label: 'Age', render: (row) => row.age },
  {
    key: 'priority',
    label: 'Priority',
    render: (row) => <Badge className={priorityStyles[row.priority]}>{row.priority}</Badge>,
  },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={supportStatusStyles[row.status]}>{row.status}</Badge>,
  },
]

export function SupportPreview() {
  return (
    <PreviewScreen
      title="Support"
      subtitle="Tickets raised from inside the app."
      feature="Support ticketing"
      kpis={[
        { title: 'Open tickets', value: 2, icon: LifeBuoy, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Waiting on user', value: 1, icon: Clock, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Resolved this week', value: 18, icon: CheckCircle2 },
        { title: 'First response', value: '3h 20m', hint: 'median', icon: MessageSquare, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel title="Queue" description="Oldest and highest priority first.">
        <DataTable data={supportRows} columns={supportColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

const retention = [
  { label: 'Week 1', retention: 100 },
  { label: 'Week 2', retention: 68 },
  { label: 'Week 3', retention: 54 },
  { label: 'Week 4', retention: 47 },
  { label: 'Week 5', retention: 43 },
  { label: 'Week 6', retention: 41 },
]

interface ReportRow {
  id: string
  name: string
  cadence: string
  recipients: string
  lastRun: string
}

const reportRows: ReportRow[] = [
  { id: 'rp1', name: 'Weekly activity summary', cadence: 'Every Monday', recipients: 'Operations', lastRun: '10 Aug 2026' },
  { id: 'rp2', name: 'Points liability', cadence: 'Monthly', recipients: 'Finance', lastRun: '1 Aug 2026' },
  { id: 'rp3', name: 'Sponsor performance', cadence: 'Monthly', recipients: 'Partnerships', lastRun: '1 Aug 2026' },
  { id: 'rp4', name: 'Fraud digest', cadence: 'Daily', recipients: 'Trust & safety', lastRun: 'Today' },
]

const reportColumns: Column<ReportRow>[] = [
  { key: 'name', label: 'Report', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
  { key: 'cadence', label: 'Cadence', render: (row) => row.cadence },
  { key: 'recipients', label: 'Recipients', render: (row) => row.recipients },
  { key: 'lastRun', label: 'Last run', render: (row) => row.lastRun },
]

export function AnalyticsPreview() {
  return (
    <PreviewScreen
      title="Analytics & reports"
      subtitle="Scheduled exports and the cohort views that sit beyond the live dashboard."
      feature="Scheduled reporting and cohort analysis"
      kpis={[
        { title: 'Scheduled reports', value: 4, icon: BarChart3 },
        { title: 'Week 4 retention', value: '47%', icon: TrendingUp, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
        { title: 'Exports this month', value: 26, icon: FileText, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Data freshness', value: '5 min', icon: Timer, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
      ]}
    >
      <Panel title="Retention by cohort week" description="Share of a signup cohort still syncing steps.">
        <ResponsiveContainer width="100%" height={280}>
          <LineChart data={retention}>
            <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
            <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
            <YAxis stroke="#a3a3a3" fontSize={12} unit="%" />
            <Tooltip />
            <Line
              type="monotone"
              dataKey="retention"
              name="Retention"
              stroke="#3A86FF"
              strokeWidth={2}
              dot={{ r: 3 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </Panel>

      <Panel title="Scheduled reports" description="Recurring exports delivered to each team.">
        <DataTable data={reportRows} columns={reportColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface ContentRow {
  id: string
  title: string
  type: string
  locale: string
  updatedAt: string
  status: 'Published' | 'Draft'
}

const contentRows: ContentRow[] = [
  { id: 'cn1', title: 'Terms of service', type: 'Legal', locale: 'en', updatedAt: '12 Jun 2026', status: 'Published' },
  { id: 'cn2', title: 'Privacy policy', type: 'Legal', locale: 'en', updatedAt: '12 Jun 2026', status: 'Published' },
  { id: 'cn3', title: 'Health tips', type: 'Feed', locale: 'en', updatedAt: '4 Aug 2026', status: 'Published' },
  { id: 'cn4', title: 'Onboarding walkthrough', type: 'In-app', locale: 'ur', updatedAt: '2 Aug 2026', status: 'Draft' },
  { id: 'cn5', title: 'Lucky draw rules', type: 'Legal', locale: 'en', updatedAt: '28 Jul 2026', status: 'Published' },
]

const contentColumns: Column<ContentRow>[] = [
  { key: 'title', label: 'Content', render: (row) => <span className="font-medium text-neutral-900">{row.title}</span> },
  { key: 'type', label: 'Type', render: (row) => row.type },
  { key: 'locale', label: 'Locale', render: (row) => row.locale.toUpperCase() },
  { key: 'updatedAt', label: 'Updated', render: (row) => row.updatedAt },
  {
    key: 'status',
    label: 'Status',
    render: (row) => (
      <Badge className={row.status === 'Published' ? 'bg-green-100 text-green-700' : 'bg-neutral-100 text-neutral-600'}>
        {row.status}
      </Badge>
    ),
  },
]

export function ContentPreview() {
  return (
    <PreviewScreen
      title="Content management"
      subtitle="Legal documents, in-app copy, and the health tips feed."
      feature="Content management"
      kpis={[
        { title: 'Published items', value: 26, icon: FileText },
        { title: 'Drafts', value: 4, icon: Clock, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Locales', value: 2, hint: 'English, Urdu', icon: MessageSquare, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Updated this month', value: 9, icon: TrendingUp, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel title="Library" description="Everything the app renders as editable content.">
        <DataTable data={contentRows} columns={contentColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface RoleRow {
  id: string
  role: string
  operators: number
  description: string
}

const roleRows: RoleRow[] = [
  { id: 'ro1', role: 'SuperAdmin', operators: 2, description: 'Full access, including settings and operator management.' },
  { id: 'ro2', role: 'Admin', operators: 5, description: 'Manages users, points adjustments, and platform settings.' },
  { id: 'ro3', role: 'Support', operators: 8, description: 'Reads user detail and answers tickets. No points adjustments.' },
  { id: 'ro4', role: 'ReadOnly', operators: 4, description: 'Sees dashboards and reports. Cannot change anything.' },
]

const roleColumns: Column<RoleRow>[] = [
  { key: 'role', label: 'Role', render: (row) => <span className="font-medium text-neutral-900">{row.role}</span> },
  { key: 'operators', label: 'Operators', render: (row) => formatNumber(row.operators) },
  { key: 'description', label: 'Grants', render: (row) => row.description },
]

export function RolesPreview() {
  return (
    <PreviewScreen
      title="Roles & permissions"
      subtitle="Who can reach which part of the back office."
      feature="Operator and role management"
      kpis={[
        { title: 'Operators', value: 19, icon: Shield },
        { title: 'Roles', value: 4, icon: UsersRound, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Sign-ins today', value: 11, icon: CheckCircle2, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
        { title: 'Access denials', value: 2, hint: 'last 7 days', icon: Timer, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
      ]}
    >
      <Panel
        title="Role matrix"
        description="The API already enforces these four roles on every admin endpoint. This screen will let you assign them."
      >
        <DataTable data={roleRows} columns={roleColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}
