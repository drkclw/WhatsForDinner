# Feature Specification: Add Recipe Entry

**Feature Branch**: `002-add-recipe-entry`  
**Created**: 2026-03-20  
**Status**: Draft  
**Input**: User description: "We need to add functionality to add recipes to a user's recipe inventory. Recipes can be added in 2 ways: by uploading an image that contains the recipe or by manual data entry by the user"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Add Recipe via Manual Entry (Priority: P1)

As a user, I want to manually enter a new recipe into my recipe inventory by filling in the recipe details (name, description, ingredients, and cook time) so that I can build up my personal collection of meals for weekly planning.

**Why this priority**: Manual entry is the fundamental way to add recipes and has no external dependencies. It provides immediate value by allowing users to grow their recipe collection beyond the initial predefined set. This is the simplest path to delivering the core "add recipe" capability.

**Independent Test**: Can be fully tested by navigating to the add recipe form, filling in recipe details, submitting, and verifying the new recipe appears in the user's recipe list. Delivers the ability to expand the recipe collection.

**Acceptance Scenarios**:

1. **Given** the user is viewing their recipe list, **When** they click the "Add Recipe" button, **Then** they are presented with an entry form containing fields for recipe name, description, ingredients, and cook time
2. **Given** the user has filled in the recipe form with at least a recipe name, **When** they submit the form, **Then** the recipe is saved to their inventory and they are navigated to the recipe list where the new recipe is visible
3. **Given** the user is on the add recipe form, **When** they submit without providing a recipe name, **Then** a validation message is displayed indicating the name is required and the form is not submitted
4. **Given** the user is on the add recipe form, **When** they choose to cancel, **Then** no recipe is saved and they are returned to the previous view
5. **Given** the user submits a valid recipe, **When** the save operation completes, **Then** a confirmation is displayed indicating the recipe was added successfully

---

### User Story 2 - Add Recipe via Image Upload (Priority: P2)

As a user, I want to upload an image of a recipe (e.g., a photo of a recipe card, cookbook page, or handwritten recipe) so that the system can extract the recipe information and add it to my inventory without me having to type everything manually.

**Why this priority**: Image upload is the more convenient method of recipe entry and differentiates this application. However, it depends on having the manual entry flow established first, since users will need to review and potentially edit the extracted recipe data before saving.

**Independent Test**: Can be fully tested by uploading a recipe image, reviewing the extracted recipe details, confirming or editing the results, and verifying the recipe is saved to the inventory. Delivers a faster way to add recipes.

**Acceptance Scenarios**:

1. **Given** the user is on the add recipe screen, **When** they choose the "Upload Image" option, **Then** they are prompted to select or capture an image file
2. **Given** the user has selected a valid image file (JPEG, PNG, or WebP), **When** the image is submitted for processing, **Then** a loading indicator is displayed while the recipe information is being extracted
3. **Given** the image has been processed successfully, **When** the extraction results are returned, **Then** the user sees the add recipe form pre-populated with the extracted recipe name, description, ingredients, and cook time
4. **Given** the form is pre-populated with extracted data, **When** the user reviews the results, **Then** they can edit any of the fields before saving the recipe
5. **Given** the user is satisfied with the extracted (and optionally edited) recipe data, **When** they submit the form, **Then** the recipe is saved to their inventory just like a manually entered recipe
6. **Given** the image processing fails to extract meaningful recipe data, **When** the extraction result is returned, **Then** the user is informed that extraction was unsuccessful and given the option to try a different image or switch to manual entry
7. **Given** the user uploads a file that is not a supported image format, **When** the upload is attempted, **Then** a validation message indicates the supported formats and the upload is rejected

---

### User Story 3 - Review and Edit Extracted Recipe Before Saving (Priority: P3)

As a user, after uploading a recipe image, I want to review and correct the extracted information before it is saved so that I can ensure accuracy in my recipe inventory.

**Why this priority**: Automated extraction from images will not always be perfectly accurate. This review step provides a safety net that ensures data quality, building user trust in the image upload feature.

**Independent Test**: Can be fully tested by uploading an image, verifying the extracted data appears in editable fields, making corrections, and confirming the saved recipe reflects the edited values. Delivers confidence in data accuracy.

**Acceptance Scenarios**:

1. **Given** recipe data has been extracted from an uploaded image, **When** the pre-populated form is displayed, **Then** all fields are editable and clearly show what was extracted
2. **Given** the user modifies one or more extracted fields, **When** they save the recipe, **Then** the saved recipe reflects the user's edits rather than the original extracted values
3. **Given** incomplete data was extracted (e.g., cook time could not be determined), **When** the form is displayed, **Then** the missing fields are left empty for the user to fill in manually

---

### User Story 4 - Delete Recipe from Inventory (Priority: P4)

As a user, I want to delete a recipe from my inventory so that I can remove entries I no longer want, such as mistakes or recipes I'll never make again.

**Why this priority**: Completes the recipe lifecycle management (add, edit, delete). Lower priority than the two add methods since it's a supporting action, but essential for a clean user experience.

**Independent Test**: Can be fully tested by selecting a recipe, choosing to delete it, confirming the deletion, and verifying it no longer appears in the recipe list. Delivers the ability to keep the recipe inventory tidy.

**Acceptance Scenarios**:

1. **Given** the user is viewing a recipe or the recipe list, **When** they choose to delete a recipe, **Then** a confirmation prompt is displayed before the recipe is removed
2. **Given** the user confirms deletion, **When** the delete operation completes, **Then** the recipe is permanently removed from their inventory and no longer visible in the recipe list
3. **Given** the user cancels the deletion confirmation, **When** the prompt is dismissed, **Then** the recipe remains unchanged in the inventory
4. **Given** a recipe is currently assigned to the weekly plan, **When** the user deletes that recipe, **Then** the recipe is removed from both the inventory and any weekly plan entries that reference it

---

### Edge Cases

- What happens when the user uploads an image that contains no recognizable recipe content (e.g., a photo of a sunset)? The system should inform the user that no recipe could be extracted and suggest trying a different image or using manual entry.
- What happens when the user uploads a very large image file? The system should enforce a maximum file size (assumed: 10 MB) and display a clear message if the limit is exceeded.
- What happens when the image extraction service is temporarily unavailable? The user should be notified that image processing is currently unavailable and offered the option to switch to manual entry.
- What happens when the user tries to add a recipe with a name identical to an existing recipe? The system should allow it — users may have variations of the same recipe (e.g., "Mom's Chili" and another "Mom's Chili" with different ingredients).
- What happens when the user loses connectivity during image upload or form submission? The system should display an appropriate error message and allow the user to retry without losing entered data.
- What happens when the extracted text is in a language different from the application's language? The system should display the extracted text as-is and let the user decide how to handle it.
- What happens when a user deletes a recipe that is assigned to the current weekly plan? The recipe should be removed from the weekly plan as well, and the weekly plan view should update accordingly.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST provide an "Add Recipe" action accessible from both the recipe list view and the weekly plan view
- **FR-002**: System MUST present a recipe entry form with fields for name, description, ingredients (single free-text textarea), and cook time
- **FR-003**: System MUST require the recipe name field to be non-empty before allowing submission
- **FR-004**: System MUST allow description, ingredients, and cook time to be optional when adding a recipe
- **FR-005**: System MUST save the new recipe to the current user's recipe inventory upon successful submission
- **FR-006**: System MUST display a confirmation to the user when a recipe is successfully added
- **FR-007**: System MUST allow the user to cancel recipe creation and return to the previous view without saving
- **FR-008**: System MUST provide an image upload option on the add recipe screen
- **FR-009**: System MUST accept image files in JPEG, PNG, and WebP formats for recipe extraction
- **FR-010**: System MUST reject image uploads that exceed 10 MB and display an appropriate error message
- **FR-011**: System MUST display a loading indicator while an uploaded image is being processed
- **FR-012**: System MUST extract recipe information (name, description, ingredients, cook time) from uploaded images using a cloud AI vision API and pre-populate the recipe entry form with the results
- **FR-013**: System MUST allow users to edit all pre-populated fields before saving an image-extracted recipe
- **FR-014**: System MUST inform the user when recipe extraction from an image fails and offer alternatives (try another image or switch to manual entry)
- **FR-015**: System MUST validate that cook time, when provided, is a non-negative number (0 or greater)
- **FR-016**: System MUST display the newly added recipe in the user's recipe list after successful creation
- **FR-017**: System MUST allow users to delete a recipe from their inventory with a confirmation prompt before permanent removal
- **FR-018**: System MUST remove deleted recipes from any weekly plan entries that reference them

### Key Entities

- **Recipe**: A meal entry in the user's inventory. Key attributes: name (required), description (optional), ingredients as free-text (optional), cook time in minutes (optional). Belongs to a single user. Can be referenced by weekly plan items.
- **Recipe Image**: A user-uploaded image file containing recipe information. Used as input for recipe data extraction. Not persisted long-term after extraction is complete — only the resulting recipe data is saved.

## Clarifications

### Session 2026-03-20

- Q: Should ingredients be a single free-text field, a structured list of individual items, or a hybrid? → A: Single free-text field (multi-line textarea). Matches existing data model and keeps image extraction simple.
- Q: Should image extraction use a local/self-hosted OCR, a cloud AI vision API, or browser-based OCR? → A: Cloud AI vision API (e.g., OpenAI Vision, Google Cloud Vision). Best extraction quality with minimal hosting overhead for a single-user app.
- Q: Should delete recipe be included in this feature or deferred? → A: In scope. Users who can add recipes will expect to remove mistakes or unwanted entries.
- Q: Should the cloud AI API key be configured server-side or provided by each user? → A: Server-side configuration (environment variable or config file), managed by the app operator.
- Q: Should "Add Recipe" be accessible only from the recipe list, or from both the recipe list and weekly plan views? → A: Both views, to reduce friction when users realize they need a new recipe while planning.

## Assumptions

- The current application has a single predefined user (no authentication). New recipes will be associated with this user.
- Duplicate recipe names are allowed since users may have multiple variations of the same dish.
- The image extraction process is handled by a cloud-based AI vision API (e.g., OpenAI Vision, Google Cloud Vision) that accepts an image and returns structured recipe text. This requires network connectivity and a valid API key.
- The cloud AI API key is configured server-side (environment variable or configuration file) by the application operator. It is not exposed to or managed by end users.
- The maximum image upload size is 10 MB, which accommodates typical smartphone photos.
- Supported image formats (JPEG, PNG, WebP) cover the vast majority of photos users would take or have saved.
- The recipe image is not stored permanently after extraction. Only the extracted and user-confirmed recipe data is persisted.
- Cook time is measured in whole minutes, consistent with the existing recipe model.
- The recipe entry form is shared between manual entry and image-upload flows — the only difference is whether fields are pre-populated.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can manually add a new recipe to their inventory in under 2 minutes
- **SC-002**: Users can add a recipe via image upload (including review and save) in under 1 minute
- **SC-003**: 90% of users successfully add a recipe on their first attempt (via either method)
- **SC-004**: Image extraction produces usable results (at least recipe name correctly extracted) for 80% of clear, well-lit recipe photos
- **SC-005**: The newly created recipe is immediately visible in the recipe list and available for weekly plan assignment without requiring a page refresh
- **SC-006**: Users who encounter an extraction failure can successfully fall back to manual entry without restarting the add recipe flow
