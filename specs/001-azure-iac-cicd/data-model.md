# Data Model: Azure IaC and CI/CD Baseline

**Feature**: 001-azure-iac-cicd  
**Date**: 2026-06-13

## Entities

### DeploymentEnvironment

Represents the single target environment for this project.

| Field | Type | Required | Description |
|---|---|---|---|
| name | string | Yes | Friendly environment identifier (single environment only). |
| location | string | Yes | Azure region used for deployed resources. |
| resourceGroupName | string | Yes | Resource group containing all feature resources. |
| tags | map<string,string> | No | Common governance and ownership tags. |
| deploymentState | enum | Yes | Current environment state (Draft, Provisioned, Failed, Updated). |

Validation rules:
- name must be non-empty and unique within the repository deployment context.
- location must map to a region supporting required services.

### InfrastructureDefinition

Represents the Bicep deployment contract.

| Field | Type | Required | Description |
|---|---|---|---|
| templatePath | string | Yes | Path to main Bicep file under infra/. |
| parameterSet | object | Yes | Parameter values passed at deployment time. |
| outputs | object | Yes | Resource endpoints and identifiers returned after deployment. |
| excludesAzureOpenAI | bool | Yes | Guard flag enforcing no Azure OpenAI creation. |

Validation rules:
- templatePath must resolve to a file in infra/.
- excludesAzureOpenAI must be true.

### StaticWebAppResource

Represents frontend hosting target.

| Field | Type | Required | Description |
|---|---|---|---|
| name | string | Yes | Static Web App resource name. |
| defaultHostname | string | Yes | Public frontend URL. |
| sku | string | Yes | Static Web Apps SKU tier. |
| deploymentTokenReference | string | No | Optional token/credential reference depending on deployment path. |

### AppServiceResource

Represents backend runtime and app configuration.

| Field | Type | Required | Description |
|---|---|---|---|
| planName | string | Yes | App Service Plan name. |
| appName | string | Yes | Web App name. |
| runtimeStack | string | Yes | Backend runtime stack configuration. |
| appSettings | map<string,string> | Yes | Runtime settings including Key Vault references. |
| managedIdentityEnabled | bool | Yes | Whether system-assigned identity is enabled. |

Validation rules:
- managedIdentityEnabled must be true when Key Vault references are configured.

### PostgreSqlFlexibleServerResource

Represents managed relational data service.

| Field | Type | Required | Description |
|---|---|---|---|
| serverName | string | Yes | PostgreSQL Flexible Server name. |
| databaseName | string | Yes | Primary app database name. |
| adminUserSecretRef | string | Yes | Key Vault secret reference for admin username/password material. |
| publicAccessMode | enum | Yes | PublicAccessPermissive (selected model). |
| tlsEnforced | bool | Yes | Whether TLS is required. |

Validation rules:
- publicAccessMode must be PublicAccessPermissive per accepted clarification.
- tlsEnforced must be true.

### KeyVaultResource

Represents secure secret source of truth.

| Field | Type | Required | Description |
|---|---|---|---|
| name | string | Yes | Key Vault name. |
| purgeProtectionEnabled | bool | Yes | Purge protection setting. |
| softDeleteEnabled | bool | Yes | Soft delete state. |
| secretNames | array<string> | Yes | Secrets required for app/database runtime. |

Validation rules:
- purgeProtectionEnabled must be true.
- secretNames must include database credential secrets needed by backend.

### PipelineDefinition

Represents CI/CD workflow contracts.

| Field | Type | Required | Description |
|---|---|---|---|
| ciWorkflowPath | string | Yes | Path to CI workflow YAML file. |
| deployWorkflowPath | string | Yes | Path to Azure deploy workflow YAML file. |
| authMode | enum | Yes | OIDCFederated only. |
| deploymentStages | array<string> | Yes | Ordered stages, includes infra and app deployment. |
| failureBehavior | enum | Yes | FailFast with stage-level logs. |

Validation rules:
- authMode must be OIDCFederated.
- deploymentStages must include both infra and app deployment steps.

## Relationships

- DeploymentEnvironment 1:1 InfrastructureDefinition
- InfrastructureDefinition 1:1 StaticWebAppResource
- InfrastructureDefinition 1:1 AppServiceResource
- InfrastructureDefinition 1:1 PostgreSqlFlexibleServerResource
- InfrastructureDefinition 1:1 KeyVaultResource
- PipelineDefinition deploys InfrastructureDefinition into DeploymentEnvironment
- AppServiceResource consumes secrets from KeyVaultResource

## State Transitions

### DeploymentEnvironment lifecycle

Draft -> Provisioning -> Provisioned -> Updating -> Provisioned
Draft -> Provisioning -> Failed
Provisioned -> Updating -> Failed
Failed -> Provisioning (retry)

### PipelineDefinition run lifecycle

Queued -> RunningValidation -> RunningDeployment -> Succeeded
Queued -> RunningValidation -> Failed
Queued -> RunningValidation -> RunningDeployment -> Failed
