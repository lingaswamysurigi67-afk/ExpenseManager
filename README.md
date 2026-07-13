# 💰 ExpenseFlow — Daily Expense Manager

A personal daily expense manager for a single individual. **React** frontend + **ASP.NET Core (.NET 9) Web API** backend, with **JSON file** storage and **JWT** register/login.

## Features
- Register / login (JWT auth, passwords hashed with ASP.NET Core `PasswordHasher`)
- Add / edit / delete daily expenses (amount, category, date, payment method, notes)
- Categories (8 built-in defaults + create/delete your own, with colors)
- Dashboard with total / this-month / today / average cards + category donut chart
- Reports: monthly trend bar chart + category breakdown
- Filter expenses by year / month / category
- Fancy dark glassmorphism UI

## Project structure
```
ExpenseManager/
├─ backend/ExpenseManager.Api/   # ASP.NET Core Web API (.NET 9)
│  ├─ Controllers/               # Auth, Expenses, Categories, Reports
│  ├─ Models/  Dtos/  Services/  # JSON file store, JWT token service
│  └─ Data/                      # users.json, expenses.json, categories.json (auto-created)
└─ frontend/                     # React + Vite app
   └─ src/                       # pages, components, context, api client
```

## Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org)

## Run the backend
```powershell
cd backend/ExpenseManager.Api
dotnet run --launch-profile http
```
API runs at **http://localhost:5015**. Data files are created under `Data/` on first run.

## Run the frontend
In a second terminal:
```powershell
cd frontend
npm install      # first time only
npm run dev
```
App runs at **http://localhost:5173** and proxies `/api` calls to the backend.

Open http://localhost:5173, create an account, and start tracking expenses.

## Configuration
Backend settings live in `backend/ExpenseManager.Api/appsettings.json`:
- `Jwt.Key` — **change this secret** before any real deployment
- `DataDirectory` — where the JSON files are stored (default `Data`)
- `AllowedOrigins` — CORS origins for the frontend

## Notes
- Storage is JSON files (per the chosen setup) with thread-safe reads/writes — great for a single user, not intended for high concurrency.
- Each user only sees their own expenses and categories.
