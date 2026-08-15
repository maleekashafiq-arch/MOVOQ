import type {
  AdminRole,
  AdminStatus,
  FraudSignalSeverity,
  LuckyDrawStatus,
  PrizeFulfillmentStatus,
  RewardStatus,
  UserStatus,
} from '../lib/types'

const userStatusStyles: Record<UserStatus, string> = {
  Active: 'bg-green-100 text-green-700',
  Suspended: 'bg-red-100 text-red-700',
  PendingVerification: 'bg-amber-100 text-amber-700',
  Deleted: 'bg-neutral-200 text-neutral-600',
}

const userStatusLabels: Record<UserStatus, string> = {
  Active: 'Active',
  Suspended: 'Suspended',
  PendingVerification: 'Pending',
  Deleted: 'Deleted',
}

export function UserStatusBadge({ status }: { status: UserStatus }) {
  return <Badge className={userStatusStyles[status]}>{userStatusLabels[status]}</Badge>
}

const severityStyles: Record<FraudSignalSeverity, string> = {
  Critical: 'bg-red-100 text-red-700',
  High: 'bg-orange-100 text-orange-700',
  Medium: 'bg-amber-100 text-amber-700',
  Low: 'bg-neutral-100 text-neutral-600',
}

export function SeverityBadge({ severity }: { severity: FraudSignalSeverity }) {
  return <Badge className={severityStyles[severity]}>{severity}</Badge>
}

export function Badge({ className, children }: { className: string; children: React.ReactNode }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium ${className}`}
    >
      {children}
    </span>
  )
}

const drawStatusStyles: Record<LuckyDrawStatus, string> = {
  Draft: 'bg-neutral-100 text-neutral-600',
  Active: 'bg-green-100 text-green-700',
  Closed: 'bg-blue-100 text-blue-700',
  WinnerSelected: 'bg-purple-100 text-purple-700',
  Cancelled: 'bg-red-100 text-red-700',
}

export function DrawStatusBadge({ status }: { status: LuckyDrawStatus }) {
  return <Badge className={drawStatusStyles[status]}>{status === 'WinnerSelected' ? 'Winner selected' : status}</Badge>
}

const fulfillmentStyles: Record<PrizeFulfillmentStatus, string> = {
  AwaitingDetails: 'bg-amber-100 text-amber-700',
  Packed: 'bg-blue-100 text-blue-700',
  Shipped: 'bg-purple-100 text-purple-700',
  Delivered: 'bg-green-100 text-green-700',
}

const fulfillmentLabels: Record<PrizeFulfillmentStatus, string> = {
  AwaitingDetails: 'Awaiting details',
  Packed: 'Packed',
  Shipped: 'Shipped',
  Delivered: 'Delivered',
}

export function FulfillmentBadge({ status }: { status: PrizeFulfillmentStatus }) {
  return <Badge className={fulfillmentStyles[status]}>{fulfillmentLabels[status]}</Badge>
}

const rewardStatusStyles: Record<RewardStatus, string> = {
  Draft: 'bg-neutral-100 text-neutral-600',
  Active: 'bg-green-100 text-green-700',
  Inactive: 'bg-amber-100 text-amber-700',
  OutOfStock: 'bg-red-100 text-red-700',
  Expired: 'bg-neutral-100 text-neutral-600',
  Discontinued: 'bg-neutral-200 text-neutral-600',
}

const rewardStatusLabels: Record<RewardStatus, string> = {
  Draft: 'Draft',
  Active: 'Live',
  Inactive: 'Paused',
  OutOfStock: 'Out of stock',
  Expired: 'Expired',
  Discontinued: 'Discontinued',
}

export function RewardStatusBadge({ status }: { status: RewardStatus }) {
  return <Badge className={rewardStatusStyles[status]}>{rewardStatusLabels[status]}</Badge>
}

const operatorStatusStyles: Record<AdminStatus, string> = {
  Active: 'bg-green-100 text-green-700',
  PendingActivation: 'bg-amber-100 text-amber-700',
  Suspended: 'bg-red-100 text-red-700',
  Deactivated: 'bg-neutral-200 text-neutral-600',
}

const operatorStatusLabels: Record<AdminStatus, string> = {
  Active: 'Active',
  PendingActivation: 'Pending',
  Suspended: 'Suspended',
  Deactivated: 'Deactivated',
}

export function OperatorStatusBadge({ status }: { status: AdminStatus }) {
  return <Badge className={operatorStatusStyles[status]}>{operatorStatusLabels[status]}</Badge>
}

const roleStyles: Record<AdminRole, string> = {
  SuperAdmin: 'bg-purple-100 text-purple-700',
  Admin: 'bg-blue-100 text-blue-700',
  Support: 'bg-teal-100 text-teal-700',
  ReadOnly: 'bg-neutral-100 text-neutral-600',
}

export function RoleBadge({ role }: { role: AdminRole }) {
  return <Badge className={roleStyles[role]}>{role === 'ReadOnly' ? 'Read only' : role}</Badge>
}
