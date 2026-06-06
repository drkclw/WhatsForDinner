# Tasks: Multi-Image Recipe Extraction with Preparation

**Input**: Design documents from `/specs/003-multi-image-extraction/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Included per constitution (Section II: Testing Standards — 80% coverage critical paths).

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup

**Purpose**: Database migration and shared model/DTO changes that all user stories depend on.

- [x] T001 Add `Preparation` property to Recipe entity in `backend/src/WhatsForDinner.Api/Models/Recipe.cs`
- [x] T002 Add `Preparation` column configuration in `backend/src/WhatsForDinner.Api/Data/Configurations/RecipeConfiguration.cs` (HasColumnName "preparation", HasMaxLength 10000)
- [x] T003 Generate EF Core migration for AddPreparationField in `backend/src/WhatsForDinner.Api/Migrations/`
- [x] T004 [P] Add `Preparation` property to `RecipeCreateRequest` in `backend/src/WhatsForDinner.Api/Models/Dtos/RecipeCreateRequest.cs` (optional, max 10000 chars)
- [x] T005 [P] Add `Preparation` property to `RecipeUpdateRequest` in `backend/src/WhatsForDinner.Api/Models/Dtos/RecipeUpdateRequest.cs` (optional, max 10000 chars)
- [x] T006 [P] Add `Preparation` property to `RecipeDto` in `backend/src/WhatsForDinner.Api/Models/Dtos/RecipeDto.cs`
- [x] T007 [P] Add `Preparation` property to `RecipeImageExtractResult` in `backend/src/WhatsForDinner.Api/Models/Dtos/RecipeImageExtractResult.cs`
- [x] T008 [P] Add `preparation` field to TypeScript `Recipe`, `RecipeCreateRequest`, `RecipeUpdateRequest`, and `RecipeImageExtractResult` types in `frontend/src/types/Recipe.ts`
- [x] T009 Update Recipe-to-DTO mapping in `RecipeService` or controller to include Preparation field in `backend/src/WhatsForDinner.Api/Services/RecipeService.cs` (and/or `backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs`)

**Checkpoint**: Database schema updated. All models/DTOs include preparation field. Existing CRUD operations pass preparation through.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Extend the extraction service interface and AI prompt to support multi-image and preparation. These changes block all user story work.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [x] T010 Change `IRecipeImageExtractor` method signature from `ExtractFromImageAsync(byte[] imageData, string contentType)` to `ExtractFromImagesAsync(List<(byte[] Data, string ContentType)> images)` in `backend/src/WhatsForDinner.Api/Services/IRecipeImageExtractor.cs`
- [x] T011 Update `RecipeImageExtractor.ExtractFromImagesAsync` to build multiple `ChatMessageContentPart.CreateImagePart()` calls from the image list, one per image, within a single `UserChatMessage` in `backend/src/WhatsForDinner.Api/Services/RecipeImageExtractor.cs`
- [x] T012 Update the system prompt in `RecipeImageExtractor` to include `preparation` field extraction (cooking/baking instructions) and update the JSON schema to add `"preparation": { "type": ["string", "null"] }` in `backend/src/WhatsForDinner.Api/Services/RecipeImageExtractor.cs`
- [x] T013 Add `Preparation` property to the internal `ExtractedRecipe` record and update `BuildResult` to map it to `RecipeImageExtractResult` in `backend/src/WhatsForDinner.Api/Services/RecipeImageExtractor.cs`
- [x] T014 Make AI timeout configurable: read `OpenAI:TimeoutSeconds` from configuration (default 90 seconds) instead of hardcoded 30 seconds in `backend/src/WhatsForDinner.Api/Services/RecipeImageExtractor.cs`
- [x] T015 Add `OpenAI:TimeoutSeconds` setting to `backend/src/WhatsForDinner.Api/appsettings.json` with default value of 90

**Checkpoint**: Extraction service accepts multiple images, extracts preparation, and uses configurable timeout. Ready for controller/frontend integration.

---

## Phase 3: User Story 1 — Extract Preparation from a Single Image (Priority: P1) 🎯 MVP

**Goal**: Users upload a single image and get all five fields extracted including preparation. The preparation field is visible and editable in the recipe form, and persisted on save.

**Independent Test**: Upload one recipe image with visible preparation → form shows all 5 fields populated → save → view recipe shows preparation.

### Tests for User Story 1

- [x] T016 [P] [US1] Add unit test for `BuildResult` with preparation field (non-null preparation, null preparation, all-null returns failure) in `backend/tests/WhatsForDinner.Api.Tests/Unit/RecipeImageExtractorTests.cs`
- [x] T017 [P] [US1] Add unit test for single-image extraction call (verify single image produces one `CreateImagePart` in messages) in `backend/tests/WhatsForDinner.Api.Tests/Unit/RecipeImageExtractorTests.cs`

### Implementation for User Story 1

- [x] T018 [US1] Update `RecipesController.ExtractFromImage` endpoint to accept `List<IFormFile> files` (with backward-compatible support for single file), update `RequestSizeLimit` to 50 MB, validate 1–5 files individually, and call `ExtractFromImagesAsync` in `backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs`
- [x] T019 [US1] Add `Preparation` textarea field to `RecipeForm.vue` with max 10,000 character validation, AI-extracted badge support, and label "Preparation" in `frontend/src/components/RecipeForm.vue`
- [x] T020 [US1] Update `recipeService.extractFromImage()` to accept `File[]` and build FormData with `files` key (append each file) in `frontend/src/services/recipeService.ts`
- [x] T021 [US1] Update `RecipeCreateView.vue` to pass single-file array to updated extraction service and map `preparation` from extraction result to form, including AI-extracted field tracking in `frontend/src/views/RecipeCreateView.vue`
- [x] T022 [P] [US1] Update `RecipeEditView.vue` to display and allow editing of the `preparation` field in `frontend/src/views/RecipeEditView.vue`
- [x] T023 [P] [US1] Update `RecipeListView.vue` or recipe detail display to show the preparation field when viewing a recipe in `frontend/src/views/RecipeListView.vue`
- [x] T024 [US1] Update `recipeStore` actions (createRecipe, updateRecipe) to include preparation field in payloads in `frontend/src/stores/recipeStore.ts`

**Checkpoint**: Single image upload extracts all 5 fields. Preparation visible in create/edit/view. Saves correctly to database.

---

## Phase 4: User Story 2 — Upload Multiple Images for One Recipe (Priority: P2)

**Goal**: Users upload 1–5 images with thumbnail previews. All images sent in a single AI request. Merged extraction result populates the form. Loading spinner shows image count.

**Independent Test**: Upload 2+ images (e.g., ingredients page + preparation page) → thumbnails shown → click Extract → spinner shows "Extracting from N images..." → form shows merged data from all images.

### Tests for User Story 2

- [x] T025 [P] [US2] Add unit test for multi-image extraction (verify multiple images produce multiple `CreateImagePart` calls in single message) in `backend/tests/WhatsForDinner.Api.Tests/Unit/RecipeImageExtractorTests.cs`
- [x] T026 [P] [US2] Add integration test for multi-file upload endpoint (POST with 2–3 files, verify 200 response with extraction result) in `backend/tests/WhatsForDinner.Api.Tests/Integration/RecipeExtractionTests.cs`
- [x] T027 [P] [US2] Add integration test for file count validation (0 files → 400, 6 files → 400) in `backend/tests/WhatsForDinner.Api.Tests/Integration/RecipeExtractionTests.cs`

### Implementation for User Story 2

- [x] T028 [US2] Refactor `ImageUpload.vue` to manage array of files (max 5): add `multiple` attribute to file input, maintain cumulative file list state, enforce max count of 5, emit `files-changed` event with `File[]` in `frontend/src/components/ImageUpload.vue`
- [x] T029 [US2] Add thumbnail grid to `ImageUpload.vue`: display preview for each uploaded image using `URL.createObjectURL`, add accessible remove button per thumbnail, revoke object URLs on removal in `frontend/src/components/ImageUpload.vue`
- [x] T030 [US2] Add "Extract" button to `ImageUpload.vue` that triggers extraction only when files are present, disabled during loading in `frontend/src/components/ImageUpload.vue`
- [x] T031 [US2] Update loading state in `RecipeCreateView.vue` to show dynamic message "Extracting from N images..." based on file count during extraction in `frontend/src/views/RecipeCreateView.vue`
- [x] T032 [US2] Update `RecipeCreateView.vue` to pass full `File[]` from `files-changed` event to `recipeService.extractFromImage()` in `frontend/src/views/RecipeCreateView.vue`
- [x] T033 [US2] Add client-side validation in `ImageUpload.vue` for file type (JPEG, PNG, WebP via accept attribute and mime-type check), per-file size (10 MB each), and total count (max 5), with clear error messages in `frontend/src/components/ImageUpload.vue`

**Checkpoint**: Multi-image upload with previews works. Extraction processes all images in single AI request. Loading feedback shows image count.

---

## Phase 5: User Story 3 — Remove Images Before Extraction (Priority: P3)

**Goal**: Users can remove individual images from the upload set before triggering extraction. When all images are removed, the upload prompt is shown again.

**Independent Test**: Upload 3 images → remove the 2nd → verify only 2 remain → extract → only 2 images sent.

### Tests for User Story 3

- [x] T034 [P] [US3] Add component test for ImageUpload: verify removing an image from the list updates previews and emitted file array in `frontend/tests/unit/components/ImageUpload.spec.ts`


### Implementation for User Story 3

- [x] T035 [US3] Implement remove handler in `ImageUpload.vue`: clicking remove button filters the image from state, revokes its object URL, emits updated `files-changed`, and re-shows upload prompt if list becomes empty in `frontend/src/components/ImageUpload.vue`
- [x] T036 [US3] Ensure keyboard accessibility for remove buttons (focusable, Enter/Space triggers, aria-label "Remove image N") in `frontend/src/components/ImageUpload.vue`

**Checkpoint**: Individual image removal works. Empty state shows upload prompt. Only remaining images are sent for extraction.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Cleanup, validation, and edge case handling across all stories.

- [x] T037 [P] Clean up object URL memory: ensure all object URLs are revoked on component unmount in `frontend/src/components/ImageUpload.vue`
- [x] T038 [P] Update existing `RecipeImageExtractor` unit tests to use new `List<(byte[], string)>` signature in `backend/tests/WhatsForDinner.Api.Tests/Unit/RecipeImageExtractorTests.cs`
- [x] T039 Run database migration against development database and verify `preparation` column exists with `dotnet ef database update`
- [x] T040 Run full backend test suite with `dotnet test` in `backend/tests/WhatsForDinner.Api.Tests/` and verify all pass
- [x] T041 Run full frontend test suite with `npm run test` in `frontend/` and verify all pass
- [x] T042 Validate quickstart.md scenarios: manual entry with preparation, single-image extraction, multi-image extraction per `specs/003-multi-image-extraction/quickstart.md`
- [x] T043 [P] Add or update E2E test for multi-image extraction with preparation: upload image(s) → verify preparation field populated → save → view recipe shows preparation in `frontend/tests/e2e/add-recipe.spec.ts` (or new `extract-recipe.spec.ts`)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on T001–T003 (Recipe model + migration) — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Phase 2 completion
- **User Story 2 (Phase 4)**: Depends on Phase 3 (US1 establishes the updated endpoint and form)
- **User Story 3 (Phase 5)**: Depends on Phase 4 (US2 builds the multi-image component that US3 adds removal to)
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **US1 (P1)**: After Foundational → Builds updated endpoint + form with preparation field
- **US2 (P2)**: After US1 → Extends the single-image component to multi-image
- **US3 (P3)**: After US2 → Adds remove capability to the multi-image component built in US2

### Within Each User Story

- Tests written alongside implementation (constitution requires tests for all new code)
- Backend changes before frontend changes (API must exist for frontend to call)
- Models/DTOs before services before controllers before UI

### Parallel Opportunities

**Phase 1** (after T001–T003 are sequential):
- T004, T005, T006, T007, T008 can all run in parallel (different files)

**Phase 3 (US1)**:
- T016, T017 can run in parallel (different test methods)
- T022, T023 can run in parallel (different views)

**Phase 4 (US2)**:
- T025, T026, T027 can run in parallel (different test files/methods)

**Phase 5 (US3)**:
- T034 can run in parallel with other test writing

**Phase 6**:
- T037, T038, T043 can run in parallel (different files)

---

## Parallel Example: Phase 1 Setup

```text
Sequential (model must exist first):
  T001 → T002 → T003 (Recipe model → config → migration)

Then parallel:
  T004: RecipeCreateRequest DTO
  T005: RecipeUpdateRequest DTO
  T006: RecipeDto
  T007: RecipeImageExtractResult DTO
  T008: Frontend types
  T009: Backend mapping
```

## Parallel Example: User Story 2

```text
Parallel tests (write before/alongside implementation):
  T025: Multi-image unit test
  T026: Multi-file integration test
  T027: Validation integration test

Sequential implementation (component builds incrementally):
  T028 → T029 → T030 → T033 (ImageUpload refactor → thumbnails → extract button → validation)

Then:
  T031, T032 (RecipeCreateView updates)
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001–T009)
2. Complete Phase 2: Foundational (T010–T015)
3. Complete Phase 3: User Story 1 (T016–T024)
4. **STOP and VALIDATE**: Single image extraction with preparation field works end-to-end
5. Deploy/demo if ready — users already get preparation extraction from single images

### Incremental Delivery

1. Setup + Foundational → Schema and service ready
2. User Story 1 → Single-image extraction with preparation → **MVP deployed**
3. User Story 2 → Multi-image upload with previews → **Enhanced capability**
4. User Story 3 → Image removal before extraction → **Complete experience**
5. Polish → Memory cleanup, accessibility, full test validation

### Suggested MVP Scope

**User Story 1 only (T001–T024)** delivers the highest-value increment: preparation extraction from images. This is fully functional without multi-image support and addresses the core user need.

---

## Notes

- [P] tasks = different files, no dependencies on in-progress tasks
- [Story] label maps task to specific user story for traceability
- US2 depends on US1 because it extends the same endpoint and component
- US3 depends on US2 because it adds removal to the multi-image component
- Commit after each task or logical group
- Run `dotnet ef database update` after T003 to apply migration
