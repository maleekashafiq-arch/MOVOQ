import { useState } from 'react'
import { Link, Outlet, useLocation } from 'react-router'
import {
  Activity,
  BarChart3,
  Bell,
  Bot,
  DollarSign,
  FileText,
  Gift,
  LayoutDashboard,
  LifeBuoy,
  ListChecks,
  LogOut,
  Megaphone,
  Menu,
  Package,
  Settings,
  Shield,
  ShieldAlert,
  Store,
  Ticket,
  Trophy,
  Users,
  UsersRound,
  Wallet,
  X,
  type LucideIcon,
} from 'lucide-react'
import { useAuth } from '../lib/auth'

interface MenuItem {
  icon: LucideIcon
  label: string
  path: string
  /** False while the screen still renders design sample data. */
  live?: boolean
}

const menuItems: MenuItem[] = [
  { icon: LayoutDashboard, label: 'Dashboard', path: '/admin', live: true },
  { icon: Users, label: 'Users', path: '/admin/users', live: true },
  { icon: Activity, label: 'Activity', path: '/admin/activity', live: true },
  { icon: Wallet, label: 'Points Wallet', path: '/admin/points', live: true },
  { icon: ShieldAlert, label: 'Fraud Monitoring', path: '/admin/fraud', live: true },
  { icon: ListChecks, label: 'Audit Trail', path: '/admin/audit', live: true },
  { icon: Settings, label: 'Settings', path: '/admin/settings', live: true },
  { icon: Ticket, label: 'Tickets', path: '/admin/tickets', live: true },
  { icon: Gift, label: 'Rewards', path: '/admin/rewards', live: true },
  { icon: Trophy, label: 'Lucky Draws', path: '/admin/draws', live: true },
  { icon: Package, label: 'Prizes & Fulfillment', path: '/admin/prizes', live: true },
  { icon: Store, label: 'Sponsors & Marketplace', path: '/admin/sponsors', live: true },
  { icon: UsersRound, label: 'Teams & Coaches', path: '/admin/teams' },
  { icon: Bot, label: 'AI Coach', path: '/admin/ai-coach' },
  { icon: DollarSign, label: 'Ads & Revenue', path: '/admin/ads' },
  { icon: Megaphone, label: 'Campaigns', path: '/admin/campaigns' },
  { icon: Bell, label: 'Announcements', path: '/admin/announcements' },
  { icon: LifeBuoy, label: 'Support', path: '/admin/support' },
  { icon: BarChart3, label: 'Analytics & Reports', path: '/admin/analytics' },
  { icon: FileText, label: 'Content Management', path: '/admin/content' },
  { icon: Shield, label: 'Roles & Permissions', path: '/admin/roles', live: true },
]

export function AdminLayout() {
  const location = useLocation()
  const { state, signOut } = useAuth()
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const session = state.status === 'ready' ? state.session : null
  const devMode = state.status === 'ready' && state.devMode

  return (
    <div className="min-h-screen bg-neutral-50">
      <aside
        className={`fixed left-0 top-0 z-40 h-screen w-64 border-r border-neutral-200 bg-white transition-transform ${
          sidebarOpen ? 'translate-x-0' : '-translate-x-full'
        } lg:translate-x-0`}
      >
        <div className="flex h-full flex-col">
          <div className="flex h-16 items-center justify-between border-b border-neutral-200 px-6">
            <Link to="/admin" className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-gradient-to-br from-[#00C9A7] to-[#3A86FF]">
                <Trophy className="h-5 w-5 text-white" />
              </div>
              <span className="text-lg font-semibold text-neutral-900">MOVOQ Admin</span>
            </Link>
            <button type="button" onClick={() => setSidebarOpen(false)} className="lg:hidden">
              <X className="h-5 w-5 text-neutral-500" />
            </button>
          </div>

          <nav className="flex-1 overflow-y-auto p-4">
            <ul className="space-y-1">
              {menuItems.map((item) => {
                const Icon = item.icon
                const isActive = location.pathname === item.path

                return (
                  <li key={item.path}>
                    <Link
                      to={item.path}
                      onClick={() => setSidebarOpen(false)}
                      className={`flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors ${
                        isActive
                          ? 'bg-gradient-to-r from-[#00C9A7]/10 to-[#3A86FF]/10 text-[#3A86FF]'
                          : 'text-neutral-600 hover:bg-neutral-100 hover:text-neutral-900'
                      }`}
                    >
                      <Icon className="h-5 w-5 shrink-0" />
                      <span className="flex-1 truncate">{item.label}</span>
                      {!item.live && (
                        <span
                          title="Design preview — no backend yet"
                          className="rounded-full bg-neutral-100 px-1.5 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-neutral-500"
                        >
                          Preview
                        </span>
                      )}
                    </Link>
                  </li>
                )
              })}
            </ul>
          </nav>
        </div>
      </aside>

      {sidebarOpen && (
        <button
          type="button"
          aria-label="Close navigation"
          className="fixed inset-0 z-30 bg-black/50 lg:hidden"
          onClick={() => setSidebarOpen(false)}
        />
      )}

      <div className="lg:pl-64">
        <header className="sticky top-0 z-20 border-b border-neutral-200 bg-white">
          <div className="flex h-16 items-center justify-between gap-4 px-6">
            <button type="button" onClick={() => setSidebarOpen(true)} className="lg:hidden">
              <Menu className="h-6 w-6 text-neutral-600" />
            </button>

            <div className="flex-1">
              {devMode && (
                <span className="rounded-full bg-amber-100 px-3 py-1 text-xs font-medium text-amber-800">
                  Local development session
                </span>
              )}
            </div>

            <div className="flex items-center gap-3">
              <div className="hidden items-center gap-3 rounded-lg border border-neutral-200 px-3 py-1.5 sm:flex">
                <div className="h-8 w-8 rounded-full bg-gradient-to-br from-[#00C9A7] to-[#3A86FF]" />
                <div>
                  <p className="text-sm font-medium text-neutral-900">
                    {session?.displayName ?? session?.email ?? 'Operator'}
                  </p>
                  <p className="text-xs text-neutral-500">{session?.role}</p>
                </div>
              </div>
              <button
                type="button"
                onClick={() => void signOut()}
                title="Sign out"
                className="rounded-lg p-2 text-neutral-600 transition-colors hover:bg-neutral-100"
              >
                <LogOut className="h-5 w-5" />
              </button>
            </div>
          </div>
        </header>

        <main className="p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
