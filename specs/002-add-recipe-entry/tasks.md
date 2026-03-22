# Tasks: Add Recipe Entry

**Input**: Design documents from `/specs/002-add-recipe-entry/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per constitution §II (Testing Standards). Tests are written before implementation in each user story phase.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `backend/src/WhatsForDinner.Api/`
- **Backend Tests**: `backend/tests/WhatsForDinner.Api.Tests/`
- **Frontend**: `frontend/src/`
- **Frontend Tests**: `frontend/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Install new dependencies and configure shared services needed by multiple user stories

- [x] T001 Add `OpenAI` NuGet package (v2.x) to `backend/src/WhatsForDinner.Api/WhatsForDinner.Api.csproj`
- [x] T002 Add OpenAI configuration section to `backend/src/WhatsForDinner.Api/appsettings.json` and `backend/src/WhatsForDinner.Api/appsettings.Development.json` with `OpenAI:ApiKey` and `OpenAI:Model` (default `gpt-4o-mini`) settings
- [x] T003 [P] Create `RecipeCreateRequest` DTO record in `backend/src/WhatsForDinner.Api/Models/Dtos/RecipeCreateRequest.cs` with Name (required, max 200), Description (optional, max 1000), Ingredients (optional, max 2000), CookTimeMinutes (optional, ≥ 0) using DataAnnotations
- [x] T004 [P] Create `RecipeImageExtractResult` DTO record in `backend/src/WhatsForDinner.Api/Models/Dtos/RecipeImageExtractResult.cs` with Success (bool), Name, Description, Ingredients, CookTimeMinutes, Message fields per OpenAPI contract
- [x] T005 [P] Add `RecipeCreateRequest` and `RecipeImageExtractResult` TypeScript interfaces to `frontend/src/types/Recipe.ts`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core backend service methods and frontend infrastructure that MUST be complete before ANY user story UI can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T006 Add `CreateRecipeAsync(RecipeCreateRequest request, int userId)` and `DeleteRecipeAsync(int id, int userId)` method signatures to `backend/src/WhatsForDinner.Api/Services/IRecipeService.cs`
- [x] T007 Implement `CreateRecipeAsync` in `backend/src/WhatsForDinner.Api/Services/RecipeService.cs` — map DTO to Recipe entity, set UserId/CreatedAt/UpdatedAt, add to DbContext, save, return RecipeDto
- [x] T008 Implement `DeleteRecipeAsync` in `backend/src/WhatsForDinner.Api/Services/RecipeService.cs` — find recipe by id and userId, remove from DbContext (cascade handles WeeklyPlanItems), return bool for found/not-found
- [x] T009 [P] Create `IRecipeImageExtractor` interface in `backend/src/WhatsForDinner.Api/Services/IRecipeImageExtractor.cs` with `Task<RecipeImageExtractResult> ExtractFromImageAsync(byte[] imageData, string contentType)` method
- [x] T010 Implement `RecipeImageExtractor` in `backend/src/WhatsForDinner.Api/Services/RecipeImageExtractor.cs` — use OpenAI Chat Completions with `gpt-4o-mini`, send base64 image in multi-modal message, use Structured Outputs JSON schema for `{ name, description, ingredients, cookTimeMinutes }`, handle errors (missing API key → 503, invalid key → 502, timeout → 504, unreadable image → 422)
- [x] T011 Register `IRecipeImageExtractor`/`RecipeImageExtractor` as scoped service and `OpenAIClient` as singleton in `backend/src/WhatsForDinner.Api/Program.cs`
- [x] T012 [P] Add `postFormData<T>` method to `frontend/src/services/apiClient.ts` that sends `FormData` via fetch without setting `Content-Type` header (browser auto-sets with boundary)
- [x] T013 [P] Add `createRecipe`, `deleteRecipe`, and `extractFromImage` methods to `frontend/src/services/recipeService.ts` — createRecipe uses `apiClient.post`, deleteRecipe uses `apiClient.delete`, extractFromImage uses `apiClient.postFormData`
- [x] T014 Add `createRecipe` and `deleteRecipe` actions to `frontend/src/stores/recipeStore.ts` — createRecipe calls service and prepends to local recipes list, deleteRecipe calls service and removes from local list

**Checkpoint**: Foundation ready — all backend endpoints have service layer support, frontend has API client methods. User story implementation can now begin.

---

## Phase 3: User Story 1 — Add Recipe via Manual Entry (Priority: P1) 🎯 MVP

**Goal**: Users can navigate to an add recipe form, fill in recipe details (name, description, ingredients, cook time), submit, and see the new recipe in their list.

**Independent Test**: Navigate to Recipes → click "Add Recipe" → fill in form → submit → verify new recipe appears in recipe list with confirmation message.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T015 [P] [US1] Integration test for `POST /api/recipes` in `backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs` — test 201 on valid create, 400 on missing name, 400 on invalid cook time
- [x] T016 [P] [US1] Component test for `RecipeForm.vue` in `frontend/tests/unit/components/RecipeForm.spec.ts` — test name required validation, emit on submit, emit on cancel, pre-population via initialData prop

### Implementation for User Story 1

- [x] T017 [US1] Add `POST /api/recipes` endpoint to `backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs` — accept `[FromBody] RecipeCreateRequest`, call `CreateRecipeAsync`, return 201 with created RecipeDto and `Location` header
- [x] T018 [P] [US1] Create `RecipeForm.vue` component in `frontend/src/components/RecipeForm.vue` — shared form for create/edit with fields: name (required), description (textarea), ingredients (textarea), cookTimeMinutes (number, min 0); emit `submit` with form data and `cancel` event; client-side validation (name required, cookTimeMinutes ≥ 0); support pre-population via `initialData` prop; WCAG 2.1 AA compliant labels and validation messages
- [x] T019 [US1] Create `RecipeCreateView.vue` in `frontend/src/views/RecipeCreateView.vue` — uses `RecipeForm.vue`, calls `recipeStore.createRecipe` on submit, shows success confirmation, navigates to recipe list on success, handles cancel by navigating back
- [x] T020 [US1] Add `/recipes/new` route to `frontend/src/router/index.ts` — lazy-loaded `RecipeCreateView.vue`, placed BEFORE `/recipes/:id/edit` to avoid route conflict
- [x] T021 [US1] Add "Add Recipe" button to `frontend/src/views/RecipeListView.vue` — navigates to `/recipes/new` via router-link
- [x] T022 [US1] Add "Add Recipe" button to `frontend/src/views/WeeklyPlanView.vue` — navigates to `/recipes/new` via router-link (per FR-001, add recipe accessible from both recipe list and weekly plan views)

**Checkpoint**: User Story 1 complete — users can manually add recipes via form from both views. Independently testable.

---

## Phase 4: User Story 2 — Add Recipe via Image Upload (Priority: P2)

**Goal**: Users can upload a recipe image, have it processed by cloud AI to extract recipe details, and see the form pre-populated with extracted data.

**Independent Test**: On add recipe screen → click "Upload Image" → select recipe photo → loading indicator appears → extracted data pre-populates form → user can submit. Also test: upload non-recipe image → error message with fallback options.

### Tests for User Story 2

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T023 [P] [US2] Unit test for `RecipeImageExtractor` in `backend/tests/WhatsForDinner.Api.Tests/Unit/RecipeImageExtractorTests.cs` — mock OpenAI client, test successful extraction returns populated result, test failure modes (missing API key → 503, invalid key → 502, timeout → 504, unreadable image → 422)
- [x] T024 [P] [US2] Integration test for `POST /api/recipes/extract-from-image` in `backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs` — test 400 on unsupported format, 413 on oversize file, 200 on valid image with mocked extractor
- [x] T025 [P] [US2] Component test for `ImageUpload.vue` in `frontend/tests/unit/components/ImageUpload.spec.ts` — test file type validation rejects unsupported formats, size validation rejects >10 MB, preview renders on valid file, emits file-selected event

### Implementation for User Story 2

- [x] T026 [US2] Add `POST /api/recipes/extract-from-image` endpoint to `backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs` — accept `[FromForm] IFormFile file`, apply `[RequestSizeLimit(10_485_760)]`, validate Content-Type (JPEG/PNG/WebP) and magic bytes, convert to byte array, call `IRecipeImageExtractor.ExtractFromImageAsync`, return `RecipeImageExtractResult`; handle errors: unsupported format → 400, file too large → 413, extraction failure → 422, AI service errors → 502/503/504
- [x] T027 [US2] Create `ImageUpload.vue` component in `frontend/src/components/ImageUpload.vue` — file input accepting image/jpeg,image/png,image/webp; client-side validation for file type and size (10 MB max); image preview via `URL.createObjectURL` with cleanup in `onUnmounted`; drag-and-drop support as progressive enhancement; emit `file-selected` with File object; loading state prop; ARIA labels, `aria-live` for status, `aria-describedby` for errors
- [x] T028 [US2] Integrate `ImageUpload.vue` into `frontend/src/views/RecipeCreateView.vue` — add upload tab/section alongside manual entry; on file selected call `recipeService.extractFromImage`; show `LoadingSpinner` during extraction; on success pre-populate `RecipeForm` with extracted data; on failure show error message with options to try another image or switch to manual entry

**Checkpoint**: User Story 2 complete — users can add recipes via image upload. Independently testable.

---

## Phase 5: User Story 3 — Review and Edit Extracted Recipe Before Saving (Priority: P3)

**Goal**: After image extraction, users can review and correct all extracted fields before saving, ensuring data accuracy.

**Independent Test**: Upload image → extracted data appears in editable form fields → modify fields → submit → saved recipe reflects edited values, not original extraction.

### Implementation for User Story 3

- [x] T029 [US3] Enhance `RecipeForm.vue` in `frontend/src/components/RecipeForm.vue` — add visual indicator (e.g., subtle highlight or "AI extracted" badge) for pre-populated fields to distinguish from user-entered data; ensure all fields remain fully editable; handle partially extracted data (empty fields left blank for user input); add `source` prop ('manual' | 'extracted') to adjust UX messaging
- [x] T030 [US3] Add extraction status messaging in `frontend/src/views/RecipeCreateView.vue` — display informational message when form is pre-populated (e.g., "Review the extracted recipe details below and make any corrections before saving"); show which fields were successfully extracted vs. left empty

**Checkpoint**: User Story 3 complete — users can confidently review and edit extracted data before saving.

---

## Phase 6: User Story 4 — Delete Recipe from Inventory (Priority: P4)

**Goal**: Users can delete recipes from their inventory with a confirmation dialog, and deleted recipes are cascade-removed from the weekly plan.

**Independent Test**: On recipe list → click delete on a recipe → confirmation dialog appears → confirm → recipe removed from list. Also test: delete recipe assigned to weekly plan → recipe removed from plan too. Cancel deletion → recipe unchanged.

### Tests for User Story 4

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T031 [P] [US4] Integration test for `DELETE /api/recipes/{id}` in `backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs` — test 204 on successful delete, 404 on missing recipe, verify cascade removes WeeklyPlanItems
- [x] T032 [P] [US4] Component test for `ConfirmDialog.vue` in `frontend/tests/unit/components/ConfirmDialog.spec.ts` — test confirm emits confirm event, cancel emits cancel event, Escape key cancels, Enter key confirms, ARIA role="dialog" present

### Implementation for User Story 4

- [x] T033 [US4] Add `DELETE /api/recipes/{id}` endpoint to `backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs` — call `DeleteRecipeAsync`, return 204 on success or 404 if not found
- [x] T034 [P] [US4] Create `ConfirmDialog.vue` component in `frontend/src/components/ConfirmDialog.vue` — reusable modal with title, message, confirm/cancel buttons; emit `confirm` and `cancel` events; keyboard accessible (Escape to cancel, Enter to confirm); focus trap within dialog; ARIA role="dialog" and aria-modal
- [x] T035 [US4] Add delete functionality to `frontend/src/views/RecipeListView.vue` — add delete button to each recipe card/list item; on click show `ConfirmDialog`; on confirm call `recipeStore.deleteRecipe`; show success feedback; handle errors
- [x] T036 [US4] Refresh weekly plan store after recipe deletion in `frontend/src/stores/recipeStore.ts` — after successful delete, call `weeklyPlanStore.fetchWeeklyPlan()` to reflect cascade-removed plan items

**Checkpoint**: User Story 4 complete — users can delete recipes with confirmation and cascade behavior.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final validation

- [x] T037 [P] E2E test for add-recipe flows in `frontend/tests/e2e/add-recipe.spec.ts` — manual entry happy path (fill form, submit, verify in list), image upload flow (upload, review, submit), delete with confirmation (delete, confirm, verify removed)
- [x] T038 [P] Update `backend/src/WhatsForDinner.Api/WhatsForDinner.Api.http` with example requests for POST create recipe, POST extract-from-image, and DELETE recipe endpoints
- [x] T039 [P] Refactor `RecipeEditView.vue` in `frontend/src/views/RecipeEditView.vue` to use the shared `RecipeForm.vue` component — replace inline form markup with RecipeForm component, pass existing recipe data via initialData prop (DRY — both create and edit use same form)
- [x] T040 Run quickstart.md validation — start backend and frontend, test all 4 flows: manual add, image upload add, review/edit extracted, delete recipe; verify API responses match OpenAPI contract

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T001-T002 for OpenAI package/config, T003-T005 for DTOs)
- **User Story 1 (Phase 3)**: Depends on Phase 2 completion — BLOCKS on T006-T008 (backend service), T012-T014 (frontend service/store)
- **User Story 2 (Phase 4)**: Depends on Phase 2 and Phase 3 (uses RecipeForm.vue from US1, uses RecipeCreateView.vue from US1)
- **User Story 3 (Phase 5)**: Depends on Phase 4 (enhances extraction review from US2)
- **User Story 4 (Phase 6)**: Depends on Phase 2 only — can proceed in parallel with US2/US3
- **Polish (Phase 7)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Phase 2 — no dependencies on other stories
- **User Story 2 (P2)**: Depends on US1 (uses RecipeForm.vue and RecipeCreateView.vue)
- **User Story 3 (P3)**: Depends on US2 (enhances the extraction review flow)
- **User Story 4 (P4)**: Can start after Phase 2 — independent of US1/US2/US3

### Within Each User Story

- Tests MUST be written and FAIL before implementation begins
- Backend endpoints before frontend views that call them
- Components before views that use them
- Core implementation before integration refinements

### Parallel Opportunities

- **Phase 1**: T003, T004, T005 can all run in parallel (different files)
- **Phase 2**: T009 + T012 + T013 can run in parallel; T006 must precede T007/T008
- **Phase 3**: T015 + T016 tests in parallel; then T017 (backend) + T018 (RecipeForm.vue) in parallel
- **Phase 4**: T023 + T024 + T025 tests in parallel; then T026 (backend) + T027 (ImageUpload.vue) in parallel
- **Phase 6**: T031 + T032 tests in parallel; then T033 (backend) + T034 (ConfirmDialog.vue) in parallel
- **Phase 6** can start as soon as Phase 2 is complete, in parallel with Phase 4/5

---

## Parallel Example: Phase 1 Setup

```text
# All three DTO tasks can run simultaneously (different files):
Task T003: Create RecipeCreateRequest.cs
Task T004: Create RecipeImageExtractResult.cs
Task T005: Add TypeScript interfaces to Recipe.ts
```

## Parallel Example: User Story 1

```text
# Tests first (in parallel):
Task T015: Integration test for POST /api/recipes
Task T016: Component test for RecipeForm.vue
# Then backend endpoint and frontend form component simultaneously:
Task T017: POST /api/recipes endpoint in RecipesController.cs
Task T018: RecipeForm.vue component
# Then sequentially:
Task T019: RecipeCreateView.vue (uses T018)
Task T020: Router update
Task T021: Add Recipe button on RecipeListView.vue
Task T022: Add Recipe button on WeeklyPlanView.vue
```

## Parallel Example: User Story 4

```text
# Tests first (in parallel):
Task T031: Integration test for DELETE /api/recipes/{id}
Task T032: Component test for ConfirmDialog.vue
# Then backend endpoint and confirm dialog simultaneously:
Task T033: DELETE /api/recipes/{id} endpoint
Task T034: ConfirmDialog.vue component
# Then sequentially:
Task T035: Delete button on RecipeListView.vue (uses T034)
Task T036: Refresh weekly plan after deletion
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T005)
2. Complete Phase 2: Foundational (T006–T014)
3. Complete Phase 3: User Story 1 (T015–T022)
4. **STOP and VALIDATE**: Users can manually add recipes via form
5. Deploy/demo if ready — delivers core "add recipe" capability

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add User Story 1 → Manual recipe entry works → Deploy/Demo (MVP!)
3. Add User Story 2 → Image upload extraction works → Deploy/Demo
4. Add User Story 3 → Extraction review polished → Deploy/Demo
5. Add User Story 4 → Recipe deletion works → Deploy/Demo
6. Polish → Refactor, validate, documentation, E2E tests → Final release

### Parallel Track Strategy

Since US4 (Delete) is independent of US2/US3:

1. Complete Setup + Foundational together
2. Once Foundational is done:
   - **Track A**: US1 → US2 → US3 (sequential — each builds on previous)
   - **Track B**: US4 (can start immediately after Phase 2)
3. Polish after both tracks complete

---

## Notes

- No schema changes or migrations required — existing Recipe table supports all needed fields
- The `RecipeForm.vue` component is designed to be shared between create (US1) and edit (existing RecipeEditView) flows
- OpenAI API key is only required for US2 (image extraction) — US1 and US4 work without it
- Cascade delete is already configured in the database — no EF Core changes needed
- The `apiClient.ts` needs a new `postFormData` method since the existing methods always set `Content-Type: application/json`
- Commit after each task or logical group; stop at any checkpoint to validate independently
