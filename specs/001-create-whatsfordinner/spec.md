# Feature Specification: Create WhatsForDinner

**Feature Branch**: `001-create-whatsfordinner`  
**Created**: 2026-03-04  
**Status**: Draft  
**Input**: User description: "Develop WhatsForDinner, an application that helps people create a meal plan for the week based on their recipes. Users should be able to add and edit recipes in their list and they should also be able to add or remove recipes to the current week. For this first phase of the project, which we will call 'Create WhatsForDinner' we will create one predefined user with 5 different recipes assigned to it, no login will be required at this point since we are just implementing the basic functionality."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Weekly Meal Plan (Priority: P1)

As a user, when I launch the application, I want to see the list of recipes currently assigned to this week's meal plan so I can quickly review what meals are planned.

**Why this priority**: This is the first screen users see and represents the core value proposition - knowing what's planned for the week. Without this, users cannot interact with the application meaningfully.

**Independent Test**: Can be fully tested by launching the application and verifying the weekly meal plan displays with any assigned recipes. Delivers immediate visibility into the week's meals.

**Acceptance Scenarios**:

1. **Given** the application is launched for the first time, **When** the main screen loads, **Then** the user sees the weekly meal plan view (initially empty or with pre-seeded recipes)
2. **Given** recipes have been added to the week, **When** the user views the main screen, **Then** all assigned recipes are displayed in a list format
3. **Given** the weekly meal plan is empty, **When** the user views the main screen, **Then** an appropriate empty state message is displayed indicating no recipes are planned

---

### User Story 2 - Add Recipe to Weekly Plan (Priority: P2)

As a user, I want to add recipes from my available recipes to the current week's meal plan so I can build my weekly menu.

**Why this priority**: This is the primary action that enables meal planning. Without adding recipes, the weekly plan would remain empty and the app would not fulfill its core purpose.

**Independent Test**: Can be fully tested by clicking the add button, selecting a recipe from the list, and verifying it appears in the weekly plan. Delivers the ability to plan meals.

**Acceptance Scenarios**:

1. **Given** the user is viewing the weekly meal plan, **When** they click the "Add" button, **Then** a list of available recipes is displayed
2. **Given** the available recipes list is displayed, **When** the user clicks on a recipe, **Then** that recipe is added to the weekly meal plan
3. **Given** a recipe is selected from the available list, **When** the addition completes, **Then** the user returns to the weekly meal plan view and sees the newly added recipe
4. **Given** the user has the available recipes list open, **When** they decide not to add anything, **Then** they can dismiss the list and return to the weekly plan unchanged

---

### User Story 3 - Remove Recipe from Weekly Plan (Priority: P3)

As a user, I want to remove recipes from the current week's meal plan so I can adjust my menu when plans change.

**Why this priority**: Essential for managing the weekly plan. Users need both add and remove capabilities for a functional planning experience.

**Independent Test**: Can be fully tested by having recipes in the weekly plan, clicking remove, and verifying the recipe is no longer displayed. Delivers plan flexibility.

**Acceptance Scenarios**:

1. **Given** the weekly meal plan has recipes, **When** the user clicks the "Remove" button on a recipe, **Then** that recipe is removed from the weekly plan
2. **Given** a recipe is removed, **When** the removal completes, **Then** the recipe no longer appears in the weekly meal plan view
3. **Given** the weekly meal plan becomes empty after removal, **When** the view updates, **Then** the empty state message is displayed

---

### User Story 4 - View Available Recipes (Priority: P4)

As a user, I want to view all my available recipes so I can see what options I have for meal planning and browse my recipe collection.

**Why this priority**: Supports the add-to-week functionality and provides visibility into the recipe catalog. Users need to know what recipes exist before they can plan.

**Independent Test**: Can be fully tested by navigating to the recipe list and verifying all predefined recipes are displayed. Delivers recipe visibility.

**Acceptance Scenarios**:

1. **Given** the user wants to browse recipes, **When** they access the recipe list (via add flow or direct navigation), **Then** all available recipes are displayed
2. **Given** the predefined user exists, **When** viewing available recipes, **Then** all 5 predefined recipes are shown with their names

---

### User Story 5 - Edit Recipe Details (Priority: P5)

As a user, I want to edit the details of my recipes so I can update names, descriptions, ingredients, or cook times as needed.

**Why this priority**: Secondary feature for recipe management. Core meal planning works without editing; this enhances the user's ability to maintain their recipe collection.

**Independent Test**: Can be fully tested by selecting a recipe, editing its details (name, description, ingredients, cook time), saving, and verifying changes persist. Delivers recipe customization.

**Acceptance Scenarios**:

1. **Given** a recipe exists, **When** the user chooses to edit it, **Then** an edit view is displayed with the current recipe details (name, description, ingredients, cook time)
2. **Given** the user is editing a recipe, **When** they modify any field and save, **Then** the updated values are persisted and displayed
3. **Given** the user is editing a recipe, **When** they cancel the edit, **Then** no changes are saved and the original details remain

---

### Edge Cases

- What happens when the user tries to add a recipe that's already in the weekly plan? (Assumed: Allow duplicates - user may want the same meal multiple times per week)
- What happens when the user tries to remove from an empty weekly plan? (Remove button should be disabled or hidden when no recipes exist)
- What happens when viewing recipes and no recipes exist? (For MVP, this won't occur since 5 recipes are predefined; display appropriate empty state for future-proofing)

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a weekly meal plan view as the main screen when the application launches
- **FR-002**: System MUST show all recipes currently assigned to the weekly plan in a list format
- **FR-003**: System MUST provide an "Add" button visible on the weekly meal plan screen
- **FR-004**: System MUST display a list of available recipes when the Add button is clicked
- **FR-005**: System MUST add a selected recipe to the weekly plan when clicked from the available recipes list
- **FR-006**: System MUST provide a "Remove" button/action for each recipe in the weekly plan
- **FR-007**: System MUST remove a recipe from the weekly plan when the Remove action is triggered
- **FR-008**: System MUST persist the weekly meal plan state across sessions via PostgreSQL database
- **FR-009**: System MUST include one predefined user with 5 different recipes pre-seeded
- **FR-010**: System MUST allow users to edit recipe details (name, description, ingredients, cook time)
- **FR-011**: System MUST display an appropriate empty state when no recipes are in the weekly plan

### Assumptions

- Single predefined user - no authentication or user management required for MVP
- The 5 predefined recipes will be sample meal recipes (e.g., "Spaghetti Bolognese", "Grilled Chicken Salad", etc.)
- "Current week" refers to a single week context without date-specific scheduling (recipes are planned for "this week" as a whole, not specific days)
- **Data Persistence**: PostgreSQL database via .NET 10 backend API - data persists across sessions and devices
- Duplicate recipes in the same week are allowed (user may eat the same meal multiple times)
- **Platform**: Web application (browser-based, responsive design) - works across desktop and mobile browsers

### Key Entities

- **User**: Represents a person using the application; has a collection of recipes and a weekly meal plan. For MVP, one predefined user exists.
- **Recipe**: A meal that can be planned; attributes include name, description, ingredients list, and cook time. 5 recipes are predefined for the MVP user.
- **Weekly Plan**: The collection of recipes assigned to the current week for a user; can contain zero or more recipes (including duplicates).

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view their weekly meal plan within 2 seconds of launching the application
- **SC-002**: Users can add a recipe to the weekly plan in 3 clicks or fewer (Add button → Select recipe → Confirmation)
- **SC-003**: Users can remove a recipe from the weekly plan in 2 clicks or fewer
- **SC-004**: 100% of the 5 predefined recipes are visible and selectable in the available recipes list
- **SC-005**: Users can complete a full planning workflow (add 3+ recipes, remove 1) in under 60 seconds
- **SC-006**: The application displays clear feedback for empty states (no recipes planned)

## Clarifications

### Session 2026-03-05

- Q: What type of application should WhatsForDinner be? → A: Web application (browser-based, responsive design)
- Q: How should the weekly meal plan data be persisted? → A: PostgreSQL database via .NET 10 backend API (updated from initial local storage decision)
- Q: What information should each recipe contain? → A: Name + description + ingredients list + cook time
- Q: Should recipes be assigned to specific days? → A: Unordered list (bag of recipes for the week, no day assignment)
