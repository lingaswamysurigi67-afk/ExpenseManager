import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../utils'

export default function ResetPassword() {
  const { resetPassword } = useAuth()
  const navigate = useNavigate()
  const [params] = useSearchParams()
  const token = params.get('token') ?? ''

  const [form, setForm] = useState({ password: '', confirm: '' })
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    if (!token) {
      setError('This reset link is invalid. Please request a new one.')
      return
    }
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
      const message = await resetPassword(token, form.password)
      setSuccess(message || 'Password has been reset. Redirecting to sign in…')
      setTimeout(() => navigate('/login'), 1500)
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to reset password. Please request a new link.'))
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

        <h2>Choose a new password</h2>
        <p className="muted">Enter a new password for your account.</p>

        {!token && <div className="error-banner">Missing or invalid reset link. Please request a new one.</div>}
        {error && <div className="error-banner">{error}</div>}
        {success && <div className="success-banner">{success}</div>}

        <form onSubmit={submit}>
          <div className="field">
            <label>New password</label>
            <input
              className="input"
              type="password"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              placeholder="At least 6 characters"
              autoFocus
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
          <button className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading || !token}>
            {loading ? <span className="spinner" /> : 'Reset password'}
          </button>
        </form>

        <div className="auth-switch">
          Need a new link? <Link to="/forgot-password">Request reset</Link>
        </div>
      </div>
    </div>
  )
}
