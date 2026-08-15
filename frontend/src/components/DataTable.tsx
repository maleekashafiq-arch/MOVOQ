import { useState, type ReactNode } from 'react'
import { ChevronDown, ChevronUp } from 'lucide-react'

export interface Column<T> {
  key: string
  label: string
  render?: (item: T) => ReactNode
  sortable?: boolean
  /** Reads the value used for sorting when the rendered cell is not the raw value. */
  sortValue?: (item: T) => string | number
}

interface DataTableProps<T> {
  data: T[]
  columns: Column<T>[]
  rowKey: (item: T) => string
  onRowClick?: (item: T) => void
  emptyMessage?: string
}

export function DataTable<T>({
  data,
  columns,
  rowKey,
  onRowClick,
  emptyMessage = 'Nothing to show yet.',
}: DataTableProps<T>) {
  const [sortColumn, setSortColumn] = useState<string | null>(null)
  const [ascending, setAscending] = useState(true)

  const handleSort = (column: Column<T>) => {
    if (!column.sortable) return

    if (sortColumn === column.key) {
      setAscending(!ascending)
      return
    }

    setSortColumn(column.key)
    setAscending(true)
  }

  const active = columns.find((column) => column.key === sortColumn)
  const rows =
    active?.sortValue === undefined
      ? data
      : [...data].sort((a, b) => {
          const left = active.sortValue!(a)
          const right = active.sortValue!(b)
          if (left === right) return 0
          return (left < right ? -1 : 1) * (ascending ? 1 : -1)
        })

  return (
    <div className="overflow-hidden rounded-xl border border-neutral-200 bg-white">
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead className="border-b border-neutral-200 bg-neutral-50">
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  className="px-6 py-3 text-left text-sm font-semibold text-neutral-700"
                >
                  <button
                    type="button"
                    onClick={() => handleSort(column)}
                    disabled={!column.sortable}
                    className={`flex items-center gap-2 ${
                      column.sortable ? 'cursor-pointer hover:text-neutral-900' : 'cursor-default'
                    }`}
                  >
                    {column.label}
                    {column.sortable &&
                      sortColumn === column.key &&
                      (ascending ? (
                        <ChevronUp className="h-4 w-4" />
                      ) : (
                        <ChevronDown className="h-4 w-4" />
                      ))}
                  </button>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-neutral-200">
            {rows.length === 0 ? (
              <tr>
                <td
                  colSpan={columns.length}
                  className="px-6 py-12 text-center text-sm text-neutral-500"
                >
                  {emptyMessage}
                </td>
              </tr>
            ) : (
              rows.map((item) => (
                <tr
                  key={rowKey(item)}
                  onClick={() => onRowClick?.(item)}
                  className={`transition-colors ${
                    onRowClick ? 'cursor-pointer hover:bg-neutral-50' : ''
                  }`}
                >
                  {columns.map((column) => (
                    <td key={column.key} className="px-6 py-4 text-sm text-neutral-600">
                      {column.render?.(item) ?? null}
                    </td>
                  ))}
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
