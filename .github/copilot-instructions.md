# Copilot instructions — ExpenseManager

Follow these repo conventions for **every** code change. When adding a feature, mirror the existing patterns rather than introducing new ones.

## Non-negotiable rules for EVERY edit
Apply these on every single request, without being reminded:

1. **No data loss, at any cost.** Never do anything that can drop, delete, or corrupt database data. No hard deletes (soft-delete only via `IsActive = false`). Never add/apply EF migrations or run `dotnet ef` / schema changes without explicit user approval. Prefer additive, reversible changes; preserve existing data and structure.
2. **Mobile + desktop compatible.** Every UI change must work on both. Wrap tables in `.table-scroll`, use responsive classes (`grid stats` / `grid stats cols-3`), never hardcode inline `gridTemplateColumns` (it breaks the media queries). Verify layout holds at desktop, tablet (≤900px), and phone (≤560px).
3. **Consistent coding standards across all layers (frontend, backend, database).** Follow the conventions in this file for whichever layer you touch, and keep them consistent with the existing codebase. These standards are the baseline — if they evolve, follow the latest version in this file for every future request. When in doubt, mirror existing patterns rather than inventing new ones.

## Architecture
- **Backend**: ASP.NET Core Web API (`backend/ExpenseManager.Api`), EF Core, controller-based.
- **Frontend**: React + TypeScript + Vite (`frontend`), React Router, Axios.
- Money-owed model: `Expense` and `Income` both carry an optional `PersonId`. Expense = money given; Income = payback.

## Backend conventions
- Controllers: `[ApiController]`, `[Authorize]`, `[Route("api/[controller]")]` at class level.
- Get the current user via `private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;`.
- Always scope queries by `UserId`.
- Use `async`/`await` with EF (`await ...ToListAsync()`), return `IActionResult` (`Ok`, `NotFound`, `BadRequest`, `Conflict`).
- Money is `decimal`. Timestamps are `DateTime.UtcNow`.
- **Soft delete**: `Expense`, `Income`, `Person`, `Category` have a global `HasQueryFilter(x => x.IsActive)` in `AppDbContext`. Do NOT add `IsActive` filters by hand in queries — the global filter handles it. Deletes set `IsActive = false`, never hard-delete.
- Audit fields on writes: set `CreatedBy`/`CreatedDate` on create, `UpdatedBy`/`UpdatedDate` on update.
- DTOs live in `Dtos/`; response DTOs use `...Response`, breakdown/row DTOs use descriptive names (e.g. `CategoryBreakdown`, `ReceivableRow`). Return DTOs, not entities, where existing controllers do.
- **Never** add/apply EF migrations or run `dotnet ef` without explicit user approval. Preserve data.

## Frontend conventions
- Types go in `src/types.ts`; API calls use the shared `src/api/client.ts` (`client.get('/path', { params })`).
- New page → add to `src/pages/`, register the route in `src/App.tsx`, and add a nav link in `src/components/Layout.tsx`.
- Reuse shared building blocks: `SortHeader`, `Pagination`, `ConfirmDialog`, and helpers from `src/utils.ts` (`currency`, `monthNames`, `formatDate`, `getErrorMessage`).
- Page shell classes: `fade-up`, `card`, `topbar`, `filters`, `table-scroll`, `grid stats`.
- List pages follow the Expenses/Income pattern: filters (year/month/search) + client-side sort + pagination.
- `useEffect` that calls a `load()` depending on filters uses `// eslint-disable-next-line react-hooks/exhaustive-deps`.

## Responsiveness (mobile + desktop)
- Tables must be wrapped in `.table-scroll` for horizontal scroll on mobile.
- Stat card rows: use `className="grid stats cols-3"` (or the default 4-up `.stats`). Do NOT hardcode `gridTemplateColumns` inline — that overrides the responsive media queries. `.stats` variants collapse at 900px / 560px in `src/index.css`.

## Formatting / currency
- Currency is INR, formatted via `currency()` in `src/utils.ts`.

## General
- Make additive, reversible changes. Match existing style. Do not over-engineer or add unrequested features.
- After changes, verify: backend `dotnet build`, frontend `npx tsc --noEmit -p tsconfig.app.json`.
