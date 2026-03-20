# Research: Create WhatsForDinner

**Feature**: 001-create-whatsfordinner  
**Date**: 2026-03-05  
**Purpose**: Resolve technical unknowns and document decisions for implementation

## Research Tasks

### 1. .NET 10 API Architecture

**Task**: Research best practices for .NET 10 Web API structure

**Findings**:
- .NET 10 continues minimal API support introduced in .NET 6, but controller-based APIs remain standard for larger applications
- Recommended structure: Controllers → Services → Repositories/DbContext
- Use `Program.cs` with top-level statements and builder pattern
- Global exception handling via middleware
- Use `IResult` return types for cleaner API responses

**Decision**: Use controller-based API architecture for clarity and maintainability
**Rationale**: Controller-based approach provides better organization as the application grows, clearer routing, and established patterns for the team
**Alternatives considered**: Minimal APIs - rejected because they are better suited for microservices and simple endpoints; this MVP is expected to grow

### 2. Entity Framework Core with PostgreSQL

**Task**: Research EF Core patterns for PostgreSQL in .NET 10

**Findings**:
- Use `Npgsql.EntityFrameworkCore.PostgreSQL` package
- Code-first migrations recommended for controlled schema evolution
- Use `DbContext` with dependency injection
- Eager loading with `.Include()` prevents N+1 queries
- Use `IQueryable` for deferred execution and composition
- Connection pooling handled automatically by Npgsql

**Decision**: Code-first EF Core with explicit eager loading
**Rationale**: Code-first allows schema to evolve with the code; explicit includes ensure predictable query behavior
**Alternatives considered**: Database-first approach - rejected because the database doesn't exist yet and code-first provides better developer experience

### 3. Vue.js 3 + Pinia State Management

**Task**: Research Vue.js 3 patterns for SPA with API backend

**Findings**:
- Vue 3 Composition API preferred over Options API for new projects
- Pinia is the official state management solution (replaced Vuex)
- `<script setup>` syntax provides cleanest component code
- TypeScript with strict mode recommended
- Vue Router 4 for client-side routing
- Axios or Fetch API for HTTP requests (Fetch is native, Axios has interceptors)

**Decision**: Vue 3 Composition API with `<script setup>`, Pinia stores, TypeScript strict mode, Fetch API with a thin wrapper service
**Rationale**: Composition API + `<script setup>` is the recommended modern approach; Fetch API is native and sufficient for this MVP; wrapper service allows easy testing and error handling
**Alternatives considered**: Options API (rejected - legacy), Vuex (rejected - superseded by Pinia), Axios (rejected - unnecessary dependency for MVP)

### 4. Frontend-Backend Integration

**Task**: Research patterns for Vue.js + .NET API integration

**Findings**:
- CORS configuration required on .NET backend
- Use environment variables for API base URL
- TypeScript interfaces should mirror C# DTOs
- Consider OpenAPI/Swagger for contract documentation
- Vite proxy for development (avoids CORS during dev)

**Decision**: 
- Development: Vite proxy to backend API
- Production: Configure CORS on backend for deployed frontend origin
- Use OpenAPI specification for contract documentation
- Generate TypeScript types manually (auto-generation is overkill for MVP)

**Rationale**: Vite proxy simplifies local development; manual type definitions are sufficient for 3 entities
**Alternatives considered**: Auto-generated TypeScript clients (rejected - complexity overhead for small API)

### 5. Testing Strategy

**Task**: Research testing approaches for .NET + Vue.js application

**Findings**:
- **Backend**: xUnit is the standard for .NET; FluentAssertions improves readability; WebApplicationFactory for integration tests
- **Frontend**: Vitest is fast and Vite-native; Vue Test Utils for component testing; Playwright or Cypress for E2E
- Test pyramid: Many unit tests, fewer integration tests, minimal E2E tests

**Decision**: 
- Backend: xUnit + FluentAssertions + WebApplicationFactory
- Frontend: Vitest + Vue Test Utils
- E2E: Playwright (one critical user journey)

**Rationale**: Standard tooling with good documentation and community support
**Alternatives considered**: NUnit (rejected - xUnit is more modern), Jest (rejected - Vitest is faster for Vite projects), Cypress (rejected - Playwright is faster and more reliable)

### 6. Database Seeding Strategy

**Task**: Research how to pre-seed the database with predefined user and recipes

**Findings**:
- EF Core supports `HasData()` in model configuration for seed data
- Alternatively, use a `DbInitializer` class called at startup
- For MVP with fixed seed data, `HasData()` is cleaner
- Seed data is applied during migrations

**Decision**: Use EF Core `HasData()` in entity configurations
**Rationale**: Seed data is part of the schema definition, making it explicit and version-controlled
**Alternatives considered**: Runtime seeding in Program.cs (rejected - mixes concerns, not idempotent)

## Summary

All technical unknowns resolved. Key decisions:

| Area | Decision |
|------|----------|
| Backend Architecture | Controller-based .NET 10 Web API |
| ORM | Entity Framework Core with code-first migrations |
| Database | PostgreSQL with Npgsql provider |
| Frontend Framework | Vue.js 3 with Composition API + `<script setup>` |
| State Management | Pinia |
| HTTP Client | Fetch API with wrapper service |
| Testing | xUnit + Vitest + Playwright |
| Seeding | EF Core `HasData()` |
