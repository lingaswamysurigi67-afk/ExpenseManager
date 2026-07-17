import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { getErrorMessage } from '../utils'

export default function ForgotPassword() {
  const { requestPasswordReset } = useAuth()
  const [userNameOrEmail, setUserNameOrEmail] = useState('')
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [loading, setLoading] = useState(false)

  const submit = async (e: FormEvent) => {
    e.preventDefault()
    setError('')
    setSuccess('')
    setLoading(true)
    try {
      const message = await requestPasswordReset(userNameOrEmail.trim())
      setSuccess(message || 'If an account matches, a password reset link has been sent to its email.')
    } catch (err) {
      setError(getErrorMessage(err, 'Unable to send reset link. Please try again.'))
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
        <p className="muted">Enter your username or email and we'll send a reset link to your email.</p>

        {error && <div className="error-banner">{error}</div>}
        {success && <div className="success-banner">{success}</div>}

        <form onSubmit={submit}>
          <div className="field">
            <label>Username or Email</label>
            <input
              className="input"
              value={userNameOrEmail}
              onChange={(e) => setUserNameOrEmail(e.target.value)}
              placeholder="you@example.com"
              autoFocus
              required
            />
          </div>
          <button className="btn" style={{ width: '100%', marginTop: 6 }} disabled={loading}>
            {loading ? <span className="spinner" /> : 'Send reset link'}
          </button>
        </form>

        <div className="auth-switch">
          Remembered it? <Link to="/login">Back to sign in</Link>
        </div>
      </div>
    </div>
  )
}
