import { useEffect, useMemo, useState } from 'react'
import client from '../api/client'
import ExpenseModal from '../components/ExpenseModal'
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

  const total = useMemo(
    () => expenses.reduce((sum, e) => sum + Number(e.amount), 0),
    [expenses]
  )

  const catColor = (id: number) => categories.find((c) => c.id === id)?.color || '#64748b'

  const save = async (payload: ExpensePayload) => {
    if (editing) {
      await client.put(`/expenses/${editing.id}`, payload)
    } else {
      await client.post('/expenses', payload)
    }
    await load()
  }

  const remove = async (id: number) => {
    if (!confirm('Delete this expense?')) return
    await client.delete(`/expenses/${id}`)
    await load()
  }

  const years: number[] = []
  for (let y = now.getFullYear(); y >= now.getFullYear() - 5; y--) years.push(y)

  return (
    <div className="fade-up">
      <div className="topbar" style={{ marginBottom: 18 }}>
        <div>
          <h2 style={{ fontSize: 20 }}>Expenses</h2>
          <p>{expenses.length} record{expenses.length !== 1 ? 's' : ''} · Total {currency(total)}</p>
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
          {(filters.year || filters.month || filters.categoryId) && (
            <button className="btn ghost sm" onClick={() => setFilters({ year: '', month: '', categoryId: '' })}>
              Clear filters
            </button>
          )}
        </div>

        {error && <div className="error-banner">{error}</div>}

        {loading ? (
          <div className="empty"><span className="spinner" /></div>
        ) : expenses.length === 0 ? (
          <div className="empty">No expenses yet. Add your first one!</div>
        ) : (
          <div style={{ overflowX: 'auto' }}>
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>On</th>
                  <th>Category</th>
                  <th>Notes</th>
                  <th>Method</th>
                  <th style={{ textAlign: 'right' }}>Amount</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {expenses.map((e) => (
                  <tr key={e.id}>
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
      </div>

      <ExpenseModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSave={save}
        categories={categories}
        people={people}
        initial={editing}
      />
    </div>
  )
}
