# WhatsForDinner Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-03-05

## Active Technologies
- C# / .NET 10 (backend), TypeScript ~5.4 / Vue.js 3.5 (frontend) + ASP.NET Core Web API, Entity Framework Core 10, Npgsql, Vue 3, Vue Router 4, Pinia 2, Cloud AI Vision API (e.g., OpenAI) (002-add-recipe-entry)
- PostgreSQL (existing, no schema changes — Recipe table already supports all needed fields) (002-add-recipe-entry)
- C# / .NET 10 (backend), TypeScript / Vue.js 3.x (frontend) + ASP.NET Core Web API, Entity Framework Core (Npgsql), OpenAI SDK 2.x, Vue.js 3, Vue Router, Pinia (003-multi-image-extraction)
- PostgreSQL (existing `whatsfordinner` database) (003-multi-image-extraction)
- C# / .NET 10 (backend) · TypeScript / Node 20+ (frontend) + ASP.NET Core 10 (controllers) · EF Core 10 + Npgsql · `Google.Apis.Auth` v1.68.0 · `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0 · Vue 3.5 · Pinia 2 · Vue Router 4 · Vite 5 · Vitest · Playwright (004-google-auth-login)
- PostgreSQL 15 via EF Core (004-google-auth-login)
- Bicep (latest supported by Azure CLI/Bicep CLI) + GitHub Actions YAML; existing app stack is C#/.NET 10 (backend) and TypeScript/Vue 3 (frontend) + Azure Resource Manager via Bicep, Azure Static Web Apps, Azure App Service (+ App Service Plan), Azure Database for PostgreSQL Flexible Server, Azure Key Vault, GitHub Actions with `azure/login` OIDC auth (001-azure-iac-cicd)
- Azure Database for PostgreSQL Flexible Server (application data) + Key Vault (secret material) (001-azure-iac-cicd)

- C# / .NET 10 (backend), TypeScript / Vue.js 3.x (frontend) + ASP.NET Core Web API, Entity Framework Core, Vue.js 3, Vue Router, Pinia (state management) (001-create-whatsfordinner)

## Project Structure

```text
backend/
frontend/
tests/
```

## Commands

npm test; npm run lint

## Code Style

C# / .NET 10 (backend), TypeScript / Vue.js 3.x (frontend): Follow standard conventions

## Recent Changes
- 001-azure-iac-cicd: Added Bicep (latest supported by Azure CLI/Bicep CLI) + GitHub Actions YAML; existing app stack is C#/.NET 10 (backend) and TypeScript/Vue 3 (frontend) + Azure Resource Manager via Bicep, Azure Static Web Apps, Azure App Service (+ App Service Plan), Azure Database for PostgreSQL Flexible Server, Azure Key Vault, GitHub Actions with `azure/login` OIDC auth
- 004-google-auth-login: Added C# / .NET 10 (backend) · TypeScript / Node 20+ (frontend) + ASP.NET Core 10 (controllers) · EF Core 10 + Npgsql · `Google.Apis.Auth` v1.68.0 · `Microsoft.AspNetCore.Authentication.JwtBearer` v10.0.0 · Vue 3.5 · Pinia 2 · Vue Router 4 · Vite 5 · Vitest · Playwright
- 003-multi-image-extraction: Added C# / .NET 10 (backend), TypeScript / Vue.js 3.x (frontend) + ASP.NET Core Web API, Entity Framework Core (Npgsql), OpenAI SDK 2.x, Vue.js 3, Vue Router, Pinia


<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->
