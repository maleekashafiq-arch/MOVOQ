import type { LucideIcon } from 'lucide-react'

interface KPICardProps {
  title: string
  value: string | number
  hint?: string
  icon: LucideIcon
  colorClass?: string
}

export function KPICard({
  title,
  value,
  hint,
  icon: Icon,
  colorClass = 'from-[#00C9A7] to-[#3A86FF]',
}: KPICardProps) {
  return (
    <div className="rounded-xl border border-neutral-200 bg-white p-6 shadow-sm transition-shadow hover:shadow-md">
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0 flex-1">
          <p className="text-sm font-medium text-neutral-500">{title}</p>
          <p className="mt-2 truncate text-3xl font-bold text-neutral-900">{value}</p>
          {hint && <p className="mt-2 text-sm text-neutral-500">{hint}</p>}
        </div>
        <div
          className={`flex h-12 w-12 shrink-0 items-center justify-center rounded-lg bg-gradient-to-br ${colorClass}`}
        >
          <Icon className="h-6 w-6 text-white" />
        </div>
      </div>
    </div>
  )
}
