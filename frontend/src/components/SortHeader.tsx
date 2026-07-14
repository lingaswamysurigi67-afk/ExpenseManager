import type { CSSProperties } from 'react'

export type SortDir = 'asc' | 'desc'

interface SortHeaderProps {
  label: string
  sortKey: string
  activeKey: string
  dir: SortDir
  onSort: (key: string) => void
  style?: CSSProperties
}

export default function SortHeader({ label, sortKey, activeKey, dir, onSort, style }: SortHeaderProps) {
  const isActive = activeKey === sortKey
  return (
    <th className="sortable" style={style} onClick={() => onSort(sortKey)}>
      {label}
      <span className="sort-ind">{isActive ? (dir === 'asc' ? ' ▲' : ' ▼') : ' ↕'}</span>
    </th>
  )
}
