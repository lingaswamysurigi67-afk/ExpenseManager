import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import client from '../api/client'
import { getErrorMessage } from '../utils'
import type { Person } from '../types'

export default function People() {
  const [people, setPeople] = useState<Person[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [name, setName] = useState('')
  const [editingId, setEditingId] = useState<number | null>(null)
  const [saving, setSaving] = useState(false)

  const load = async () => {
    setLoading(true)
    try {
      const { data } = await client.get<Person[]>('/people')
      setPeople(data)
    } catch {
      setError('Failed to load people.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const resetForm = () => {
    setEditingId(null)
    setName('')
  }

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (!name.trim()) return
    setSaving(true)
    try {
      if (editingId != null) {
        await client.put(`/people/${editingId}`, { name: name.trim() })
      } else {
        await client.post('/people', { name: name.trim() })
      }
      resetForm()
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not save person.'))
    } finally {
      setSaving(false)
    }
  }

  const startEdit = (p: Person) => {
    setEditingId(p.id)
    setName(p.name)
    setError('')
  }

  return (
    <div className="fade-up">
      <div className="grid cols-2">
        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">People</h3>
          {error && <div className="error-banner">{error}</div>}
          {loading ? (
            <div className="empty"><span className="spinner" /></div>
          ) : people.length === 0 ? (
            <div className="empty">No people yet. Add someone to tag your expenses and income.</div>
          ) : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {people.map((p) => (
                <div
                  key={p.id}
                  style={{
                    display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                    padding: '12px 14px', borderRadius: 12, border: '1px solid var(--border)',
                    background: 'var(--glass)',
                  }}
                >
                  <span style={{ fontWeight: 600 }}>{p.name}</span>
                  <button className="btn ghost icon" title="Edit" onClick={() => startEdit(p)}>✎</button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">{editingId != null ? 'Rename person' : 'Add a person'}</h3>
          <form onSubmit={submit}>
            <div className="field">
              <label>Name</label>
              <input
                className="input"
                value={name}
                onChange={(e) => setName(e.target.value)}
                placeholder="e.g. Self, Wife, Kids, Parents"
                maxLength={80}
              />
            </div>
            <div style={{ display: 'flex', gap: 10 }}>
              <button className="btn" style={{ flex: 1 }} disabled={saving}>
                {saving ? <span className="spinner" /> : (editingId != null ? 'Update' : '＋ Add person')}
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
