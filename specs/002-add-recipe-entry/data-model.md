# Data Model: Add Recipe Entry

**Feature**: 002-add-recipe-entry  
**Date**: 2026-03-20  
**Source**: [spec.md](spec.md) Key Entities section

## Entity Relationship Diagram

```
┌─────────────┐       1:N       ┌─────────────┐
│    User     │────────────────▶│   Recipe    │
└─────────────┘                 └─────────────┘
       │                               │
       │ 1:1                           │
       ▼                               │
┌─────────────┐                        │
│ WeeklyPlan  │                        │
└─────────────┘                        │
       │                               │
       │ 1:N                           │
       ▼                               │
┌─────────────────┐      N:1          │
│ WeeklyPlanItem  │───────────────────┘
└─────────────────┘
```

> **No schema changes required.** The existing `recipes` table already has all the fields needed for recipe creation (name, description, ingredients, cook_time_minutes). This feature adds new API operations (create, delete) on the existing schema.

## Entities (unchanged from 001)

### Recipe

A meal entry in a user's inventory. This feature adds creation and deletion capabilities.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | int | PK, auto-increment | Unique identifier |
| UserId | int | FK → User.Id, Required | Owner of the recipe |
| Name | string | Required, max 200 chars | Recipe title |
| Description | string | Optional, max 1000 chars | Brief description of the dish |
| Ingredients | string | Optional, max 2000 chars | Ingredients as free-text (single textarea) |
| CookTimeMinutes | int | Optional, min 0 | Cooking time in minutes |
| CreatedAt | DateTime | Required, default NOW | Creation timestamp |
| UpdatedAt | DateTime | Required, default NOW | Last modification timestamp |

**Relationships**:
- Belongs to User (N:1)
- Can appear in WeeklyPlanItems (1:N)
- WeeklyPlanItems cascade-delete when Recipe is deleted (ON DELETE CASCADE)

**Validation Rules** (for create):
- Name is required, must be non-empty, max 200 characters
- Description is optional, max 1000 characters
- Ingredients is optional, max 2000 characters
- CookTimeMinutes must be a non-negative integer if provided (≥ 0)
- Duplicate names are allowed

**State Transitions (new)**:

```
                        ┌──────────────┐
         create         │              │      delete
  ────────────────────▶ │   Existing   │ ─────────────────▶ (removed)
  (manual or image)     │              │   (cascade removes
                        └──────────────┘    WeeklyPlanItems)
                               │
                               │ update (existing)
                               ▼
                        ┌──────────────┐
                        │   Updated    │
                        └──────────────┘
```

## New DTOs

### RecipeCreateRequest

Used for both manual entry and post-image-extraction submission. Same shape as `RecipeUpdateRequest`.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Name | string | Required, min 1, max 200 | Recipe title |
| Description | string? | Optional, max 1000 | Brief description |
| Ingredients | string? | Optional, max 2000 | Ingredients as free-text |
| CookTimeMinutes | int? | Optional, ≥ 1 | Cooking time in minutes |

> **Note**: `RecipeCreateRequest` has the same fields as the existing `RecipeUpdateRequest`. Consider using a single shared DTO (e.g., `RecipeSaveRequest`) or keeping them separate for independent evolution. Recommendation: keep separate for clarity — create may diverge from update in future (e.g., source tracking).

### RecipeImageExtractResult

Returned by the image extraction endpoint. Contains the extracted recipe data that the frontend will display in the pre-populated form.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Success | bool | Required | Whether extraction produced usable results |
| Name | string? | Optional | Extracted recipe name |
| Description | string? | Optional | Extracted description |
| Ingredients | string? | Optional | Extracted ingredients as text |
| CookTimeMinutes | int? | Optional | Extracted cook time |
| Message | string? | Optional | Error/info message (e.g., "Could not extract recipe from image") |

## Cascade Delete Behavior

When a Recipe is deleted:

1. All `WeeklyPlanItem` rows referencing the recipe are automatically deleted via PostgreSQL `ON DELETE CASCADE`
2. The delete is atomic — happens within the same database transaction
3. No application code needed to explicitly remove child records
4. The frontend must refresh the weekly plan state after a recipe deletion to reflect removed items

## Indexes (unchanged)

| Table | Index | Columns | Purpose |
|-------|-------|---------|---------|
| recipes | ix_recipes_user_id | user_id | Fast lookup of user's recipes |
| weekly_plan_items | ix_weekly_plan_items_weekly_plan_id | weekly_plan_id | Fast lookup of plan contents |
| weekly_plans | uq_weekly_plan_user_id | user_id (UNIQUE) | Enforce one plan per user |

> No new indexes required. The existing `ix_recipes_user_id` index supports all new query patterns (create inserts, delete by PK).

## Database Schema (unchanged)

The existing schema from feature 001 supports all operations in this feature. No migrations required.

```sql
-- Existing tables (no changes)
CREATE TABLE recipes (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    ingredients VARCHAR(2000),
    cook_time_minutes INTEGER CHECK (cook_time_minutes >= 0),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_recipes_user_id ON recipes(user_id);

-- WeeklyPlanItem FK with CASCADE (existing)
CREATE TABLE weekly_plan_items (
    id SERIAL PRIMARY KEY,
    weekly_plan_id INTEGER NOT NULL REFERENCES weekly_plans(id) ON DELETE CASCADE,
    recipe_id INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    added_at TIMESTAMP NOT NULL DEFAULT NOW()
);
```
