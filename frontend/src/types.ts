export interface AuthUser {
  userName: string
  email: string
}

export interface AuthResponse {
  token: string
  userName: string
  email: string
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

export interface ExpenditureOn {
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
  expenditureOnId: number | null
  expenditureOn: string
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
  expenditureOnId: number
  date: string
  paymentMethod: string
  notes: string
}

export interface IncomePayload {
  amount: number
  categoryId: number
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
