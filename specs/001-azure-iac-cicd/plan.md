# Implementation Plan: Azure IaC and CI/CD Baseline

**Branch**: `001-azure-iac-cicd` | **Date**: 2026-06-13 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-azure-iac-cicd/spec.md`

## Summary

Create a single-environment Azure deployment baseline using Bicep for frontend (Static Web Apps), backend (App Service), PostgreSQL Flexible Server, and Key Vault-backed secret references, then wire GitHub Actions CI/CD using GitHub OIDC federation for Azure authentication. The implementation excludes Azure OpenAI provisioning and focuses on repeatable infrastructure + deployment automation for this personal project.

## Technical Context

**Language/Version**: Bicep (latest supported by Azure CLI/Bicep CLI) + GitHub Actions YAML; existing app stack is C#/.NET 10 (backend) and TypeScript/Vue 3 (frontend)  
**Primary Dependencies**: Azure Resource Manager via Bicep, Azure Static Web Apps, Azure App Service (+ App Service Plan), Azure Database for PostgreSQL Flexible Server, Azure Key Vault, GitHub Actions with `azure/login` OIDC auth  
**Storage**: Azure Database for PostgreSQL Flexible Server (application data) + Key Vault (secret material)  
**Testing**: Bicep compile/lint validation, ARM what-if validation, YAML and PowerShell lint checks, GitHub Actions workflow checks, constitution-required coverage enforcement for critical paths, and existing `dotnet test` and `npm test` gates for app quality  
**Target Platform**: Azure resource group deployment + GitHub-hosted Linux runners  
**Project Type**: Web application with infrastructure-as-code and CI/CD automation  
**Performance Goals**: Deployment workflows complete reliably with stage-level visibility; failure root cause identifiable from logs in <=10 minutes (aligned with SC-003)  
**Constraints**: Single environment only; no Azure OpenAI creation; OIDC-only Azure auth for workflows; secrets sourced from Key Vault references; PostgreSQL uses a public endpoint with TLS enforced and explicit firewall rules declared in deployment configuration; Bicep files under `infra/`  
**Scale/Scope**: Personal project, one Azure environment, one frontend app, one backend app, one PostgreSQL server, one Key Vault, one CI/CD pipeline set

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate

| Principle | Status | Notes |
|-----------|--------|-------|
| **Code Quality** | PASS | Plan keeps clear separation between infrastructure definitions, workflow logic, and app code; no duplicated deployment logic is introduced. |
| **Testing Standards** | PASS | CI workflows include test gates (`dotnet test`, frontend tests) before deploy and validation steps for infra changes. |
| **User Experience Consistency** | PASS | No UX surface redesign in this feature; deployment work preserves existing frontend behavior and delivery path. |
| **Performance Requirements** | PASS | No runtime path changes to core app request handling; infra/workflow changes focus on deployment reliability and observability. |

### Post-Design Re-Check

| Principle | Status | Notes |
|-----------|--------|-------|
| **Code Quality** | PASS | Artifact boundaries are explicit (`infra/`, `.github/workflows/`, specs contracts); naming and responsibilities are scoped. |
| **Testing Standards** | PASS | Quickstart and contracts define validation order and required checks before deploy. |
| **User Experience Consistency** | PASS | Design does not alter UI interaction contracts; only deployment and hosting topology are introduced. |
| **Performance Requirements** | PASS | Proposed topology supports current app scale; no constitutional performance regressions are introduced by design. |

**Gate Result**: PASS. No constitutional violations requiring exception tracking.

## Project Structure

### Documentation (this feature)

```text
specs/001-azure-iac-cicd/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── README.md
│   └── deployment-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
backend/
├── src/WhatsForDinner.Api/
└── tests/WhatsForDinner.Api.Tests/

frontend/
├── src/
└── tests/

infra/
├── main.bicep
├── main.bicepparam
└── modules/
    ├── static-web-app.bicep
    ├── app-service.bicep
    ├── postgres-flex.bicep
    ├── key-vault.bicep
    └── monitoring-and-outputs.bicep

scripts/
└── validate-no-openai.ps1

docs/
└── operations/
    └── azure-deploy-runbook.md

.github/
└── workflows/
    ├── ci.yml
    └── deploy-azure.yml
```

**Structure Decision**: Use the existing web application layout and add dedicated infrastructure (`infra/`), automation (`.github/workflows/`), operational documentation (`docs/operations/`), and validation script (`scripts/`) folders. This keeps deployment logic, documentation, and policy guardrails separate and matches the task breakdown.

## Complexity Tracking

No constitution violations to justify.
