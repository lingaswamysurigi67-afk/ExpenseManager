import { useEffect, useState } from 'react'
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts'
import client from '../api/client'
import { currency, monthNames } from '../utils'
import type { Summary } from '../types'

interface MonthDatum {
  name: string
  total: number
}

export default function Reports() {
  const now = new Date()
  const [filters, setFilters] = useState({ year: '', month: '' })
  const [summary, setSummary] = useState<Summary | null>(null)
  const [loading, setLoading] = useState(true)

  const load = async () => {
    setLoading(true)
    try {
      const params: Record<string, string> = {}
      if (filters.year) params.year = filters.year
      if (filters.month) params.month = filters.month
      const { data } = await client.get<Summary>('/reports/summary', { params })
      setSummary(data)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [filters])

  const years: number[] = []
  for (let y = now.getFullYear(); y >= now.getFullYear() - 5; y--) years.push(y)

  const monthData: MonthDatum[] = summary?.byMonth.map((m) => ({ name: m.label, total: Number(m.total) })) || []

  return (
    <div className="fade-up">
      <div className="card" style={{ padding: 18, marginBottom: 18 }}>
        <div className="filters" style={{ marginBottom: 0 }}>
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
        </div>
      </div>

      {loading || !summary ? (
        <div className="empty"><span className="spinner" /></div>
      ) : (
        <>
          <div className="grid stats cols-3" style={{ marginBottom: 18 }}>
            <div className="card stat">
              <div className="label">Total in range</div>
              <div className="value">{currency(summary.total)}</div>
            </div>
            <div className="card stat">
              <div className="label">Transactions</div>
              <div className="value">{summary.count}</div>
            </div>
            <div className="card stat">
              <div className="label">Average</div>
              <div className="value">{currency(summary.average)}</div>
            </div>
          </div>

          <div className="card" style={{ padding: 22, marginBottom: 18 }}>
            <h3 className="section-title">Monthly trend</h3>
            {monthData.length === 0 ? (
              <div className="empty">No data for this range.</div>
            ) : (
              <div className="chart-wrap">
                <ResponsiveContainer>
                  <BarChart data={monthData} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
                    <defs>
                      <linearGradient id="barGrad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="#9b5efc" />
                        <stop offset="100%" stopColor="#6d5efc" />
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="rgba(255,255,255,0.08)" vertical={false} />
                    <XAxis dataKey="name" tick={{ fill: '#5c6478', fontSize: 13 }} axisLine={false} tickLine={false} />
                    <YAxis tick={{ fill: '#5c6478', fontSize: 13 }} axisLine={false} tickLine={false} width={50} />
                    <Tooltip
                      cursor={{ fill: 'rgba(109,94,252,0.06)' }}
                      formatter={(v) => currency(v as number)}
                      contentStyle={{ background: '#ffffff', border: '1px solid #e2e6f0', borderRadius: 10, color: '#1f2430', boxShadow: '0 10px 30px rgba(31,36,48,0.12)' }}
                    />
                    <Bar dataKey="total" fill="url(#barGrad)" radius={[8, 8, 0, 0]} maxBarSize={54} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            )}
          </div>

          <div className="card" style={{ padding: 22 }}>
            <h3 className="section-title">Category breakdown</h3>
            {summary.byCategory.length === 0 ? (
              <div className="empty">No expenses in this range.</div>
            ) : (
              <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
                {summary.byCategory.map((c) => (
                  <div key={c.categoryId}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6, fontSize: 14 }}>
                      <span className="tag" style={{ background: c.color + '22', color: 'var(--text)' }}>
                        <span className="dot" style={{ background: c.color }} />{c.category}
                      </span>
                      <span className="amount">{currency(c.total)} · {c.percentage}%</span>
                    </div>
                    <div className="bar-track">
                      <div className="bar-fill" style={{ width: `${c.percentage}%`, background: c.color }} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </>
      )}
    </div>
  )
}
