# API Contracts: Create WhatsForDinner

**Feature**: 001-create-whatsfordinner  
**Date**: 2026-03-05  
**Format**: OpenAPI 3.0.3

## Overview

This document describes the REST API contract for the WhatsForDinner backend.

## Base URL

- **Development**: `http://localhost:5000/api`
- **Production**: TBD

## Endpoints Summary

| Method | Path | Description | Request Body | Response |
|--------|------|-------------|--------------|----------|
| GET | `/recipes` | Get all user's recipes | - | `Recipe[]` |
| GET | `/recipes/{id}` | Get recipe by ID | - | `Recipe` |
| PUT | `/recipes/{id}` | Update recipe | `RecipeUpdateRequest` | `Recipe` |
| GET | `/weekly-plan` | Get weekly plan with items | - | `WeeklyPlan` |
| POST | `/weekly-plan/items` | Add recipe to plan | `AddToWeeklyPlanRequest` | `WeeklyPlanItem` |
| DELETE | `/weekly-plan/items/{id}` | Remove item from plan | - | 204 No Content |

## Data Types

### Recipe

```typescript
interface Recipe {
  id: number;
  name: string;                    // Required, max 200 chars
  description: string | null;      // Optional, max 1000 chars
  ingredients: string | null;      // Optional, max 2000 chars, newline-separated
  cookTimeMinutes: number | null;  // Optional, >= 0
  createdAt: string;               // ISO 8601 datetime
  updatedAt: string;               // ISO 8601 datetime
}
```

### WeeklyPlan

```typescript
interface WeeklyPlan {
  id: number;
  items: WeeklyPlanItem[];
  createdAt: string;
  updatedAt: string;
}
```

### WeeklyPlanItem

```typescript
interface WeeklyPlanItem {
  id: number;       // Use this ID for DELETE operations
  recipe: Recipe;
  addedAt: string;
}
```

## Error Responses

### 404 Not Found

```json
{
  "message": "Recipe not found"
}
```

### 400 Validation Error

```json
{
  "message": "Validation failed",
  "errors": {
    "name": ["Name is required"],
    "cookTimeMinutes": ["Cook time must be a non-negative number"]
  }
}
```

## Usage Notes

1. **Single User MVP**: All endpoints operate on the predefined user (ID=1). No authentication required.

2. **Duplicate Recipes in Plan**: The same recipe can be added to the weekly plan multiple times. Each addition creates a new `WeeklyPlanItem` with its own ID.

3. **Removing from Plan**: Use the `WeeklyPlanItem.id` (not `Recipe.id`) when calling DELETE. This allows removing a specific instance when duplicates exist.

4. **Ingredients Format**: Ingredients are stored as plain text with newline separators. Frontend should split/join as needed for display.

## OpenAPI Specification

Full machine-readable specification: [openapi.yaml](openapi.yaml)
