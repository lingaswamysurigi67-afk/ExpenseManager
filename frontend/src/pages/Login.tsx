import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../utils'

export default function Login() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ userNameOrEmail: '', password: '' })
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      await login(form.userNameOrEmail.trim(), form.password)
      navigate('/')
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to sign in. Please try again.'))
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

        <h2>Welcome back</h2>
        <p className="muted">Sign in to track your daily spending.</p>

        {error && <div className="error-banner">{error}</div>}

        <form onSubmit={submit}>
          <div className="field">
            <label>Username or Email</label>
            <input
              className="input"
              value={form.userNameOrEmail}
              onChange={(e) => setForm({ ...form, userNameOrEmail: e.target.value })}
              placeholder="you@example.com"
              autoFocus
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
              placeholder="••••••••"
              required
            />
          </div>
          <button className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading}>
            {loading ? <span className="spinner" /> : 'Sign in'}
          </button>
        </form>

        <div className="auth-switch">
          New here? <Link to="/register">Create an account</Link>
        </div>
      </div>
    </div>
  )
}
