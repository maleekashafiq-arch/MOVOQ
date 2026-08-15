import { FlaskConical } from 'lucide-react'

/**
 * Marks a screen whose feature has no backend yet. Everything below the banner is design-time sample
 * data, so an operator never mistakes a mock number for a real one.
 */
export function PreviewNotice({ feature }: { feature: string }) {
  return (
    <div className="mb-6 flex items-start gap-3 rounded-xl border border-amber-200 bg-amber-50 p-4">
      <FlaskConical className="mt-0.5 h-5 w-5 shrink-0 text-amber-600" />
      <div>
        <p className="font-medium text-amber-900">Design preview — not live data</p>
        <p className="mt-1 text-sm text-amber-800">
          {feature} has no backend yet. The figures below are sample data from the design so the layout
          can be reviewed. Nothing here reads from or writes to the database.
        </p>
      </div>
    </div>
  )
}
