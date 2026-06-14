# Research: Azure IaC and CI/CD Baseline

**Feature**: 001-azure-iac-cicd  
**Date**: 2026-06-13  
**Status**: Complete

## Decision 1: Infrastructure Language and Layout

Decision: Use Bicep as the only IaC language and place all templates under infra/ with a composable main/modules structure.

Rationale: The feature explicitly requires Bicep. A module-based layout improves readability, reuse, and separation of concerns while matching Azure best practices.

Alternatives considered: Terraform was rejected because it conflicts with the explicit requirement to use Bicep; a single monolithic Bicep file was rejected due to maintainability concerns.

## Decision 2: Hosting Topology

Decision: Deploy frontend to Azure Static Web Apps and backend to Azure App Service on a dedicated App Service Plan.

Rationale: This directly satisfies FR-002 and FR-003, aligns with current frontend/backend split, and keeps operational complexity low for a personal project.

Alternatives considered: Hosting backend in Azure Container Apps was rejected because the requirement explicitly calls for App Service.

## Decision 3: Database Tier

Decision: Use Azure Database for PostgreSQL Flexible Server with one application database.

Rationale: This is the required managed relational service in scope and supports current backend data needs.

Alternatives considered: Azure SQL Database and Cosmos DB were rejected because they do not satisfy the stated PostgreSQL Flexible Server requirement.

## Decision 4: CI/CD Authentication

Decision: Use GitHub OIDC federated identity to authenticate GitHub Actions to Azure.

Rationale: This avoids long-lived secrets and is the selected clarification outcome. It improves security posture and reduces credential rotation overhead.

Alternatives considered: Service principal client secret and publish-profile based auth were rejected due to secret sprawl and weaker security controls.

## Decision 5: Secret Management

Decision: Store database/application secrets in Azure Key Vault and reference them from App Service configuration.

Rationale: This meets FR-013 and prevents secret material from being committed to source control or embedded in workflow YAML.

Alternatives considered: Plain GitHub repository secrets as the runtime source of truth were rejected because the clarification selected Key Vault as the secret source.

## Decision 6: Environment Strategy

Decision: Implement a single deployment environment with one CI/CD deployment path.

Rationale: The project is personal and the chosen clarification explicitly removes multi-environment promotion complexity.

Alternatives considered: Dev/staging/prod promotion pipelines were rejected as unnecessary overhead for current scope.

## Decision 7: PostgreSQL Network Access Model

Decision: Configure PostgreSQL Flexible Server with public access and permissive allow rules as requested.

Rationale: This is a direct clarification decision and minimizes setup complexity for a personal project.

Alternatives considered: Private endpoint + VNet integration and strict IP allowlisting were rejected due to additional setup and operational complexity for current project needs.

## Decision 8: Azure OpenAI Boundary

Decision: Do not create Azure OpenAI resources in this feature's Bicep templates.

Rationale: FR-006 and the user request explicitly state Azure OpenAI already exists and is out of scope.

Alternatives considered: Including Azure OpenAI provisioning in the same stack was rejected as scope creep and a direct requirement violation.

## Decision 9: Workflow Shape

Decision: Use separate CI and deployment workflows: ci.yml for validation/tests and deploy-azure.yml for infra + app deployment.

Rationale: This improves run observability, allows independent reruns, and supports fail-fast behavior with clearer stage boundaries.

Alternatives considered: A single large workflow file was rejected because it makes troubleshooting and ownership boundaries less clear.
