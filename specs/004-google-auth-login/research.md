# Research: Google Authentication & User Data Isolation

**Feature**: 004-google-auth-login  
**Date**: 2026-06-06  
**Status**: Complete — all NEEDS CLARIFICATION items resolved

---

## Decision 1: OAuth Flow Architecture

**Decision**: Frontend-initiated Google Identity Services (GIS) flow  
**Rationale**: The Vue 3 SPA and ASP.NET Core API are separate origins. Backend-redirect OIDC (`Microsoft.AspNetCore.Authentication.Google`) is designed for server-rendered apps; after the Google callback, it would need to redirect back to the Vue origin while handing off a token — either via URL fragment (XSS risk) or cross-origin cookie (SameSite pain). The GIS popup flow keeps everything in-page:
1. Vue loads `accounts.google.com/gsi/client` via `<script async defer>` in `index.html`
2. User clicks the rendered Google button → GIS popup
3. GIS calls back with a signed JWT ID token (`response.credential`)
4. Vue POSTs `{ credential }` to `POST /api/auth/google`
5. Backend verifies the ID token, upserts the `User` record, issues its own session cookie

**Alternatives considered**: Backend-redirect flow with `Microsoft.AspNetCore.Authentication.Google` — rejected due to awkward token handoff for SPA + REST API setup.

---

## Decision 2: Backend Identity Verification

**Decision**: `Google.Apis.Auth` NuGet package — `GoogleJsonWebSignature.ValidateAsync()`  
**Rationale**: Official Google library for server-side ID token verification. Validates signature, audience, and expiry; caches Google's public keys automatically. No need to call `tokeninfo` endpoint.  
**Package**: `Google.Apis.Auth` v1.68.0  
**Key fields from verified payload**:
- `payload.Subject` — stable, immutable Google user ID; used as the `google_id` lookup key
- `payload.Email` — display attribute only
- `payload.Name` — display name
- `payload.Picture` — profile picture URL

**Alternatives considered**: Manual JWT parsing — rejected; maintaining key-fetch caching manually is error-prone.

---

## Decision 3: Session Management

**Decision**: Backend-issued JWT stored in an httpOnly, Secure, SameSite=Lax cookie with 30-day sliding expiration  
**Rationale**: The spec requires a rolling 30-day session. httpOnly cookie means the token is never accessible to JavaScript (XSS-proof). Rolling expiry is achieved by rewriting the cookie's Max-Age on every authenticated request. The Vue frontend uses `credentials: 'include'` (Axios withCredentials) so the cookie is sent automatically — no manual token management in the SPA.  
**Packages**: `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0 + `System.IdentityModel.Tokens.Jwt` (transitively included)  
**CORS update required**: Must add `.AllowCredentials()` to the existing policy (incompatible with `AllowAnyOrigin()`; existing config already uses `WithOrigins(...)` ✓)  

**Alternatives considered**: localStorage/Bearer token — rejected (XSS risk). Separate refresh token flow — rejected (adds complexity; rolling cookie achieves the same 30-day behavior simpler).

---

## Decision 4: Frontend Session Detection

**Decision**: `GET /api/auth/me` called on app startup  
**Rationale**: The frontend cannot read an httpOnly cookie by design. On first navigation, the Vue Router guard calls `authStore.restoreSession()` which calls `GET /api/auth/me`. If the cookie is valid the backend returns `{ id, name, email, avatarUrl }` (200); otherwise 401. A `sessionChecked` flag in the Pinia store prevents re-calling on subsequent navigations.  
**Pattern**: Router `beforeEach` guard awaits `restoreSession()` exactly once per page load.

---

## Decision 5: User Entity Identity Anchor

**Decision**: `google_id` (`sub` claim) as the lookup key; `email` stored as a display attribute only  
**Rationale**: Per clarification Q3, email was selected as the user-facing identifier. However, Google's `sub` claim is the only immutable identifier — an email address can change (Google Workspace migration, Gmail rename). The correct implementation stores both: `google_id` is used for all DB lookups (indexed, unique), `email` is stored for display. This is consistent with the user's intent (identify users by their Google account) while being robust to edge cases. If the user's email changes on Google, their account is matched by `google_id` and the email column is updated.

---

## Decision 6: User Entity Changes

**Decision**: Extend the existing `User` model — add `GoogleId`, `Email`, `DisplayName`, `PictureUrl`, `LastLoginAt`; rename/replace existing `Name` with `DisplayName`; remove seed data; add unique index on `GoogleId`  
**Rationale**: The existing model has an integer PK and `UserId` FKs on `Recipe` and `WeeklyPlan` — no changes to FK columns needed. The seed "Default User" (Id=1) is replaced by real users; seed data is removed as part of this migration.  
**Migration strategy**: New EF Core migration that:
1. Deletes all existing data (ownerless — confirmed in clarifications)
2. Alters `users` table: drop `name`, add `google_id`, `email`, `display_name`, `picture_url`, `last_login_at`
3. Adds `UNIQUE` index on `google_id`

---

## Decision 7: Google Client ID Handling

**Decision**: Expose `VITE_GOOGLE_CLIENT_ID` as a Vite env variable in `frontend/.env`  
**Rationale**: The Google Client ID is a public identifier — safe to ship in client-side bundles. Security is enforced by "Authorized JavaScript origins" in Google Cloud Console (locked to the app's domain). The Client Secret is only needed for server-side flows and is not used here.

---

## NuGet Packages to Add

| Package | Version | Where |
|---------|---------|-------|
| `Google.Apis.Auth` | `1.68.0` | `WhatsForDinner.Api` |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `10.0.0` | `WhatsForDinner.Api` |

## npm Packages to Add

None required — GIS is loaded via CDN `<script>` tag (not an ES module). Type declarations written manually in `src/types/google-gsi.d.ts`.

---

## Existing Codebase Compatibility Notes

- `Recipe.UserId` and `WeeklyPlan.UserId` (int FK) — **no change needed**. The FK points to `User.Id` (int PK) which is preserved.
- `IRecipeService` and `IWeeklyPlanService` already accept `userId` parameters (currently defaulting to `1`). These defaults will be replaced by the authenticated user's id resolved from the session cookie.
- `ApplicationDbContext` already has `DbSet<User>` — no change to context.
- `AllowCredentials()` must be added to the CORS policy in `Program.cs`.
- All existing integration tests use hardcoded `userId=1`; they will need updates to provide authenticated context via the test `WebApplicationFactory`.
