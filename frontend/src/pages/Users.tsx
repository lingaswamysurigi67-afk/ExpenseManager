import { useEffect, useMemo, useState } from 'react'
import client from '../api/client'
import SortHeader from '../components/SortHeader'
import type { SortDir } from '../components/SortHeader'
import Pagination from '../components/Pagination'
import { formatDate } from '../utils'
import type { AdminUsersResponse, AdminUserRow } from '../types'

export default function Users() {
  const [data, setData] = useState<AdminUsersResponse | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<{ key: string; dir: SortDir }>({ key: 'createdDate', dir: 'desc' })
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)

  const toggleSort = (key: string) =>
    setSort((s) => (s.key === key ? { key, dir: s.dir === 'asc' ? 'desc' : 'asc' } : { key, dir: 'asc' }))

  const load = async () => {
    setLoading(true)
    setError('')
    try {
      const { data } = await client.get<AdminUsersResponse>('/admin/users')
      setData(data)
    } catch {
      setError('Failed to load users.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  const sortVal = (u: AdminUserRow, key: string): string | number => {
    switch (key) {
      case 'userName': return (u.userName || '').toLowerCase()
      case 'email': return (u.email || '').toLowerCase()
      case 'createdDate': return new Date(u.createdDate).getTime()
      default: return 0
    }
  }

  const visible = useMemo(() => {
    const rows = data?.users ?? []
    const q = search.trim().toLowerCase()
    const filtered = q
      ? rows.filter((u) => u.userName.toLowerCase().includes(q) || u.email.toLowerCase().includes(q))
      : rows
    const sorted = [...filtered].sort((a, b) => {
      const va = sortVal(a, sort.key)
      const vb = sortVal(b, sort.key)
      const cmp = va < vb ? -1 : va > vb ? 1 : 0
      return sort.dir === 'asc' ? cmp : -cmp
    })
    return sorted
  }, [data, search, sort])

  useEffect(() => { setPage(1) }, [search, sort, pageSize])
  const paged = visible.slice((page - 1) * pageSize, page * pageSize)

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
        ) : visible.length === 0 ? (
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
                {paged.map((u) => (
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
        <Pagination page={page} pageSize={pageSize} total={visible.length} onPage={setPage} onPageSize={setPageSize} />
      </div>
    </div>
  )
}
