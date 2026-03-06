# Quickstart: Create WhatsForDinner

**Feature**: 001-create-whatsfordinner  
**Date**: 2026-03-05

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (includes npm)
- [PostgreSQL 15+](https://www.postgresql.org/download/)
- Git

## Initial Setup

### 1. Clone and Navigate

```bash
git clone <repository-url>
cd WhatsForDinner
git checkout 001-create-whatsfordinner
```

### 2. Database Setup

Create a PostgreSQL database:

```bash
# Connect to PostgreSQL
psql -U postgres

# Create database
CREATE DATABASE whatsfordinner;

# Exit
\q
```

### 3. Backend Setup

```bash
cd backend/src/WhatsForDinner.Api

# Restore dependencies
dotnet restore

# Update connection string in appsettings.Development.json
# Default: "Host=localhost;Database=whatsfordinner;Username=postgres;Password=postgres"

# Run migrations
dotnet ef database update

# Start the API (runs on http://localhost:5000)
dotnet run
```

### 4. Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Start development server (runs on http://localhost:5173)
npm run dev
```

## Running the Application

### Development Mode

**Terminal 1 - Backend:**
```bash
cd backend/src/WhatsForDinner.Api
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd frontend
npm run dev
```

Open http://localhost:5173 in your browser.

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
npm run test:e2e    # E2E tests (requires backend running)
```

## Project Structure

```
WhatsForDinner/
├── backend/
│   ├── src/
│   │   └── WhatsForDinner.Api/
│   │       ├── Controllers/       # API endpoints
│   │       ├── Models/            # Entity classes
│   │       ├── Services/          # Business logic
│   │       ├── Data/              # DbContext, configurations
│   │       ├── appsettings.json
│   │       └── Program.cs
│   └── tests/
│       └── WhatsForDinner.Api.Tests/
│           ├── Unit/
│           └── Integration/
│
├── frontend/
│   ├── src/
│   │   ├── components/           # Vue components
│   │   ├── views/                # Page components
│   │   ├── stores/               # Pinia state stores
│   │   ├── services/             # API client
│   │   ├── types/                # TypeScript interfaces
│   │   ├── App.vue
│   │   └── main.ts
│   ├── tests/
│   │   ├── unit/
│   │   └── e2e/
│   ├── package.json
│   └── vite.config.ts
│
└── specs/
    └── 001-create-whatsfordinner/
        ├── spec.md
        ├── plan.md
        ├── research.md
        ├── data-model.md
        ├── quickstart.md         # This file
        └── contracts/
            ├── README.md
            └── openapi.yaml
```

## Environment Variables

### Backend (`appsettings.Development.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=whatsfordinner;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Frontend (`.env.development`)

```
VITE_API_BASE_URL=http://localhost:5000/api
```

## Common Tasks

### Reset Database

```bash
cd backend/src/WhatsForDinner.Api
dotnet ef database drop --force
dotnet ef database update
```

### Add a Migration

```bash
cd backend/src/WhatsForDinner.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Build for Production

**Backend:**
```bash
cd backend/src/WhatsForDinner.Api
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
cd frontend
npm run build
# Output in dist/
```

## Troubleshooting

### Port Already in Use

- Backend default: 5000 (change in `launchSettings.json`)
- Frontend default: 5173 (change in `vite.config.ts`)

### Database Connection Failed

1. Verify PostgreSQL is running
2. Check credentials in `appsettings.Development.json`
3. Ensure database exists: `psql -U postgres -c "\l"`

### CORS Errors

During development, use the Vite proxy (configured in `vite.config.ts`). For production, ensure backend CORS policy includes frontend origin.
