# Implementation Plan: Add Recipe Entry

**Branch**: `002-add-recipe-entry` | **Date**: 2026-03-20 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-add-recipe-entry/spec.md`

## Summary

Add the ability for users to create new recipes in their inventory via two methods: (1) manual data entry through a form, and (2) uploading an image of a recipe that is processed by a cloud AI vision API to extract recipe details. Also includes the ability to delete recipes (with cascade removal from weekly plan). The backend extends the existing .NET 10 API with new endpoints (POST create, POST image-extract, DELETE); the frontend adds a new Vue view for recipe creation with image upload support.

## Technical Context

**Language/Version**: C# / .NET 10 (backend), TypeScript ~5.4 / Vue.js 3.5 (frontend)  
**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core 10, Npgsql, Vue 3, Vue Router 4, Pinia 2, Cloud AI Vision API (e.g., OpenAI)  
**Storage**: PostgreSQL (existing, no schema changes — Recipe table already supports all needed fields)  
**Testing**: xUnit 2.9 + FluentAssertions 8.8 + WebApplicationFactory + InMemory DB (backend), Vitest 1.6 + Vue Test Utils 2.4 (frontend), Playwright 1.44 (E2E)  
**Target Platform**: Modern web browsers (Chrome, Firefox, Safari, Edge — latest 2 versions)  
**Project Type**: Web application (SPA frontend + REST API backend)  
**Performance Goals**: <500ms API response for create/delete, <10s for image extraction (cloud AI round-trip), <100ms UI interaction feedback  
**Constraints**: Image upload max 10 MB, supported formats JPEG/PNG/WebP, cloud AI API key managed server-side, WCAG 2.1 AA accessibility  
**Scale/Scope**: Single user MVP, extends existing 5-recipe seed data, adds 3 new API endpoints, 1 new Vue view, modifications to 2 existing views

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status | Notes |
|-----------|-------------|--------|-------|
| I. Code Quality | Readability, single responsibility, DRY, type safety | ✅ PASS | New service methods follow existing patterns; shared DTO for create/update; TypeScript strict mode |
| I. Code Quality | Consistent style, explicit error handling | ✅ PASS | Reuses existing exception middleware; new endpoints follow existing controller conventions |
| II. Testing | 80% coverage critical paths, test pyramid | ✅ PASS | Integration tests for new endpoints; unit tests for extraction service; E2E for image upload flow |
| II. Testing | Tests block merge, clear naming | ✅ PASS | CI pipeline enforces; test naming follows existing `should...When...` patterns |
| III. UX | Design system, responsive, loading states | ✅ PASS | Reuses existing form patterns from RecipeEditView; loading spinner for image extraction |
| III. UX | Error feedback, accessibility (WCAG 2.1 AA) | ✅ PASS | Validation messages, extraction failure fallback, ARIA labels on upload input, keyboard nav |
| IV. Performance | <2s FCP, <100ms interaction, <500ms API | ✅ PASS | Create/delete are simple DB ops; image extraction has separate timeout expectation (<10s) |
| IV. Performance | Bundle <200KB, no memory leaks, no N+1 | ✅ PASS | No new heavy dependencies; file input cleaned up on unmount; single DB query for delete with cascade |

**Gate Status**: ✅ PASS — No violations, proceed to Phase 0

### Post-Design Re-evaluation (Phase 1 Complete)

| Principle | Requirement | Status | Evidence |
|-----------|-------------|--------|----------|
| I. Code Quality | Type safety | ✅ PASS | `RecipeCreateRequest` and `RecipeImageExtractResult` are strongly-typed record DTOs with DataAnnotations; TypeScript interfaces mirror them |
| I. Code Quality | Single responsibility | ✅ PASS | `IRecipeImageExtractor` encapsulates AI integration separate from `IRecipeService`; `ImageUpload.vue` isolated from `RecipeForm.vue` |
| I. Code Quality | DRY enforcement | ✅ PASS | Shared `RecipeForm.vue` component for both create and edit flows; existing `apiClient.ts` extended (not duplicated) |
| I. Code Quality | Error handling | ✅ PASS | Explicit error responses for all extraction failure modes (400/413/422/502/503/504); frontend catches and displays actionable messages |
| II. Testing | Test pyramid | ✅ PASS | Unit tests for extraction service (mocked AI); integration tests for create/delete endpoints; E2E for image upload flow |
| II. Testing | 80% coverage | ✅ PASS | All new code paths have corresponding test cases in plan |
| III. UX | Loading states | ✅ PASS | Loading indicator during image extraction (FR-011); reuses existing `LoadingSpinner.vue` |
| III. UX | Error feedback | ✅ PASS | Extraction failure shows actionable alternatives (FR-014); validation messages for form fields; confirmation on success (FR-006) |
| III. UX | Accessibility (WCAG 2.1 AA) | ✅ PASS | File input with `aria-label`, `aria-live` for status, `aria-describedby` for errors; keyboard-navigable confirm dialog |
| III. UX | Design system compliance | ✅ PASS | Reuses existing form patterns from `RecipeEditView`; consistent button/input styling |
| IV. Performance | <500ms API | ✅ PASS | Create and delete are simple DB operations; extraction has separate 10s timeout |
| IV. Performance | No N+1 queries | ✅ PASS | Delete uses cascade (single query); create is a single INSERT |
| IV. Performance | Bundle size | ✅ PASS | New route lazy-loaded; `ImageUpload.vue` uses native APIs only; `OpenAI` NuGet is backend-only |
| IV. Performance | Memory management | ✅ PASS | `URL.revokeObjectURL()` cleanup on unmount; no file persistence in memory after extraction |

**Post-Design Gate Status**: ✅ PASS — Design adheres to constitution

## Project Structure

### Documentation (this feature)

```text
specs/002-add-recipe-entry/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── WhatsForDinner.Api/
│       ├── Controllers/
│       │   └── RecipesController.cs          # Add POST, POST upload, DELETE actions
│       ├── Models/
│       │   └── Dtos/
│       │       ├── RecipeCreateRequest.cs     # NEW: Create recipe DTO
│       │       └── RecipeImageExtractResult.cs  # NEW: Extraction result DTO
│       ├── Services/
│       │   ├── IRecipeService.cs              # Add CreateRecipeAsync, DeleteRecipeAsync
│       │   ├── RecipeService.cs               # Implement create, delete
│       │   ├── IRecipeImageExtractor.cs       # NEW: Image extraction interface
│       │   └── RecipeImageExtractor.cs        # NEW: Cloud AI vision implementation
│       ├── Data/
│       └── Program.cs                         # Register new services, configure HttpClient
└── tests/
    └── WhatsForDinner.Api.Tests/
        ├── Unit/
        │   └── RecipeImageExtractorTests.cs   # NEW: Extraction service unit tests
        └── Integration/
            └── RecipesControllerTests.cs      # Add create, delete, upload test cases

frontend/
├── src/
│   ├── components/
│   │   ├── RecipeForm.vue                     # NEW: Shared form for create/edit
│   │   ├── ImageUpload.vue                    # NEW: Image upload with preview
│   │   └── ConfirmDialog.vue                  # NEW: Reusable confirmation modal
│   ├── views/
│   │   ├── RecipeCreateView.vue               # NEW: Add recipe view (manual + image)
│   │   ├── RecipeListView.vue                 # Add "Add Recipe" button, delete action
│   │   └── WeeklyPlanView.vue                 # Add "Add Recipe" button
│   ├── stores/
│   │   └── recipeStore.ts                     # Add createRecipe, deleteRecipe actions
│   ├── services/
│   │   └── recipeService.ts                   # Add createRecipe, deleteRecipe, extractFromImage
│   ├── types/
│   │   └── Recipe.ts                          # Add RecipeCreateRequest, ImageExtractResponse types
│   └── router/
│       └── index.ts                           # Add /recipes/new route
├── tests/
│   ├── unit/
│   │   ├── components/
│   │   │   ├── RecipeForm.spec.ts             # NEW
│   │   │   ├── ImageUpload.spec.ts            # NEW
│   │   │   └── ConfirmDialog.spec.ts          # NEW
│   │   └── views/
│   │       └── RecipeCreateView.spec.ts       # NEW
│   └── e2e/
│       └── add-recipe.spec.ts                 # NEW: E2E for add recipe flows
```

**Structure Decision**: Extends the existing web application structure. Backend follows the existing Controller → Service → DbContext pattern. New `IRecipeImageExtractor` service encapsulates the cloud AI integration behind an interface for testability. Frontend reuses existing form patterns and introduces reusable components (form, upload, confirm dialog).

## Complexity Tracking

> No complexity violations to justify — design follows existing patterns and constitution principles. The new `IRecipeImageExtractor` service adds one interface but follows the same DI pattern already used for `IRecipeService` and `IWeeklyPlanService`.
