# API Contracts: Add Recipe Entry

**Feature**: 002-add-recipe-entry  
**Date**: 2026-03-20  

This document describes the new and modified API endpoints for the Add Recipe Entry feature. The OpenAPI specification in `openapi.yaml` provides the machine-readable contract.

## New Endpoints

| Method | Path | Description | Request | Response |
|--------|------|-------------|---------|----------|
| POST | `/api/recipes` | Create a new recipe via manual entry | JSON `RecipeCreateRequest` | 201 `Recipe` |
| POST | `/api/recipes/extract-from-image` | Extract recipe data from uploaded image | `multipart/form-data` with image file | 200 `RecipeImageExtractResult` |
| DELETE | `/api/recipes/{id}` | Delete a recipe (cascades to weekly plan) | — | 204 No Content |

## Existing Endpoints (unchanged)

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/recipes` | Get all recipes for current user |
| GET | `/api/recipes/{id}` | Get a specific recipe by ID |
| PUT | `/api/recipes/{id}` | Update a recipe |
| GET | `/api/weekly-plan` | Get weekly plan |
| POST | `/api/weekly-plan/items` | Add recipe to weekly plan |
| DELETE | `/api/weekly-plan/items/{id}` | Remove from weekly plan |

## Error Responses

All error responses follow the existing `Error` and `ValidationError` schemas.

| Status | When |
|--------|------|
| 400 | Validation errors (missing name, invalid cook time, unsupported file type) |
| 404 | Recipe not found (delete) |
| 413 | Image file exceeds 10 MB |
| 422 | Image extraction failed (unreadable image, no recipe content found) |
| 502 | Cloud AI service error (invalid API key, unexpected response) |
| 503 | Cloud AI service not configured (missing API key) |
| 504 | Cloud AI service timeout |
