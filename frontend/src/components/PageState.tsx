import { AlertTriangle, Loader2 } from 'lucide-react'
import { ApiError } from '../lib/api'

export function PageLoading({ label = 'Loading' }: { label?: string }) {
  return (
    <div className="flex items-center justify-center gap-3 rounded-xl border border-neutral-200 bg-white py-20 text-sm text-neutral-500">
      <Loader2 className="h-5 w-5 animate-spin" />
      {label}...
    </div>
  )
}

export function PageError({ error, onRetry }: { error: unknown; onRetry?: () => void }) {
  const message =
    error instanceof ApiError
      ? error.message
      : error instanceof Error
        ? error.message
        : 'Something went wrong.'

  const hint =
    error instanceof ApiError && error.isForbidden
      ? 'Your operator role does not allow this.'
      : 'The API may be unavailable. Check that it is running and try again.'

  return (
    <div className="rounded-xl border border-red-200 bg-red-50 p-6">
      <div className="flex items-start gap-3">
        <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0 text-red-600" />
        <div className="flex-1">
          <p className="font-medium text-red-900">{message}</p>
          <p className="mt-1 text-sm text-red-700">{hint}</p>
          {onRetry && (
            <button
              type="button"
              onClick={onRetry}
              className="mt-3 rounded-lg border border-red-300 bg-white px-3 py-1.5 text-sm font-medium text-red-700 transition-colors hover:bg-red-100"
            >
              Try again
            </button>
          )}
        </div>
      </div>
    </div>
  )
}

export function PageHeader({
  title,
  subtitle,
  actions,
}: {
  title: string
  subtitle?: string
  actions?: React.ReactNode
}) {
  return (
    <div className="mb-6 flex flex-wrap items-start justify-between gap-4">
      <div>
        <h1 className="text-2xl font-bold text-neutral-900">{title}</h1>
        {subtitle && <p className="mt-1 text-sm text-neutral-500">{subtitle}</p>}
      </div>
      {actions && <div className="flex items-center gap-2">{actions}</div>}
    </div>
  )
}

export function Panel({
  title,
  description,
  children,
  actions,
}: {
  title: string
  description?: string
  children: React.ReactNode
  actions?: React.ReactNode
}) {
  return (
    <section className="rounded-xl border border-neutral-200 bg-white p-6 shadow-sm">
      <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
        <div>
          <h2 className="font-semibold text-neutral-900">{title}</h2>
          {description && <p className="mt-1 text-sm text-neutral-500">{description}</p>}
        </div>
        {actions}
      </div>
      {children}
    </section>
  )
}
