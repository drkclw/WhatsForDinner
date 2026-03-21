# Quickstart: Add Recipe Entry

**Feature**: 002-add-recipe-entry  
**Date**: 2026-03-20

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (includes npm)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- Git
- An OpenAI API key (for image extraction feature)

## Initial Setup

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd WhatsForDinner
git checkout 002-add-recipe-entry
```

### 2. Database Setup

The existing database from feature 001 works without changes. No new migrations are required.

If starting fresh:

```bash
cd backend/src/WhatsForDinner.Api
dotnet ef database update
```

### 3. OpenAI API Key Configuration

The image extraction feature requires an OpenAI API key. Configure it in one of these ways:

**Option A — Environment variable (recommended for production):**
```bash
# PowerShell
$env:OpenAI__ApiKey = "sk-your-api-key-here"

# Bash
export OpenAI__ApiKey="sk-your-api-key-here"
```

**Option B — User secrets (recommended for development):**
```bash
cd backend/src/WhatsForDinner.Api
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-api-key-here"
```

**Option C — appsettings.Development.json (local only, do NOT commit):**
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-api-key-here"
  }
}
```

> **Note**: The manual recipe entry and delete features work without an API key. Only image extraction requires it. If no key is configured, the extraction endpoint returns 503.

### 4. Backend Setup

```bash
cd backend/src/WhatsForDinner.Api

# Restore dependencies (includes new OpenAI NuGet package)
dotnet restore

# Start the API (runs on http://localhost:5140)
dotnet run
```

### 5. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start development server (runs on http://localhost:5173)
npm run dev
```

## Running the Application

### Development Mode

**Terminal 1 — Backend:**
```bash
cd backend/src/WhatsForDinner.Api
dotnet run
```

**Terminal 2 — Frontend:**
```bash
cd frontend
npm run dev
```

Open http://localhost:5173 in your browser.

### New Features to Test

1. **Add Recipe (Manual)**: Navigate to Recipes → click "Add Recipe" → fill in the form → submit
2. **Add Recipe (Image)**: On the add recipe screen → click "Upload Image" → select a photo of a recipe → review extracted data → submit
3. **Add Recipe (from Weekly Plan)**: On the Weekly Plan screen → click "Add Recipe" → you'll be taken to the create recipe form
4. **Delete Recipe**: On the recipe list → click the delete button on a recipe card → confirm deletion

### Running Tests

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

## API Endpoints (New)

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/recipes` | Create a recipe (JSON body) |
| POST | `/api/recipes/extract-from-image` | Extract recipe from image (multipart form) |
| DELETE | `/api/recipes/{id}` | Delete a recipe |

### Example: Create Recipe

```bash
curl -X POST http://localhost:5140/api/recipes \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Homemade Lasagna",
    "description": "Layered pasta with meat sauce and cheese",
    "ingredients": "Lasagna noodles\nGround beef\nRicotta cheese\nMozzarella\nTomato sauce",
    "cookTimeMinutes": 60
  }'
```

### Example: Extract from Image

```bash
curl -X POST http://localhost:5140/api/recipes/extract-from-image \
  -F "file=@recipe-photo.jpg"
```

### Example: Delete Recipe

```bash
curl -X DELETE http://localhost:5140/api/recipes/6
```
