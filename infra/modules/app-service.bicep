targetScope = 'resourceGroup'

param location string
param planName string
param planSkuName string = 'B1'
param planSkuTier string = 'Basic'
param appName string
param keyVaultName string
param keyVaultUri string
param allowedOrigin string
param jwtIssuer string
param jwtAudience string
param jwtExpiryDays int = 30
param openAiModel string
param openAiTimeoutSeconds int = 90
param tags object = {}

resource appServicePlan 'Microsoft.Web/serverfarms@2024-11-01' = {
  name: planName
  location: location
  kind: 'linux'
  sku: {
    name: planSkuName
    tier: planSkuTier
    size: planSkuName
    capacity: 1
  }
  properties: {
    reserved: true
  }
  tags: tags
}

resource appService 'Microsoft.Web/sites@2024-11-01' = {
  name: appName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    // Keep runtime configuration in Key Vault references so deployment pipelines
    // never need to write long-lived application secrets into source-controlled files.
    serverFarmId: appServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    keyVaultReferenceIdentity: 'SystemAssigned'
    reserved: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: true
      http20Enabled: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'Cors__AllowedOrigins__0'
          value: allowedOrigin
        }
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/postgres-connection-string/)'
        }
        {
          name: 'Authentication__Google__ClientId'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/google-client-id/)'
        }
        {
          name: 'Authentication__Jwt__Key'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/jwt-signing-key/)'
        }
        {
          name: 'Authentication__Jwt__Issuer'
          value: jwtIssuer
        }
        {
          name: 'Authentication__Jwt__Audience'
          value: jwtAudience
        }
        {
          name: 'Authentication__Jwt__ExpiryDays'
          value: string(jwtExpiryDays)
        }
        {
          name: 'OpenAI__ApiKey'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/openai-api-key/)'
        }
        {
          name: 'OpenAI__Endpoint'
          value: '@Microsoft.KeyVault(SecretUri=${keyVaultUri}secrets/openai-endpoint/)'
        }
        {
          name: 'OpenAI__Model'
          value: openAiModel
        }
        {
          name: 'OpenAI__TimeoutSeconds'
          value: string(openAiTimeoutSeconds)
        }
      ]
    }
  }
  tags: tags
}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: keyVaultName
}

resource keyVaultSecretsUserRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appService.id, 'KeyVaultSecretsUser')
  scope: keyVault
  properties: {
    principalId: appService.identity.principalId
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4633458b-17de-408a-b874-0445c86b69e6'
    )
    principalType: 'ServicePrincipal'
  }
}

output planResourceName string = planName
output appResourceName string = appName
output defaultHostname string = appService.properties.defaultHostName
output managedIdentityPrincipalId string = appService.identity.principalId
output appliedTags object = tags
