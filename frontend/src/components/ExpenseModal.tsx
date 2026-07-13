import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { toInputDate, paymentMethods, getErrorMessage } from '../utils'
import type { Category, Expense, ExpensePayload } from '../types'

interface ExpenseModalProps {
  open: boolean
  onClose: () => void
  onSave: (payload: ExpensePayload) => Promise<void>
  categories: Category[]
  initial: Expense | null
}

interface FormState {
  amount: string
  categoryId: string
  date: string
  paymentMethod: string
  notes: string
}

export default function ExpenseModal({ open, onClose, onSave, categories, initial }: ExpenseModalProps) {
  const [form, setForm] = useState<FormState>({
    amount: '',
    categoryId: '',
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
          categoryId: String(initial.categoryId),
          date: toInputDate(initial.date),
          paymentMethod: initial.paymentMethod || 'Cash',
          notes: initial.notes || '',
        })
      } else {
        setForm({
          amount: '',
          categoryId: categories[0] ? String(categories[0].id) : '',
          date: toInputDate(),
          paymentMethod: 'Cash',
          notes: '',
        })
      }
    }
  }, [open, initial, categories])

  if (!open) return null

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    const amount = parseFloat(form.amount)
    if (!amount || amount <= 0) return setError('Enter a valid amount greater than 0.')
    if (!form.categoryId) return setError('Please choose a category.')

    setSaving(true)
    try {
      await onSave({
        amount,
        categoryId: Number(form.categoryId),
        date: new Date(form.date).toISOString(),
        paymentMethod: form.paymentMethod,
        notes: form.notes,
      })
      onClose()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save the expense.'))
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <div className="card modal fade-up" onMouseDown={(e) => e.stopPropagation()}>
        <h3 className="section-title">{initial ? 'Edit expense' : 'Add expense'}</h3>
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
            <label>Category</label>
            <select
              className="input"
              value={form.categoryId}
              onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
            >
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          <div className="field">
            <label>Payment method</label>
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
              {saving ? <span className="spinner" /> : (initial ? 'Save changes' : 'Add expense')}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}
