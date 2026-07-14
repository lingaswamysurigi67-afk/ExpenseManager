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
      <div className="pager-left">
        <span className="pager-info">Showing {from}–{to} of {total}</span>
        <label className="pager-size">
          Rows:
          <select
            className="input"
            value={pageSize}
            onChange={(e) => onPageSize(Number(e.target.value))}
          >
            {pageSizeOptions.map((n) => (
              <option key={n} value={n}>{n}</option>
            ))}
          </select>
        </label>
      </div>
      {total > pageSize && (
        <div className="pager-controls">
          <button className="btn ghost sm" disabled={page <= 1} onClick={() => onPage(1)}>« First</button>
          <button className="btn ghost sm" disabled={page <= 1} onClick={() => onPage(page - 1)}>‹ Prev</button>
          <span className="pager-page">Page {page} / {pageCount}</span>
          <button className="btn ghost sm" disabled={page >= pageCount} onClick={() => onPage(page + 1)}>Next ›</button>
          <button className="btn ghost sm" disabled={page >= pageCount} onClick={() => onPage(pageCount)}>Last »</button>
        </div>
      )}
    </div>
  )
}
