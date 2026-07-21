import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import client from '../api/client'
import { getErrorMessage } from '../utils'
import ConfirmDialog from '../components/ConfirmDialog'
import type { Category, SubCategory, FeeType } from '../types'

const swatches = ['#ef4444', '#f59e0b', '#22c55e', '#14b8a6', '#3b82f6', '#6d5efc', '#a855f7', '#ec4899', '#64748b']

export default function Categories() {
  const [categories, setCategories] = useState<Category[]>([])
  const [subCategories, setSubCategories] = useState<SubCategory[]>([])
  const [feeTypes, setFeeTypes] = useState<FeeType[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ name: '', color: swatches[5] })
  const [editingId, setEditingId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)

  const [expandedId, setExpandedId] = useState<number | null>(null)
  const [subName, setSubName] = useState('')
  const [editingSubId, setEditingSubId] = useState<number | null>(null)
  const [subSaving, setSubSaving] = useState(false)

  const [expandedSubId, setExpandedSubId] = useState<number | null>(null)
  const [feeName, setFeeName] = useState('')
  const [editingFeeId, setEditingFeeId] = useState<number | null>(null)
  const [feeSaving, setFeeSaving] = useState(false)
  const [confirmState, setConfirmState] = useState<{ message: string; onConfirm: () => Promise<void> } | null>(null)

  const load = async () => {
    setLoading(true)
    try {
      const [cat, sub, fee] = await Promise.all([
        client.get<Category[]>('/categories'),
        client.get<SubCategory[]>('/subcategories'),
        client.get<FeeType[]>('/feetypes'),
      ])
      setCategories(cat.data)
      setSubCategories(sub.data)
      setFeeTypes(fee.data)
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

  const subsFor = (categoryId: number) => subCategories.filter((s) => s.categoryId === categoryId)

  const toggleExpand = (categoryId: number) => {
    setExpandedId((prev) => (prev === categoryId ? null : categoryId))
    setSubName('')
    setEditingSubId(null)
    setExpandedSubId(null)
    setFeeName('')
    setEditingFeeId(null)
    setError('')
  }

  const submitSub = async (categoryId: number, e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (!subName.trim()) return
    setSubSaving(true)
    try {
      if (editingSubId != null) {
        await client.put(`/subcategories/${editingSubId}`, { name: subName.trim() })
      } else {
        await client.post(`/subcategories?categoryId=${categoryId}`, { name: subName.trim() })
      }
      setSubName('')
      setEditingSubId(null)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save sub-category.'))
    } finally {
      setSubSaving(false)
    }
  }

  const startEditSub = (s: SubCategory) => {
    setEditingSubId(s.id)
    setSubName(s.name)
    setError('')
  }

  const removeSub = (s: SubCategory) => {
    setConfirmState({
      message: `Delete sub-category “${s.name}”? Existing expenses keep their value.`,
      onConfirm: async () => {
        await client.delete(`/subcategories/${s.id}`)
        if (editingSubId === s.id) { setEditingSubId(null); setSubName('') }
        if (expandedSubId === s.id) setExpandedSubId(null)
        await load()
      },
    })
  }

  const feesFor = (subCategoryId: number) => feeTypes.filter((f) => f.subCategoryId === subCategoryId)

  const toggleExpandSub = (subCategoryId: number) => {
    setExpandedSubId((prev) => (prev === subCategoryId ? null : subCategoryId))
    setFeeName('')
    setEditingFeeId(null)
    setError('')
  }

  const submitFee = async (subCategoryId: number, e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (!feeName.trim()) return
    setFeeSaving(true)
    try {
      if (editingFeeId != null) {
        await client.put(`/feetypes/${editingFeeId}`, { name: feeName.trim() })
      } else {
        await client.post(`/feetypes?subCategoryId=${subCategoryId}`, { name: feeName.trim() })
      }
      setFeeName('')
      setEditingFeeId(null)
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save fee type.'))
    } finally {
      setFeeSaving(false)
    }
  }

  const startEditFee = (f: FeeType) => {
    setEditingFeeId(f.id)
    setFeeName(f.name)
    setError('')
  }

  const removeFee = (f: FeeType) => {
    setConfirmState({
      message: `Delete fee type “${f.name}”? Existing expenses keep their value.`,
      onConfirm: async () => {
        await client.delete(`/feetypes/${f.id}`)
        if (editingFeeId === f.id) { setEditingFeeId(null); setFeeName('') }
        await load()
      },
    })
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
              {categories.map((c) => {
                const subs = subsFor(c.id)
                const expanded = expandedId === c.id
                return (
                  <div
                    key={c.id}
                    style={{
                      padding: '12px 14px', borderRadius: 12, border: '1px solid var(--border)',
                      background: 'var(--glass)',
                    }}
                  >
                    <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 10 }}>
                      <span className="tag" style={{ background: c.color + '22', color: 'var(--text)', fontSize: 15 }}>
                        <span className="dot" style={{ background: c.color }} />
                        {c.name}
                      </span>
                      <div style={{ display: 'flex', alignItems: 'center', gap: 10 }}>
                        {subs.length > 0 && (
                          <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>{subs.length} sub</span>
                        )}
                        {c.isDefault && (
                          <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>Default</span>
                        )}
                        <button className="btn ghost icon" title="Edit" onClick={() => startEdit(c)}>✎</button>
                        <button className="btn ghost sm" onClick={() => toggleExpand(c.id)}>
                          {expanded ? 'Hide' : 'Sub-categories'}
                        </button>
                      </div>
                    </div>

                    {expanded && (
                      <div style={{ marginTop: 12, borderTop: '1px solid var(--border)', paddingTop: 12 }}>
                        {subs.length === 0 ? (
                          <p style={{ fontSize: 13, color: 'var(--text-dim)', margin: '0 0 10px' }}>
                            No sub-categories yet. Add one below.
                          </p>
                        ) : (
                          <div style={{ display: 'flex', flexDirection: 'column', gap: 8, marginBottom: 10 }}>
                            {subs.map((s) => {
                              const fees = feesFor(s.id)
                              const subExpanded = expandedSubId === s.id
                              return (
                                <div
                                  key={s.id}
                                  style={{
                                    padding: '8px 10px', borderRadius: 10,
                                    border: '1px solid var(--border)', background: 'var(--glass)',
                                  }}
                                >
                                  <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', gap: 8 }}>
                                    <span style={{ fontWeight: 600 }}>{s.name}</span>
                                    <div style={{ display: 'flex', alignItems: 'center', gap: 6 }}>
                                      {fees.length > 0 && (
                                        <span style={{ fontSize: 12, color: 'var(--text-dim)' }}>{fees.length} fee</span>
                                      )}
                                      <button
                                        type="button"
                                        className="btn ghost icon sm"
                                        title="Edit"
                                        onClick={() => startEditSub(s)}
                                      >✎</button>
                                      <button
                                        type="button"
                                        className="btn danger icon sm"
                                        title="Delete"
                                        onClick={() => removeSub(s)}
                                      >🗑</button>
                                      <button
                                        type="button"
                                        className="btn ghost sm"
                                        onClick={() => toggleExpandSub(s.id)}
                                      >{subExpanded ? 'Hide' : 'Fee types'}</button>
                                    </div>
                                  </div>

                                  {subExpanded && (
                                    <div style={{ marginTop: 10, borderTop: '1px solid var(--border)', paddingTop: 10 }}>
                                      {fees.length === 0 ? (
                                        <p style={{ fontSize: 13, color: 'var(--text-dim)', margin: '0 0 10px' }}>
                                          No fee types yet. Add one below.
                                        </p>
                                      ) : (
                                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: 8, marginBottom: 10 }}>
                                          {fees.map((f) => (
                                            <span
                                              key={f.id}
                                              className="tag"
                                              style={{ background: 'var(--glass)', border: '1px solid var(--border)', gap: 8 }}
                                            >
                                              {f.name}
                                              <button
                                                type="button"
                                                className="btn ghost icon sm"
                                                title="Edit"
                                                onClick={() => startEditFee(f)}
                                              >✎</button>
                                              <button
                                                type="button"
                                                className="btn danger icon sm"
                                                title="Delete"
                                                onClick={() => removeFee(f)}
                                              >🗑</button>
                                            </span>
                                          ))}
                                        </div>
                                      )}
                                      <form onSubmit={(e) => submitFee(s.id, e)} style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                                        <input
                                          className="input"
                                          style={{ flex: 1, minWidth: 160 }}
                                          value={feeName}
                                          onChange={(e) => setFeeName(e.target.value)}
                                          placeholder={editingFeeId != null ? 'Edit fee type…' : 'e.g. Tuition Fee'}
                                          maxLength={40}
                                        />
                                        <button className="btn sm" disabled={feeSaving}>
                                          {feeSaving ? <span className="spinner" /> : (editingFeeId != null ? 'Update' : '＋ Add')}
                                        </button>
                                        {editingFeeId != null && (
                                          <button
                                            type="button"
                                            className="btn ghost sm"
                                            onClick={() => { setEditingFeeId(null); setFeeName('') }}
                                          >Cancel</button>
                                        )}
                                      </form>
                                    </div>
                                  )}
                                </div>
                              )
                            })}
                          </div>
                        )}
                        <form onSubmit={(e) => submitSub(c.id, e)} style={{ display: 'flex', gap: 8, flexWrap: 'wrap' }}>
                          <input
                            className="input"
                            style={{ flex: 1, minWidth: 160 }}
                            value={subName}
                            onChange={(e) => setSubName(e.target.value)}
                            placeholder={editingSubId != null ? 'Edit sub-category…' : 'e.g. Class 1'}
                            maxLength={40}
                          />
                          <button className="btn sm" disabled={subSaving}>
                            {subSaving ? <span className="spinner" /> : (editingSubId != null ? 'Update' : '＋ Add')}
                          </button>
                          {editingSubId != null && (
                            <button
                              type="button"
                              className="btn ghost sm"
                              onClick={() => { setEditingSubId(null); setSubName('') }}
                            >Cancel</button>
                          )}
                        </form>
                      </div>
                    )}
                  </div>
                )
              })}
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
