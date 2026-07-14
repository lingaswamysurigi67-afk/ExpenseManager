import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import client from '../api/client'
import { getErrorMessage } from '../utils'
import type { ExpenditureOn as Person } from '../types'

export default function ExpenditureOn() {
  const [people, setPeople] = useState<Person[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [name, setName] = useState('')
  const [saving, setSaving] = useState(false)

  const load = async () => {
    setLoading(true)
    try {
      const { data } = await client.get<Person[]>('/expenditureon')
      setPeople(data)
    } catch {
      setError('Failed to load people.')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => { load() }, [])

  const add = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (!name.trim()) return
    setSaving(true)
    try {
      await client.post('/expenditureon', { name: name.trim() })
      setName('')
      await load()
    } catch (err) {
      setError(getErrorMessage(err, 'Could not add person.'))
    } finally {
      setSaving(false)
    }
  }

  const remove = async (id: number) => {
    if (!confirm('Delete this person?')) return
    try {
      await client.delete(`/expenditureon/${id}`)
      await load()
    } catch {
      setError('Could not delete person.')
    }
  }

  return (
    <div className="fade-up">
      <div className="grid cols-2">
        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">Expenditure On (people)</h3>
          {error && <div className="error-banner">{error}</div>}
          {loading ? (
            <div className="empty"><span className="spinner" /></div>
          ) : people.length === 0 ? (
            <div className="empty">No people yet. Add someone to tag your expenses.</div>
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
                  <button className="btn danger icon" title="Delete" onClick={() => remove(p.id)}>🗑</button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="card" style={{ padding: 22 }}>
          <h3 className="section-title">Add a person</h3>
          <form onSubmit={add}>
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
            <button className="btn" style={{ width: '100%', marginTop: 8 }} disabled={saving}>
              {saving ? <span className="spinner" /> : '＋ Add person'}
            </button>
          </form>
        </div>
      </div>
    </div>
  )
}
