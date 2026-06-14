# Azure Deployment Runbook

## Purpose

This runbook captures operational guidance for the Azure infrastructure and CI/CD deployment flow.

## Initial Scope

- Required GitHub OIDC variables and Azure prerequisites
- What-if validation before deployment
- Troubleshooting and rerun guidance
- Validation that Azure OpenAI resources remain out of scope
- Coverage and lint gates that must pass before deployment
- Success-rate tracking approach for deployment workflow runs

## Operator Checklist

1. Confirm `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID` are configured.
2. Validate Bicep templates with a local build and what-if run.
3. Confirm deployment inputs match the target resource group and region.
4. Review workflow logs for failing stages before rerunning a deployment.

## Required GitHub Variables and Secrets

Variables:

- `AZURE_RESOURCE_GROUP`
- `AZURE_LOCATION`
- `AZURE_APP_SERVICE_NAME`
- `AZURE_APP_SERVICE_PLAN_NAME`
- `AZURE_STATIC_WEB_APP_NAME`
- `AZURE_POSTGRES_SERVER_NAME`
- `AZURE_POSTGRES_DATABASE_NAME`
- `AZURE_KEY_VAULT_NAME`
- `GOOGLE_CLIENT_ID`
- `OPENAI_ENDPOINT`
- `FRONTEND_ALLOWED_ORIGIN`
- `AZURE_POSTGRES_ADMIN_LOGIN` (optional; defaults to `wfdadmin`)

Secrets:

- `AZURE_POSTGRES_ADMIN_PASSWORD`
- `AZURE_JWT_SIGNING_KEY`
- `AZURE_OPENAI_API_KEY`

## Troubleshooting Matrix

| Symptom | Likely Cause | Operator Action |
|---|---|---|
| `azure/login` fails | OIDC federation mismatch | Verify federated credential subject, audience, and repository binding |
| Bicep what-if fails | Invalid parameters or unsupported region | Review the deployment error and confirm parameter overrides match the target region |
| App Service starts but API fails | Key Vault access or secret content issue | Check role assignment to the app's managed identity and validate Key Vault secret values |
| Frontend cannot reach backend | Incorrect `VITE_API_BASE_URL` or CORS origin | Verify `FRONTEND_ALLOWED_ORIGIN` and the backend URL used during frontend build |

## OpenAI Boundary Verification

- CI runs `scripts/validate-no-openai.ps1` to prevent OpenAI-related resource references from being added to source files.
- The deployment workflow queries the target resource group after infrastructure deployment and fails if any `Microsoft.CognitiveServices/accounts` resources are present.

## Idempotency Validation

1. Run the deployment workflow once and capture the output summary.
2. Run the same workflow a second time with unchanged inputs.
3. Confirm the workflow succeeds again without adding new primary resources.
4. Compare resource inventory and deployment outputs between the two runs.

## Deployment Success-Rate Measurement

- Track `Deploy Azure` workflow outcomes over a rolling 30-day period using the GitHub Actions run history for the default branch.
- Count only runs that reached the deployment workflow with a valid target configuration.
- Success rate formula: successful end-to-end runs / total end-to-end runs over the last 30 days.
- Investigate if the measured success rate drops below 95%.