# Research: Add Recipe Entry

**Feature**: 002-add-recipe-entry  
**Date**: 2026-03-20  
**Purpose**: Resolve technical unknowns and document decisions for implementation

## Research Tasks

### 1. OpenAI Vision API Integration from .NET 10

**Task**: Research how to integrate a cloud AI vision API for recipe extraction from images

**Findings**:
- The official NuGet package is **`OpenAI`** (v2.x, published by OpenAI). It provides first-class C# support with typed request/response models via `OpenAI.Chat` namespace
- Vision uses the Chat Completions endpoint (`gpt-4o` or `gpt-4o-mini`) — not a separate endpoint. Images are sent as base64 data URLs inside multi-modal chat messages
- Structured Outputs (`response_format: { type: "json_schema", ... }`) guarantee valid JSON responses, eliminating fragile parsing
- For recipe extraction, a system prompt + JSON schema defining `{ name, description, ingredients, cookTimeMinutes }` yields reliable results
- `gpt-4o-mini` at `detail: "low"` costs ~85 tokens per image (fractions of a cent) — negligible for a single-user app
- API key should be stored via `IConfiguration["OpenAI:ApiKey"]` (env variable in production, appsettings in development)
- Error scenarios: missing API key (503), invalid key (502), rate limited (429 with retry), timeout (504), unreadable image (422), content policy (422)
- `OpenAIClient` should be registered as singleton in DI

**Decision**: Use official `OpenAI` NuGet package (v2.x) with `gpt-4o-mini` model, base64 image embedding via Chat Completions, Structured Outputs for guaranteed JSON
**Rationale**: Official SDK is well-maintained and strongly typed. `gpt-4o-mini` offers best cost/quality tradeoff. Base64 embedding avoids needing blob storage. Structured Outputs eliminate fragile JSON parsing
**Alternatives considered**: Azure OpenAI (adds Azure dependency), Google Cloud Vision + Gemini (different ecosystem, less mature .NET SDK), raw HttpClient calls (more boilerplate, loses SDK retry/error handling), community NuGet packages (unofficial)

### 2. ASP.NET Core File Upload for .NET 10

**Task**: Research best practices for handling image uploads with size/type validation

**Findings**:
- `IFormFile` is the standard approach for file uploads — framework handles `multipart/form-data` parsing automatically
- Max request size enforced via `[RequestSizeLimit(10_485_760)]` attribute on the action; Kestrel returns 413 automatically if exceeded
- File type validation requires two checks: (1) `ContentType` header (first pass, spoofable), (2) magic bytes (JPEG: `FF D8 FF`, PNG: `89 50 4E 47`, WebP: `52 49 46 46...57 45 42 50`)
- For images ≤10 MB, in-memory buffering via `IFormFile` is perfectly acceptable (no streaming needed)
- Convert `IFormFile` → `byte[]` via `MemoryStream`, then `Convert.ToBase64String()` for forwarding to OpenAI
- No need to persist the image to disk or blob storage — it's transient

**Decision**: Accept uploads via `IFormFile` with `[FromForm]`, apply `[RequestSizeLimit(10_485_760)]` on the upload action, validate Content-Type + magic bytes, buffer in memory, convert to base64 for API forwarding
**Rationale**: Idiomatic ASP.NET Core approach. 10 MB is small enough for in-memory buffering. Magic byte validation is essential for security. No persistence needed
**Alternatives considered**: Streaming with `Request.Body` (overkill for <10 MB), accept base64 in JSON body (33% size overhead, worse DX), save to disk then process (unnecessary persistence)

### 3. EF Core Cascade Delete Behavior

**Task**: Confirm cascade delete behavior when deleting a Recipe that has WeeklyPlanItem references

**Findings**:
- The existing `WeeklyPlanItemConfiguration` explicitly configures `OnDelete(DeleteBehavior.Cascade)` for the Recipe → WeeklyPlanItem relationship
- The PostgreSQL migration confirms FK with `onDelete: ReferentialAction.Cascade`
- When `_context.Recipes.Remove(recipe)` + `SaveChangesAsync()` executes, PostgreSQL automatically deletes all `weekly_plan_items` where `recipe_id` matches — this is atomic within the same transaction
- If related entities are tracked in the change tracker, EF Core redundantly marks them for deletion — harmless but unnecessary
- Simple `Remove()` + `SaveChangesAsync()` is sufficient; no need to explicitly load and remove child entities

**Decision**: Rely on database-level `ON DELETE CASCADE`. Use simple `Remove()` + `SaveChangesAsync()` pattern. Do not explicitly load or remove related WeeklyPlanItems
**Rationale**: Cascade is already configured at both EF Core and database levels. PostgreSQL cascade is atomic. Explicitly removing children is redundant, adds queries, and couples delete logic to schema
**Alternatives considered**: Explicitly load and remove WeeklyPlanItems (unnecessary extra query), change to `DeleteBehavior.Restrict` (more error-prone), soft delete (over-engineered for this app)

### 4. Vue 3 File Upload Component Patterns

**Task**: Research Vue 3 patterns for image upload with preview, validation, and accessibility

**Findings**:
- Use `<input type="file" accept="image/jpeg,image/png,image/webp">` with template ref for programmatic trigger
- `URL.createObjectURL(file)` for image preview — faster and less memory than FileReader + base64; clean up with `URL.revokeObjectURL()` in `onUnmounted`
- Client-side validation: check `file.type` and `file.size` before upload for instant feedback
- Send file via `fetch()` with `FormData` — do NOT set Content-Type header (browser auto-sets with correct boundary)
- Existing `apiClient.ts` always sets `Content-Type: application/json`; need a new `postFormData` method that sends `FormData` without JSON serialization
- Simple `isUploading` boolean for loading state — `fetch()` doesn't natively support progress tracking, which is adequate for <10 MB files
- Accessibility: native `<input type="file">` is keyboard/screen-reader accessible by default; add `aria-label`, use `aria-live="polite"` for status announcements, link errors via `aria-describedby`
- Drag-and-drop is a progressive enhancement: listen for `dragover`/`drop` events, prevent default, extract from `dataTransfer.files`

**Decision**: Create reusable `ImageUpload.vue` component with Composition API + TypeScript. Use `URL.createObjectURL()` for preview. Add `postFormData` method to existing apiClient. Client-side validation for type/size. Simple loading boolean (no progress bar). Drag-and-drop as progressive enhancement. WCAG-compliant labeling and live regions
**Rationale**: `createObjectURL` is fastest for preview. `FormData` is the standard browser mechanism. Loading boolean is sufficient for single small files. Drag-and-drop enhances UX without being required
**Alternatives considered**: FileReader + base64 preview (slower, more memory), send base64 in JSON (33% overhead), XMLHttpRequest for progress (complexity for marginal benefit), third-party upload library (unnecessary dependency)

## Summary of Decisions

| Topic | Decision |
|---|---|
| OpenAI Integration | Official `OpenAI` NuGet (v2.x), `gpt-4o-mini`, base64 via Chat Completions, Structured Outputs |
| File Upload (Backend) | `IFormFile` + `[RequestSizeLimit(10MB)]`, Content-Type + magic bytes validation, in-memory buffer |
| EF Core Cascade | Rely on existing `ON DELETE CASCADE` — simple `Remove()` + `SaveChangesAsync()` |
| File Upload (Frontend) | `ImageUpload.vue`, `URL.createObjectURL`, `FormData` via fetch, client-side validation |
