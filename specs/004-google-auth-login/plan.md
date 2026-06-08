# Implementation Plan: Google Authentication & User Data Isolation

**Branch**: `004-google-auth-login` | **Date**: 2026-06-06 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-google-auth-login/spec.md`

## Summary

Add Google OAuth 2.0 sign-in to WhatsForDinner so each user has a private, isolated set of recipes and weekly plans. The frontend uses Google Identity Services (GIS) to obtain an ID token; the backend verifies it with `Google.Apis.Auth`, upserts the `User` record, and issues a 30-day rolling session via an httpOnly JWT cookie. All existing recipe and weekly plan endpoints are protected by the session cookie and scoped to the authenticated user. A new `LoginView` with a funny app description and the Google sign-in button guards unauthenticated access.

## Technical Context

**Language/Version**: C# / .NET 10 (backend) · TypeScript / Node 20+ (frontend)  
**Primary Dependencies**: ASP.NET Core 10 (controllers) · EF Core 10 + Npgsql · `Google.Apis.Auth` v1.68.0 · `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0 · Vue 3.5 · Pinia 2 · Vue Router 4 · Vite 5 · Vitest · Playwright  
**Storage**: PostgreSQL 15 via EF Core  
**Testing**: xUnit + FluentAssertions + `Microsoft.AspNetCore.Mvc.Testing` (backend) · Vitest (frontend unit) · Playwright (E2E)  
**Target Platform**: Web — browser SPA + Linux/Windows server  
**Project Type**: Web application (separate frontend SPA + REST API backend)  
**Performance Goals**: `GET /api/auth/me` ≤ 100 ms p95 (simple cookie validation); all other API endpoints ≤ 500 ms (existing requirement)  
**Constraints**: HttpOnly cookies require `AllowCredentials()` in CORS (already uses `WithOrigins`); production requires HTTPS for `Secure` cookie flag  
**Scale/Scope**: Personal-use app; single-user sessions; no concurrency constraints beyond existing

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **Code Quality — Single Responsibility** | ✓ | New `AuthController`, `AuthService`, `authStore` each have one purpose |
| **Code Quality — Type Safety** | ✓ | Nullable enabled in C#; strict TypeScript; GIS type declarations in `google-gsi.d.ts` |
| **Code Quality — Error Handling** | ✓ | Invalid/expired tokens throw and are caught; 401 returned — no silent failures |
| **Testing Standards — Coverage** | ✓ | Unit tests for `AuthService`; integration tests for `AuthController`; Vitest for auth store; E2E for login flow |
| **Testing Standards — Test Pyramid** | ✓ | Unit → integration → E2E pyramid followed |
| **UX Consistency — Loading States** | ✓ | Login button shows spinner during token exchange |
| **UX Consistency — Error Feedback** | ✓ | Auth failure shown as user-friendly message on login page |
| **UX Consistency — Accessibility** | ✓ | Google's rendered button is accessible; nav dropdown meets WCAG 2.1 AA |
| **Performance — API Response** | ✓ | Cookie validation is O(1) JWT verify; no DB call needed per request beyond first |
| **Performance — No N+1 Queries** | ✓ | User lookup on auth is a single indexed query by `google_id` |
| **Security — httpOnly Cookie** | ✓ | Session token never exposed to JavaScript |
| **Security — CORS + Credentials** | ✓ | `AllowCredentials()` added; `AllowAnyOrigin()` must NOT be used (already uses `WithOrigins`) |

**No gate violations.**

## Project Structure

### Documentation (this feature)

```text
specs/004-google-auth-login/
├── plan.md              ← This file
├── research.md          ← Phase 0 output
├── data-model.md        ← Phase 1 output
├── quickstart.md        ← Phase 1 output
├── contracts/
│   ├── openapi.yaml     ← Phase 1 output
│   └── README.md        ← Phase 1 output
└── tasks.md             ← Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── WhatsForDinner.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs          ← NEW: /api/auth/google, /me, /logout
│       │   ├── RecipesController.cs       ← MODIFY: add [Authorize], resolve userId from claims
│       │   └── WeeklyPlanController.cs    ← MODIFY: add [Authorize], resolve userId from claims
│       ├── Data/
│       │   └── Configurations/
│       │       └── UserConfiguration.cs   ← MODIFY: add google_id/email/etc, remove seed data
│       ├── Middleware/                     ← no changes
│       ├── Migrations/
│       │   └── 20260606XXXXXX_AddGoogleAuth.cs  ← NEW
│       ├── Models/
│       │   ├── User.cs                    ← MODIFY: add GoogleId, Email, DisplayName, PictureUrl, LastLoginAt
│       │   └── Dtos/
│       │       └── AuthDtos.cs            ← NEW: GoogleSignInRequest, AuthUserDto
│       ├── Services/
│       │   ├── AuthService.cs             ← NEW: token verification + user upsert
│       │   ├── IAuthService.cs            ← NEW
│       │   ├── IRecipeService.cs          ← MODIFY: remove userId defaults
│       │   └── IWeeklyPlanService.cs      ← MODIFY: remove userId defaults
│       ├── appsettings.json               ← MODIFY: add Authentication section schema
│       └── Program.cs                     ← MODIFY: add JWT auth + cookie middleware
└── tests/
    └── WhatsForDinner.Api.Tests/
        ├── Unit/
        │   └── Services/
        │       └── AuthServiceTests.cs    ← NEW
        └── Integration/
            ├── AuthControllerTests.cs     ← NEW
            ├── RecipesControllerTests.cs  ← MODIFY: add auth context to requests
            └── WeeklyPlanControllerTests.cs ← MODIFY: add auth context to requests

frontend/
├── index.html                             ← MODIFY: add GIS script tag
├── src/
│   ├── App.vue                            ← MODIFY: add user avatar + sign-out dropdown to nav
│   ├── router/
│   │   └── index.ts                       ← MODIFY: add /login route + beforeEach auth guard
│   ├── stores/
│   │   └── authStore.ts                   ← NEW: user, sessionChecked, login, logout, restoreSession
│   ├── services/
│   │   └── apiClient.ts                   ← MODIFY: ensure withCredentials: true
│   ├── types/
│   │   └── google-gsi.d.ts                ← NEW: TypeScript declarations for window.google
│   └── views/
│       └── LoginView.vue                  ← NEW: funny description + Google sign-in button
└── tests/
    ├── unit/
    │   └── stores/
    │       └── authStore.spec.ts           ← NEW
    └── e2e/
        └── auth.spec.ts                    ← NEW: login → weekly plan → sign out flow
```

**Structure Decision**: Option 2 (web application with separate backend/ and frontend/) — matches existing project layout. No new projects added; this feature touches existing projects only.

## Complexity Tracking

No constitution violations to justify. All patterns (JWT cookie auth, route guards, service-layer auth) are standard for this project type.
