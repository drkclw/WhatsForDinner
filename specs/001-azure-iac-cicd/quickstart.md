# Quickstart: Azure IaC and CI/CD Baseline

**Feature**: 001-azure-iac-cicd  
**Date**: 2026-06-13

## Prerequisites

- Azure subscription with permission to create resource groups and role assignments
- GitHub repository admin access for Actions and OIDC configuration
- Azure CLI (latest) with Bicep support
- .NET 10 SDK and Node.js 20+ for application validation steps

## 1. Checkout Feature Branch

```bash
git checkout 001-azure-iac-cicd
```

## 2. Configure GitHub OIDC Federation

Create an Entra application/service principal and federated credential for this repository workflow.

Required workflow configuration values:
- AZURE_CLIENT_ID
- AZURE_TENANT_ID
- AZURE_SUBSCRIPTION_ID
- AZURE_RESOURCE_GROUP
- AZURE_LOCATION
- AZURE_APP_SERVICE_NAME
- AZURE_APP_SERVICE_PLAN_NAME
- AZURE_STATIC_WEB_APP_NAME
- AZURE_POSTGRES_SERVER_NAME
- AZURE_POSTGRES_DATABASE_NAME
- AZURE_KEY_VAULT_NAME
- GOOGLE_CLIENT_ID
- OPENAI_ENDPOINT
- FRONTEND_ALLOWED_ORIGIN

Required workflow secrets:
- AZURE_POSTGRES_ADMIN_PASSWORD
- AZURE_JWT_SIGNING_KEY
- AZURE_OPENAI_API_KEY

Store these as repository environment variables/secrets as appropriate.

## 3. Prepare Deployment Inputs

Define values for:
- Azure region
- Resource group name
- Static Web App name
- App Service plan/app names
- PostgreSQL server/database names
- Key Vault name

Provide secure values for database credentials through Key Vault population flow.
If you omit explicit deployment overrides, the sample parameter file values are used and should be replaced before production deployment.

## 4. Validate Infrastructure Definition

```bash
az bicep build --file infra/main.bicep
az deployment group what-if \
  --resource-group <rg-name> \
  --template-file infra/main.bicep \
  --parameters infra/main.bicepparam
```

## 5. Run CI Workflow

Trigger or push changes to execute:
- Backend tests (`dotnet test`)
- Frontend tests (`npm run test`)
- Frontend type check (`npm run type-check`)
- Frontend production build (`npm run build`)
- Bicep lint/format validation and OpenAI exclusion guard

## 6. Run Deployment Workflow

Execute the Azure deployment workflow to perform:
- OIDC sign-in to Azure
- Infrastructure deployment (Bicep)
- Backend deployment to App Service
- Frontend deployment to Static Web Apps
- Post-deployment check that no Azure OpenAI resources were created in the target resource group

Expected outcome:
- Frontend URL is available from Static Web Apps
- Backend URL is available from App Service
- PostgreSQL Flexible Server and database are provisioned
- No Azure OpenAI resource is created

## 7. Post-Deploy Validation

- Verify workflow logs identify each stage clearly
- Confirm backend app settings resolve secrets from Key Vault
- Confirm PostgreSQL server uses a public endpoint with TLS enforced and explicit firewall rules
- Confirm re-running deployment does not duplicate primary resources

## 8. Troubleshooting

- OIDC auth fails: verify federated credential subject/audience and workflow permissions
- Bicep deployment fails: inspect what-if output and Azure deployment operation details
- App Service cannot read secrets: validate managed identity permissions on Key Vault
- Frontend/backend stage mismatch: rerun only the failed workflow after fixing stage-specific configuration
