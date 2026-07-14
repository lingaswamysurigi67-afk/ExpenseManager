import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { toInputDate, paymentMethods, getErrorMessage } from '../utils'
import type { Category, ExpenditureOn, Expense, ExpensePayload } from '../types'

interface ExpenseModalProps {
  open: boolean
  onClose: () => void
  onSave: (payload: ExpensePayload) => Promise<void>
  categories: Category[]
  expenditureOns: ExpenditureOn[]
  initial: Expense | null
}

interface FormState {
  amount: string
  expenditureOnId: string
  categoryId: string
  date: string
  paymentMethod: string
  notes: string
}

export default function ExpenseModal({ open, onClose, onSave, categories, expenditureOns, initial }: ExpenseModalProps) {
  const [form, setForm] = useState<FormState>({
    amount: '',
    expenditureOnId: '',
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
          expenditureOnId: initial.expenditureOnId != null ? String(initial.expenditureOnId) : '',
          categoryId: String(initial.categoryId),
          date: toInputDate(initial.date),
          paymentMethod: initial.paymentMethod || 'Cash',
          notes: initial.notes || '',
        })
      } else {
        setForm({
          amount: '',
          expenditureOnId: '',
          categoryId: '',
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
    if (!form.expenditureOnId) return setError('Please select who the expense was on.')
    if (!form.categoryId) return setError('Please choose a category.')

    setSaving(true)
    try {
      await onSave({
        amount,
        expenditureOnId: Number(form.expenditureOnId),
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
            <label>Expenditure On (required)</label>
            <select
              className="input"
              value={form.expenditureOnId}
              onChange={(e) => setForm({ ...form, expenditureOnId: e.target.value })}
            >
              <option value="">Select a person…</option>
              {expenditureOns.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
            {expenditureOns.length === 0 && (
              <small style={{ color: 'var(--text-dim)', marginTop: 6 }}>
                No people yet — add one on the “Expenditure On” page first.
              </small>
            )}
          </div>

          <div className="field">
            <label>Category</label>
            <select
              className="input"
              value={form.categoryId}
              disabled={!form.expenditureOnId}
              onChange={(e) => setForm({ ...form, categoryId: e.target.value })}
            >
              <option value="">{form.expenditureOnId ? 'Select a category…' : 'Select a person first'}</option>
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
