import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../utils'

export default function Register() {
  const { register } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ userName: '', email: '', password: '', confirm: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    if (form.password !== form.confirm) {
      setError('Passwords do not match.')
      return
    }
    if (form.password.length < 6) {
      setError('Password must be at least 6 characters.')
      return
    }
    setLoading(true)
    try {
      await register(form.userName.trim(), form.email.trim(), form.password)
      navigate('/')
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to create account. Please try again.'))
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-wrap">
      <div className="card auth-card fade-up">
        <div className="auth-brand">
          <div className="logo" style={{ width: 44, height: 44, borderRadius: 12, display: 'grid', placeItems: 'center', fontSize: 22, background: 'linear-gradient(135deg, var(--primary), var(--accent))' }}>💰</div>
          <div style={{ textAlign: 'left' }}>
            <strong style={{ color: 'var(--text)', fontSize: 20 }}>ExpenseFlow</strong>
            <div style={{ color: 'var(--text-dim)', fontSize: 13 }}>Daily expense manager</div>
          </div>
        </div>

        <h2>Create your account</h2>
        <p className="muted">Start managing your daily expenses in seconds.</p>

        {error && <div className="error-banner">{error}</div>}

        <form onSubmit={submit}>
          <div className="field">
            <label>Username</label>
            <input
              className="input"
              value={form.userName}
              onChange={(e) => setForm({ ...form, userName: e.target.value })}
              placeholder="janedoe"
              autoFocus
              required
              minLength={3}
            />
          </div>
          <div className="field">
            <label>Email</label>
            <input
              className="input"
              type="email"
              value={form.email}
              onChange={(e) => setForm({ ...form, email: e.target.value })}
              placeholder="you@example.com"
              required
            />
          </div>
          <div className="field">
            <label>Password</label>
            <input
              className="input"
              type="password"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              placeholder="At least 6 characters"
              required
            />
          </div>
          <div className="field">
            <label>Confirm password</label>
            <input
              className="input"
              type="password"
              value={form.confirm}
              onChange={(e) => setForm({ ...form, confirm: e.target.value })}
              placeholder="Repeat password"
              required
            />
          </div>
          <button className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading}>
            {loading ? <span className="spinner" /> : 'Create account'}
          </button>
        </form>

        <div className="auth-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </div>
      </div>
    </div>
  )
}
