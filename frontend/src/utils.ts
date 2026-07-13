import axios from 'axios'

// Small shared helpers for formatting and dates.

export const currency = (n: number | string): string =>
  new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    maximumFractionDigits: 2,
  }).format(Number(n) || 0)

export const formatDate = (iso: string): string => {
  const d = new Date(iso)
  return d.toLocaleDateString(undefined, { day: '2-digit', month: 'short', year: 'numeric' })
}

export const toInputDate = (iso?: string): string => {
  const d = iso ? new Date(iso) : new Date()
  return d.toISOString().slice(0, 10)
}

export const monthNames: string[] = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
]

export const paymentMethods: string[] = ['Cash', 'Card', 'UPI', 'Bank Transfer', 'Wallet', 'Other']

export const getErrorMessage = (err: unknown, fallback: string): string => {
  if (axios.isAxiosError(err)) {
    const data = err.response?.data as { message?: string } | undefined
    return data?.message || fallback
  }
  return fallback
}
