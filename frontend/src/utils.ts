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
  // Use local date parts (not toISOString, which shifts to UTC and can roll the day back).
  const year = d.getFullYear()
  const month = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
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
