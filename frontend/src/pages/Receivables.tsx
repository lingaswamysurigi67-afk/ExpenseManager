import { useEffect, useRef, useState } from 'react'
import client from '../api/client'
import SortHeader from '../components/SortHeader'
import type { SortDir } from '../components/SortHeader'
import Pagination from '../components/Pagination'
import { useDebouncedValue } from '../hooks'
import { currency, monthNames } from '../utils'
import type { Receivables as ReceivablesData } from '../types'

export default function Receivables() {
  const now = new Date()
  const [filters, setFilters] = useState({ year: '', month: '' })
  const [data, setData] = useState<ReceivablesData | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const [sort, setSort] = useState<{ key: string; dir: SortDir }>({ key: 'remaining', dir: 'desc' })
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const toggleSort = (key: string) =>
    setSort((s) => (s.key === key ? { key, dir: s.dir === 'asc' ? 'desc' : 'asc' } : { key, dir: 'asc' }))

  const load = async (pageArg: number) => {
    setLoading(true)
    setError('')
    try {
      const params: Record<string, string> = {
        page: String(pageArg),
        pageSize: String(pageSize),
        sort: sort.key,
        dir: sort.dir,
      }
      if (filters.year) params.year = filters.year
      if (filters.month) params.month = filters.month
      if (debouncedSearch.trim()) params.search = debouncedSearch.trim()

      const { data } = await client.get<ReceivablesData>('/reports/receivables', { params })

      const lastPage = Math.max(1, Math.ceil(data.filteredCount / pageSize))
      if (pageArg > lastPage) {
        setPage(lastPage)
        return
      }
      setData(data)
    } catch {
      setError('Failed to load receivables.')
    } finally {
      setLoading(false)
    }
  }

  const filterKey = JSON.stringify({ filters, search: debouncedSearch, sort, pageSize })
  const prevKey = useRef(filterKey)
  useEffect(() => {
    if (prevKey.current !== filterKey) {
      prevKey.current = filterKey
      if (page !== 1) {
        setPage(1)
        return
      }
    }
    load(page)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filterKey, page])

  const rows = data?.rows ?? []
  const filteredCount = data?.filteredCount ?? 0
  const filteredRemaining = data?.filteredRemaining ?? 0

  const years: number[] = []
  for (let y = now.getFullYear(); y >= now.getFullYear() - 5; y--) years.push(y)

  return (
    <div className="fade-up">
      <div className="topbar" style={{ marginBottom: 18 }}>
        <div>
          <h2 style={{ fontSize: 20 }}>Receivables</h2>
          <p>{filteredCount} {filteredCount === 1 ? 'person owes' : 'people owe'} you · Outstanding {currency(filteredRemaining)}</p>
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
        ) : rows.length === 0 ? (
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
                {rows.map((r) => (
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
        <Pagination page={page} pageSize={pageSize} total={filteredCount} onPage={setPage} onPageSize={setPageSize} />
      </div>
    </div>
  )
}
