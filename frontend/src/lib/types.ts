/** Mirrors the DTOs in MOVOQ.Application/Features/Admin/AdminContracts.cs. */

export type AdminRole = 'SuperAdmin' | 'Admin' | 'Support' | 'ReadOnly'

export type AdminStatus = 'PendingActivation' | 'Active' | 'Suspended' | 'Deactivated'

export type UserStatus = 'PendingVerification' | 'Active' | 'Suspended' | 'Deleted'

export type PointsTransactionType =
  | 'StepsEarned'
  | 'AdEarned'
  | 'RedemptionDeducted'
  | 'TicketDeducted'
  | 'AdminAdjustment'
  | 'Expired'
  | 'Reversed'

export type FraudSignalSeverity = 'Low' | 'Medium' | 'High' | 'Critical'

export interface AdminSession {
  adminUserId: string
  email: string
  displayName: string | null
  role: AdminRole
  lastLoginAt: string | null
}

export interface AdminOperator {
  id: string
  email: string
  displayName: string | null
  role: AdminRole
  status: AdminStatus
  lastLoginAt: string | null
  createdAt: string
}

export interface AdminOperatorsReport {
  operatorCount: number
  superAdminCount: number
  signedInToday: number
  items: AdminOperator[]
}

export interface SaveOperatorPayload {
  email: string
  displayName: string | null
  role: AdminRole
  status: AdminStatus
}

export interface MonthValue {
  label: string
  year: number
  month: number
  value: number
}

export interface MonthFlow {
  label: string
  year: number
  month: number
  earned: number
  spent: number
}

export interface DaySteps {
  date: string
  steps: number
  activeUsers: number
}

export interface HourBucket {
  hour: number
  syncs: number
  steps: number
}

export interface AdminActivityItem {
  id: string
  user: string
  action: string
  details: string
  occurredAt: string
}

export interface AdminTopUser {
  rank: number
  userId: string
  name: string
  steps: number
  points: number
}

export interface AdminDashboard {
  totalUsers: number
  newUsersThisMonth: number
  activeUsersToday: number
  stepsToday: number
  goalsCompletedToday: number
  pointsIssuedToday: number
  pointsSpentToday: number
  outstandingBalance: number
  openFraudSignals: number
  userGrowth: MonthValue[]
  stepActivity: DaySteps[]
  pointsFlow: MonthFlow[]
  recentActivity: AdminActivityItem[]
  topUsers: AdminTopUser[]
}

export interface AdminActivityReport {
  stepsToday: number
  activeUsersToday: number
  averageStepsPerActiveUser: number
  goalsCompletedToday: number
  daily: DaySteps[]
  syncsByHour: HourBucket[]
}

export interface PointsRules {
  dailyGoalReward: number
  adReward: number
  streakReward: number
  spinMaxReward: number
}

export interface AdminPointsReport {
  issuedToday: number
  spentToday: number
  lifetimeEarned: number
  lifetimeSpent: number
  outstandingBalance: number
  flow: MonthFlow[]
  rules: PointsRules
}

export interface AdminUser {
  id: string
  email: string
  name: string
  phoneNumber: string | null
  city: string | null
  country: string | null
  gender: string | null
  dailyTargetSteps: number
  points: number
  steps: number
  tickets: number
  status: UserStatus
  createdAt: string
  lastLoginAt: string | null
}

export interface AdminUserList {
  items: AdminUser[]
  totalCount: number
  page: number
  pageSize: number
  activeCount: number
  suspendedCount: number
  pendingCount: number
}

export interface AdminUserStepDay {
  date: string
  steps: number
  goal: number
  goalReached: boolean
  pointsAwarded: number
}

export interface AdminLedgerEntry {
  id: string
  amount: number
  transactionType: PointsTransactionType
  description: string | null
  occurredAt: string
}

export interface AdminUserDetail {
  user: AdminUser
  stepHistory: AdminUserStepDay[]
  recentLedger: AdminLedgerEntry[]
}

export interface AdjustPointsResult {
  userId: string
  amount: number
  balance: number
}

export interface AdminFraudSignal {
  id: string
  userId: string
  userName: string
  signalType: string
  severity: FraudSignalSeverity
  description: string
  detectedAt: string
}

export interface AdminAuditEntry {
  id: string
  operator: string
  action: string
  entityName: string
  entityId: string | null
  oldValue: string | null
  newValue: string | null
  ipAddress: string | null
  occurredAt: string
}

export interface PlatformSettings {
  appName: string
  supportEmail: string
  dailyGoalReward: number
  adReward: number
  streakReward: number
  spinMaxReward: number
  maxPointsPerDay: number
  maxTicketsPerDay: number
  maxAdsPerDay: number
  ticketConversionRate: number
  ticketExpiryDays: number
  adFrequency: number
  adsOnHome: boolean
  adsOnMarketplace: boolean
  coachPrice: number
  coachTrialDays: number
  featureNotifications: boolean
  featureAds: boolean
  featureLuckyDraws: boolean
  featureTeams: boolean
  featureAiCoach: boolean
}

export type LuckyDrawStatus = 'Draft' | 'Active' | 'Closed' | 'WinnerSelected' | 'Cancelled'

export type TicketStatus = 'Active' | 'Used' | 'Cancelled' | 'Expired'

export type PrizeFulfillmentStatus = 'AwaitingDetails' | 'Packed' | 'Shipped' | 'Delivered'

export type RewardStatus = 'Draft' | 'Active' | 'Inactive' | 'OutOfStock' | 'Expired' | 'Discontinued'

export interface AdminDraw {
  id: string
  name: string
  description: string | null
  prizeDescription: string
  emoji: string | null
  imageUrl: string | null
  ticketCostPoints: number
  startAt: string
  endAt: string
  status: LuckyDrawStatus
  maxTickets: number | null
  isFeatured: boolean
  tickets: number
  participants: number
  pointsCollected: number
  isWinnerSelected: boolean
  winnerName: string | null
  createdAt: string
}

export interface AdminDrawEntry {
  ticketId: string
  ticketNumber: string
  userId: string
  userName: string
  pointsCost: number
  status: TicketStatus
  purchasedAt: string
}

export interface AdminPrize {
  id: string
  drawId: string
  drawName: string
  prizeDescription: string
  ticketId: string
  ticketNumber: string
  userId: string
  userName: string
  selectedAt: string
  selectionMethod: string | null
  fulfillmentStatus: PrizeFulfillmentStatus
  notes: string | null
}

export interface AdminDrawDetail {
  draw: AdminDraw
  entries: AdminDrawEntry[]
  winner: AdminPrize | null
}

export interface SaveDrawPayload {
  name: string
  description: string | null
  prizeDescription: string
  emoji: string | null
  imageUrl: string | null
  ticketCostPoints: number
  startAt: string
  endAt: string
  maxTickets: number | null
  isFeatured: boolean
}

export interface AdminTicketRow {
  id: string
  ticketNumber: string
  userId: string
  userName: string
  drawId: string
  drawName: string
  pointsCost: number
  status: TicketStatus
  purchasedAt: string
}

export interface AdminTicketsReport {
  ticketsToday: number
  buyersToday: number
  pointsSpentToday: number
  activeTickets: number
  ticketsAllTime: number
  recent: AdminTicketRow[]
}

export interface AdminPrizesReport {
  awaitingDetails: number
  inTransit: number
  delivered: number
  awardedAllTime: number
  items: AdminPrize[]
}

export interface AdminSponsor {
  id: string
  name: string
  contactPersonName: string | null
  contactEmail: string | null
  contactPhone: string | null
  websiteUrl: string | null
  isActive: boolean
  offers: number
  redemptions: number
}

export interface AdminSponsorsReport {
  activeSponsors: number
  liveOffers: number
  redemptionsThisMonth: number
  items: AdminSponsor[]
}

export interface SaveSponsorPayload {
  name: string
  contactPersonName: string | null
  contactEmail: string | null
  contactPhone: string | null
  websiteUrl: string | null
  isActive: boolean
}

export interface AdminReward {
  id: string
  sponsorId: string
  sponsorName: string
  name: string
  description: string | null
  pointsCost: number
  stockQuantity: number | null
  status: RewardStatus
  redeemed: number
  validFrom: string | null
  validUntil: string | null
}

export interface AdminSponsorOption {
  id: string
  name: string
  isActive: boolean
}

export interface AdminRewardsReport {
  liveRewards: number
  redemptionsToday: number
  pointsRedeemedThisMonth: number
  outOfStock: number
  items: AdminReward[]
  sponsors: AdminSponsorOption[]
}

export interface SaveRewardPayload {
  sponsorId: string
  name: string
  description: string | null
  pointsCost: number
  stockQuantity: number | null
  status: RewardStatus
  validFrom: string | null
  validUntil: string | null
  termsAndConditions: string | null
}
