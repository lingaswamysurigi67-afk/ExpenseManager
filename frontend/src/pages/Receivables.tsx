import { useEffect, useMemo, useState } from 'react'
import client from '../api/client'
import SortHeader from '../components/SortHeader'
import type { SortDir } from '../components/SortHeader'
import Pagination from '../components/Pagination'
import { currency, monthNames } from '../utils'
import type { Receivables as ReceivablesData, ReceivableRow } from '../types'

export default function Receivables() {
  const now = new Date()
  const [filters, setFilters] = useState({ year: '', month: '' })
  const [data, setData] = useState<ReceivablesData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<{ key: string; dir: SortDir }>({ key: 'remaining', dir: 'desc' })
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const toggleSort = (key: string) =>
    setSort((s) => (s.key === key ? { key, dir: s.dir === 'asc' ? 'desc' : 'asc' } : { key, dir: 'asc' }))

  const load = async () => {
    setLoading(true)
    setError('')
    try {
      const params: Record<string, string> = {}
      if (filters.year) params.year = filters.year
      if (filters.month) params.month = filters.month
      const { data } = await client.get<ReceivablesData>('/reports/receivables', { params })
      setData(data)
    } catch {
      setError('Failed to load receivables.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filters])

  const sortVal = (r: ReceivableRow, key: string): string | number => {
    switch (key) {
      case 'person': return (r.person || '').toLowerCase()
      case 'given': return Number(r.given)
      case 'returned': return Number(r.returned)
      case 'remaining': return Number(r.remaining)
      default: return 0
    }
  }

  const visible = useMemo(() => {
    const rows = data?.rows ?? []
    const q = search.trim().toLowerCase()
    const filtered = q ? rows.filter((r) => r.person.toLowerCase().includes(q)) : rows
    const sorted = [...filtered].sort((a, b) => {
      const va = sortVal(a, sort.key)
      const vb = sortVal(b, sort.key)
      const cmp = va < vb ? -1 : va > vb ? 1 : 0
      return sort.dir === 'asc' ? cmp : -cmp
    })
    return sorted
  }, [data, search, sort])

  useEffect(() => { setPage(1) }, [search, sort, filters, pageSize])
  const paged = visible.slice((page - 1) * pageSize, page * pageSize)

  const shownRemaining = useMemo(
    () => visible.reduce((sum, r) => sum + Number(r.remaining), 0),
    [visible]
  )

  const years: number[] = []
  for (let y = now.getFullYear(); y >= now.getFullYear() - 5; y--) years.push(y)

  return (
    <div className="fade-up">
      <div className="topbar" style={{ marginBottom: 18 }}>
        <div>
          <h2 style={{ fontSize: 20 }}>Receivables</h2>
          <p>{visible.length} {visible.length === 1 ? 'person owes' : 'people owe'} you · Outstanding {currency(shownRemaining)}</p>
        </div>
      </div>

      <div className="grid stats cols-3" style={{ marginBottom: 18 }}>
        <div className="card stat">
          <div className="label">Total given</div>
          <div className="value">{currency(data?.totalGiven ?? 0)}</div>
        </div>
        <div className="card stat">
          <div className="label">Total returned</div>
          <div className="value">{currency(data?.totalReturned ?? 0)}</div>
        </div>
        <div className="card stat">
          <div className="label">Still to receive</div>
          <div className="value">{currency(data?.totalRemaining ?? 0)}</div>
        </div>
      </div>

      <div className="card" style={{ padding: 18 }}>
        <div className="filters">
          <div className="field">
            <label>Year</label>
            <select className="input" value={filters.year} onChange={(e) => setFilters({ ...filters, year: e.target.value })}>
              <option value="">All years</option>
              {years.map((y) => <option key={y} value={y}>{y}</option>)}
            </select>
          </div>
          <div className="field">
            <label>Month</label>
            <select className="input" value={filters.month} onChange={(e) => setFilters({ ...filters, month: e.target.value })}>
              <option value="">All months</option>
              {monthNames.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
            </select>
          </div>
          <div className="field" style={{ flex: 1, minWidth: 180 }}>
            <label>Search</label>
            <input
              className="input"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search person…"
            />
          </div>
          {(filters.year || filters.month || search) && (
            <button className="btn ghost sm" onClick={() => { setFilters({ year: '', month: '' }); setSearch('') }}>
              Clear filters
            </button>
          )}
        </div>

        {error && <div className="error-banner">{error}</div>}

        {loading ? (
          <div className="empty"><span className="spinner" /></div>
        ) : visible.length === 0 ? (
          <div className="empty">No outstanding amounts. Everyone is settled up. 🎉</div>
        ) : (
          <div className="table-scroll">
            <table>
              <thead>
                <tr>
                  <SortHeader label="Person" sortKey="person" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                  <SortHeader label="Given" sortKey="given" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} style={{ textAlign: 'right' }} />
                  <SortHeader label="Returned" sortKey="returned" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} style={{ textAlign: 'right' }} />
                  <SortHeader label="Remaining" sortKey="remaining" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} style={{ textAlign: 'right' }} />
                </tr>
              </thead>
              <tbody>
                {paged.map((r) => (
                  <tr key={r.personId}>
                    <td style={{ fontWeight: 600 }}>{r.person}</td>
                    <td className="amount" style={{ textAlign: 'right' }}>{currency(r.given)}</td>
                    <td className="amount" style={{ textAlign: 'right', color: 'var(--text-dim)' }}>{currency(r.returned)}</td>
                    <td className="amount" style={{ textAlign: 'right', fontWeight: 700, color: r.remaining < 0 ? '#ef4444' : 'var(--text)' }}>
                      {currency(r.remaining)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} pageSize={pageSize} total={visible.length} onPage={setPage} onPageSize={setPageSize} />
      </div>
    </div>
  )
}
