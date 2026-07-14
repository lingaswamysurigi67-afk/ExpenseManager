import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { toInputDate, paymentMethods, getErrorMessage } from '../utils'
import type { Category, Person, Income, IncomePayload } from '../types'

interface IncomeModalProps {
  open: boolean
  onClose: () => void
  onSave: (payload: IncomePayload) => Promise<void>
  categories: Category[]
  people: Person[]
  initial: Income | null
}

interface FormState {
  amount: string
  personId: string
  categoryId: string
  source: string
  date: string
  paymentMethod: string
  notes: string
}

export default function IncomeModal({ open, onClose, onSave, categories, people, initial }: IncomeModalProps) {
  const [form, setForm] = useState<FormState>({
    amount: '',
    personId: '',
    categoryId: '',
    source: '',
    date: toInputDate(),
    paymentMethod: 'Cash',
    notes: '',
  })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  useEffect(() => {
    if (open) {
      setError('')
      if (initial) {
        setForm({
          amount: String(initial.amount),
          personId: initial.personId != null ? String(initial.personId) : '',
          categoryId: String(initial.categoryId),
          source: initial.source || '',
          date: toInputDate(initial.date),
          paymentMethod: initial.paymentMethod || 'Cash',
          notes: initial.notes || '',
        })
      } else {
        setForm({
          amount: '',
          personId: '',
          categoryId: '',
          source: '',
          date: toInputDate(),
          paymentMethod: 'Cash',
          notes: '',
        })
      }
    }
  }, [open, initial])

  if (!open) return null

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    const amount = parseFloat(form.amount)
    if (!amount || amount <= 0) return setError('Enter a valid amount greater than 0.')
    if (!form.personId) return setError('Please select who the money came from.')
    if (!form.categoryId) return setError('Please choose a category.')

    setSaving(true)
    try {
      await onSave({
        amount,
        personId: Number(form.personId),
        categoryId: Number(form.categoryId),
        source: form.source.trim(),
        date: new Date(form.date).toISOString(),
        paymentMethod: form.paymentMethod,
        notes: form.notes,
      })
      onClose()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save the income.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <div className="card modal fade-up" onMouseDown={(e) => e.stopPropagation()}>
        <h3 className="section-title">{initial ? 'Edit income' : 'Add income'}</h3>
        {error && <div className="error-banner">{error}</div>}
        <form onSubmit={submit}>
          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 12 }}>
            <div className="field">
              <label>Amount</label>
              <input
                className="input"
                type="number"
                step="0.01"
                min="0"
                value={form.amount}
                onChange={(e) => setForm({ ...form, amount: e.target.value })}
                placeholder="0.00"
                autoFocus
              />
            </div>
            <div className="field">
              <label>Date</label>
              <input
                className="input"
                type="date"
                value={form.date}
                onChange={(e) => setForm({ ...form, date: e.target.value })}
              />
            </div>
          </div>

          <div className="field">
            <label>Income Came From (required)</label>
            <select
              className="input"
              value={form.personId}
              onChange={(e) => setForm({ ...form, personId: e.target.value })}
            >
              <option value="">Select a person…</option>
              {people.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            {people.length === 0 && (
              <small style={{ color: 'var(--text-dim)', marginTop: 6 }}>
                No people yet — add one on the “People” page first.
              </small>
            )}
          </div>

          <div className="field">
            <label>Category</label>
            <select
              className="input"
              value={form.categoryId}
              disabled={!form.personId}
              onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
            >
              <option value="">{form.personId ? 'Select a category…' : 'Select a person first'}</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Reference / note (optional)</label>
            <input
              className="input"
              type="text"
              value={form.source}
              onChange={(e) => setForm({ ...form, source: e.target.value })}
              placeholder="e.g. invoice #, extra detail…"
            />
          </div>

          <div className="field">
            <label>Received via</label>
            <select
              className="input"
              value={form.paymentMethod}
              onChange={(e) => setForm({ ...form, paymentMethod: e.target.value })}
            >
              {paymentMethods.map((m) => (
                <option key={m} value={m}>{m}</option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Notes (optional)</label>
            <textarea
              className="input"
              rows={2}
              value={form.notes}
              onChange={(e) => setForm({ ...form, notes: e.target.value })}
              placeholder="What was this for?"
            />
          </div>

          <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 8 }}>
            <button type="button" className="btn ghost" onClick={onClose}>Cancel</button>
            <button className="btn" disabled={saving}>
              {saving ? <span className="spinner" /> : (initial ? 'Save changes' : 'Add income')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
