import { useEffect, useRef, useState } from 'react'
import client from '../api/client'
import { currency, formatDate, getErrorMessage } from '../utils'
import type {
  Category,
  Person,
  ImportPreviewResponse,
  ImportCommitResponse,
  ImportCommitRow,
} from '../types'

interface ImportModalProps {
  open: boolean
  kind: 'expense' | 'income'
  onClose: () => void
  onImported: () => void
  categories: Category[]
  people: Person[]
}

type Step = 'select' | 'preview' | 'result'

const csvCell = (v: string) => (/[",\n]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v)

export default function ImportModal({ open, kind, onClose, onImported, categories, people }: ImportModalProps) {
  const base = kind === 'expense' ? '/expenses' : '/incomes'
  const label = kind === 'expense' ? 'expenses' : 'income'

  const [step, setStep] = useState<Step>('select')
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [fileName, setFileName] = useState('')
  const [preview, setPreview] = useState<ImportPreviewResponse | null>(null)
  const [result, setResult] = useState<ImportCommitResponse | null>(null)
  const inputRef = useRef<HTMLInputElement>(null)

  useEffect(() => {
    if (open) {
      setStep('select')
      setLoading(false)
      setError('')
      setFileName('')
      setPreview(null)
      setResult(null)
      if (inputRef.current) inputRef.current.value = ''
    }
  }, [open])

  if (!open) return null

  const headers =
    kind === 'income'
      ? ['Date', 'Amount', 'Category', 'Person', 'Source', 'Payment Method', 'Notes']
      : ['Date', 'Amount', 'Category', 'Person', 'Payment Method', 'Notes']

  const downloadTemplate = () => {
    const cat = categories[0]?.name ?? 'Other'
    const person = people[0]?.name ?? 'John Doe'
    const today = new Date().toISOString().slice(0, 10)
    const sample =
      kind === 'income'
        ? [today, '5000', cat, person, 'Salary', 'Bank Transfer', 'Example row']
        : [today, '1200.50', cat, person, 'UPI', 'Example row']
    const csv = [headers.map(csvCell).join(','), sample.map(csvCell).join(',')].join('\r\n')
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${label}-import-template.csv`
    a.click()
    URL.revokeObjectURL(url)
  }

  const handleFile = async (file: File) => {
    setError('')
    setFileName(file.name)
    setLoading(true)
    try {
      const form = new FormData()
      form.append('file', file)
      const res = await client.post<ImportPreviewResponse>(`${base}/import/preview`, form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      })
      setPreview(res.data)
      setStep('preview')
    } catch (err) {
      setError(getErrorMessage(err, 'Could not read the file.'))
    } finally {
      setLoading(false)
    }
  }

  const doImport = async () => {
    if (!preview) return
    const rows: ImportCommitRow[] = preview.rows
      .filter((r) => r.valid && r.amount != null && r.date != null)
      .map((r) => ({
        amount: r.amount as number,
        category: r.category,
        person: r.person,
        source: r.source,
        date: r.date as string,
        paymentMethod: r.paymentMethod,
        notes: r.notes,
      }))
    if (rows.length === 0) return
    setLoading(true)
    setError('')
    try {
      const res = await client.post<ImportCommitResponse>(`${base}/import`, { rows })
      setResult(res.data)
      setStep('result')
      onImported()
    } catch (err) {
      setError(getErrorMessage(err, 'Import failed.'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <div className="card modal import-modal fade-up" onMouseDown={(e) => e.stopPropagation()}>
        <h3 className="section-title">Import {label}</h3>
        {error && <div className="error-banner">{error}</div>}

        {step === 'select' && (
          <div>
            <p style={{ color: 'var(--text-dim)', marginTop: 0, fontSize: 15 }}>
              Upload a <strong>.csv</strong> or <strong>.xlsx</strong> file. The first row must be the column
              headers: <code>{headers.join(', ')}</code>. Category and Person must already exist — rows with unknown
              names are reported and skipped.
            </p>

            <div className="import-drop">
              <input
                ref={inputRef}
                type="file"
                accept=".csv,.xlsx"
                onChange={(e) => {
                  const f = e.target.files?.[0]
                  if (f) handleFile(f)
                }}
                disabled={loading}
              />
              {loading && <span className="spinner" style={{ borderTopColor: 'var(--primary)' }} />}
            </div>
            {fileName && !loading && <p style={{ color: 'var(--text-dim)', fontSize: 14 }}>Selected: {fileName}</p>}

            <div style={{ display: 'flex', gap: 10, justifyContent: 'space-between', marginTop: 16, flexWrap: 'wrap' }}>
              <button type="button" className="btn ghost sm" onClick={downloadTemplate}>
                ⬇ Download CSV template
              </button>
              <button type="button" className="btn ghost" onClick={onClose}>
                Close
              </button>
            </div>
          </div>
        )}

        {step === 'preview' && preview && (
          <div>
            <div className="grid stats cols-3" style={{ marginBottom: 16 }}>
              <div className="card stat">
                <div className="label">Total rows</div>
                <div className="value">{preview.total}</div>
              </div>
              <div className="card stat">
                <div className="label">Ready to import</div>
                <div className="value" style={{ color: 'var(--success)' }}>{preview.validCount}</div>
              </div>
              <div className="card stat">
                <div className="label">Will be skipped</div>
                <div className="value" style={{ color: '#c62f45' }}>{preview.invalidCount}</div>
              </div>
            </div>

            <div className="table-scroll">
              <table>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Status</th>
                    <th>Date</th>
                    <th style={{ textAlign: 'right' }}>Amount</th>
                    <th>Category</th>
                    <th>Person</th>
                    {kind === 'income' && <th>Source</th>}
                    <th>Method</th>
                    <th>Issue</th>
                  </tr>
                </thead>
                <tbody>
                  {preview.rows.map((r) => (
                    <tr key={r.rowNumber} className={r.valid ? '' : 'row-invalid'}>
                      <td>{r.rowNumber}</td>
                      <td>
                        <span className={`import-badge ${r.valid ? 'ok' : 'bad'}`}>
                          {r.valid ? '✓ OK' : '✕ Skip'}
                        </span>
                      </td>
                      <td>{r.date ? formatDate(r.date) : r.dateText || '—'}</td>
                      <td style={{ textAlign: 'right' }} className="amount">
                        {r.amount != null ? currency(r.amount) : '—'}
                      </td>
                      <td>{r.category || '—'}</td>
                      <td>{r.person || '—'}</td>
                      {kind === 'income' && <td>{r.source || '—'}</td>}
                      <td style={{ color: 'var(--text-dim)' }}>{r.paymentMethod}</td>
                      <td style={{ color: r.valid ? 'var(--text-dim)' : '#c62f45', maxWidth: 220 }}>
                        {r.error || ''}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 16, flexWrap: 'wrap' }}>
              <button type="button" className="btn ghost" onClick={() => setStep('select')} disabled={loading}>
                Back
              </button>
              <button type="button" className="btn" onClick={doImport} disabled={loading || preview.validCount === 0}>
                {loading ? <span className="spinner" /> : `Import ${preview.validCount} row${preview.validCount !== 1 ? 's' : ''}`}
              </button>
            </div>
          </div>
        )}

        {step === 'result' && result && (
          <div>
            <div className="success-banner">
              Imported {result.imported} {label} record{result.imported !== 1 ? 's' : ''}.
              {result.failed > 0 && ` ${result.failed} row${result.failed !== 1 ? 's' : ''} were skipped.`}
            </div>

            {result.errors.length > 0 && (
              <div className="table-scroll" style={{ maxHeight: '40vh' }}>
                <table>
                  <thead>
                    <tr>
                      <th>Row</th>
                      <th>Issue</th>
                    </tr>
                  </thead>
                  <tbody>
                    {result.errors.map((e) => (
                      <tr key={e.rowNumber}>
                        <td>{e.rowNumber}</td>
                        <td style={{ color: '#c62f45' }}>{e.message}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}

            <div style={{ display: 'flex', gap: 10, justifyContent: 'flex-end', marginTop: 16 }}>
              <button type="button" className="btn" onClick={onClose}>
                Done
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
