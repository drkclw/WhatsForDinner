# Quickstart: Google Authentication & User Data Isolation

**Feature**: 004-google-auth-login  
**Date**: 2026-06-06

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (includes npm)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- Git
- A Google Cloud project with an OAuth 2.0 Client ID (see step 3)

## Initial Setup

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd WhatsForDinner
git checkout 004-google-auth-login
```

### 2. Database Migration

This feature adds Google auth fields to the `users` table and **removes all pre-existing data** (ownerless rows from before authentication was added).

```bash
cd backend/src/WhatsForDinner.Api
dotnet ef database update
```

> **Warning**: This migration deletes all existing recipes, weekly plans, and users. This is intentional — all prior data was anonymous and unowned.

### 3. Google Cloud Console Setup

1. Go to [Google Cloud Console](https://console.cloud.google.com/) → APIs & Services → Credentials
2. Create an **OAuth 2.0 Client ID** (Application type: **Web application**)
3. Add Authorized JavaScript origins:
   - `http://localhost:5173` (development)
4. Add Authorized redirect URIs: *(none needed for GIS popup flow)*
5. Copy the **Client ID** (e.g., `123456789-abc.apps.googleusercontent.com`)
   - The **Client Secret is not needed** for this flow

### 4. Backend Configuration

```bash
cd backend/src/WhatsForDinner.Api

# Google Client ID (used to verify ID tokens)
dotnet user-secrets set "Authentication:Google:ClientId" "YOUR-CLIENT-ID"

# JWT signing key — generate a strong random string (32+ chars)
dotnet user-secrets set "Authentication:Jwt:Key" "your-super-secret-key-at-least-32-chars"
```

Optional overrides in `appsettings.Development.json`:
```json
{
  "Authentication": {
    "Jwt": {
      "Issuer": "whatsfordinner-api",
      "Audience": "whatsfordinner-spa",
      "ExpiryDays": 30
    }
  }
}
```

### 5. Frontend Configuration

```bash
cd frontend
```

Create `frontend/.env.local` (not committed):
```
VITE_GOOGLE_CLIENT_ID=YOUR-CLIENT-ID
```

> The Client ID is the same value used in the backend. It is safe to include in client-side code.

### 6. Backend Setup

```bash
cd backend/src/WhatsForDinner.Api
dotnet restore
dotnet run
```

Backend runs on http://localhost:5140.

### 7. Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

Frontend runs on http://localhost:5173.

## Features to Test

### New: Login Page

1. Open http://localhost:5173 — you are redirected to `/login`
2. The login page shows a funny description of the app and a **"Sign in with Google"** button
3. Click the button — Google popup appears
4. Sign in with a Google account — you land on the **Weekly Plan** view

### New: User Data Isolation

1. Sign in as **User A** → create a recipe → sign out
2. Sign in as **User B** (different Google account) → the recipe list is empty
3. User B's recipe list and weekly plan are completely separate from User A's

### New: Sign Out

1. Click the user avatar in the top navigation bar
2. Select **Sign out** from the dropdown
3. You are returned to the login page
4. Navigating to any protected route (e.g., `/recipes`) redirects back to login

### Backward Compatible

- All recipe and weekly plan features work exactly as before — just now scoped to the signed-in user

## Running Tests

**Backend Tests:**
```bash
cd backend/tests/WhatsForDinner.Api.Tests
dotnet test
```

**Frontend Tests:**
```bash
cd frontend
npm run test        # Unit tests
npm run test:watch  # Unit tests in watch mode
npm run test:e2e    # E2E tests (requires backend running + signed-in session)
```

## API Changes

### New Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/api/auth/google` | Exchange Google ID token for session cookie |
| `GET` | `/api/auth/me` | Get current user (called on startup to restore session) |
| `POST` | `/api/auth/logout` | Clear session and sign out |

### Example: Sign In

```bash
# Step 1: Get an ID token from Google (done by the browser via GIS)
# Step 2: POST to backend
curl -X POST http://localhost:5140/api/auth/google \
  -H "Content-Type: application/json" \
  -c cookies.txt \
  -d '{"credential": "<google-id-token>"}'
```

### Example: Get Current User

```bash
curl http://localhost:5140/api/auth/me \
  -b cookies.txt
```

### Example: Sign Out

```bash
curl -X POST http://localhost:5140/api/auth/logout \
  -b cookies.txt \
  -c cookies.txt
```

### Modified: All Recipe & Weekly Plan Endpoints

All existing endpoints now require an authenticated session cookie (`wfd_session`). Unauthenticated requests return `401`.

## Validation Notes

Executed locally during implementation:

- Backend compile: `dotnet build` in `backend/src/WhatsForDinner.Api` succeeded.
- Migration generation: `dotnet ef migrations add AddGoogleAuth` generated `20260607031118_AddGoogleAuth` and updated snapshot.
- Backend tests: `dotnet test --nologo` in `backend/tests/WhatsForDinner.Api.Tests` passed.
  - Result: 52 total, 52 passed, 0 failed.
- Frontend unit tests: `npm test` in `frontend` passed.
  - Result: 32 total, 32 passed, 0 failed.
- Frontend build/type-check: `npm run build` in `frontend` passed.
- Frontend unit tests (re-run): `npm test -- --run` in `frontend` passed.
  - Result: 4 files passed, 32 tests passed, 0 failed.
- Frontend auth E2E slice: `npx playwright test tests/e2e/auth.spec.ts` in `frontend` passed.
  - Result: 3 total, 3 passed, 0 failed.
- Frontend full E2E suite: `npm run test:e2e` in `frontend` passed.
  - Result: 13 total, 13 passed, 0 failed.
  - Note: legacy specs now use shared auth-aware setup helpers so protected-route flows execute with a seeded authenticated session.
- Success-criteria E2E checks (production preview): `npx playwright test tests/e2e/success-criteria.spec.ts` with `E2E_BASE_URL=http://localhost:4173` passed.
  - Result: 3 total, 3 passed, 0 failed.
  - SC-001 sign-in completion time: `72 ms` (threshold: `< 60000 ms`).
  - SC-004 returning-user persistence after re-authentication: `PASS`.
  - SC-005 login-page FCP under simulated 3G: `1272 ms` (threshold: `<= 2000 ms`).

Not executed yet:
- None.
