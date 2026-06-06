# Quickstart: Multi-Image Recipe Extraction with Preparation

**Feature**: 003-multi-image-extraction
**Date**: 2026-04-12

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (includes npm)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- Git
- An OpenAI API key (for image extraction)

## Initial Setup

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd WhatsForDinner
git checkout 003-multi-image-extraction
```

### 2. Database Setup

This feature adds a new migration for the `preparation` column. Run migrations to update the schema:

```bash
cd backend/src/WhatsForDinner.Api
dotnet ef database update
```

This adds nullable `preparation` (varchar 10,000) to the `recipes` table. Existing recipes get `null` for this field.

### 3. OpenAI Configuration

The image extraction feature requires an OpenAI API key:

```bash
cd backend/src/WhatsForDinner.Api
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-api-key-here"
```

Optional — configure timeout for multi-image requests (default: 90 seconds):
```json
{
  "OpenAI": {
    "TimeoutSeconds": 90
  }
}
```

### 4. Backend Setup

```bash
cd backend/src/WhatsForDinner.Api
dotnet restore
dotnet run
```

Backend runs on http://localhost:5140.

### 5. Frontend Setup

```bash
cd frontend
npm install
npm run dev
```

Frontend runs on http://localhost:5173.

## Features to Test

### New: Preparation Field
1. **Manual entry**: Navigate to Recipes → Add Recipe → the form now includes a "Preparation" textarea
2. **View/Edit**: Existing recipe detail and edit views show the preparation field

### New: Multi-Image Extraction
1. Navigate to Recipes → Add Recipe → "Upload Image" tab
2. Select 1–5 images (click or drag-and-drop)
3. Each image shows a thumbnail preview with a remove button
4. Click "Extract" to process all images
5. Loading spinner shows "Extracting from N images..."
6. Extracted data (including preparation) pre-populates the form
7. Review and edit all fields, then submit

### Backward Compatible
- Single image upload works exactly as before, plus the new preparation field
- Manual recipe entry still works with the new preparation field optional

## Running Tests

**Backend Tests:**
```bash
cd backend/tests/WhatsForDinner.Api.Tests
dotnet test
```

**Frontend Tests:**
```bash
cd frontend
npm run test        # Unit tests
npm run test:watch  # Unit tests in watch mode
npm run test:e2e    # E2E tests (requires backend running)
```

## API Changes

### Modified Endpoint

| Method | Path | Change |
|--------|------|--------|
| POST | `/api/recipes/extract-from-image` | Accepts 1–5 files via `files` field; returns `preparation` |

### Example: Multi-Image Extraction

```bash
curl -X POST http://localhost:5140/api/recipes/extract-from-image \
  -F "files=@recipe-page1.jpg" \
  -F "files=@recipe-page2.jpg"
```

### Example: Create Recipe with Preparation

```bash
curl -X POST http://localhost:5140/api/recipes \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Homemade Lasagna",
    "description": "Layered pasta with meat sauce and cheese",
    "ingredients": "Lasagna noodles\nGround beef\nRicotta cheese",
    "preparation": "1. Preheat oven to 375°F.\n2. Cook noodles.\n3. Layer and bake 45 min.",
    "cookTimeMinutes": 60
  }'
```
