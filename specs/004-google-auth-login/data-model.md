# Data Model: Google Authentication & User Data Isolation

**Feature**: 004-google-auth-login  
**Date**: 2026-06-06

---

## Entity Changes

### User (modified)

The existing `User` entity is extended with Google auth fields. The integer surrogate PK is preserved; all existing `UserId` foreign keys on `Recipe` and `WeeklyPlan` require no changes.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | `int` | PK, auto-increment | Internal surrogate key (unchanged) |
| `google_id` | `varchar(255)` | NOT NULL, UNIQUE | Google `sub` claim — immutable lookup key |
| `email` | `varchar(254)` | NOT NULL | Display-only; updated on each sign-in |
| `display_name` | `varchar(200)` | NOT NULL | User's full name from Google profile |
| `picture_url` | `varchar(2048)` | NULL | Profile picture URL from Google |
| `created_at` | `timestamptz` | NOT NULL, default NOW() | First sign-in timestamp (unchanged) |
| `last_login_at` | `timestamptz` | NOT NULL, default NOW() | Updated on every successful sign-in |

**Removed**: `name varchar(100)` — replaced by `display_name`  
**Removed**: seed data row (Id=1, "Default User")

**Indexes**:
- `PK_users` on `id` (existing, unchanged)
- `UX_users_google_id` on `google_id` (new, unique) — used for O(log n) user lookup on every auth request

---

### Recipe (no structural change)

`user_id int NOT NULL REFERENCES users(id) ON DELETE CASCADE` — already present, no migration needed.  
**Behavioral change**: All queries now filter by the authenticated user's `id`. No anonymous/shared access.

---

### WeeklyPlan (no structural change)

`user_id int NOT NULL REFERENCES users(id) ON DELETE CASCADE` — already present.  
**Behavioral change**: Queries scoped to authenticated user's `id`.

---

### WeeklyPlanItem (no structural change)

Owned by `WeeklyPlan` which is owned by `User`. Isolation is transitive.

---

## Migration Strategy

### Migration: `AddGoogleAuth`

**Step 1 — Delete ownerless data** (confirmed in clarifications: all pre-existing data is deleted):
```sql
DELETE FROM weekly_plan_items;
DELETE FROM weekly_plans;
DELETE FROM recipes;
DELETE FROM users;
```

**Step 2 — Alter `users` table**:
```sql
ALTER TABLE users DROP COLUMN name;
ALTER TABLE users ADD COLUMN google_id    VARCHAR(255) NOT NULL;
ALTER TABLE users ADD COLUMN email        VARCHAR(254) NOT NULL;
ALTER TABLE users ADD COLUMN display_name VARCHAR(200) NOT NULL;
ALTER TABLE users ADD COLUMN picture_url  VARCHAR(2048);
ALTER TABLE users ADD COLUMN last_login_at TIMESTAMPTZ NOT NULL DEFAULT NOW();
CREATE UNIQUE INDEX UX_users_google_id ON users(google_id);
```

**Step 3 — Remove EF Core seed data** for the "Default User" (handled in `UserConfiguration.cs`).

---

## State Transitions

### User Authentication Lifecycle

```
[First visit — unauthenticated]
        │
        ▼ GET /api/auth/me → 401
        │
[Login page displayed]
        │
        ▼ User clicks "Sign in with Google" → Google popup → ID token
        │
[POST /api/auth/google { credential }]
        │
        ├── User.google_id EXISTS → update email/displayName/pictureUrl/lastLoginAt
        │       ↓
        └── User.google_id NOT EXISTS → INSERT new User row
                ↓
[Issue session cookie (JWT, httpOnly, 30-day sliding)]
        │
        ▼
[Authenticated — all API requests carry cookie]
        │
        ▼ POST /api/auth/logout
        │
[Cookie cleared; session ended]
        │
        ▼ GET /api/auth/me → 401
        │
[Login page displayed]
```

---

## Configuration

### Backend (`appsettings.json` additions)

```json
"Authentication": {
  "Google": {
    "ClientId": "<your-google-client-id>"
  },
  "Jwt": {
    "Key": "<32+ char randomly generated secret — use user-secrets in dev>",
    "Issuer": "whatsfordinner-api",
    "Audience": "whatsfordinner-spa",
    "ExpiryDays": 30
  }
}
```

Secret values (`Jwt:Key`, `Authentication:Google:ClientId`) are stored via `dotnet user-secrets` in development and environment variables / secret manager in production.

### Frontend (`.env` / `.env.local`)

```
VITE_GOOGLE_CLIENT_ID=123456789-abc.apps.googleusercontent.com
```

The `VITE_` prefix is required for Vite to expose the value to the browser bundle. The Client ID is not a secret.
