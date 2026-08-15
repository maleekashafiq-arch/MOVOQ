import { useEffect, useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Loader2, Save } from 'lucide-react'
import { api, ApiError } from '../lib/api'
import type { PlatformSettings } from '../lib/types'
import { PageError, PageHeader, PageLoading, Panel } from '../components/PageState'

export function Settings() {
  const queryClient = useQueryClient()
  const [draft, setDraft] = useState<PlatformSettings | null>(null)
  const [feedback, setFeedback] = useState<{ tone: 'ok' | 'error'; message: string } | null>(null)

  const { data, isPending, error, refetch } = useQuery({
    queryKey: ['settings'],
    queryFn: api.settings,
  })

  useEffect(() => {
    if (data) setDraft(data)
  }, [data])

  const save = useMutation({
    mutationFn: (settings: PlatformSettings) => api.saveSettings(settings),
    onSuccess: async (saved) => {
      setDraft(saved)
      setFeedback({ tone: 'ok', message: 'Settings saved.' })
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ['settings'] }),
        queryClient.invalidateQueries({ queryKey: ['points'] }),
      ])
    },
    onError: (cause) =>
      setFeedback({
        tone: 'error',
        message: cause instanceof ApiError ? cause.message : 'Settings could not be saved.',
      }),
  })

  if (isPending || !draft) return <PageLoading label="Loading settings" />
  if (error) return <PageError error={error} onRetry={() => void refetch()} />

  const set = <K extends keyof PlatformSettings>(key: K, value: PlatformSettings[K]) => {
    setFeedback(null)
    setDraft({ ...draft, [key]: value })
  }

  const dirty = data !== undefined && JSON.stringify(draft) !== JSON.stringify(data)

  return (
    <>
      <PageHeader
        title="Settings"
        subtitle="Platform configuration. Changes are saved to the database and recorded in the audit trail."
        actions={
          <button
            type="button"
            disabled={!dirty || save.isPending}
            onClick={() => save.mutate(draft)}
            className="flex items-center gap-2 rounded-lg bg-gradient-to-r from-[#00C9A7] to-[#3A86FF] px-4 py-2 text-sm font-semibold text-white transition-opacity hover:opacity-90 disabled:opacity-40"
          >
            {save.isPending ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Save className="h-4 w-4" />
            )}
            Save changes
          </button>
        }
      />

      {feedback && (
        <p
          className={`mb-6 rounded-lg p-3 text-sm ${
            feedback.tone === 'ok' ? 'bg-green-50 text-green-800' : 'bg-red-50 text-red-700'
          }`}
        >
          {feedback.message}
        </p>
      )}

      <div className="grid gap-6 lg:grid-cols-2">
        <Panel title="General">
          <div className="space-y-4">
            <TextField
              label="App name"
              value={draft.appName}
              onChange={(value) => set('appName', value)}
            />
            <TextField
              label="Support email"
              type="email"
              value={draft.supportEmail}
              onChange={(value) => set('supportEmail', value)}
            />
          </div>
        </Panel>

        <Panel
          title="Reward rules"
          description="The daily goal reward is live: the steps API reads it on every sync. The rest are stored for features still being built."
        >
          <div className="grid gap-4 sm:grid-cols-2">
            <NumberField
              label="Daily goal reward"
              suffix="pts"
              value={draft.dailyGoalReward}
              onChange={(value) => set('dailyGoalReward', value)}
              live
            />
            <NumberField
              label="Rewarded ad"
              suffix="pts"
              value={draft.adReward}
              onChange={(value) => set('adReward', value)}
            />
            <NumberField
              label="Streak day"
              suffix="pts"
              value={draft.streakReward}
              onChange={(value) => set('streakReward', value)}
            />
            <NumberField
              label="Spin wheel maximum"
              suffix="pts"
              value={draft.spinMaxReward}
              onChange={(value) => set('spinMaxReward', value)}
            />
          </div>
        </Panel>

        <Panel title="Daily limits">
          <div className="grid gap-4 sm:grid-cols-3">
            <NumberField
              label="Max points"
              value={draft.maxPointsPerDay}
              onChange={(value) => set('maxPointsPerDay', value)}
            />
            <NumberField
              label="Max tickets"
              value={draft.maxTicketsPerDay}
              onChange={(value) => set('maxTicketsPerDay', value)}
            />
            <NumberField
              label="Max ads"
              value={draft.maxAdsPerDay}
              onChange={(value) => set('maxAdsPerDay', value)}
            />
          </div>
        </Panel>

        <Panel
          title="Lucky draw tickets"
          description="A ticket is bought with points and entered into one specific draw."
        >
          <div className="grid gap-4 sm:grid-cols-2">
            <NumberField
              label="Points per ticket"
              value={draft.ticketConversionRate}
              onChange={(value) => set('ticketConversionRate', value)}
            />
            <NumberField
              label="Ticket expiry"
              suffix="days"
              value={draft.ticketExpiryDays}
              onChange={(value) => set('ticketExpiryDays', value)}
            />
          </div>
        </Panel>

        <Panel title="Ads">
          <div className="space-y-4">
            <NumberField
              label="Show an ad every"
              suffix="actions"
              value={draft.adFrequency}
              onChange={(value) => set('adFrequency', value)}
            />
            <Toggle
              label="Ads on the home screen"
              checked={draft.adsOnHome}
              onChange={(value) => set('adsOnHome', value)}
            />
            <Toggle
              label="Ads in the marketplace"
              checked={draft.adsOnMarketplace}
              onChange={(value) => set('adsOnMarketplace', value)}
            />
          </div>
        </Panel>

        <Panel title="AI Coach">
          <div className="grid gap-4 sm:grid-cols-2">
            <NumberField
              label="Monthly price"
              suffix="Rs"
              value={draft.coachPrice}
              onChange={(value) => set('coachPrice', value)}
            />
            <NumberField
              label="Free trial"
              suffix="days"
              value={draft.coachTrialDays}
              onChange={(value) => set('coachTrialDays', value)}
            />
          </div>
        </Panel>

        <div className="lg:col-span-2">
          <Panel title="Feature toggles" description="Switch whole areas of the app on or off.">
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              <Toggle
                label="Notifications"
                checked={draft.featureNotifications}
                onChange={(value) => set('featureNotifications', value)}
              />
              <Toggle
                label="Ads"
                checked={draft.featureAds}
                onChange={(value) => set('featureAds', value)}
              />
              <Toggle
                label="Lucky draws"
                checked={draft.featureLuckyDraws}
                onChange={(value) => set('featureLuckyDraws', value)}
              />
              <Toggle
                label="Teams"
                checked={draft.featureTeams}
                onChange={(value) => set('featureTeams', value)}
              />
              <Toggle
                label="AI Coach"
                checked={draft.featureAiCoach}
                onChange={(value) => set('featureAiCoach', value)}
              />
            </div>
          </Panel>
        </div>
      </div>
    </>
  )
}

function TextField({
  label,
  value,
  type = 'text',
  onChange,
}: {
  label: string
  value: string
  type?: string
  onChange: (value: string) => void
}) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-sm font-medium text-neutral-700">{label}</span>
      <input
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none transition-colors focus:border-[#3A86FF]"
      />
    </label>
  )
}

function NumberField({
  label,
  value,
  suffix,
  live,
  onChange,
}: {
  label: string
  value: number
  suffix?: string
  live?: boolean
  onChange: (value: number) => void
}) {
  return (
    <label className="block">
      <span className="mb-1.5 flex items-center gap-2 text-sm font-medium text-neutral-700">
        {label}
        {live && (
          <span className="rounded-full bg-green-100 px-1.5 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-green-700">
            Live
          </span>
        )}
      </span>
      <div className="flex items-center gap-2">
        <input
          type="number"
          min={0}
          value={value}
          onChange={(event) => onChange(Number.parseInt(event.target.value, 10) || 0)}
          className="w-full rounded-lg border border-neutral-200 px-3 py-2 text-sm outline-none transition-colors focus:border-[#3A86FF]"
        />
        {suffix && <span className="shrink-0 text-sm text-neutral-500">{suffix}</span>}
      </div>
    </label>
  )
}

function Toggle({
  label,
  checked,
  onChange,
}: {
  label: string
  checked: boolean
  onChange: (value: boolean) => void
}) {
  return (
    <div className="flex items-center justify-between gap-3 rounded-lg border border-neutral-200 px-3 py-2.5">
      <span className="text-sm font-medium text-neutral-700">{label}</span>
      <button
        type="button"
        role="switch"
        aria-checked={checked}
        aria-label={label}
        onClick={() => onChange(!checked)}
        className={`relative h-6 w-11 shrink-0 rounded-full transition-colors ${
          checked ? 'bg-[#00C9A7]' : 'bg-neutral-300'
        }`}
      >
        <span
          className={`absolute top-0.5 h-5 w-5 rounded-full bg-white transition-transform ${
            checked ? 'translate-x-5' : 'translate-x-0.5'
          }`}
        />
      </button>
    </div>
  )
}
