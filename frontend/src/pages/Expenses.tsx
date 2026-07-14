import { useEffect, useMemo, useState } from 'react'
import client from '../api/client'
import ExpenseModal from '../components/ExpenseModal'
import ConfirmDialog from '../components/ConfirmDialog'
import SortHeader from '../components/SortHeader'
import type { SortDir } from '../components/SortHeader'
import Pagination from '../components/Pagination'
import { currency, formatDate, monthNames } from '../utils'
import type { Category, Person, Expense, ExpensePayload } from '../types'

export default function Expenses() {
  const [expenses, setExpenses] = useState<Expense[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [people, setPeople] = useState<Person[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [modalOpen, setModalOpen] = useState(false)
  const [editing, setEditing] = useState<Expense | null>(null)

  const now = new Date()
  const [filters, setFilters] = useState({ year: '', month: '', categoryId: '' })
  const [search, setSearch] = useState('')
  const [sort, setSort] = useState<{ key: string; dir: SortDir }>({ key: 'date', dir: 'desc' })
  const [page, setPage] = useState(1)
  const [pageSize, setPageSize] = useState(10)
  const [selected, setSelected] = useState<Set<number>>(new Set())
  const [confirmState, setConfirmState] = useState<{ message: string; onConfirm: () => Promise<void> } | null>(null)

  const toggleSort = (key: string) =>
    setSort((s) => (s.key === key ? { key, dir: s.dir === 'asc' ? 'desc' : 'asc' } : { key, dir: 'asc' }))

  const load = async () => {
    setLoading(true)
    setError('')
    try {
      const params: Record<string, string> = {}
      if (filters.year) params.year = filters.year
      if (filters.month) params.month = filters.month
      if (filters.categoryId) params.categoryId = filters.categoryId
      const [ex, cat, eo] = await Promise.all([
        client.get<Expense[]>('/expenses', { params }),
        client.get<Category[]>('/categories'),
        client.get<Person[]>('/people'),
      ])
      setExpenses(ex.data)
      setCategories(cat.data)
      setPeople(eo.data)
      setSelected(new Set())
    } catch {
      setError('Failed to load expenses.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filters])

  const catColor = (id: number) => categories.find((c) => c.id === id)?.color || '#64748b'

  const sortVal = (e: Expense, key: string): string | number => {
    switch (key) {
      case 'date': return new Date(e.date).getTime()
      case 'amount': return Number(e.amount)
      case 'person': return (e.personName || '').toLowerCase()
      case 'category': return (e.category || '').toLowerCase()
      default: return 0
    }
  }

  const visible = useMemo(() => {
    const q = search.trim().toLowerCase()
    let rows = expenses
    if (q) {
      rows = rows.filter((e) =>
        (e.personName || '').toLowerCase().includes(q) ||
        (e.category || '').toLowerCase().includes(q) ||
        (e.notes || '').toLowerCase().includes(q) ||
        (e.paymentMethod || '').toLowerCase().includes(q)
      )
    }
    const sorted = [...rows].sort((a, b) => {
      const va = sortVal(a, sort.key)
      const vb = sortVal(b, sort.key)
      const cmp = va < vb ? -1 : va > vb ? 1 : 0
      return sort.dir === 'asc' ? cmp : -cmp
    })
    return sorted
  }, [expenses, search, sort])

  const total = useMemo(
    () => visible.reduce((sum, e) => sum + Number(e.amount), 0),
    [visible]
  )

  useEffect(() => { setPage(1) }, [search, sort, filters, pageSize])
  const paged = visible.slice((page - 1) * pageSize, page * pageSize)

  const allPageSelected = paged.length > 0 && paged.every((e) => selected.has(e.id))
  const somePageSelected = paged.some((e) => selected.has(e.id))

  const toggleRow = (id: number) => {
    setSelected((prev) => {
      const next = new Set(prev)
      if (next.has(id)) next.delete(id); else next.add(id)
      return next
    })
  }

  const toggleAllOnPage = () => {
    setSelected((prev) => {
      const next = new Set(prev)
      if (allPageSelected) paged.forEach((e) => next.delete(e.id))
      else paged.forEach((e) => next.add(e.id))
      return next
    })
  }

  const bulkDelete = () => {
    if (selected.size === 0) return
    setConfirmState({
      message: `Delete ${selected.size} selected expense${selected.size !== 1 ? 's' : ''}? This cannot be undone.`,
      onConfirm: async () => { await client.post('/expenses/bulk-delete', { ids: [...selected] }); await load() },
    })
  }

  const save = async (payload: ExpensePayload) => {
    if (editing) {
      await client.put(`/expenses/${editing.id}`, payload)
    } else {
      await client.post('/expenses', payload)
    }
    await load()
  }

  const remove = (id: number) => {
    setConfirmState({
      message: 'Delete this expense? This cannot be undone.',
      onConfirm: async () => { await client.delete(`/expenses/${id}`); await load() },
    })
  }

  const years: number[] = []
  for (let y = now.getFullYear(); y >= now.getFullYear() - 5; y--) years.push(y)

  return (
    <div className="fade-up">
      <div className="topbar" style={{ marginBottom: 18 }}>
        <div>
          <h2 style={{ fontSize: 20 }}>Expenses</h2>
          <p>{visible.length} record{visible.length !== 1 ? 's' : ''} · Total {currency(total)}</p>
        </div>
        <button className="btn" onClick={() => { setEditing(null); setModalOpen(true) }}>
          ＋ Add expense
        </button>
      </div>

      <div className="card" style={{ padding: 18 }}>
        <div className="filters">
          <div className="field">
            <label>Year</label>
            <select className="input" value={filters.year} onChange={(e) => setFilters({ ...filters, year: e.target.value })}>
              <option value="">All</option>
              {years.map((y) => <option key={y} value={y}>{y}</option>)}
            </select>
          </div>
          <div className="field">
            <label>Month</label>
            <select className="input" value={filters.month} onChange={(e) => setFilters({ ...filters, month: e.target.value })}>
              <option value="">All</option>
              {monthNames.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
            </select>
          </div>
          <div className="field">
            <label>Category</label>
            <select className="input" value={filters.categoryId} onChange={(e) => setFilters({ ...filters, categoryId: e.target.value })}>
              <option value="">All</option>
              {categories.map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div className="field" style={{ flex: 1, minWidth: 180 }}>
            <label>Search</label>
            <input
              className="input"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search person, category, notes…"
            />
          </div>
          {(filters.year || filters.month || filters.categoryId || search) && (
            <button className="btn ghost sm" onClick={() => { setFilters({ year: '', month: '', categoryId: '' }); setSearch('') }}>
              Clear filters
            </button>
          )}
        </div>

        {error && <div className="error-banner">{error}</div>}

        {selected.size > 0 && (
          <div className="bulk-bar">
            <span>{selected.size} selected</span>
            <div style={{ display: 'flex', gap: 8 }}>
              <button className="btn danger sm" onClick={bulkDelete}>🗑 Delete selected</button>
              <button className="btn ghost sm" onClick={() => setSelected(new Set())}>Clear</button>
            </div>
          </div>
        )}

        {loading ? (
          <div className="empty"><span className="spinner" /></div>
        ) : visible.length === 0 ? (
          <div className="empty">No expenses match. Try clearing filters or search.</div>
        ) : (
          <div className="table-scroll">
            <table>
              <thead>
                <tr>
                  <th className="chk-col">
                    <input
                      type="checkbox"
                      checked={allPageSelected}
                      ref={(el) => { if (el) el.indeterminate = !allPageSelected && somePageSelected }}
                      onChange={toggleAllOnPage}
                    />
                  </th>
                  <SortHeader label="Date" sortKey="date" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                  <SortHeader label="Expenditure On" sortKey="person" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                  <SortHeader label="Category" sortKey="category" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} />
                  <th>Notes</th>
                  <th>Method</th>
                  <SortHeader label="Amount" sortKey="amount" activeKey={sort.key} dir={sort.dir} onSort={toggleSort} style={{ textAlign: 'right' }} />
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {paged.map((e) => (
                  <tr key={e.id}>
                    <td className="chk-col">
                      <input type="checkbox" checked={selected.has(e.id)} onChange={() => toggleRow(e.id)} />
                    </td>
                    <td>{formatDate(e.date)}</td>
                    <td style={{ fontWeight: 600 }}>{e.personName || '—'}</td>
                    <td>
                      <span className="tag" style={{ background: catColor(e.categoryId) + '22', color: 'var(--text)' }}>
                        <span className="dot" style={{ background: catColor(e.categoryId) }} />
                        {e.category}
                      </span>
                    </td>
                    <td style={{ color: 'var(--text-dim)', maxWidth: 240, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                      {e.notes || '—'}
                    </td>
                    <td style={{ color: 'var(--text-dim)' }}>{e.paymentMethod}</td>
                    <td className="amount" style={{ textAlign: 'right' }}>{currency(e.amount)}</td>
                    <td>
                      <div className="row-actions" style={{ justifyContent: 'flex-end' }}>
                        <button className="btn ghost icon" title="Edit" onClick={() => { setEditing(e); setModalOpen(true) }}>✎</button>
                        <button className="btn danger icon" title="Delete" onClick={() => remove(e.id)}>🗑</button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} pageSize={pageSize} total={visible.length} onPage={setPage} onPageSize={setPageSize} />
      </div>

      <ExpenseModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSave={save}
        categories={categories}
        people={people}
        initial={editing}
      />

      <ConfirmDialog
        open={!!confirmState}
        title="Delete"
        message={confirmState?.message || ''}
        confirmLabel="Delete"
        danger
        onCancel={() => setConfirmState(null)}
        onConfirm={async () => { const action = confirmState?.onConfirm; setConfirmState(null); if (action) await action() }}
      />
    </div>
  )
}
