# Research: Multi-Image Recipe Extraction with Preparation

**Feature**: 003-multi-image-extraction
**Date**: 2026-04-12

## R1: Multi-Image in Single OpenAI Chat Request

**Decision**: Send all images as multiple `ChatMessageContentPart.CreateImagePart()` calls within a single `UserChatMessage`.

**Rationale**: The OpenAI Chat API natively supports multiple images per message. The .NET SDK v2.x `UserChatMessage` constructor accepts `params ChatMessageContentPart[]`, so passing multiple image parts is directly supported. A single request lets the AI see full context across all recipe pages and return one merged result — no application-level merging logic needed.

**Alternatives considered**:
- Separate requests per image with application-level merge: Rejected because it requires complex field-priority/conflict-resolution logic, produces worse merge quality (AI can't see cross-image context), and costs more API calls.
- Batch API: Rejected because it adds latency (polling) and the Chat API already handles multi-image natively.

## R2: Timeout for Multi-Image Requests

**Decision**: Increase the AI request timeout from 30 seconds to 90 seconds, and make it configurable via `appsettings.json` (`OpenAI:TimeoutSeconds`).

**Rationale**: The current 30-second hardcoded timeout in `RecipeImageExtractor` is calibrated for a single image. With up to 5 high-detail images, the AI needs more processing time. 90 seconds provides comfortable headroom while still failing fast enough for the user. Making it configurable avoids future hardcoded changes.

**Alternatives considered**:
- Keep 30 seconds: Rejected because multi-image requests will routinely timeout.
- Per-image timeout (30s × N): Rejected as overly generous (150s for 5 images) and unnecessarily complex.

## R3: Backend Endpoint — Multi-File Upload

**Decision**: Change the `extract-from-image` endpoint from `IFormFile file` to `IFormFileCollection files` (or `List<IFormFile>`). Update `RequestSizeLimit` to 50 MB. Validate 1–5 files individually (type, size, magic bytes).

**Rationale**: ASP.NET Core natively supports multi-file uploads via `IFormFileCollection`. The existing per-file validation (content type whitelist + magic byte check) can be applied in a loop. The 50 MB limit (5 × 10 MB) aligns with the spec constraint.

**Alternatives considered**:
- Separate endpoint for multi-image: Rejected to avoid API proliferation; the existing endpoint naturally extends.
- Accept base64 JSON array: Rejected because multipart form-data is more bandwidth-efficient and already established.

## R4: Extractor Service Interface Change

**Decision**: Change `IRecipeImageExtractor.ExtractFromImageAsync` signature from `(byte[] imageData, string contentType)` to accept a list of image tuples: `List<(byte[] Data, string ContentType)>`. Single-image callers pass a one-element list.

**Rationale**: Keeps backward compatibility at the service level — single image is just a special case of multi-image. Avoids needing two methods or an overload.

**Alternatives considered**:
- Overload with single and multi variants: Rejected as unnecessary duplication; one method handles both.
- Separate `ExtractFromImagesAsync` method: Rejected; renaming the existing method is cleaner since the old single-image path is subsumed.

## R5: Preparation Field — Database Schema

**Decision**: Add `Preparation` column (nullable `text` type, max 10,000 characters) to the `recipes` table via EF Core migration. Follow existing configuration pattern in `RecipeConfiguration.cs`.

**Rationale**: Matches the existing pattern for `Ingredients` (nullable text with max length). PostgreSQL `text` type has no inherent length limit, but the EF Core `.HasMaxLength(10000)` enforces it at the application level. A simple `ALTER TABLE ADD COLUMN` migration is low-risk.

**Alternatives considered**:
- Separate `preparation_steps` table with structured rows: Rejected per spec — preparation is free-text, same as ingredients.
- JSONB column: Rejected as over-engineering for a simple text field.

## R6: Frontend Multi-Image Component

**Decision**: Refactor `ImageUpload.vue` to manage an array of files (max 5) with individual thumbnail previews and remove buttons. Change the emitted event from `file-selected` (single File) to `files-changed` (File[]). Add an "Extract" button that the user clicks after selecting all images.

**Rationale**: The current component is tightly coupled to single-file selection. Supporting multi-image requires: (1) `multiple` attribute on the file input, (2) thumbnail grid with remove buttons, (3) cumulative file list state, (4) max-count enforcement.

**Alternatives considered**:
- Keep single-file component, add separate "batch" component: Rejected because it duplicates validation and preview logic.
- Drag-and-drop only: Rejected because click-to-upload must remain as an alternative (accessibility).

## R7: Frontend FormData for Multi-File

**Decision**: Update `recipeService.extractFromImage()` to accept `File[]` and append each file as `files` in the FormData (matching the backend `IFormFileCollection` parameter name).

**Rationale**: `FormData.append()` with the same key name multiple times is the standard way to send multiple files in a single multipart request. The backend `IFormFileCollection` parameter binds to this automatically.

**Alternatives considered**:
- Indexed keys (`files[0]`, `files[1]`): Rejected because ASP.NET Core model binding handles repeated keys natively.

## R8: AI Prompt Update for Preparation

**Decision**: Update the system prompt and JSON schema in `RecipeImageExtractor` to include a `preparation` field (string|null). Update `ExtractedRecipe` internal record and `RecipeImageExtractResult` DTO accordingly.

**Rationale**: The existing structured output (JSON schema format) makes adding a field straightforward. The AI is already instructed to set fields to null when not visible — this pattern extends naturally to preparation.

**Alternatives considered**:
- Separate extraction pass for preparation: Rejected because it doubles API cost and the single-pass approach already works for 4 fields.
