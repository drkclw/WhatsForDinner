# Data Model: Create WhatsForDinner

**Feature**: 001-create-whatsfordinner  
**Date**: 2026-03-05  
**Source**: [spec.md](spec.md) Key Entities section

## Entity Relationship Diagram

```
┌─────────────┐       1:N       ┌─────────────┐
│    User     │────────────────▶│   Recipe    │
└─────────────┘                 └─────────────┘
       │                               │
       │ 1:1                           │
       ▼                               │
┌─────────────┐       N:M              │
│ WeeklyPlan  │◀───────────────────────┘
└─────────────┘
       │
       │ 1:N
       ▼
┌─────────────────┐
│ WeeklyPlanItem  │
└─────────────────┘
```

## Entities

### User

Represents a person using the application. For MVP, one predefined user exists with ID=1.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | int | PK, auto-increment | Unique identifier |
| Name | string | Required, max 100 chars | Display name |
| CreatedAt | DateTime | Required, default NOW | Account creation timestamp |

**Relationships**:
- Has many Recipes (1:N)
- Has one WeeklyPlan (1:1)

**Notes**: For MVP, single user is seeded. No authentication required.

---

### Recipe

A meal that can be planned. Belongs to a user's recipe collection.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | int | PK, auto-increment | Unique identifier |
| UserId | int | FK → User.Id, Required | Owner of the recipe |
| Name | string | Required, max 200 chars | Recipe title |
| Description | string | Optional, max 1000 chars | Brief description of the dish |
| Ingredients | string | Optional, max 2000 chars | Ingredients as text (one per line) |
| CookTimeMinutes | int | Optional, min 0 | Cooking time in minutes |
| CreatedAt | DateTime | Required, default NOW | Creation timestamp |
| UpdatedAt | DateTime | Required, default NOW | Last modification timestamp |

**Relationships**:
- Belongs to User (N:1)
- Can appear in WeeklyPlanItems (1:N)

**Validation Rules**:
- Name is required and must be non-empty
- CookTimeMinutes must be non-negative if provided
- Ingredients stored as plain text; future versions may normalize to separate table

**Seed Data** (5 predefined recipes for MVP user):

| Name | Description | Ingredients | CookTimeMinutes |
|------|-------------|-------------|-----------------|
| Spaghetti Bolognese | Classic Italian pasta with meat sauce | Spaghetti, Ground beef, Tomato sauce, Onion, Garlic, Olive oil, Salt, Pepper, Parmesan | 45 |
| Grilled Chicken Salad | Fresh salad with grilled chicken breast | Chicken breast, Mixed greens, Cherry tomatoes, Cucumber, Red onion, Olive oil, Lemon juice | 25 |
| Vegetable Stir Fry | Quick and healthy Asian-inspired dish | Broccoli, Bell peppers, Carrots, Snap peas, Soy sauce, Garlic, Ginger, Sesame oil, Rice | 20 |
| Beef Tacos | Mexican-style tacos with seasoned beef | Ground beef, Taco shells, Lettuce, Tomatoes, Cheese, Sour cream, Taco seasoning | 30 |
| Margherita Pizza | Simple Italian pizza with fresh ingredients | Pizza dough, Tomato sauce, Fresh mozzarella, Basil, Olive oil | 25 |

---

### WeeklyPlan

The current week's meal plan for a user. One-to-one with User.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | int | PK, auto-increment | Unique identifier |
| UserId | int | FK → User.Id, Required, Unique | Owner of this plan |
| CreatedAt | DateTime | Required, default NOW | Plan creation timestamp |
| UpdatedAt | DateTime | Required, default NOW | Last modification timestamp |

**Relationships**:
- Belongs to User (1:1)
- Has many WeeklyPlanItems (1:N)

**Notes**: For MVP, represents a single "bag of recipes" without day assignment. Future versions may add day-of-week fields.

---

### WeeklyPlanItem

Junction entity linking recipes to a weekly plan. Allows duplicates (same recipe multiple times per week).

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | int | PK, auto-increment | Unique identifier |
| WeeklyPlanId | int | FK → WeeklyPlan.Id, Required | Parent plan |
| RecipeId | int | FK → Recipe.Id, Required | The recipe in the plan |
| AddedAt | DateTime | Required, default NOW | When recipe was added to plan |

**Relationships**:
- Belongs to WeeklyPlan (N:1)
- References Recipe (N:1)

**Notes**: 
- Duplicates are allowed (same RecipeId can appear multiple times for same WeeklyPlanId)
- No unique constraint on (WeeklyPlanId, RecipeId)
- Deleting removes from plan; does not delete the Recipe

---

## State Transitions

### WeeklyPlan States

```
┌─────────┐    add recipe    ┌───────────────┐
│  Empty  │─────────────────▶│ Has Recipes   │
└─────────┘                  └───────────────┘
     ▲                              │
     │          remove last         │
     └──────────────────────────────┘
```

| State | Condition | UI Behavior |
|-------|-----------|-------------|
| Empty | WeeklyPlanItems.Count == 0 | Show empty state message, disable Remove buttons |
| Has Recipes | WeeklyPlanItems.Count > 0 | Show recipe list, enable Remove buttons |

### Recipe Edit Flow

```
┌──────────┐    edit     ┌─────────┐    save    ┌──────────┐
│ Viewing  │────────────▶│ Editing │───────────▶│ Updated  │
└──────────┘             └─────────┘            └──────────┘
                              │
                              │ cancel
                              ▼
                         ┌──────────┐
                         │ Unchanged│
                         └──────────┘
```

## Indexes

| Table | Index | Columns | Purpose |
|-------|-------|---------|---------|
| Recipe | IX_Recipe_UserId | UserId | Fast lookup of user's recipes |
| WeeklyPlanItem | IX_WeeklyPlanItem_WeeklyPlanId | WeeklyPlanId | Fast lookup of plan contents |
| WeeklyPlan | UQ_WeeklyPlan_UserId | UserId (Unique) | Enforce one plan per user |

## Database Schema (PostgreSQL)

```sql
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

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

CREATE TABLE weekly_plans (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE REFERENCES users(id) ON DELETE CASCADE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE weekly_plan_items (
    id SERIAL PRIMARY KEY,
    weekly_plan_id INTEGER NOT NULL REFERENCES weekly_plans(id) ON DELETE CASCADE,
    recipe_id INTEGER NOT NULL REFERENCES recipes(id) ON DELETE CASCADE,
    added_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX ix_weekly_plan_items_weekly_plan_id ON weekly_plan_items(weekly_plan_id);
```
