# Data Model: Multi-Image Recipe Extraction with Preparation

**Feature**: 003-multi-image-extraction
**Date**: 2026-04-12
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

> **Schema change required.** The `recipes` table gains a new `preparation` column (nullable text, max 10,000 characters). All DTOs that reference recipe fields are updated to include preparation. No new tables or relationships.

## Entities

### Recipe (modified)

A meal entry in a user's inventory. This feature adds a `Preparation` field for cooking/baking instructions.

| Field | Type | Constraints | Description | Change |
|-------|------|-------------|-------------|--------|
| Id | int | PK, auto-increment | Unique identifier | — |
| UserId | int | FK → User.Id, Required | Owner of the recipe | — |
| Name | string | Required, max 200 chars | Recipe title | — |
| Description | string | Optional, max 1000 chars | Brief description of the dish | — |
| Ingredients | string | Optional, max 2000 chars | Ingredients as free-text | — |
| **Preparation** | **string** | **Optional, max 10,000 chars** | **Cooking/baking instructions as free-text** | **NEW** |
| CookTimeMinutes | int | Optional, min 0 | Cooking time in minutes | — |
| CreatedAt | DateTime | Required, default NOW | Creation timestamp | — |
| UpdatedAt | DateTime | Required, default NOW | Last modification timestamp | — |

**Relationships**: Unchanged from features 001/002.

**Validation Rules** (updated for create/update):
- Name is required, must be non-empty, max 200 characters
- Description is optional, max 1000 characters
- Ingredients is optional, max 2000 characters
- **Preparation is optional, max 10,000 characters**
- CookTimeMinutes must be a non-negative integer if provided (≥ 0)

## Modified DTOs

### RecipeDto (response — modified)

| Field | Type | Constraints | Description | Change |
|-------|------|-------------|-------------|--------|
| Id | int | Required | Unique identifier | — |
| Name | string | Required | Recipe title | — |
| Description | string? | Optional | Brief description | — |
| Ingredients | string? | Optional | Ingredients as free-text | — |
| **Preparation** | **string?** | **Optional** | **Cooking instructions as free-text** | **NEW** |
| CookTimeMinutes | int? | Optional | Cooking time in minutes | — |
| CreatedAt | DateTime | Required | Creation timestamp | — |
| UpdatedAt | DateTime | Required | Last modification timestamp | — |

### RecipeCreateRequest (modified)

| Field | Type | Constraints | Description | Change |
|-------|------|-------------|-------------|--------|
| Name | string | Required, min 1, max 200 | Recipe title | — |
| Description | string? | Optional, max 1000 | Brief description | — |
| Ingredients | string? | Optional, max 2000 | Ingredients as free-text | — |
| **Preparation** | **string?** | **Optional, max 10,000** | **Cooking instructions as free-text** | **NEW** |
| CookTimeMinutes | int? | Optional, ≥ 0 | Cooking time in minutes | — |

### RecipeUpdateRequest (modified)

Same fields as `RecipeCreateRequest`. Adds `Preparation` field with same constraints.

### RecipeImageExtractResult (modified)

| Field | Type | Constraints | Description | Change |
|-------|------|-------------|-------------|--------|
| Success | bool | Required | Whether extraction produced usable results | — |
| Name | string? | Optional | Extracted recipe name | — |
| Description | string? | Optional | Extracted description | — |
| Ingredients | string? | Optional | Extracted ingredients as text | — |
| **Preparation** | **string?** | **Optional** | **Extracted cooking instructions as text** | **NEW** |
| CookTimeMinutes | int? | Optional | Extracted cook time | — |
| Message | string? | Optional | Error/info message | — |

## Database Migration

### Migration: AddPreparationField

```sql
ALTER TABLE recipes
ADD COLUMN preparation VARCHAR(10000);
```

### EF Core Configuration Update (RecipeConfiguration.cs)

Add to existing configuration:

```csharp
builder.Property(r => r.Preparation)
    .HasColumnName("preparation")
    .HasMaxLength(10000);
```

## Indexes

No new indexes required. The `preparation` column is only used for display/storage, not for querying or filtering.

| Table | Index | Columns | Purpose | Change |
|-------|-------|---------|---------|--------|
| recipes | ix_recipes_user_id | user_id | Fast lookup of user's recipes | — |
| weekly_plan_items | ix_weekly_plan_items_weekly_plan_id | weekly_plan_id | Fast lookup of plan contents | — |
| weekly_plans | uq_weekly_plan_user_id | user_id (UNIQUE) | Enforce one plan per user | — |

## Seed Data Update

Existing seed recipes should have their `Preparation` field set to `null` (default for new nullable column). No seed data changes required — the migration's `ADD COLUMN` with no default naturally produces null for existing rows.
