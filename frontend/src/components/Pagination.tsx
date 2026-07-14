interface PaginationProps {
  page: number
  pageSize: number
  total: number
  onPage: (page: number) => void
  onPageSize: (size: number) => void
  pageSizeOptions?: number[]
}

export default function Pagination({
  page,
  pageSize,
  total,
  onPage,
  onPageSize,
  pageSizeOptions = [10, 25, 50, 100],
}: PaginationProps) {
  const pageCount = Math.max(1, Math.ceil(total / pageSize))
  const from = total === 0 ? 0 : (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, total)

  return (
    <div className="pager">
      <span className="pager-size">
        Items per page:
        <select value={pageSize} onChange={(e) => onPageSize(Number(e.target.value))}>
          {pageSizeOptions.map((n) => (
            <option key={n} value={n}>{n}</option>
          ))}
        </select>
      </span>

      <span className="pager-info">{from}–{to} of {total}</span>

      <div className="pager-controls">
        <button className="pager-btn" disabled={page <= 1} onClick={() => onPage(1)} title="First page" aria-label="First page">⏮</button>
        <button className="pager-btn" disabled={page <= 1} onClick={() => onPage(page - 1)} title="Previous page" aria-label="Previous page">‹</button>
        <button className="pager-btn" disabled={page >= pageCount} onClick={() => onPage(page + 1)} title="Next page" aria-label="Next page">›</button>
        <button className="pager-btn" disabled={page >= pageCount} onClick={() => onPage(pageCount)} title="Last page" aria-label="Last page">⏭</button>
      </div>
    </div>
  )
}
