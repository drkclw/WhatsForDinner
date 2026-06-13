# Azure Infrastructure

This folder contains the Bicep templates for the WhatsForDinner Azure deployment baseline.

## Current Scope

- Shared deployment parameters and naming contract
- Infrastructure module entrypoints for Static Web Apps, App Service, PostgreSQL Flexible Server, and Key Vault
- Workflow-facing outputs consumed by CI/CD
- Key Vault-backed runtime settings for backend deployment
- Explicit PostgreSQL TLS and firewall configuration

## Validation

Run these commands before opening a pull request:

```powershell
az bicep build --file infra/main.bicep
az deployment group what-if --resource-group <resource-group> --template-file infra/main.bicep --parameters infra/main.bicepparam
```

## Next Steps

- Provide environment-specific secure parameter values through GitHub secrets or deployment overrides
- Run a second deployment to confirm idempotency before promoting this workflow for regular use
- Keep Azure OpenAI provisioning out of this stack and validate that boundary in CI