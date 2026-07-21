import { useEffect, useState } from 'react'
import client from '../api/client'
import { currency, getErrorMessage } from '../utils'
import type { Category, Person, SubCategorySpending } from '../types'

export default function ClassSpending() {
  const [people, setPeople] = useState<Person[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [personId, setPersonId] = useState('')
  const [categoryId, setCategoryId] = useState('')
  const [data, setData] = useState<SubCategorySpending | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    const loadLookups = async () => {
      try {
        const [ppl, cat] = await Promise.all([
          client.get<Person[]>('/people'),
          client.get<Category[]>('/categories'),
        ])
        setPeople(ppl.data)
        setCategories(cat.data)
      } catch {
        setError('Failed to load people and categories.')
      }
    }
    loadLookups()
  }, [])

  useEffect(() => {
    if (!personId) {
      setData(null)
      return
    }
    const load = async () => {
      setLoading(true)
      setError('')
      try {
        const params: Record<string, string> = { personId }
        if (categoryId) params.categoryId = categoryId
        const { data } = await client.get<SubCategorySpending>('/reports/subcategory-spending', { params })
        setData(data)
      } catch (err) {
        setError(getErrorMessage(err, 'Failed to load class spending.'))
        setData(null)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [personId, categoryId])

  const rows = data?.rows ?? []
  const yearsLabel = (from: number, to: number) => (from === to ? String(from) : `${from}–${to}`)

  return (
    <div className="fade-up">
      <div className="topbar" style={{ marginBottom: 18 }}>
        <div>
          <h2 style={{ fontSize: 20 }}>Class spending</h2>
          <p>Spending per class (sub-category) for a person, with the year-over-year change.</p>
        </div>
      </div>

      <div className="card" style={{ padding: 18 }}>
        <div className="filters">
          <div className="field" style={{ minWidth: 220 }}>
            <label>Person (required)</label>
            <select className="input" value={personId} onChange={(e) => setPersonId(e.target.value)}>
              <option value="">Select a person…</option>
              {people.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>
          <div className="field" style={{ minWidth: 200 }}>
            <label>Category (optional)</label>
            <select className="input" value={categoryId} onChange={(e) => setCategoryId(e.target.value)}>
              <option value="">All categories</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
          {categoryId && (
            <button className="btn ghost sm" onClick={() => setCategoryId('')}>Clear category</button>
          )}
        </div>

        {error && <div className="error-banner">{error}</div>}

        {!personId ? (
          <div className="empty">Select a person to see their class-wise spending.</div>
        ) : loading ? (
          <div className="empty"><span className="spinner" /></div>
        ) : rows.length === 0 ? (
          <div className="empty">No sub-category spending found for this person.</div>
        ) : (
          <>
            <div className="grid stats cols-3" style={{ margin: '4px 0 18px' }}>
              <div className="card stat">
                <div className="label">Total spent</div>
                <div className="value">{currency(data?.grandTotal ?? 0)}</div>
              </div>
              <div className="card stat">
                <div className="label">Classes tracked</div>
                <div className="value">{rows.length}</div>
              </div>
              <div className="card stat">
                <div className="label">Latest class</div>
                <div className="value" style={{ fontSize: 18 }}>{rows[rows.length - 1]?.subCategory || '—'}</div>
              </div>
            </div>

            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>Class</th>
                    <th>Year(s)</th>
                    <th style={{ textAlign: 'right' }}>Amount</th>
                    <th style={{ textAlign: 'right' }}>Increase</th>
                    <th style={{ textAlign: 'right' }}>Increase %</th>
                    <th style={{ textAlign: 'right' }}>Entries</th>
                  </tr>
                </thead>
                <tbody>
                  {rows.map((r) => {
                    const up = (r.increaseAmount ?? 0) > 0
                    const down = (r.increaseAmount ?? 0) < 0
                    const color = up ? '#ef4444' : down ? '#22c55e' : 'var(--text-dim)'
                    return (
                      <tr key={r.subCategoryId ?? r.subCategory}>
                        <td style={{ fontWeight: 600 }}>
                          {r.subCategory}
                          {r.fees.length > 0 && (
                            <div style={{ display: 'flex', flexDirection: 'column', gap: 2, marginTop: 4, fontWeight: 400 }}>
                              {r.fees.map((f) => (
                                <span key={f.feeTypeId ?? f.feeType} style={{ fontSize: 12, color: 'var(--text-dim)' }}>
                                  {f.feeType}: {currency(f.total)}
                                </span>
                              ))}
                            </div>
                          )}
                        </td>
                        <td style={{ color: 'var(--text-dim)' }}>{yearsLabel(r.firstYear, r.lastYear)}</td>
                        <td className="amount" style={{ textAlign: 'right' }}>{currency(r.total)}</td>
                        <td style={{ textAlign: 'right', color }}>
                          {r.increaseAmount == null ? '—' : `${up ? '+' : ''}${currency(r.increaseAmount)}`}
                        </td>
                        <td style={{ textAlign: 'right', color }}>
                          {r.increasePercentage == null ? '—' : `${r.increasePercentage > 0 ? '+' : ''}${r.increasePercentage}%`}
                        </td>
                        <td style={{ textAlign: 'right', color: 'var(--text-dim)' }}>{r.count}</td>
                      </tr>
                    )
                  })}
                </tbody>
              </table>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
