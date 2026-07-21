export interface AuthUser {
  userName: string
  email: string
  isAdmin: boolean
}

export interface AuthResponse {
  token: string
  userName: string
  email: string
  isAdmin: boolean
  expiresAt: string
}

export interface Category {
  id: number
  name: string
  color: string
  isDefault: boolean
  createdBy: string
  createdDate: string
  updatedBy: string | null
  updatedDate: string | null
}

export interface Person {
  id: number
  userId: string
  name: string
  createdBy: string
  createdDate: string
  updatedBy: string | null
  updatedDate: string | null
}

export interface Expense {
  id: number
  userId: string
  amount: number
  categoryId: number
  category: string
  personId: number | null
  date: string
  paymentMethod: string
  notes: string
  createdBy: string
  createdDate: string
  updatedBy: string | null
  updatedDate: string | null
}

export interface Income {
  id: number
  userId: string
  amount: number
  categoryId: number
  category: string
  personId: number | null
  source: string
  date: string
  paymentMethod: string
  notes: string
  createdBy: string
  createdDate: string
  updatedBy: string | null
  updatedDate: string | null
}

export interface ExpensePayload {
  amount: number
  categoryId: number
  personId: number
  date: string
  paymentMethod: string
  notes: string
}

export interface IncomePayload {
  amount: number
  categoryId: number
  personId: number
  source: string
  date: string
  paymentMethod: string
  notes: string
}

export interface CategoryBreakdown {
  categoryId: number
  category: string
  color: string
  total: number
  count: number
  percentage: number
}

export interface MonthlyBreakdown {
  year: number
  month: number
  label: string
  total: number
  count: number
}

export interface Summary {
  total: number
  count: number
  average: number
  thisMonthTotal: number
  todayTotal: number
  byCategory: CategoryBreakdown[]
  byMonth: MonthlyBreakdown[]
}

export interface ReceivableRow {
  personId: number
  person: string
  given: number
  returned: number
  remaining: number
}
export interface Receivables {
  totalGiven: number
  totalReturned: number
  totalRemaining: number
  rows: ReceivableRow[]
}

export interface AdminUserRow {
  id: string
  userName: string
  email: string
  createdDate: string
  updatedDate: string | null
}

export interface AdminUsersResponse {
  totalUsers: number
  users: AdminUserRow[]
}

// ----- Bulk import -----
export interface ImportRowPreview {
  rowNumber: number
  amount: number | null
  category: string
  person: string
  source: string
  date: string | null
  dateText: string
  paymentMethod: string
  notes: string
  valid: boolean
  error: string | null
}

export interface ImportPreviewResponse {
  total: number
  validCount: number
  invalidCount: number
  rows: ImportRowPreview[]
}

export interface ImportCommitRow {
  amount: number
  category: string
  person: string
  source: string
  date: string
  paymentMethod: string
  notes: string
}

export interface ImportRowError {
  rowNumber: number
  message: string
}

export interface ImportCommitResponse {
  imported: number
  failed: number
  errors: ImportRowError[]
}