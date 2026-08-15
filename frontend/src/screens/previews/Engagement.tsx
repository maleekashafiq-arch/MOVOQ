import { Coins, Gift, Package, Ticket, Timer, Trophy, TrendingUp, Users } from 'lucide-react'
import { DataTable, type Column } from '../../components/DataTable'
import { Panel } from '../../components/PageState'
import { Badge } from '../../components/StatusBadge'
import { formatNumber } from '../../lib/format'
import { PreviewScreen } from './PreviewScreen'

interface TicketRow {
  id: string
  user: string
  draw: string
  tickets: number
  points: number
  purchasedAt: string
}

const ticketRows: TicketRow[] = [
  { id: 't1', user: 'Ayesha Khan', draw: 'August Grand Draw', tickets: 5, points: 500, purchasedAt: 'Today, 09:14' },
  { id: 't2', user: 'Bilal Ahmed', draw: 'Weekly Cash Draw', tickets: 2, points: 200, purchasedAt: 'Today, 08:41' },
  { id: 't3', user: 'Sana Malik', draw: 'August Grand Draw', tickets: 10, points: 1000, purchasedAt: 'Yesterday, 21:02' },
  { id: 't4', user: 'Hamza Iqbal', draw: 'Fitness Gear Draw', tickets: 1, points: 100, purchasedAt: 'Yesterday, 18:37' },
  { id: 't5', user: 'Zara Sheikh', draw: 'Weekly Cash Draw', tickets: 3, points: 300, purchasedAt: 'Yesterday, 12:20' },
]

const ticketColumns: Column<TicketRow>[] = [
  { key: 'user', label: 'User', render: (row) => <span className="font-medium text-neutral-900">{row.user}</span> },
  { key: 'draw', label: 'Draw', render: (row) => row.draw },
  { key: 'tickets', label: 'Tickets', render: (row) => formatNumber(row.tickets) },
  {
    key: 'points',
    label: 'Points spent',
    render: (row) => <span className="text-[#8338EC]">{formatNumber(row.points)}</span>,
  },
  { key: 'purchasedAt', label: 'Purchased', render: (row) => row.purchasedAt },
]

export function TicketsPreview() {
  return (
    <PreviewScreen
      title="Tickets"
      subtitle="Points converted into entries for a specific lucky draw."
      feature="Ticket purchasing"
      kpis={[
        { title: 'Tickets issued today', value: '1,248', icon: Ticket },
        { title: 'Active tickets', value: '18,430', hint: 'not yet drawn', icon: Timer, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Points converted', value: '124,800', hint: 'today', icon: Coins, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Buyers today', value: '312', icon: Users, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel
        title="Recent purchases"
        description="A ticket belongs to one draw and is priced by the conversion rate on the settings screen."
      >
        <DataTable data={ticketRows} columns={ticketColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface RewardRow {
  id: string
  name: string
  category: string
  cost: number
  stock: number
  redeemed: number
  status: 'Live' | 'Paused' | 'Out of stock'
}

const rewardRows: RewardRow[] = [
  { id: 'r1', name: 'Rs 500 mobile top-up', category: 'Voucher', cost: 5000, stock: 250, redeemed: 1840, status: 'Live' },
  { id: 'r2', name: 'Coffee shop gift card', category: 'Voucher', cost: 3000, stock: 80, redeemed: 620, status: 'Live' },
  { id: 'r3', name: 'MOVOQ water bottle', category: 'Merchandise', cost: 8000, stock: 0, redeemed: 145, status: 'Out of stock' },
  { id: 'r4', name: 'Gym day pass', category: 'Partner', cost: 4000, stock: 120, redeemed: 310, status: 'Paused' },
  { id: 'r5', name: 'Running shoes discount', category: 'Partner', cost: 12000, stock: 40, redeemed: 58, status: 'Live' },
]

const rewardStatusStyles: Record<RewardRow['status'], string> = {
  Live: 'bg-green-100 text-green-700',
  Paused: 'bg-amber-100 text-amber-700',
  'Out of stock': 'bg-red-100 text-red-700',
}

const rewardColumns: Column<RewardRow>[] = [
  { key: 'name', label: 'Reward', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
  { key: 'category', label: 'Category', render: (row) => row.category },
  {
    key: 'cost',
    label: 'Cost',
    render: (row) => <span className="font-medium text-[#00C9A7]">{formatNumber(row.cost)} pts</span>,
  },
  { key: 'stock', label: 'Stock', render: (row) => formatNumber(row.stock) },
  { key: 'redeemed', label: 'Redeemed', render: (row) => formatNumber(row.redeemed) },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={rewardStatusStyles[row.status]}>{row.status}</Badge>,
  },
]

export function RewardsPreview() {
  return (
    <PreviewScreen
      title="Rewards"
      subtitle="The catalogue users spend points on."
      feature="The reward catalogue"
      kpis={[
        { title: 'Live rewards', value: 18, icon: Gift },
        { title: 'Redemptions today', value: 96, icon: TrendingUp, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Points redeemed', value: '412,000', hint: 'this month', icon: Coins, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Out of stock', value: 3, icon: Package, colorClass: 'from-[#E74C3C] to-[#8338EC]' },
      ]}
    >
      <Panel title="Catalogue" description="Cost, stock, and lifetime redemptions per reward.">
        <DataTable data={rewardRows} columns={rewardColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface DrawRow {
  id: string
  name: string
  prize: string
  entries: number
  drawDate: string
  status: 'Open' | 'Drawing' | 'Completed'
}

const drawRows: DrawRow[] = [
  { id: 'd1', name: 'August Grand Draw', prize: 'Rs 100,000 cash', entries: 12480, drawDate: '31 Aug 2026', status: 'Open' },
  { id: 'd2', name: 'Weekly Cash Draw', prize: 'Rs 10,000 cash', entries: 3120, drawDate: '17 Aug 2026', status: 'Open' },
  { id: 'd3', name: 'Fitness Gear Draw', prize: 'Smart watch', entries: 2840, drawDate: '14 Aug 2026', status: 'Drawing' },
  { id: 'd4', name: 'July Grand Draw', prize: 'Rs 100,000 cash', entries: 11090, drawDate: '31 Jul 2026', status: 'Completed' },
]

const drawStatusStyles: Record<DrawRow['status'], string> = {
  Open: 'bg-green-100 text-green-700',
  Drawing: 'bg-blue-100 text-blue-700',
  Completed: 'bg-neutral-100 text-neutral-600',
}

const drawColumns: Column<DrawRow>[] = [
  { key: 'name', label: 'Draw', render: (row) => <span className="font-medium text-neutral-900">{row.name}</span> },
  { key: 'prize', label: 'Prize', render: (row) => row.prize },
  { key: 'entries', label: 'Entries', render: (row) => formatNumber(row.entries) },
  { key: 'drawDate', label: 'Draw date', render: (row) => row.drawDate },
  {
    key: 'status',
    label: 'Status',
    render: (row) => <Badge className={drawStatusStyles[row.status]}>{row.status}</Badge>,
  },
]

export function DrawsPreview() {
  return (
    <PreviewScreen
      title="Lucky draws"
      subtitle="Scheduled draws, their prizes, and how many entries they hold."
      feature="Lucky draw management"
      kpis={[
        { title: 'Open draws', value: 2, icon: Trophy },
        { title: 'Total entries', value: '18,440', icon: Ticket, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Next draw', value: '17 Aug', hint: 'Weekly Cash Draw', icon: Timer, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'Prizes awarded', value: '14', hint: 'all time', icon: Gift, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel title="Draw schedule" description="Winners are picked from the tickets entered into each draw.">
        <DataTable data={drawRows} columns={drawColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}

interface FulfillmentRow {
  id: string
  winner: string
  prize: string
  draw: string
  wonAt: string
  status: 'Awaiting details' | 'Packed' | 'Shipped' | 'Delivered'
}

const fulfillmentRows: FulfillmentRow[] = [
  { id: 'f1', winner: 'Sana Malik', prize: 'Smart watch', draw: 'Fitness Gear Draw', wonAt: '14 Aug 2026', status: 'Awaiting details' },
  { id: 'f2', winner: 'Bilal Ahmed', prize: 'Rs 10,000 cash', draw: 'Weekly Cash Draw', wonAt: '10 Aug 2026', status: 'Packed' },
  { id: 'f3', winner: 'Ayesha Khan', prize: 'Rs 100,000 cash', draw: 'July Grand Draw', wonAt: '31 Jul 2026', status: 'Shipped' },
  { id: 'f4', winner: 'Hamza Iqbal', prize: 'Gym annual pass', draw: 'July Grand Draw', wonAt: '31 Jul 2026', status: 'Delivered' },
]

const fulfillmentStatusStyles: Record<FulfillmentRow['status'], string> = {
  'Awaiting details': 'bg-amber-100 text-amber-700',
  Packed: 'bg-blue-100 text-blue-700',
  Shipped: 'bg-purple-100 text-purple-700',
  Delivered: 'bg-green-100 text-green-700',
}

const fulfillmentColumns: Column<FulfillmentRow>[] = [
  { key: 'winner', label: 'Winner', render: (row) => <span className="font-medium text-neutral-900">{row.winner}</span> },
  { key: 'prize', label: 'Prize', render: (row) => row.prize },
  { key: 'draw', label: 'Draw', render: (row) => row.draw },
  { key: 'wonAt', label: 'Won', render: (row) => row.wonAt },
  {
    key: 'status',
    label: 'Fulfillment',
    render: (row) => <Badge className={fulfillmentStatusStyles[row.status]}>{row.status}</Badge>,
  },
]

export function PrizesPreview() {
  return (
    <PreviewScreen
      title="Prizes & fulfillment"
      subtitle="Getting the prize from the draw result into the winner's hands."
      feature="Prize fulfillment"
      kpis={[
        { title: 'Awaiting details', value: 1, icon: Timer, colorClass: 'from-[#f59e0b] to-[#E74C3C]' },
        { title: 'In transit', value: 2, icon: Package, colorClass: 'from-[#3A86FF] to-[#8338EC]' },
        { title: 'Delivered', value: 11, icon: Gift },
        { title: 'Prize value', value: 'Rs 340,000', hint: 'awarded this year', icon: Coins, colorClass: 'from-[#00C9A7] to-[#3A86FF]' },
      ]}
    >
      <Panel title="Open fulfillments" description="Each winner is tracked from the draw result to delivery.">
        <DataTable data={fulfillmentRows} columns={fulfillmentColumns} rowKey={(row) => row.id} />
      </Panel>
    </PreviewScreen>
  )
}
