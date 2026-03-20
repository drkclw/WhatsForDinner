````markdown
# Tasks: Create WhatsForDinner

**Input**: Design documents from `/specs/001-create-whatsfordinner/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/openapi.yaml, quickstart.md

**Tests**: Included per constitution compliance (II. Testing Standards). Test tasks follow implementation tasks for each user story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Backend**: `backend/src/WhatsForDinner.Api/`
- **Backend Tests**: `backend/tests/WhatsForDinner.Api.Tests/`
- **Frontend**: `frontend/src/`
- **Frontend Tests**: `frontend/tests/`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create backend solution structure with `dotnet new webapi` in backend/src/WhatsForDinner.Api/
- [X] T002 Create backend test project with `dotnet new xunit` in backend/tests/WhatsForDinner.Api.Tests/
- [X] T003 Create frontend Vue.js project with `npm create vue@latest` in frontend/
- [X] T004 [P] Configure .editorconfig for code style consistency
- [X] T005 [P] Configure backend appsettings.json and appsettings.Development.json in backend/src/WhatsForDinner.Api/
- [X] T006 [P] Configure frontend .env.development with VITE_API_BASE_URL in frontend/

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Foundation

- [X] T007 Add NuGet packages: Npgsql.EntityFrameworkCore.PostgreSQL, FluentAssertions in backend/src/WhatsForDinner.Api/WhatsForDinner.Api.csproj
- [X] T008 [P] Create User entity model in backend/src/WhatsForDinner.Api/Models/User.cs
- [X] T009 [P] Create Recipe entity model in backend/src/WhatsForDinner.Api/Models/Recipe.cs
- [X] T010 [P] Create WeeklyPlan entity model in backend/src/WhatsForDinner.Api/Models/WeeklyPlan.cs
- [X] T011 [P] Create WeeklyPlanItem entity model in backend/src/WhatsForDinner.Api/Models/WeeklyPlanItem.cs
- [X] T012 Create ApplicationDbContext with entity configurations in backend/src/WhatsForDinner.Api/Data/ApplicationDbContext.cs
- [X] T013 Create UserConfiguration with seed data in backend/src/WhatsForDinner.Api/Data/Configurations/UserConfiguration.cs
- [X] T014 [P] Create RecipeConfiguration with seed data (5 recipes) in backend/src/WhatsForDinner.Api/Data/Configurations/RecipeConfiguration.cs
- [X] T015 [P] Create WeeklyPlanConfiguration with seed data in backend/src/WhatsForDinner.Api/Data/Configurations/WeeklyPlanConfiguration.cs
- [X] T016 [P] Create WeeklyPlanItemConfiguration in backend/src/WhatsForDinner.Api/Data/Configurations/WeeklyPlanItemConfiguration.cs
- [X] T017 Configure Program.cs with DbContext, CORS, and middleware in backend/src/WhatsForDinner.Api/Program.cs
- [X] T018 Create initial EF Core migration in backend/src/WhatsForDinner.Api/

### DTO Models

- [X] T019 [P] Create RecipeDto in backend/src/WhatsForDinner.Api/Models/Dtos/RecipeDto.cs
- [X] T020 [P] Create RecipeUpdateRequest in backend/src/WhatsForDinner.Api/Models/Dtos/RecipeUpdateRequest.cs
- [X] T021 [P] Create WeeklyPlanDto in backend/src/WhatsForDinner.Api/Models/Dtos/WeeklyPlanDto.cs
- [X] T022 [P] Create WeeklyPlanItemDto in backend/src/WhatsForDinner.Api/Models/Dtos/WeeklyPlanItemDto.cs
- [X] T023 [P] Create AddToWeeklyPlanRequest in backend/src/WhatsForDinner.Api/Models/Dtos/AddToWeeklyPlanRequest.cs
- [X] T024 [P] Create ErrorResponse and ValidationErrorResponse in backend/src/WhatsForDinner.Api/Models/Dtos/ErrorResponses.cs

### Frontend Foundation

- [X] T025 Install frontend dependencies: vue-router, pinia, typescript in frontend/
- [X] T026 [P] Create Recipe TypeScript interface in frontend/src/types/Recipe.ts
- [X] T027 [P] Create WeeklyPlan TypeScript interface in frontend/src/types/WeeklyPlan.ts
- [X] T028 [P] Create WeeklyPlanItem TypeScript interface in frontend/src/types/WeeklyPlanItem.ts
- [X] T029 [P] Create ApiError types in frontend/src/types/ApiError.ts
- [X] T030 Create API client wrapper service in frontend/src/services/apiClient.ts
- [X] T031 Configure Vue Router in frontend/src/router/index.ts
- [X] T032 Configure Pinia store setup in frontend/src/stores/index.ts
- [X] T033 Create base App.vue layout with navigation in frontend/src/App.vue
- [X] T034 Configure Vite proxy for development API in frontend/vite.config.ts

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - View Weekly Meal Plan (Priority: P1) 🎯 MVP

**Goal**: Display the weekly meal plan as the main screen when the application launches

**Independent Test**: Launch application, verify weekly meal plan view loads with any assigned recipes or empty state message

### Backend Implementation for User Story 1

- [X] T035 Create IWeeklyPlanService interface in backend/src/WhatsForDinner.Api/Services/IWeeklyPlanService.cs
- [X] T036 Implement WeeklyPlanService.GetWeeklyPlanAsync in backend/src/WhatsForDinner.Api/Services/WeeklyPlanService.cs
- [X] T037 Create WeeklyPlanController with GET /api/weekly-plan in backend/src/WhatsForDinner.Api/Controllers/WeeklyPlanController.cs
- [X] T038 Register WeeklyPlanService in DI container in backend/src/WhatsForDinner.Api/Program.cs

### Frontend Implementation for User Story 1

- [X] T039 Create weeklyPlanService with getWeeklyPlan method in frontend/src/services/weeklyPlanService.ts
- [X] T040 Create useWeeklyPlanStore Pinia store in frontend/src/stores/weeklyPlanStore.ts
- [X] T041 Create WeeklyPlanView component in frontend/src/views/WeeklyPlanView.vue
- [X] T042 [P] Create RecipeCard component for displaying recipe in plan in frontend/src/components/RecipeCard.vue
- [X] T043 [P] Create EmptyState component for empty plan message in frontend/src/components/EmptyState.vue
- [X] T044 [P] Create LoadingSpinner component in frontend/src/components/LoadingSpinner.vue
- [X] T045 Add WeeklyPlanView route as home page in frontend/src/router/index.ts

### Tests for User Story 1 (Constitution Compliance)

- [X] T045a [P] [US1] Unit test WeeklyPlanService.GetWeeklyPlanAsync in backend/tests/WhatsForDinner.Api.Tests/Unit/Services/WeeklyPlanServiceTests.cs
- [X] T045b [P] [US1] Integration test GET /api/weekly-plan in backend/tests/WhatsForDinner.Api.Tests/Integration/WeeklyPlanControllerTests.cs
- [ ] T045c [P] [US1] Component test WeeklyPlanView in frontend/tests/unit/views/WeeklyPlanView.spec.ts

**Checkpoint**: User Story 1 complete - Weekly meal plan view is functional and testable independently

---

## Phase 4: User Story 2 - Add Recipe to Weekly Plan (Priority: P2)

**Goal**: Allow users to add recipes from available list to the weekly meal plan

**Independent Test**: Click add button, select recipe from list, verify recipe appears in weekly plan

### Backend Implementation for User Story 2

- [X] T046 Add AddRecipeToWeeklyPlanAsync method to IWeeklyPlanService in backend/src/WhatsForDinner.Api/Services/IWeeklyPlanService.cs
- [X] T047 Implement AddRecipeToWeeklyPlanAsync in WeeklyPlanService in backend/src/WhatsForDinner.Api/Services/WeeklyPlanService.cs
- [X] T048 Add POST /api/weekly-plan/items endpoint to WeeklyPlanController in backend/src/WhatsForDinner.Api/Controllers/WeeklyPlanController.cs

### Frontend Implementation for User Story 2

- [X] T049 Create recipeService with getRecipes method in frontend/src/services/recipeService.ts
- [X] T050 Create useRecipeStore Pinia store in frontend/src/stores/recipeStore.ts
- [X] T051 Add addToWeeklyPlan method to weeklyPlanService in frontend/src/services/weeklyPlanService.ts
- [X] T052 Add addRecipe action to useWeeklyPlanStore in frontend/src/stores/weeklyPlanStore.ts
- [X] T053 Create RecipeSelectionModal component in frontend/src/components/RecipeSelectionModal.vue
- [X] T054 [P] Create RecipeListItem component for selection in frontend/src/components/RecipeListItem.vue
- [X] T055 Add "Add Recipe" button and modal integration to WeeklyPlanView in frontend/src/views/WeeklyPlanView.vue

### Tests for User Story 2 (Constitution Compliance)

- [X] T055a [P] [US2] Unit test WeeklyPlanService.AddRecipeToWeeklyPlanAsync in backend/tests/WhatsForDinner.Api.Tests/Unit/Services/WeeklyPlanServiceTests.cs
- [X] T055b [P] [US2] Integration test POST /api/weekly-plan/items in backend/tests/WhatsForDinner.Api.Tests/Integration/WeeklyPlanControllerTests.cs
- [ ] T055c [P] [US2] Component test RecipeSelectionModal in frontend/tests/unit/components/RecipeSelectionModal.spec.ts

**Checkpoint**: User Story 2 complete - Users can add recipes to weekly plan

---

## Phase 5: User Story 3 - Remove Recipe from Weekly Plan (Priority: P3)

**Goal**: Allow users to remove recipes from the weekly meal plan

**Independent Test**: With recipes in plan, click remove button, verify recipe no longer displayed

### Backend Implementation for User Story 3

- [X] T056 Add RemoveFromWeeklyPlanAsync method to IWeeklyPlanService in backend/src/WhatsForDinner.Api/Services/IWeeklyPlanService.cs
- [X] T057 Implement RemoveFromWeeklyPlanAsync in WeeklyPlanService in backend/src/WhatsForDinner.Api/Services/WeeklyPlanService.cs
- [X] T058 Add DELETE /api/weekly-plan/items/{id} endpoint to WeeklyPlanController in backend/src/WhatsForDinner.Api/Controllers/WeeklyPlanController.cs

### Frontend Implementation for User Story 3

- [X] T059 Add removeFromWeeklyPlan method to weeklyPlanService in frontend/src/services/weeklyPlanService.ts
- [X] T060 Add removeRecipe action to useWeeklyPlanStore in frontend/src/stores/weeklyPlanStore.ts
- [X] T061 Add remove button with confirmation to RecipeCard component in frontend/src/components/RecipeCard.vue

### Tests for User Story 3 (Constitution Compliance)

- [X] T061a [P] [US3] Unit test WeeklyPlanService.RemoveFromWeeklyPlanAsync in backend/tests/WhatsForDinner.Api.Tests/Unit/Services/WeeklyPlanServiceTests.cs
- [X] T061b [P] [US3] Integration test DELETE /api/weekly-plan/items/{id} in backend/tests/WhatsForDinner.Api.Tests/Integration/WeeklyPlanControllerTests.cs

**Checkpoint**: User Story 3 complete - Users can remove recipes from weekly plan

---

## Phase 6: User Story 4 - View Available Recipes (Priority: P4)

**Goal**: Display all available recipes for browsing and selection

**Independent Test**: Navigate to recipe list, verify all 5 predefined recipes are displayed

### Backend Implementation for User Story 4

- [X] T062 Create IRecipeService interface in backend/src/WhatsForDinner.Api/Services/IRecipeService.cs
- [X] T063 Implement RecipeService.GetRecipesAsync in backend/src/WhatsForDinner.Api/Services/RecipeService.cs
- [X] T064 Create RecipesController with GET /api/recipes in backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs
- [X] T065 Register RecipeService in DI container in backend/src/WhatsForDinner.Api/Program.cs

### Frontend Implementation for User Story 4

- [X] T066 Ensure recipeService includes fetchAll capability (created in T049) in frontend/src/services/recipeService.ts
- [X] T067 Add fetchRecipes action to useRecipeStore in frontend/src/stores/recipeStore.ts
- [X] T068 Create RecipeListView component in frontend/src/views/RecipeListView.vue
- [X] T069 Add RecipeListView route to router in frontend/src/router/index.ts
- [X] T070 Add navigation link to recipe list in App.vue in frontend/src/App.vue

### Tests for User Story 4 (Constitution Compliance)

- [X] T070a [P] [US4] Unit test RecipeService.GetRecipesAsync in backend/tests/WhatsForDinner.Api.Tests/Unit/Services/RecipeServiceTests.cs
- [X] T070b [P] [US4] Integration test GET /api/recipes in backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs
- [ ] T070c [P] [US4] Component test RecipeListView in frontend/tests/unit/views/RecipeListView.spec.ts

**Checkpoint**: User Story 4 complete - Users can browse all available recipes

---

## Phase 7: User Story 5 - Edit Recipe Details (Priority: P5)

**Goal**: Allow users to edit recipe details (name, description, ingredients, cook time)

**Independent Test**: Select recipe, edit details, save, verify changes persist

### Backend Implementation for User Story 5

- [X] T071 Add GetRecipeByIdAsync method to IRecipeService in backend/src/WhatsForDinner.Api/Services/IRecipeService.cs
- [X] T072 Add UpdateRecipeAsync method to IRecipeService in backend/src/WhatsForDinner.Api/Services/IRecipeService.cs
- [X] T073 Implement GetRecipeByIdAsync in RecipeService in backend/src/WhatsForDinner.Api/Services/RecipeService.cs
- [X] T074 Implement UpdateRecipeAsync in RecipeService in backend/src/WhatsForDinner.Api/Services/RecipeService.cs
- [X] T075 Add GET /api/recipes/{id} endpoint to RecipesController in backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs
- [X] T076 Add PUT /api/recipes/{id} endpoint to RecipesController in backend/src/WhatsForDinner.Api/Controllers/RecipesController.cs

### Frontend Implementation for User Story 5

- [X] T077 Add getRecipeById method to recipeService in frontend/src/services/recipeService.ts
- [X] T078 Add updateRecipe method to recipeService in frontend/src/services/recipeService.ts
- [X] T079 Add updateRecipe action to useRecipeStore in frontend/src/stores/recipeStore.ts
- [X] T080 Create RecipeEditView component with form in frontend/src/views/RecipeEditView.vue
- [X] T081 Add RecipeEditView route with :id param to router in frontend/src/router/index.ts
- [X] T082 Add edit link/button to RecipeCard and RecipeListItem in frontend/src/components/RecipeCard.vue
- [X] T083 Add validation feedback for form fields in frontend/src/views/RecipeEditView.vue

### Tests for User Story 5 (Constitution Compliance)

- [X] T083a [P] [US5] Unit test RecipeService.GetRecipeByIdAsync in backend/tests/WhatsForDinner.Api.Tests/Unit/Services/RecipeServiceTests.cs
- [X] T083b [P] [US5] Unit test RecipeService.UpdateRecipeAsync in backend/tests/WhatsForDinner.Api.Tests/Unit/Services/RecipeServiceTests.cs
- [X] T083c [P] [US5] Integration test GET /api/recipes/{id} in backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs
- [X] T083d [P] [US5] Integration test PUT /api/recipes/{id} in backend/tests/WhatsForDinner.Api.Tests/Integration/RecipesControllerTests.cs
- [ ] T083e [P] [US5] Component test RecipeEditView in frontend/tests/unit/views/RecipeEditView.spec.ts

**Checkpoint**: User Story 5 complete - Users can edit recipe details

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T084 [P] Add global exception handling middleware in backend/src/WhatsForDinner.Api/Middleware/ExceptionHandlingMiddleware.cs
- [X] T085 [P] Add request logging middleware in backend/src/WhatsForDinner.Api/Middleware/RequestLoggingMiddleware.cs
- [X] T086 [P] Add loading states to all async operations in frontend views
- [X] T087 [P] Add error toast/notification component in frontend/src/components/ErrorNotification.vue
- [X] T088 [P] Add ARIA attributes and keyboard navigation for accessibility in frontend components
- [X] T089 [P] Add responsive CSS styles for mobile/tablet/desktop in frontend/src/assets/styles/
- [ ] T090 Validate SC-001: Measure initial page load <2s using Lighthouse or browser DevTools
- [ ] T091 Validate SC-005: Manual test full planning workflow (add 3 recipes, remove 1) completes <60s
- [ ] T092 Run quickstart.md validation to verify setup instructions work correctly
- [X] T093 Final code cleanup and remove any TODO comments

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can then proceed in priority order (P1 → P2 → P3 → P4 → P5)
  - Or in parallel if multiple developers available
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Depends on US1 frontend components (RecipeCard, WeeklyPlanView)
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Depends on US1 frontend components (RecipeCard)
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Backend can run in parallel; frontend shares recipeService with US2
- **User Story 5 (P5)**: Can start after Foundational (Phase 2) - Depends on US4 backend (RecipeService foundation)

### Parallel Opportunities by Phase

**Phase 1 Setup**: T004, T005, T006 can run in parallel after T001-T003

**Phase 2 Foundational**:
- T008, T009, T010, T011 (entity models) - all parallel
- T013, T014, T015, T016 (configurations) - all parallel after T012
- T019-T024 (DTOs) - all parallel
- T026, T027, T028, T029 (TypeScript types) - all parallel

**Phase 3 US1**: T042, T043, T044 can run in parallel after T041 started

**Phase 4 US2**: T054 can run in parallel with T053

**Phase 8 Polish**: T084-T089 can all run in parallel

---

## Parallel Example: Foundational Entity Models

```bash
# Launch all entity models together (T008-T011):
Task: "Create User entity model in backend/src/WhatsForDinner.Api/Models/User.cs"
Task: "Create Recipe entity model in backend/src/WhatsForDinner.Api/Models/Recipe.cs"
Task: "Create WeeklyPlan entity model in backend/src/WhatsForDinner.Api/Models/WeeklyPlan.cs"
Task: "Create WeeklyPlanItem entity model in backend/src/WhatsForDinner.Api/Models/WeeklyPlanItem.cs"
```

## Parallel Example: User Story 1 Components

```bash
# After WeeklyPlanView started, launch these in parallel (T042-T044):
Task: "Create RecipeCard component in frontend/src/components/RecipeCard.vue"
Task: "Create EmptyState component in frontend/src/components/EmptyState.vue"
Task: "Create LoadingSpinner component in frontend/src/components/LoadingSpinner.vue"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready - users can view weekly meal plan

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP - View Plan!)
3. Add User Story 2 → Test independently → Deploy/Demo (Can add recipes!)
4. Add User Story 3 → Test independently → Deploy/Demo (Can remove recipes!)
5. Add User Story 4 → Test independently → Deploy/Demo (Can browse recipes!)
6. Add User Story 5 → Test independently → Deploy/Demo (Can edit recipes!)
7. Each story adds value without breaking previous stories

### Suggested MVP Scope

**Minimal MVP**: User Story 1 (View Weekly Plan)
- Users can see what's planned for the week
- Delivers core value proposition immediately

**Recommended MVP**: User Stories 1-3 (View + Add + Remove)
- Complete meal planning workflow
- Users can manage their weekly plan

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Backend uses controller-based architecture per research.md decisions
- Frontend uses Vue 3 Composition API with `<script setup>` syntax
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence

````
