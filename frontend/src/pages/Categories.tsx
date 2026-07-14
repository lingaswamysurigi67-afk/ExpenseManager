import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import client from '../api/client'
import { getErrorMessage } from '../utils'
import type { Category } from '../types'

const swatches = ['#ef4444', '#f59e0b', '#22c55e', '#14b8a6', '#3b82f6', '#6d5efc', '#a855f7', '#ec4899', '#64748b']

export default function Categories() {
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ name: '', color: swatches[5] })
  const [editingId, setEditingId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)

  const load = async () => {
    setLoading(true)
    try {
      const { data } = await client.get<Category[]>('/categories')
      setCategories(data)
    } catch {
      setError('Failed to load categories.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const resetForm = () => {
    setEditingId(null)
    setForm({ name: '', color: swatches[5] })
  }

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (!form.name.trim()) return
    setSaving(true)
    try {
      if (editingId != null) {
        await client.put(`/categories/${editingId}`, form)
      } else {
        await client.post('/categories', form)
      }
      resetForm()
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save category.'))
    } finally {
      setSaving(false)
    }
  }

  const startEdit = (c: Category) => {
    setEditingId(c.id)
    setForm({ name: c.name, color: c.color })
    setError('')
  }

  return (
    <div className="fade-up">
      <div className="grid cols-2">
        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">Your categories</h3>
          {error && <div className="error-banner">{error}</div>}
          {loading ? (
            <div className="empty"><span className="spinner" /></div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {categories.map((c) => (
                <div
                  key={c.id}
                  style={{
                    display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                    padding: '12px 14px', borderRadius: 12, border: '1px solid var(--border)',
                    background: 'var(--glass)',
                  }}
                >
                  <span className="tag" style={{ background: c.color + '22', color: 'var(--text)', fontSize: 15 }}>
                    <span className="dot" style={{ background: c.color }} />
                    {c.name}
                  </span>
                  <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                    {c.isDefault && (
                      <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>Default</span>
                    )}
                    <button className="btn ghost icon" title="Edit" onClick={() => startEdit(c)}>✎</button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">{editingId != null ? 'Edit category' : 'Add a category'}</h3>
          <form onSubmit={submit}>
            <div className="field">
              <label>Name</label>
              <input
                className="input"
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
                placeholder="e.g. Coffee"
                maxLength={40}
              />
            </div>
            <div className="field">
              <label>Color</label>
              <div style={{ display: 'flex', gap: 10, flexWrap: 'wrap' }}>
                {swatches.map((s) => (
                  <button
                    type="button"
                    key={s}
                    onClick={() => setForm({ ...form, color: s })}
                    style={{
                      width: 32, height: 32, borderRadius: 10, cursor: 'pointer',
                      background: s,
                      border: form.color === s ? '3px solid #fff' : '2px solid transparent',
                      boxShadow: form.color === s ? `0 0 0 2px ${s}` : 'none',
                    }}
                  />
                ))}
              </div>
            </div>
            <div style={{ display: 'flex', gap: 10, marginTop: 8 }}>
              <button className="btn" style={{ flex: 1 }} disabled={saving}>
                {saving ? <span className="spinner" /> : (editingId != null ? 'Update' : '＋ Add category')}
              </button>
              {editingId != null && (
                <button type="button" className="btn ghost" onClick={resetForm}>Cancel</button>
              )}
            </div>
          </form>
        </div>
      </div>
    </div>
  )
}
