import { useEffect, useState } from 'react'
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip } from 'recharts'
import client from '../api/client'
import ExpenseModal from '../components/ExpenseModal'
import { currency, formatDate } from '../utils'
import type { Category, Person, Expense, ExpensePayload, Summary, ExpensePage } from '../types'

interface PieDatum {
  name: string
  value: number
  color: string
}

export default function Dashboard() {
  const [summary, setSummary] = useState<Summary | null>(null)
  const [recent, setRecent] = useState<Expense[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [people, setPeople] = useState<Person[]>([])
  const [loading, setLoading] = useState(true)
  const [modalOpen, setModalOpen] = useState(false)

  const load = async () => {
    setLoading(true)
    try {
      const [sum, ex, cat, eo] = await Promise.all([
        client.get<Summary>('/reports/summary'),
        client.get<ExpensePage>('/expenses', { params: { page: 1, pageSize: 6, sort: 'date', dir: 'desc' } }),
        client.get<Category[]>('/categories'),
        client.get<Person[]>('/people'),
      ])
      setSummary(sum.data)
      setRecent(ex.data.items)
      setCategories(cat.data)
      setPeople(eo.data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const save = async (payload: ExpensePayload) => {
    await client.post('/expenses', payload)
    await load()
  }

  const catColor = (id: number) => categories.find((c) => c.id === id)?.color || '#64748b'

  if (loading || !summary) {
    return <div className="empty"><span className="spinner" /></div>
  }

  const stats = [
    { label: 'Total spent', value: currency(summary.total), icon: '💵', bg: 'linear-gradient(135deg,#6d5efc,#9b5efc)' },
    { label: 'This month', value: currency(summary.thisMonthTotal), icon: '📅', bg: 'linear-gradient(135deg,#29d3c2,#3b82f6)' },
    { label: 'Today', value: currency(summary.todayTotal), icon: '⚡', bg: 'linear-gradient(135deg,#f5a524,#ff8a5d)' },
    { label: 'Avg / expense', value: currency(summary.average), icon: '📊', bg: 'linear-gradient(135deg,#ec4899,#a855f7)' },
  ]

  const pieData: PieDatum[] = summary.byCategory.map((c) => ({ name: c.category, value: Number(c.total), color: c.color }))

  return (
    <div className="fade-up">
      <div style={{ display: 'flex', justifyContent: 'flex-end', marginBottom: 18 }}>
        <button className="btn" onClick={() => setModalOpen(true)}>＋ Quick add</button>
      </div>

      <div className="grid stats">
        {stats.map((s) => (
          <div key={s.label} className="card stat">
            <div className="label">{s.label}</div>
            <div className="value">{s.value}</div>
            <div className="pill" style={{ background: s.bg }}>{s.icon}</div>
          </div>
        ))}
      </div>

      <div className="grid cols-2" style={{ marginTop: 18 }}>
        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">Recent expenses</h3>
          {recent.length === 0 ? (
            <div className="empty">No expenses yet.</div>
          ) : (
            <table>
              <tbody>
                {recent.map((e) => (
                  <tr key={e.id}>
                    <td style={{ width: 40 }}>
                      <span className="dot" style={{ background: catColor(e.categoryId), width: 12, height: 12 }} />
                    </td>
                    <td>
                      <div style={{ fontWeight: 600 }}>{e.category}</div>
                      <div style={{ fontSize: 12, color: 'var(--text-dim)' }}>{formatDate(e.date)} · {e.paymentMethod}</div>
                    </td>
                    <td className="amount" style={{ textAlign: 'right' }}>{currency(e.amount)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">Spending by category</h3>
          {pieData.length === 0 ? (
            <div className="empty">No data to chart yet.</div>
          ) : (
            <>
              <div className="chart-wrap" style={{ height: 220 }}>
                <ResponsiveContainer>
                  <PieChart>
                    <Pie data={pieData} dataKey="value" nameKey="name" innerRadius={55} outerRadius={90} paddingAngle={2}>
                      {pieData.map((d, i) => <Cell key={i} fill={d.color} stroke="none" />)}
                    </Pie>
                    <Tooltip
                      formatter={(v) => currency(v as number)}
                      contentStyle={{ background: '#ffffff', border: '1px solid #e2e6f0', borderRadius: 10, color: '#1f2430', boxShadow: '0 10px 30px rgba(31,36,48,0.12)' }}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </div>
              <div className="legend-list">
                {summary.byCategory.slice(0, 5).map((c) => (
                  <div className="legend-row" key={c.categoryId}>
                    <span className="tag" style={{ background: c.color + '22', color: 'var(--text)' }}>
                      <span className="dot" style={{ background: c.color }} />{c.category}
                    </span>
                    <span className="amount">{currency(c.total)} · {c.percentage}%</span>
                  </div>
                ))}
              </div>
            </>
          )}
        </div>
      </div>

      <ExpenseModal
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSave={save}
        categories={categories}
        people={people}
        initial={null}
      />
    </div>
  )
}
