# Implementation Plan: Multi-Image Recipe Extraction with Preparation

**Branch**: `003-multi-image-extraction` | **Date**: 2026-04-12 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/003-multi-image-extraction/spec.md`

## Summary

Extend the recipe image extraction system to support uploading 1–5 images per extraction request and add a new "preparation" field to the extraction output and Recipe model. All images are sent in a single OpenAI Chat API request so the AI can see full context across pages. The frontend ImageUpload component becomes a multi-image manager with preview thumbnails and individual removal. The backend endpoint changes from single-file to multi-file upload, and the AI prompt/schema are updated to include preparation.

## Technical Context

**Language/Version**: C# / .NET 10 (backend), TypeScript / Vue.js 3.x (frontend)
**Primary Dependencies**: ASP.NET Core Web API, Entity Framework Core (Npgsql), OpenAI SDK 2.x, Vue.js 3, Vue Router, Pinia
**Storage**: PostgreSQL (existing `whatsfordinner` database)
**Testing**: xUnit + FluentAssertions (backend), Vitest + Vue Test Utils (frontend), Playwright (E2E)
**Target Platform**: Modern web browsers (Chrome, Firefox, Safari, Edge — latest 2 versions)
**Project Type**: Web application (SPA frontend + REST API backend)
**Performance Goals**: <60s extraction for up to 5 images, <500ms API response for non-AI endpoints, <100ms UI interaction feedback
**Constraints**: 10 MB per image, 50 MB total, 5 images max; 30s timeout per AI request (may need increase for multi-image); preparation field max 10,000 characters
**Scale/Scope**: Single user, extends existing recipe CRUD and extraction features

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Requirement | Status | Notes |
|-----------|-------------|--------|-------|
| I. Code Quality | Readability, single responsibility, DRY, type safety | ✅ PASS | Extends existing service/controller pattern; C# strong typing, TypeScript strict mode |
| I. Code Quality | Consistent style, explicit error handling | ✅ PASS | Follows existing middleware error handling; no new patterns introduced |
| II. Testing | 80% coverage critical paths, test pyramid | ✅ PASS | Unit tests for BuildResult/merge logic, integration tests for multi-file endpoint, E2E for upload flow |
| II. Testing | Tests block merge, clear naming | ✅ PASS | CI pipeline enforced |
| III. UX | Design system, responsive, loading states | ✅ PASS | Multi-image thumbnails follow existing card patterns; spinner with count message |
| III. UX | Error feedback, accessibility (WCAG 2.1 AA) | ✅ PASS | Clear error messages for validation failures; thumbnail grid keyboard-navigable |
| IV. Performance | <500ms API (non-AI), <100ms interaction | ✅ PASS | Validation is instant; AI call is expected longer but has loading indicator |
| IV. Performance | Bundle <200KB, no memory leaks, no N+1 | ✅ PASS | No new routes/pages; image previews use object URLs with cleanup |

**Gate Status**: ✅ PASS — No violations, proceed to Phase 0

### Post-Design Re-evaluation (Phase 1 Complete)

| Principle | Requirement | Status | Evidence |
|-----------|-------------|--------|----------|
| I. Code Quality | Type safety | ✅ PASS | C# entities with EF Core config, TypeScript interfaces updated in contracts, strongly-typed DTOs |
| I. Code Quality | Single responsibility | ✅ PASS | Extractor service handles AI interaction; controller handles HTTP/validation; no new classes needed |
| I. Code Quality | DRY enforcement | ✅ PASS | Single extraction method handles 1–5 images; no duplicate code paths for single vs multi |
| II. Testing | Test pyramid | ✅ PASS | Unit tests for BuildResult with preparation, integration tests for multi-file endpoint, E2E for upload flow |
| III. UX | Loading states | ✅ PASS | Spinner with "Extracting from N images..." message (per spec clarification) |
| III. UX | Error feedback | ✅ PASS | Per-file validation errors, partial failure notification, clear messages in contracts |
| III. UX | Accessibility | ✅ PASS | Thumbnail grid with keyboard navigation, remove buttons for each image |
| IV. Performance | No N+1 queries | ✅ PASS | Single new column added; no new queries or joins |
| IV. Performance | Bundle size | ✅ PASS | No new dependencies; image previews use native URL.createObjectURL |
| IV. Performance | No memory leaks | ✅ PASS | Object URLs revoked on removal/unmount; specified in research R6 |

**Post-Design Gate Status**: ✅ PASS — Design adheres to constitution

## Project Structure

### Documentation (this feature)

```text
specs/003-multi-image-extraction/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/
│   └── WhatsForDinner.Api/
│       ├── Controllers/
│       │   └── RecipesController.cs          # Update: multi-file endpoint
│       ├── Models/
│       │   ├── Recipe.cs                     # Update: add Preparation field
│       │   └── Dtos/
│       │       ├── RecipeCreateRequest.cs     # Update: add Preparation
│       │       ├── RecipeUpdateRequest.cs     # Update: add Preparation
│       │       └── RecipeImageExtractResult.cs # Update: add Preparation
│       ├── Services/
│       │   ├── IRecipeImageExtractor.cs       # Update: multi-image signature
│       │   └── RecipeImageExtractor.cs        # Update: multi-image + preparation prompt
│       ├── Data/
│       │   └── Configurations/                # Update: Preparation column config
│       └── Migrations/                        # New: AddPreparationField migration
└── tests/
    └── WhatsForDinner.Api.Tests/
        ├── Unit/                              # Update: BuildResult tests for preparation
        └── Integration/                       # Update: multi-file upload tests

frontend/
├── src/
│   ├── components/
│   │   ├── ImageUpload.vue                    # Update: multi-image with previews
│   │   └── RecipeForm.vue                     # Update: add Preparation field
│   ├── views/
│   │   └── RecipeCreateView.vue               # Update: multi-file extraction flow
│   ├── services/
│   │   └── recipeService.ts                   # Update: multi-file FormData
│   └── types/
│       └── Recipe.ts                          # Update: add preparation to types
└── tests/
    ├── unit/                                  # Update: component tests
    └── e2e/                                   # Update: E2E extraction flow
```

**Structure Decision**: No new directories or projects needed. This feature modifies existing files across both backend and frontend projects.
