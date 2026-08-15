const numberFormat = new Intl.NumberFormat('en-US')

const dateFormat = new Intl.DateTimeFormat('en-GB', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
})

const dateTimeFormat = new Intl.DateTimeFormat('en-GB', {
  day: 'numeric',
  month: 'short',
  hour: '2-digit',
  minute: '2-digit',
})

const shortDayFormat = new Intl.DateTimeFormat('en-GB', { day: 'numeric', month: 'short' })

export const formatNumber = (value: number) => numberFormat.format(value)

export const formatSigned = (value: number) =>
  `${value > 0 ? '+' : ''}${numberFormat.format(value)}`

export const formatDate = (value: string) => dateFormat.format(new Date(value))

export const formatDateTime = (value: string) => dateTimeFormat.format(new Date(value))

export const formatShortDay = (value: string) => shortDayFormat.format(new Date(value))

/** "3 minutes ago" style labels for activity feeds. */
export function formatRelative(value: string) {
  const elapsedMs = Date.now() - new Date(value).getTime()
  const minutes = Math.round(elapsedMs / 60_000)

  if (minutes < 1) return 'just now'
  if (minutes < 60) return `${minutes}m ago`

  const hours = Math.round(minutes / 60)
  if (hours < 24) return `${hours}h ago`

  const days = Math.round(hours / 24)
  if (days < 7) return `${days}d ago`

  return formatDate(value)
}

/** Splits a PascalCase enum name into readable words: "AbnormalStepCount" to "Abnormal step count". */
export function humanizeEnum(value: string) {
  const spaced = value.replace(/([a-z0-9])([A-Z])/g, '$1 $2')
  return spaced.charAt(0).toUpperCase() + spaced.slice(1).toLowerCase()
}
