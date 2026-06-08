# Feature Specification: Google Authentication & User Data Isolation

**Feature Branch**: `004-google-auth-login`  
**Created**: 2026-06-06  
**Status**: Draft  
**Input**: User description: "Implement login functionality in the application following these specifications: The login page should only have a 'Sign in with Google' button that redirects people to login using their Google account. Include a funny description of what the application does in the login page. All data (recipes and weekly plans) should be isolated based on user."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Sign In With Google (Priority: P1)

A new or returning user opens the application and is greeted by a login page showing a funny description of what the app does and a single "Sign in with Google" button. They click the button, complete Google's authentication flow, and land on the weekly plan view as a signed-in user.

**Why this priority**: Authentication is the gateway to all other functionality. Without it, no other feature in this spec can be tested or used.

**Independent Test**: Can be fully tested by opening the app unauthenticated, clicking "Sign in with Google", completing the OAuth flow, and verifying the user is redirected to the weekly plan view. Delivers a working login experience as a standalone slice.

**Acceptance Scenarios**:

1. **Given** an unauthenticated user visits the application, **When** the login page loads, **Then** a funny description of the application and a "Sign in with Google" button are displayed — and nothing else (no username/password fields, no other login options).
2. **Given** an unauthenticated user on the login page, **When** they click "Sign in with Google", **Then** they are redirected to Google's authentication page.
3. **Given** a user who completes Google authentication successfully, **When** they are redirected back to the application, **Then** they land on the weekly plan view as a signed-in user.
4. **Given** a user who cancels or fails Google authentication, **When** they are returned to the application, **Then** they remain on the login page and see a friendly error message.
5. **Given** an authenticated user, **When** they navigate directly to the login page URL, **Then** they are automatically redirected to the weekly plan view.

---

### User Story 2 - User Data Isolation for Recipes (Priority: P2)

A signed-in user creates, edits, and deletes recipes. Those recipes belong exclusively to that user and are not visible to any other user. When a different user signs in, they see only their own recipes.

**Why this priority**: Data privacy is a core promise once multiple users can log in. A user's personal recipe collection must never be visible to others.

**Independent Test**: Can be tested by signing in as User A, creating a recipe, then signing in as User B and verifying the recipe is not visible. Delivers per-user recipe privacy as a standalone slice.

**Acceptance Scenarios**:

1. **Given** User A is signed in and creates a recipe, **When** User B signs in with a different Google account, **Then** User B cannot see User A's recipe in any list or detail view.
2. **Given** User A is signed in, **When** they view the recipe list, **Then** only recipes created by User A are shown.
3. **Given** a new user signs in for the first time, **When** they view the recipe list, **Then** the list is empty (no recipes from other users are shown).
4. **Given** User A is signed in, **When** they attempt to access a recipe that belongs to User B, **Then** they receive a "not found" or "access denied" response.

---

### User Story 3 - User Data Isolation for Weekly Plans (Priority: P3)

A signed-in user manages their weekly meal plan. That plan belongs exclusively to them and is never visible to or modifiable by other users.

**Why this priority**: The weekly plan is the second primary data entity. Isolation follows naturally from recipe isolation but is specified separately because it is a distinct entity.

**Independent Test**: Can be tested by signing in as User A, adding meals to the weekly plan, then signing in as User B and verifying the weekly plan is empty. Delivers per-user plan privacy as a standalone slice.

**Acceptance Scenarios**:

1. **Given** User A is signed in and adds recipes to their weekly plan, **When** User B signs in, **Then** User B's weekly plan is empty (or contains only their own entries).
2. **Given** User A is signed in, **When** they view the weekly plan, **Then** only their own meal plan entries are shown.
3. **Given** a new user signs in for the first time, **When** they view the weekly plan, **Then** the weekly plan is empty.

---

### User Story 4 - Sign Out (Priority: P4)

A signed-in user wants to end their session. They can sign out from within the application, which returns them to the login page and ensures their data is no longer accessible without re-authenticating.

**Why this priority**: Sign-out completes the authentication lifecycle and is necessary for shared devices and privacy.

**Independent Test**: Can be tested by signing in, clicking sign out, and verifying the user is returned to the login page and cannot access protected views without signing in again.

**Acceptance Scenarios**:

1. **Given** a signed-in user, **When** they click the sign-out button/option, **Then** their session ends and they are redirected to the login page.
2. **Given** a user who has signed out, **When** they try to navigate directly to a protected route (e.g., recipe list), **Then** they are redirected to the login page.

---

### Edge Cases

- What happens when Google's OAuth service is temporarily unavailable? The user sees a friendly error message and the "Sign in with Google" button remains functional for retry.
- What happens when a user's Google account is used on multiple devices simultaneously? Both sessions remain active and show the same data.
- What happens when a user revokes the application's access from their Google account settings? The user is signed out on their next interaction and redirected to the login page.
- What happens when a returning user signs in with a Google account that previously had data? All their previously created recipes and weekly plan entries are still present.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a login page to unauthenticated users containing a funny description of the application and a single "Sign in with Google" button.
- **FR-002**: System MUST redirect unauthenticated users who attempt to access any protected route to the login page.
- **FR-003**: System MUST redirect already-authenticated users away from the login page to the weekly plan view. After a successful Google authentication, the user MUST be directed to the weekly plan view.
- **FR-004**: System MUST support Google OAuth 2.0 sign-in as the only authentication method.
- **FR-005**: System MUST create a user identity record upon first sign-in with a Google account. All pre-existing ownerless recipes and weekly plan entries MUST be deleted as a one-time deployment migration step.
- **FR-006**: System MUST associate all recipes created by a user with that user's identity.
- **FR-007**: System MUST associate all weekly plan entries with the user who created them.
- **FR-008**: System MUST ensure that a user can only read, create, update, and delete their own recipes.
- **FR-009**: System MUST ensure that a user can only read, create, update, and delete their own weekly plan entries.
- **FR-010**: System MUST provide a sign-out option in the top navigation bar, accessible via a user avatar or display name dropdown that is visible on all authenticated pages.
- **FR-011**: System MUST invalidate the user's session upon sign-out, requiring re-authentication before accessing protected routes.
- **FR-012**: System MUST display a user-friendly error message when Google authentication fails or is cancelled.

### Key Entities

- **User**: Represents an authenticated individual. The Google account ID (`google_id`, derived from the Google `sub` claim) serves as the stable unique lookup identifier. Key attributes: `google_id` (unique), email address (display/contact attribute), display name, profile picture URL. Created automatically on first sign-in. If a user signs in again with the same `google_id`, they are matched to the existing record and profile attributes are updated.
- **Recipe** *(existing, modified)*: Now belongs to exactly one User. Recipes are only accessible to their owner.
- **WeeklyPlanItem** *(existing, modified)*: Now belongs to exactly one User. Weekly plan entries are only accessible to their owner.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A new user can complete the full sign-in flow (login page → Google authentication → application) in under 60 seconds.
- **SC-002**: 100% of recipe and weekly plan data is scoped to the authenticated user — no data leakage between user accounts under any tested scenario.
- **SC-003**: Unauthenticated access to any protected route results in redirection to the login page 100% of the time.
- **SC-004**: A returning user's previously created recipes and weekly plan entries are fully available after re-authenticating.
- **SC-005**: First Contentful Paint for the login page MUST occur within 2 seconds on a simulated 3G connection.
- **SC-006**: User sessions expire after 30 days of inactivity; a session that has been active within the last 30 days remains valid without re-authentication.

## Clarifications

### Session 2026-06-06

- Q: How long should a user's login session last before they are required to sign in again? → A: Rolling 30 days — session renews on each visit, expires after 30 days of inactivity.
- Q: What should happen to data (recipes and weekly plans) that already exists in the database with no owner when this feature is deployed? → A: Delete it — remove all ownerless data as part of the migration.
- Q: What uniquely identifies a user across sign-ins — their Google account ID or their email address? → A: Google account ID (`google_id` from the `sub` claim) is the stable lookup key; email is a display/contact attribute.
- Q: Where should the sign-out option be placed in the application UI? → A: Top navigation bar — a user avatar or name with a dropdown containing "Sign out".
- Q: After signing in for the first time, where should a new user land? → A: Weekly plan view.

## Assumptions

- The application currently has no authentication layer; all data is shared globally. This feature adds authentication as a foundational layer.
- User identity is derived entirely from the Google account; no separate registration or profile management is required in this feature.
- The funny description on the login page is a short, lighthearted tagline about the app's purpose (helping users decide what to cook). The exact wording will be determined during implementation.
- Existing data in the database (created before this feature) has no owner and will be permanently deleted as part of the deployment migration. No migration of ownerless recipes or weekly plan entries to any user account will occur.
- The application runs in an environment where HTTPS is available, which is required for OAuth 2.0 security.
- Session persistence uses a rolling 30-day inactivity timeout: the session renews on each visit and expires after 30 consecutive days without activity. Users may also end their session at any time by signing out explicitly.

## Scope Boundaries

**In scope**:
- Login page UI with "Sign in with Google" button and funny app description
- Google OAuth 2.0 authentication flow
- User record creation on first sign-in
- Per-user data isolation for recipes and weekly plans
- Sign-out functionality
- Route protection (redirect unauthenticated users to login)

**Out of scope**:
- Other authentication providers (GitHub, Microsoft, email/password, etc.)
- User profile editing or account management
- Admin roles or multi-role authorization
- Social features (sharing recipes between users)
- Migration of pre-existing anonymous data to user accounts
