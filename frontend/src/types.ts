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

export interface SubCategory {
  id: number
  categoryId: number
  name: string
  createdBy: string
  createdDate: string
  updatedBy: string | null
  updatedDate: string | null
}

export interface FeeType {
  id: number
  name: string
  createdBy: string
  createdDate: string
  updatedBy: string | null
  updatedDate: string | null
}

export interface SubCategoryFeeType {
  id: number
  subCategoryId: number
  feeTypeCatalogId: number
  feeType: string
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
  subCategoryId: number | null
  subCategory: string | null
  feeTypeId: number | null
  feeType: string | null
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
  subCategoryId: number | null
  feeTypeId: number | null
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
  filteredCount: number
  filteredRemaining: number
  page: number
  pageSize: number
  rows: ReceivableRow[]
}

export interface SubCategorySpendingRow {
  subCategoryId: number | null
  subCategory: string
  firstYear: number
  lastYear: number
  total: number
  count: number
  increaseAmount: number | null
  increasePercentage: number | null
  fees: FeeTypeBreakdown[]
}

export interface FeeTypeBreakdown {
  feeTypeId: number | null
  feeType: string
  total: number
  count: number
  increaseAmount: number | null
  increasePercentage: number | null
}

export interface SubCategorySpending {
  personId: number
  person: string
  grandTotal: number
  rows: SubCategorySpendingRow[]
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
  filteredCount: number
  page: number
  pageSize: number
  users: AdminUserRow[]
}

// ----- Server-side paging -----
export interface Paged<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface ExpensePage extends Paged<Expense> {
  totalAmount: number
}

export interface IncomePage extends Paged<Income> {
  totalAmount: number
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