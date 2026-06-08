# API Contracts: Google Authentication & User Data Isolation

**Feature**: 004-google-auth-login

## New Endpoints

| Method | Path | Auth Required | Description |
|--------|------|---------------|-------------|
| `POST` | `/api/auth/google` | No | Exchange Google ID token for session cookie |
| `GET` | `/api/auth/me` | Yes (cookie) | Get current user profile; resets rolling expiry |
| `POST` | `/api/auth/logout` | Yes (cookie) | Clear session cookie |

## Modified Endpoints (behavior change)

All existing endpoints now require the `wfd_session` cookie (401 if absent). Data returned is scoped to the authenticated user — no cross-user data is ever returned.

| Method | Path | Change |
|--------|------|--------|
| `GET` | `/api/recipes` | Now returns only the authenticated user's recipes |
| `GET` | `/api/recipes/{id}` | Returns 404 if recipe belongs to a different user |
| `POST` | `/api/recipes` | Recipe is associated with authenticated user |
| `PUT` | `/api/recipes/{id}` | Returns 404 if recipe belongs to a different user |
| `DELETE` | `/api/recipes/{id}` | Returns 404 if recipe belongs to a different user |
| `POST` | `/api/recipes/extract-from-image` | Auth required; returns extracted fields |
| `GET` | `/api/weekly-plan` | Returns only the authenticated user's weekly plan |
| `POST` | `/api/weekly-plan/items` | Adds recipe to authenticated user's weekly plan |
| `DELETE` | `/api/weekly-plan/items/{id}` | Removes item from authenticated user's weekly plan |

## Session Cookie Specification

- **Name**: `wfd_session`
- **HttpOnly**: Yes (not readable by JavaScript)
- **Secure**: Yes (HTTPS only in production; relaxed in local dev)
- **SameSite**: Lax
- **Max-Age**: 2,592,000 seconds (30 days), reset on every authenticated request (sliding)
- **Content**: Signed JWT containing `{ sub: <userId>, iat, exp }`

## Authentication Flow Summary

```
Frontend                          Backend                    Google
   │                                  │                         │
   │──── renderButton() ──────────────┼─────────────────────────►│
   │◄─── ID token (JWT) ─────────────┼─────────────────────────┤│
   │                                  │                         │
   │──── POST /api/auth/google ───────►│                         │
   │      { credential: "<token>" }   │──── ValidateAsync() ────►│
   │                                  │◄─── Payload ────────────┤│
   │                                  │  (sub, email, name, pic)│
   │                                  │                         │
   │                                  │── UPSERT users ─────────┤
   │◄─── 200 { id, email, ... } ──────│  (by google_id)         │
   │     + Set-Cookie: wfd_session    │                         │
   │                                  │                         │
   │──── GET /api/auth/me ────────────►│ (on next page load)     │
   │◄─── 200 { id, email, ... } ──────│                         │
   │     + renewed Set-Cookie         │                         │
   │                                  │                         │
   │──── POST /api/auth/logout ───────►│                         │
   │◄─── 200 + Set-Cookie Max-Age=0 ──│                         │
```

## Full specification

See [openapi.yaml](./openapi.yaml)
