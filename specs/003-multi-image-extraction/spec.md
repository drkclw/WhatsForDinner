# Feature Specification: Multi-Image Recipe Extraction with Preparation Steps

**Feature Branch**: `003-multi-image-extraction`
**Created**: 2026-04-12
**Status**: Draft
**Input**: User description: "Implement a change to the recipe extraction process so that users can upload more than 1 image for a recipe and the preparation process can be extracted from those images in addition to the fields that are already extracted."

## Clarifications

### Session 2026-04-12

- Q: When multiple images are uploaded, should the system send all images in a single AI request or process each separately and merge? → A: Single AI request containing all images — AI sees full context and returns one merged result.
- Q: What should the maximum character length be for the preparation field? → A: 10,000 characters.
- Q: What progress feedback should the user see during multi-image extraction? → A: Single loading spinner with a message indicating how many images are being processed (e.g., "Extracting from 3 images...").
- Q: What is the canonical term for the new field — "preparation steps", "instructions", or "preparation"? → A: "Preparation" — single word, matches existing field naming style.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Extract Preparation from a Single Image (Priority: P1)

A user has a photo of a recipe that includes preparation (e.g., a page from a cookbook). They upload the image and the system extracts the recipe name, description, ingredients, cook time, and **preparation** — all from the single image. The extracted data pre-populates the recipe form for review before saving.

**Why this priority**: Adding preparation extraction is the core value of this feature. Even without multi-image support, users gain the ability to capture the full recipe — including how to make it — from a single photo.

**Independent Test**: Can be fully tested by uploading a single recipe image that contains preparation and verifying the form is populated with all five fields (name, description, ingredients, cook time, preparation).

**Acceptance Scenarios**:

1. **Given** a user is on the recipe creation page with the "Upload Image" tab selected, **When** they upload a single image containing a recipe with preparation, **Then** the system extracts and pre-populates all available fields including preparation into the recipe form.
2. **Given** a user uploads an image that shows ingredients but no preparation, **When** extraction completes, **Then** the preparation field is left empty and all other visible fields are still extracted.
3. **Given** extraction succeeds with preparation populated, **When** the user reviews the form, **Then** they can edit the extracted preparation before saving.

---

### User Story 2 - Upload Multiple Images for One Recipe (Priority: P2)

A user has a recipe spread across multiple photos — for example, one photo of the ingredient list and another photo of the preparation. They upload all relevant images at once, and the system combines the extracted information from all images into a single unified recipe form.

**Why this priority**: Many recipes in cookbooks or handwritten cards span multiple pages or sections. Supporting multiple images per extraction significantly increases the completeness of extracted data and covers a very common real-world scenario.

**Independent Test**: Can be fully tested by uploading two or more images (e.g., one with ingredients, one with preparation) and verifying that the resulting form contains data merged from all uploaded images.

**Acceptance Scenarios**:

1. **Given** a user is on the recipe creation page with the "Upload Image" tab selected, **When** they upload two images — one containing ingredients and one containing preparation, **Then** the system extracts data from both images and pre-populates the form with the merged result.
2. **Given** a user has uploaded one image, **When** they add a second image before triggering extraction, **Then** both images are shown with previews and can be removed individually.
3. **Given** a user uploads three images where two contain the recipe name, **When** extraction completes, **Then** each form field displays a single value (no duplicated or concatenated content) as determined by the AI's merged interpretation.
4. **Given** a user uploads images where one image fails pre-upload validation (e.g., unsupported file type or corrupt file), **When** validation runs, **Then** the invalid image is rejected with a clear error while valid images remain in the upload set ready for extraction.
5. **Given** a user triggers extraction of multiple images, **When** the extraction is in progress, **Then** the system displays a single loading spinner with a message indicating how many images are being processed (e.g., "Extracting from 3 images...").

---

### User Story 3 - Remove Images Before Extraction (Priority: P3)

A user accidentally uploads a wrong photo alongside the correct recipe photos. Before triggering extraction, they want to remove the incorrect image from the upload set.

**Why this priority**: Providing control over the image set before extraction prevents wasted processing time and ensures the user can correct mistakes without starting over.

**Independent Test**: Can be fully tested by uploading multiple images, removing one, and verifying only the remaining images are sent for extraction.

**Acceptance Scenarios**:

1. **Given** a user has uploaded three images with previews shown, **When** they click the remove button on the second image, **Then** the second image is removed and the remaining two images are still displayed.
2. **Given** a user has removed all uploaded images, **When** they look at the upload area, **Then** the upload prompt is shown again and extraction cannot be triggered.

---

### Edge Cases

- What happens when a user uploads image files that exceed the maximum combined size? The system rejects the upload with a clear error before sending to the extraction service.
- What happens when all uploaded images contain no recipe information? The system returns a failure message indicating no recipe data could be found in any of the provided images.
- What happens when a user uploads the maximum number of allowed images and tries to add another? The system prevents adding more images and displays a message about the limit.
- What happens when images contain overlapping but conflicting information (e.g., different cook times)? The system uses its best judgment to pick the most reasonable value and the user can review and correct before saving.
- What happens when the user uploads a mix of valid recipe images and non-recipe images (e.g., a photo of a cat)? The system extracts what it can from the recipe images and ignores irrelevant content.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST extract preparation (cooking/baking instructions) from recipe images in addition to the currently extracted fields (name, description, ingredients, cook time).
- **FR-002**: System MUST allow users to upload between 1 and 5 images per extraction request.
- **FR-003**: System MUST display a preview thumbnail for each uploaded image before extraction is triggered.
- **FR-004**: System MUST allow users to remove individual images from the upload set before triggering extraction.
- **FR-005**: System MUST send all uploaded images in a single AI extraction request so the AI can see full context across images and return one unified merged result (e.g., ingredients from one image, preparation from another).
- **FR-006**: System MUST continue to support the existing single-image extraction workflow without degradation — uploading one image works exactly as before, plus the new preparation field.
- **FR-007**: System MUST persist the preparation field when saving a recipe, and display it when viewing or editing a recipe.
- **FR-008**: System MUST validate each uploaded image individually (file type: JPEG, PNG, WebP; maximum size: 10 MB per image).
- **FR-009**: System MUST reject invalid images (wrong file type, corrupt, oversized) during pre-upload validation and provide per-file error feedback before the AI request is sent. If the AI request itself fails, the system MUST display a clear error message.
- **FR-010**: System MUST return a clear error if none of the uploaded images contain any recipe information.
- **FR-011**: The preparation field MUST be editable in the recipe form, the same as all other fields.
- **FR-012**: The extraction result MUST indicate which fields were AI-extracted so they can be visually distinguished in the form.

### Key Entities

- **Recipe**: The core entity representing a saved recipe. Gains a new "preparation" field (free-text, maximum 10,000 characters) to store cooking/baking instructions alongside the existing name, description, ingredients, and cook time fields.
- **Extraction Result**: The output of the AI extraction process. Expanded to include the new preparation field. Represents the merged result when multiple images are processed.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can extract a complete recipe — including preparation — from uploaded images in under 60 seconds for up to 5 images.
- **SC-002**: The AI extraction prompt and JSON schema include the preparation field, and the system correctly maps a non-null AI preparation response to the form field. (Extraction quality is AI-dependent and validated via manual testing per quickstart scenarios.)
- **SC-003**: Users can upload, preview, and manage (add/remove) multiple images before extraction without page reloads or navigation.
- **SC-004**: Recipes saved with extracted preparation display correctly when viewed or edited, with no data loss.
- **SC-005**: The existing single-image extraction workflow continues to work with no increase in extraction time for single images.

## Assumptions

- The maximum number of images per extraction is 5. This balances usability with AI processing cost and time.
- Preparation is stored as free-text (same approach as the existing ingredients field), not as a structured list of numbered steps.
- The AI service processes all uploaded images in a single request, receiving full context to produce one merged extraction result. No application-level field merging is required.
- The combined upload size should not exceed 50 MB total (5 images × 10 MB each).
- Image ordering matters — images are processed in the order they were uploaded, which may help the AI handle multi-page recipes.
