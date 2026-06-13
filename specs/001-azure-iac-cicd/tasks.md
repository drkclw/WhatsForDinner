# Tasks: Azure IaC and CI/CD Baseline

**Input**: Design documents from `/specs/001-azure-iac-cicd/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, contracts/, quickstart.md

**Tests**: Test tasks are included where they directly validate infrastructure and pipeline behavior required by the specification.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create base folders and entry files for infrastructure and pipeline work.

- [X] T001 Create infrastructure root templates in infra/main.bicep and infra/main.bicepparam
- [X] T002 Create infrastructure module files in infra/modules/static-web-app.bicep, infra/modules/app-service.bicep, infra/modules/postgres-flex.bicep, infra/modules/key-vault.bicep, and infra/modules/monitoring-and-outputs.bicep
- [X] T003 Create workflow entry files in .github/workflows/ci.yml and .github/workflows/deploy-azure.yml
- [X] T004 Create deployment documentation scaffold in infra/README.md and docs/operations/azure-deploy-runbook.md

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Define shared deployment conventions and guardrails required by all user stories.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [X] T005 Define shared deployment parameters, naming conventions, and tags in infra/main.bicep
- [X] T006 [P] Define baseline parameter values and secure parameter placeholders in infra/main.bicepparam
- [X] T007 [P] Implement module orchestration, dependencies, and common outputs contract in infra/modules/monitoring-and-outputs.bicep
- [X] T008 Implement top-level module wiring and output propagation in infra/main.bicep
- [X] T009 [P] Add Bicep build and what-if validation commands to CI baseline in .github/workflows/ci.yml
- [X] T035 [P] Add YAML workflow lint validation to .github/workflows/ci.yml
- [X] T036 [P] Add Bicep lint and format validation to .github/workflows/ci.yml
- [X] T037 [P] Add PowerShell script lint validation for scripts/validate-no-openai.ps1 in .github/workflows/ci.yml
- [X] T010 Add deployment preflight and fail-fast shell settings in .github/workflows/deploy-azure.yml

**Checkpoint**: Foundation ready. User story implementation can begin.

---

## Phase 3: User Story 1 - Provision Deployment Baseline (Priority: P1) 🎯 MVP

**Goal**: Provision frontend hosting, backend hosting, database, and secret store from one Bicep deployment.

**Independent Test**: Run one infrastructure deployment and confirm Static Web Apps, App Service, PostgreSQL Flexible Server, and Key Vault resources exist with expected outputs.

### Implementation for User Story 1

- [X] T011 [P] [US1] Implement Static Web Apps resource module in infra/modules/static-web-app.bicep
- [X] T012 [P] [US1] Implement App Service Plan and Web App module with managed identity in infra/modules/app-service.bicep
- [X] T013 [P] [US1] Implement PostgreSQL Flexible Server and database module with public access configuration in infra/modules/postgres-flex.bicep
- [X] T038 [US1] Declare PostgreSQL TLS and explicit firewall rule configuration in infra/modules/postgres-flex.bicep and infra/main.bicepparam
- [X] T014 [P] [US1] Implement Key Vault module with soft delete and purge protection in infra/modules/key-vault.bicep
- [X] T015 [US1] Compose all US1 modules and required outputs in infra/main.bicep
- [X] T016 [US1] Configure Key Vault secret references for backend app settings in infra/modules/app-service.bicep
- [X] T017 [US1] Define US1 deployable parameter set and sample values in infra/main.bicepparam
- [X] T018 [US1] Document one-command infrastructure deployment and verification steps in infra/README.md

**Checkpoint**: User Story 1 is deployable and independently verifiable.

---

## Phase 4: User Story 2 - Deploy Application Through CI/CD (Priority: P2)

**Goal**: Deploy infrastructure, backend, and frontend through automated GitHub Actions workflows with clear stage reporting.

**Independent Test**: Trigger workflow and verify validation, infrastructure deployment, backend deployment, and frontend deployment stages execute with pass/fail visibility.

### Implementation for User Story 2

- [X] T019 [P] [US2] Implement backend and frontend test jobs in .github/workflows/ci.yml
- [X] T039 [US2] Add backend critical-path coverage threshold enforcement to .github/workflows/ci.yml with a minimum 80% gate
- [X] T040 [US2] Add frontend critical-path coverage or equivalent validation gate to .github/workflows/ci.yml
- [X] T020 [US2] Implement GitHub OIDC Azure login and workflow permissions in .github/workflows/deploy-azure.yml
- [X] T021 [US2] Implement infrastructure what-if and deployment stages in .github/workflows/deploy-azure.yml
- [X] T022 [P] [US2] Implement backend build and App Service deployment stage in .github/workflows/deploy-azure.yml
- [X] T023 [P] [US2] Implement frontend build and Static Web Apps deployment stage in .github/workflows/deploy-azure.yml
- [X] T024 [US2] Add stage-level failure handling and deployment summary output in .github/workflows/deploy-azure.yml
- [X] T025 [US2] Document required GitHub variables/secrets and trigger strategy in specs/001-azure-iac-cicd/quickstart.md

**Checkpoint**: User Story 2 pipeline stages are independently runnable and observable.

---

## Phase 5: User Story 3 - Respect Existing AI Resource Boundaries (Priority: P3)

**Goal**: Enforce that deployment templates and workflows never create Azure OpenAI resources.

**Independent Test**: Validate templates and workflow guard checks pass while asserting no Azure OpenAI resource types are present or deployed.

### Implementation for User Story 3

- [X] T026 [US3] Add explicit out-of-scope guard comments and exclusion notes in infra/main.bicep
- [X] T027 [P] [US3] Implement static OpenAI resource-type scan script in scripts/validate-no-openai.ps1
- [X] T028 [US3] Add no-openai guard execution to CI validation job in .github/workflows/ci.yml
- [X] T029 [US3] Add post-deployment resource verification step for OpenAI exclusion in .github/workflows/deploy-azure.yml
- [X] T030 [US3] Document OpenAI exclusion verification procedure in docs/operations/azure-deploy-runbook.md

**Checkpoint**: User Story 3 boundary controls are independently validated.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final hardening and end-to-end validation across stories.

- [X] T031 [P] Add deployment troubleshooting matrix and rerun guidance in docs/operations/azure-deploy-runbook.md
- [X] T032 [P] Refine infrastructure module documentation for maintainability in infra/README.md
- [ ] T033 Validate quickstart end-to-end flow and align final instructions in specs/001-azure-iac-cicd/quickstart.md
- [ ] T041 Validate CI blocks merge on failed lint, formatting, and coverage gates in .github/workflows/ci.yml
- [ ] T042 Validate repeat deployment idempotency by executing deployment twice and confirming stable primary resource counts and outputs
- [X] T043 Document the SC-002 deployment success-rate measurement approach in docs/operations/azure-deploy-runbook.md
- [X] T034 Perform final security/configuration sweep for Key Vault and app settings references in infra/modules/key-vault.bicep and infra/modules/app-service.bicep

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Setup)**: No dependencies.
- **Phase 2 (Foundational)**: Depends on Phase 1 and blocks all user story work.
- **Phase 3 (US1)**: Depends on Phase 2.
- **Phase 4 (US2)**: Depends on Phase 2 and US1 resource contracts/outputs being defined.
- **Phase 5 (US3)**: Depends on Phase 2; validates constraints against US1/US2 implementations.
- **Phase 6 (Polish)**: Depends on completion of selected user stories.

### User Story Dependencies

- **US1 (P1)**: Starts after Foundational phase; no dependency on other user stories.
- **US2 (P2)**: Starts after Foundational phase, but uses US1 infrastructure outputs and parameters.
- **US3 (P3)**: Starts after Foundational phase; verifies and enforces boundaries on US1/US2 artifacts.

### Dependency Graph

- Setup -> Foundational -> US1 -> US2 -> Polish
- Foundational -> US3 -> Polish

---

## Parallel Opportunities

- **Setup**: T002 and T003 can proceed in parallel after T001.
- **Foundational**: T006, T007, and T009 can run in parallel after T005.
- **US1**: T011, T012, T013, and T014 can run in parallel before composition tasks.
- **US2**: T022 and T023 can run in parallel after T020 and T021 are in place.
- **US3**: T027 can run in parallel with T026.
- **Polish**: T031 and T032 can run in parallel.

### Parallel Example: User Story 1

```bash
# Parallel module implementation for US1:
Task T011 in infra/modules/static-web-app.bicep
Task T012 in infra/modules/app-service.bicep
Task T013 in infra/modules/postgres-flex.bicep
Task T014 in infra/modules/key-vault.bicep
```

### Parallel Example: User Story 2

```bash
# Parallel deployment stages once workflow scaffold exists:
Task T022 backend deploy stage in .github/workflows/deploy-azure.yml
Task T023 frontend deploy stage in .github/workflows/deploy-azure.yml
```

### Parallel Example: User Story 3

```bash
# Parallel policy boundary tasks:
Task T027 guard script in scripts/validate-no-openai.ps1
Task T029 post-deploy exclusion verification in .github/workflows/deploy-azure.yml
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1).
3. Validate independent test for US1 (single deployment creates required resources and outputs).
4. Demo/deploy MVP baseline.

### Incremental Delivery

1. Deliver US1 infrastructure baseline.
2. Add US2 CI/CD automation.
3. Add US3 boundary enforcement.
4. Finish with Phase 6 polish and quickstart validation.

### Parallel Team Strategy

1. One contributor finalizes Foundational tasks.
2. Contributor A implements US1 modules while Contributor B scaffolds CI jobs for US2.
3. Contributor C adds US3 guardrails once core files exist.
4. Merge by phase checkpoints to reduce workflow conflicts.

---

## Notes

- [P] tasks are safe for parallel execution when prerequisites are complete.
- [USx] labels ensure direct traceability from tasks to user stories.
- Re-validate Bicep build and deployment workflow logs after each major milestone.
