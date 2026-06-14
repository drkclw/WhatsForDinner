# Deployment Contract: Azure IaC and CI/CD Baseline

## 1. Infrastructure Contract (Bicep)

### 1.1 Required Inputs

| Input | Type | Required | Description |
|---|---|---|---|
| location | string | Yes | Azure region for deployment. |
| resourceGroupName | string | Yes | Target resource group name. |
| staticWebAppName | string | Yes | Static Web Apps resource name. |
| appServicePlanName | string | Yes | App Service Plan name. |
| appServiceName | string | Yes | App Service app name. |
| postgresServerName | string | Yes | PostgreSQL Flexible Server name. |
| postgresDatabaseName | string | Yes | Primary PostgreSQL database name. |
| keyVaultName | string | Yes | Key Vault resource name. |
| tags | object | No | Common resource tags. |

### 1.2 Behavioral Guarantees

- Bicep templates are the authoritative source for infrastructure provisioning.
- Deployment is idempotent for unchanged inputs.
- Azure OpenAI resources are not created by this deployment.
- PostgreSQL server is configured for public access with permissive allow rules (accepted project decision).
- App Service runtime secrets are consumed via Key Vault references.

### 1.3 Required Outputs

| Output | Type | Description |
|---|---|---|
| staticWebAppUrl | string | Frontend endpoint URL. |
| appServiceUrl | string | Backend endpoint URL. |
| postgresServerFqdn | string | PostgreSQL server DNS name. |
| keyVaultUri | string | Key Vault URI for runtime secret access. |

## 2. Workflow Contract (GitHub Actions)

### 2.1 Authentication

- Azure authentication mode: GitHub OIDC federation only.
- Long-lived client secrets are not required for Azure login.

Required repository or environment values:
- AZURE_CLIENT_ID
- AZURE_TENANT_ID
- AZURE_SUBSCRIPTION_ID

### 2.2 Workflow Stages

Minimum required stages:
1. Validate: run application tests and basic infra validation.
2. Deploy Infrastructure: apply Bicep templates to target resource group.
3. Deploy Backend: publish backend to App Service.
4. Deploy Frontend: publish frontend to Static Web Apps.

### 2.3 Failure and Logging Semantics

- Any stage failure must stop downstream stages.
- Workflow logs must identify failing stage and error context for diagnosis.
- Re-run of unchanged deployments should not create duplicate primary resources.

## 3. Security and Secret Boundaries

- Database and app secrets are stored in Azure Key Vault.
- Deployment pipeline may set/update Key Vault secrets but must not commit secret values to source control.
- App Service must access Key Vault secrets through managed identity-based permissions.

## 4. Out-of-Scope Contract Clause

- Provisioning, updating, or deleting Azure OpenAI resources is explicitly out of scope for this feature.
