import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../utils'

export default function ForgotPassword() {
  const { resetPassword } = useAuth()
  const navigate = useNavigate()
  const [form, setForm] = useState({ userName: '', email: '', password: '', confirm: '' })
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess('')
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
      const message = await resetPassword(form.userName.trim(), form.email.trim(), form.password)
      setSuccess(message || 'Password has been reset. Redirecting to sign in…')
      setTimeout(() => navigate('/login'), 1500)
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to reset password. Please try again.'))
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

        <h2>Reset your password</h2>
        <p className="muted">Confirm your username and email, then choose a new password.</p>

        {error && <div className="error-banner">{error}</div>}
        {success && <div className="success-banner">{success}</div>}

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
            <label>New password</label>
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
            <label>Confirm new password</label>
            <input
              className="input"
              type="password"
              value={form.confirm}
              onChange={(e) => setForm({ ...form, confirm: e.target.value })}
              placeholder="Repeat new password"
              required
            />
          </div>
          <button className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading}>
            {loading ? <span className="spinner" /> : 'Reset password'}
          </button>
        </form>

        <div className="auth-switch">
          Remembered it? <Link to="/login">Back to sign in</Link>
        </div>
      </div>
    </div>
  )
}
