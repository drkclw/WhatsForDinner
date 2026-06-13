# Tasks: Google Authentication & User Data Isolation

**Input**: Design documents from `/specs/004-google-auth-login/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/openapi.yaml`

**Tests**: Tests are mandatory per constitution. This task list includes unit, integration, and E2E coverage tasks mapped to each user story.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Add dependencies and baseline configuration files required by all stories.

- [X] T001 Add Google and JWT auth package references in `backend/src/WhatsForDinner.Api/WhatsForDinner.Api.csproj`
- [X] T002 Add authentication configuration schema in `backend/src/WhatsForDinner.Api/appsettings.json`
- [X] T003 [P] Add development auth defaults in `backend/src/WhatsForDinner.Api/appsettings.Development.json`
- [X] T004 [P] Add Google Client ID environment template in `frontend/.env.example`
- [X] T005 [P] Add Google Identity Services script include in `frontend/index.html`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Build core auth/session plumbing and data model updates required before any user story work.

**⚠️ CRITICAL**: No user story implementation starts until this phase is complete.

- [X] T006 Configure JWT authentication, authorization middleware, and CORS credentials in `backend/src/WhatsForDinner.Api/Program.cs`
- [X] T007 Create auth option models for strongly typed configuration in `backend/src/WhatsForDinner.Api/Models/Dtos/AuthOptions.cs`
- [X] T008 Update user entity with Google auth fields in `backend/src/WhatsForDinner.Api/Models/User.cs`
- [X] T009 Update EF Core user mapping and remove seed user in `backend/src/WhatsForDinner.Api/Data/Configurations/UserConfiguration.cs`
- [X] T010 Generate migration for Google auth columns and ownerless data deletion in `backend/src/WhatsForDinner.Api/Migrations/*_AddGoogleAuth.cs`
- [X] T011 Update EF snapshot for new user schema in `backend/src/WhatsForDinner.Api/Migrations/ApplicationDbContextModelSnapshot.cs`
- [X] T012 Configure cookie-enabled API client defaults in `frontend/src/services/apiClient.ts`

**Checkpoint**: Foundation complete. User stories can now be implemented.

---

## Phase 3: User Story 1 - Sign In With Google (Priority: P1) 🎯 MVP

**Goal**: Users can authenticate through Google from a login page containing only a funny description and a Google sign-in button, then land on weekly plan.

**Independent Test**: Open app while signed out, complete Google sign-in, verify redirect to weekly plan; cancel sign-in and verify friendly error on login page.

- [X] T043 [P] [US1] Add backend integration tests for `/api/auth/google` and `/api/auth/me` in `backend/tests/WhatsForDinner.Api.Tests/Integration/AuthControllerTests.cs`
- [X] T044 [P] [US1] Add frontend unit tests for auth store login/session restore/error state in `frontend/tests/unit/stores/authStore.spec.ts`
- [X] T045 [P] [US1] Add E2E login success/cancel/error journeys in `frontend/tests/e2e/auth.spec.ts`
- [X] T013 [P] [US1] Add auth request/response DTOs in `backend/src/WhatsForDinner.Api/Models/Dtos/AuthDtos.cs`
- [X] T014 [P] [US1] Define auth service contract in `backend/src/WhatsForDinner.Api/Services/IAuthService.cs`
- [X] T015 [US1] Implement Google token verification, user upsert, and session issuance in `backend/src/WhatsForDinner.Api/Services/AuthService.cs`
- [X] T016 [US1] Register auth service dependency in `backend/src/WhatsForDinner.Api/Program.cs`
- [X] T017 [US1] Implement `/api/auth/google` and `/api/auth/me` endpoints in `backend/src/WhatsForDinner.Api/Controllers/AuthController.cs`
- [X] T018 [P] [US1] Add Google GIS TypeScript declarations in `frontend/src/types/google-gsi.d.ts`
- [X] T019 [US1] Implement auth state management (login + restore session + auth error state) in `frontend/src/stores/authStore.ts`
- [X] T020 [US1] Add `/login` route and auth/guest navigation guards in `frontend/src/router/index.ts`
- [X] T021 [US1] Build login page with funny description and single Google sign-in button in `frontend/src/views/LoginView.vue`
- [X] T022 [US1] Wire app startup auth restoration and protected route handling in `frontend/src/main.ts`

**Checkpoint**: User Story 1 is independently functional as MVP.

---

## Phase 4: User Story 2 - User Data Isolation for Recipes (Priority: P2)

**Goal**: Recipes are fully scoped to the authenticated user and inaccessible to other users.

**Independent Test**: User A creates recipes; User B signs in and cannot view or fetch User A recipes; User B sees only their own recipe data.

- [X] T046 [P] [US2] Add integration tests for recipe cross-user isolation and 404 access control in `backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs`
- [X] T023 [P] [US2] Remove default user fallback from recipe service contract in `backend/src/WhatsForDinner.Api/Services/IRecipeService.cs`
- [X] T024 [US2] Enforce required user context in recipe service implementation signatures in `backend/src/WhatsForDinner.Api/Services/RecipeService.cs`
- [X] T025 [US2] Add `[Authorize]` and claim-based user resolution in `backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs`
- [X] T026 [US2] Enforce per-user filtering and cross-user 404 behavior in `backend/src/WhatsForDinner.Api/Services/RecipeService.cs`
- [X] T027 [US2] Update recipe API calls to propagate auth failures cleanly in `frontend/src/services/recipeService.ts`
- [X] T028 [US2] Update recipe store behavior for per-user empty state and unauthorized handling in `frontend/src/stores/recipeStore.ts`

**Checkpoint**: User Story 2 is independently functional.

---

## Phase 5: User Story 3 - User Data Isolation for Weekly Plans (Priority: P3)

**Goal**: Weekly plan data is fully scoped to the authenticated user and inaccessible to other users.

**Independent Test**: User A edits weekly plan; User B signs in and sees only User B weekly plan data, with no visibility into User A entries.

- [X] T047 [P] [US3] Add integration tests for weekly plan cross-user isolation in `backend/tests/WhatsForDinner.Api.Tests/Integration/WeeklyPlanControllerTests.cs`
- [X] T029 [P] [US3] Remove default user fallback from weekly plan service contract in `backend/src/WhatsForDinner.Api/Services/IWeeklyPlanService.cs`
- [X] T030 [US3] Enforce required user context in weekly plan service signatures in `backend/src/WhatsForDinner.Api/Services/WeeklyPlanService.cs`
- [X] T031 [US3] Add `[Authorize]` and claim-based user resolution in `backend/src/WhatsForDinner.Api/Controllers/WeeklyPlanController.cs`
- [X] T032 [US3] Enforce per-user weekly plan reads and writes in `backend/src/WhatsForDinner.Api/Services/WeeklyPlanService.cs`
- [X] T033 [US3] Update weekly plan API client behavior for authenticated-only access in `frontend/src/services/weeklyPlanService.ts`
- [X] T034 [US3] Update weekly plan store to handle per-user empty state and unauthorized responses in `frontend/src/stores/weeklyPlanStore.ts`

**Checkpoint**: User Story 3 is independently functional.

---

## Phase 6: User Story 4 - Sign Out (Priority: P4)

**Goal**: Authenticated users can sign out from top navigation avatar/name dropdown and are redirected to login, losing access to protected routes.

**Independent Test**: Sign in, use top-nav sign out, verify redirect to login and protected route access blocked until sign-in.

- [X] T048 [P] [US4] Add E2E sign-out and protected-route redirect coverage in `frontend/tests/e2e/auth.spec.ts`
- [X] T035 [P] [US4] Implement `/api/auth/logout` endpoint and cookie-clearing behavior in `backend/src/WhatsForDinner.Api/Controllers/AuthController.cs`
- [X] T036 [US4] Implement logout action and local auth-state reset in `frontend/src/stores/authStore.ts`
- [X] T037 [US4] Add top navigation user avatar/name dropdown with sign-out action in `frontend/src/App.vue`
- [X] T038 [US4] Update router guard flows for post-logout protected route redirects in `frontend/src/router/index.ts`

**Checkpoint**: User Story 4 is independently functional.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening, contract alignment, and end-to-end validation across stories.

- [X] T039 [P] Align OpenAPI auth and security requirements with implementation in `specs/004-google-auth-login/contracts/openapi.yaml`
- [X] T040 [P] Update API contract usage notes for final endpoint behavior in `specs/004-google-auth-login/contracts/README.md`
- [X] T041 Validate full quickstart workflow and update any drift in `specs/004-google-auth-login/quickstart.md`
- [X] T042 Run backend/frontend validation commands and capture execution notes in `specs/004-google-auth-login/quickstart.md`
- [X] T049 Execute backend test suite and capture results in `specs/004-google-auth-login/quickstart.md`
- [X] T050 Execute frontend unit and E2E test suites and capture results in `specs/004-google-auth-login/quickstart.md`
- [X] T051 Measure and record SC-001 sign-in completion time (<60s) using timed E2E/manual runs in `specs/004-google-auth-login/quickstart.md`
- [X] T052 Validate and record SC-004 returning-user data persistence after re-authentication in `specs/004-google-auth-login/quickstart.md`
- [X] T053 Measure and record SC-005 login-page FCP under simulated 3G (<=2s) in `specs/004-google-auth-login/quickstart.md`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies; starts immediately.
- **Phase 2 (Foundational)**: Depends on Phase 1; blocks all user stories.
- **Phase 3 (US1)**: Depends on Phase 2; establishes MVP auth entry flow.
- **Phase 4 (US2)**: Depends on Phase 2 and uses authenticated user context from US1.
- **Phase 5 (US3)**: Depends on Phase 2 and uses authenticated user context from US1.
- **Phase 6 (US4)**: Depends on US1 (logout requires established auth/session flow).
- **Phase 7 (Polish)**: Depends on completion of all targeted stories.

### User Story Dependency Graph

- **US1 (P1)** → enables **US2 (P2)**, **US3 (P3)**, and **US4 (P4)**
- **US2 (P2)** and **US3 (P3)** can proceed in parallel after US1 baseline auth is complete
- **US4 (P4)** can proceed once US1 auth store and nav context exist

---

## Parallel Execution Examples

### User Story 1

- Run `T013` and `T014` in parallel (DTOs and interface in different files)
- Run `T018` in parallel with backend tasks (`T015`–`T017`)

### User Story 2

- Run `T023` and `T025` in parallel (service contract vs controller updates)
- Run frontend tasks `T027` and `T028` in parallel after backend behavior is defined

### User Story 3

- Run `T029` and `T031` in parallel (service contract vs controller updates)
- Run frontend tasks `T033` and `T034` in parallel after backend behavior is defined

### User Story 4

- Run `T035` and `T037` in parallel (backend logout endpoint vs UI dropdown)
- Run `T036` after `T035`, then `T038` after `T036`

---

## Implementation Strategy

### MVP First (US1 only)

1. Complete Phase 1 and Phase 2.
2. Implement Phase 3 (US1) end-to-end.
3. Validate login flow independently (sign-in success, cancel/error, redirect behavior).
4. Demo/deploy MVP with authentication entry complete.

### Incremental Delivery

1. Deliver US1 (authentication entry) first.
2. Add US2 (recipe isolation) and validate with two accounts.
3. Add US3 (weekly plan isolation) and validate with two accounts.
4. Add US4 (sign out UX) and validate session invalidation.
5. Finish with Phase 7 cross-cutting hardening and quickstart validation.
