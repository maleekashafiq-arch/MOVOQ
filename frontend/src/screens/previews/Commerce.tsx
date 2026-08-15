import {
  Bot,
  DollarSign,
  Eye,
  Megaphone,
  MousePointerClick,
  Percent,
  Sparkles,
  Store,
  TrendingUp,
  UserCheck,
} from 'lucide-react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import { DataTable, type Column } from '../../components/DataTable'
import { Panel } from '../../components/PageState'
import { Badge } from '../../components/StatusBadge'
import { formatNumber } from '../../lib/format'
import { PreviewScreen } from './PreviewScreen'

const liveStyle = 'bg-green-100 text-green-700'
const pausedStyle = 'bg-amber-100 text-amber-700'
const endedStyle = 'bg-neutral-100 text-neutral-600'

interface SponsorRow {
  id: string
  name: string
  tier: 'Platinum' | 'Gold' | 'Silver'
  offers: number
  redemptions: number
  spend: string
  status: 'Live' | 'Paused'
}

const sponsorRows: SponsorRow[] = [
  { id: 's1', name: 'FitLife Gyms', tier: 'Platinum', offers: 6, redemptions: 1840, spend: 'Rs 450,000', status: 'Live' },
  { id: 's2', name: 'GreenMart', tier: 'Gold', offers: 4, redemptions: 920, spend: 'Rs 220,000', status: 'Live' },
  { id: 's3', name: 'Cafe Nine', tier: 'Silver', offers: 2, redemptions: 310, spend: 'Rs 80,000', status: 'Paused' },
  { id: 's4', name: 'StrideWear', tier: 'Gold', offers: 3, redemptions: 640, spend: 'Rs 180,000', status: 'Live' },
]

const sponsorColumns: Column<SponsorRow>[] = [
  { key: 'name', label: 'Sponsor', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
  { key: 'tier', label: 'Tier', render: (row) => row.tier },
  { key: 'offers', label: 'Offers', render: (row) => formatNumber(row.offers) },
  { key: 'redemptions', label: 'Redemptions', render: (row) => formatNumber(row.redemptions) },
  { key: 'spend', label: 'Contract value', render: (row) => row.spend },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={row.status === 'Live' ? liveStyle : pausedStyle}>{row.status}</Badge>,
  },
]

export function SponsorsPreview() {
  return (
    <PreviewScreen
      title="Sponsors & marketplace"
      subtitle="Brands funding the reward pool and the offers they place in the app."
      feature="Sponsor and marketplace management"
      kpis={[
        { title: 'Active sponsors', value: 12, icon: Store },
        { title: 'Live offers', value: 34, icon: Sparkles, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Offer redemptions', value: '3,710', hint: 'this month', icon: TrendingUp, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
        { title: 'Sponsor revenue', value: 'Rs 930,000', hint: 'contracted this quarter', icon: DollarSign, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
      ]}
    >
      <Panel title="Sponsors" description="Contract tier, offer count, and how often their offers convert.">
        <DataTable data={sponsorRows} columns={sponsorColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

const adRevenue = [
  { label: 'Mar', revenue: 82000, impressions: 410000 },
  { label: 'Apr', revenue: 96000, impressions: 468000 },
  { label: 'May', revenue: 118000, impressions: 540000 },
  { label: 'Jun', revenue: 131000, impressions: 602000 },
  { label: 'Jul', revenue: 149000, impressions: 668000 },
  { label: 'Aug', revenue: 92000, impressions: 388000 },
]

interface AdUnitRow {
  id: string
  placement: string
  format: string
  impressions: number
  completionRate: string
  revenue: string
}

const adUnitRows: AdUnitRow[] = [
  { id: 'a1', placement: 'Home banner', format: 'Banner', impressions: 218000, completionRate: '—', revenue: 'Rs 34,000' },
  { id: 'a2', placement: 'Points boost', format: 'Rewarded video', impressions: 96000, completionRate: '87%', revenue: 'Rs 41,000' },
  { id: 'a3', placement: 'Marketplace tile', format: 'Native', impressions: 54000, completionRate: '—', revenue: 'Rs 12,000' },
  { id: 'a4', placement: 'Draw entry', format: 'Interstitial', impressions: 20000, completionRate: '64%', revenue: 'Rs 5,000' },
]

const adUnitColumns: Column<AdUnitRow>[] = [
  { key: 'placement', label: 'Placement', render: (row) => <span className="font-medium text-neutral-900">{row.placement}</span> },
  { key: 'format', label: 'Format', render: (row) => row.format },
  { key: 'impressions', label: 'Impressions', render: (row) => formatNumber(row.impressions) },
  { key: 'completionRate', label: 'Completion', render: (row) => row.completionRate },
  { key: 'revenue', label: 'Revenue', render: (row) => row.revenue },
]

export function AdsPreview() {
  return (
    <PreviewScreen
      title="Ads & revenue"
      subtitle="Ad inventory across the app and what it earns."
      feature="Ad serving and revenue reporting"
      kpis={[
        { title: 'Impressions', value: '388,000', hint: 'this month', icon: Eye },
        { title: 'Rewarded views', value: '96,000', icon: MousePointerClick, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Revenue', value: 'Rs 92,000', hint: 'this month', icon: DollarSign, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Fill rate', value: '94%', icon: Percent, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel title="Revenue by month" description="Gross ad revenue against served impressions.">
        <ResponsiveContainer width="100%" height={280}>
          <BarChart data={adRevenue}>
            <CartesianGrid strokeDasharray="3 3" stroke="#e5e5e5" />
            <XAxis dataKey="label" stroke="#a3a3a3" fontSize={12} />
            <YAxis stroke="#a3a3a3" fontSize={12} />
            <Tooltip />
            <Bar dataKey="revenue" name="Revenue (Rs)" fill="#00C9A7" radius={[8, 8, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </Panel>

      <Panel title="Placements" description="Where ads appear and how each unit performs.">
        <DataTable data={adUnitRows} columns={adUnitColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface CampaignRow {
  id: string
  name: string
  audience: string
  channel: string
  reach: number
  engagement: string
  status: 'Live' | 'Scheduled' | 'Ended'
}

const campaignRows: CampaignRow[] = [
  { id: 'c1', name: 'Ramadan step challenge', audience: 'All users', channel: 'Push + in-app', reach: 42000, engagement: '38%', status: 'Live' },
  { id: 'c2', name: 'Win back lapsed walkers', audience: 'Inactive 14 days', channel: 'Push', reach: 8600, engagement: '12%', status: 'Live' },
  { id: 'c3', name: 'Double points weekend', audience: 'Active users', channel: 'In-app', reach: 31000, engagement: '—', status: 'Scheduled' },
  { id: 'c4', name: 'Refer a friend', audience: 'Streak holders', channel: 'Email', reach: 12400, engagement: '21%', status: 'Ended' },
]

const campaignStatusStyles: Record<CampaignRow['status'], string> = {
  Live: liveStyle,
  Scheduled: 'bg-blue-100 text-blue-700',
  Ended: endedStyle,
}

const campaignColumns: Column<CampaignRow>[] = [
  { key: 'name', label: 'Campaign', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
  { key: 'audience', label: 'Audience', render: (row) => row.audience },
  { key: 'channel', label: 'Channel', render: (row) => row.channel },
  { key: 'reach', label: 'Reach', render: (row) => formatNumber(row.reach) },
  { key: 'engagement', label: 'Engagement', render: (row) => row.engagement },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={campaignStatusStyles[row.status]}>{row.status}</Badge>,
  },
]

export function CampaignsPreview() {
  return (
    <PreviewScreen
      title="Campaigns"
      subtitle="Targeted pushes that nudge users back into the app."
      feature="Campaign management"
      kpis={[
        { title: 'Live campaigns', value: 2, icon: Megaphone },
        { title: 'Reach', value: '50,600', hint: 'users targeted', icon: UserCheck, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Average engagement', value: '24%', icon: TrendingUp, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
        { title: 'Scheduled', value: 1, icon: Sparkles, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
      ]}
    >
      <Panel title="All campaigns" description="Audience, delivery channel, and measured engagement.">
        <DataTable data={campaignRows} columns={campaignColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface CoachPlanRow {
  id: string
  plan: string
  price: string
  subscribers: number
  churn: string
  status: 'Live' | 'Paused'
}

const coachPlanRows: CoachPlanRow[] = [
  { id: 'p1', plan: 'Monthly', price: 'Rs 499', subscribers: 1840, churn: '6.2%', status: 'Live' },
  { id: 'p2', plan: 'Quarterly', price: 'Rs 1,299', subscribers: 620, churn: '3.8%', status: 'Live' },
  { id: 'p3', plan: 'Annual', price: 'Rs 4,499', subscribers: 210, churn: '1.4%', status: 'Live' },
  { id: 'p4', plan: 'Team coaching', price: 'Rs 9,999', subscribers: 24, churn: '—', status: 'Paused' },
]

const coachPlanColumns: Column<CoachPlanRow>[] = [
  { key: 'plan', label: 'Plan', render: (row) => <span className="font-medium text-neutral-900">{row.plan}</span> },
  { key: 'price', label: 'Price', render: (row) => row.price },
  { key: 'subscribers', label: 'Subscribers', render: (row) => formatNumber(row.subscribers) },
  { key: 'churn', label: 'Monthly churn', render: (row) => row.churn },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={row.status === 'Live' ? liveStyle : pausedStyle}>{row.status}</Badge>,
  },
]

export function AiCoachPreview() {
  return (
    <PreviewScreen
      title="AI Coach"
      subtitle="The paid coaching tier: plans, subscribers, and how the assistant is used."
      feature="AI Coach subscriptions"
      kpis={[
        { title: 'Subscribers', value: '2,694', icon: Bot },
        { title: 'Trials running', value: 318, icon: Sparkles, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Monthly revenue', value: 'Rs 1.4M', icon: DollarSign, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Trial conversion', value: '41%', icon: Percent, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel title="Plans" description="Pricing and trial length are set on the settings screen.">
        <DataTable data={coachPlanRows} columns={coachPlanColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}
