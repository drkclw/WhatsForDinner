# API Contracts: Multi-Image Recipe Extraction with Preparation

**Feature**: 003-multi-image-extraction
**Date**: 2026-04-12

This document describes the modified API endpoints for the multi-image extraction feature. The OpenAPI specification in `openapi.yaml` provides the machine-readable contract.

## Modified Endpoints

| Method | Path | Description | Change |
|--------|------|-------------|--------|
| POST | `/api/recipes/extract-from-image` | Extract recipe data from uploaded images | Now accepts 1–5 files; returns `preparation` field |
| POST | `/api/recipes` | Create a new recipe | Request body includes `preparation` field |
| PUT | `/api/recipes/{id}` | Update a recipe | Request body includes `preparation` field |
| GET | `/api/recipes` | Get all recipes | Response includes `preparation` field |
| GET | `/api/recipes/{id}` | Get a specific recipe | Response includes `preparation` field |

## Key Changes

### Extract From Image Endpoint

**Before (Feature 002)**:
- Accepts a single `file` field (one image, max 10 MB)
- Request size limit: 10 MB
- Returns: name, description, ingredients, cookTimeMinutes

**After (Feature 003)**:
- Accepts a `files` field with 1–5 images (each max 10 MB)
- Request size limit: 50 MB (5 × 10 MB)
- Returns: name, description, ingredients, **preparation**, cookTimeMinutes
- All images sent to AI in a single request for cross-image context

### Recipe Schemas

All recipe-related schemas (`Recipe`, `RecipeCreateRequest`, `RecipeUpdateRequest`, `RecipeImageExtractResult`) gain a new `preparation` field (nullable string, max 10,000 characters).

## Unchanged Endpoints

| Method | Path | Description |
|--------|------|-------------|
| DELETE | `/api/recipes/{id}` | Delete a recipe |
| GET | `/api/weekly-plan` | Get weekly plan |
| POST | `/api/weekly-plan/items` | Add recipe to weekly plan |
| DELETE | `/api/weekly-plan/items/{id}` | Remove from weekly plan |

## Error Responses

All error responses follow the existing `Error` and `ValidationError` schemas. New validation errors for the extraction endpoint:

| Status | Condition |
|--------|-----------|
| 400 | No files provided |
| 400 | More than 5 files provided |
| 400 | Any file has unsupported format |
| 400 | Any file fails magic byte validation |
| 413 | Total request size exceeds 50 MB |
| 504 | AI service timeout (extended to 90s for multi-image) |
