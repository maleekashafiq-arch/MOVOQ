import type { LucideIcon } from 'lucide-react'
import { KPICard } from '../../components/KPICard'
import { PageHeader } from '../../components/PageState'
import { PreviewNotice } from '../../components/PreviewNotice'

export interface PreviewKpi {
  title: string
  value: string | number
  hint?: string
  icon: LucideIcon
  colorClass?: string
}

interface PreviewScreenProps {
  title: string
  subtitle: string
  /** Named in the banner, e.g. "Lucky draws". */
  feature: string
  kpis?: PreviewKpi[]
  children: React.ReactNode
}

/**
 * Shared frame for the screens whose feature has no API yet. Layout and copy are final; every number
 * below the banner is design sample data.
 */
export function PreviewScreen({ title, subtitle, feature, kpis, children }: PreviewScreenProps) {
  return (
    <>
      <PageHeader title={title} subtitle={subtitle} />
      <PreviewNotice feature={feature} />

      {kpis && kpis.length > 0 && (
        <div className="mb-6 grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
          {kpis.map((kpi) => (
            <KPICard key={kpi.title} {...kpi} />
          ))}
        </div>
      )}

      <div className="space-y-6">{children}</div>
    </>
  )
}
