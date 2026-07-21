import { useEffect, useRef, useState } from 'react'
import client from '../api/client'
import SortHeader from '../components/SortHeader'
import type { SortDir } from '../components/SortHeader'
import Pagination from '../components/Pagination'
import { useDebouncedValue } from '../hooks'
import { formatDate } from '../utils'
import type { AdminUsersResponse } from '../types'

export default function Users() {
  const [data, setData] = useState<AdminUsersResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const debouncedSearch = useDebouncedValue(search)
  const [sort, setSort] = useState<{ key: string; dir: SortDir }>({ key: 'createdDate', dir: 'desc' })
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
      if (debouncedSearch.trim()) params.search = debouncedSearch.trim()

      const { data } = await client.get<AdminUsersResponse>('/admin/users', { params })

      const lastPage = Math.max(1, Math.ceil(data.filteredCount / pageSize))
      if (pageArg > lastPage) {
        setPage(lastPage)
        return
      }
      setData(data)
    } catch {
      setError('Failed to load users.')
    } finally {
      setLoading(false)
    }
  }

  const filterKey = JSON.stringify({ search: debouncedSearch, sort, pageSize })
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

  const users = data?.users ?? []

  return (
    <div className="fade-up">
      <div className="topbar" style={{ marginBottom: 18 }}>
        <div>
          <h2 style={{ fontSize: 20 }}>Registered users</h2>
          <p>Admin view · read-only</p>
        </div>
      </div>

      <div className="grid stats cols-3" style={{ marginBottom: 18 }}>
        <div className="card stat">
          <div className="label">Total users</div>
          <div className="value">{data?.totalUsers ?? 0}</div>
        </div>
      </div>

      <div className="card" style={{ padding: 18 }}>
        <div className="filters">
          <div className="field" style={{ flex: 1, minWidth: 180 }}>
            <label>Search</label>
            <input
              className="input"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search name or email…"
            />
          </div>
          {search && (
            <button className="btn ghost sm" onClick={() => setSearch('')}>
              Clear
            </button>
          )}
        </div>

        {error && <div className="error-banner">{error}</div>}

        {loading ? (
          <div className="empty"><span className="spinner" /></div>
        ) : users.length === 0 ? (
          <div className="empty">No users found.</div>
        ) : (
          <div className="table-scroll">
            <table>
              <thead>
                <tr>
                  <SortHeader label="Username" sortKey="userName" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                  <SortHeader label="Email" sortKey="email" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                  <SortHeader label="Registered on" sortKey="createdDate" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                </tr>
              </thead>
              <tbody>
                {users.map((u) => (
                  <tr key={u.id}>
                    <td style={{ fontWeight: 600 }}>{u.userName}</td>
                    <td style={{ color: 'var(--text-dim)' }}>{u.email}</td>
                    <td>{formatDate(u.createdDate)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} pageSize={pageSize} total={data?.filteredCount ?? 0} onPage={setPage} onPageSize={setPageSize} />
      </div>
    </div>
  )
}
