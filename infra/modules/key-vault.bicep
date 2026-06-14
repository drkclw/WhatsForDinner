targetScope = 'resourceGroup'

param location string
param name string
param tenantId string
param postgresAdministratorLogin string
@secure()
param postgresAdministratorPassword string
@secure()
param postgresConnectionString string
param googleClientId string
@secure()
param jwtSigningKey string
@secure()
param openAiApiKey string
@secure()
param openAiEndpoint string
param tags object = {}

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: name
  location: location
  properties: {
    // Purge protection stays enabled to comply with Azure security guidance and
    // to prevent accidental permanent secret loss during redeployments.
    tenantId: tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableSoftDelete: true
    enablePurgeProtection: true
    enableRbacAuthorization: true
    enabledForDeployment: false
    enabledForTemplateDeployment: false
    enabledForDiskEncryption: false
    publicNetworkAccess: 'Enabled'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
    accessPolicies: []
  }
  tags: tags
}

resource postgresAdminUserSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'postgres-admin-username'
  parent: keyVault
  properties: {
    value: postgresAdministratorLogin
  }
}

resource postgresAdminPasswordSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'postgres-admin-password'
  parent: keyVault
  properties: {
    value: postgresAdministratorPassword
  }
}

resource postgresConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'postgres-connection-string'
  parent: keyVault
  properties: {
    value: postgresConnectionString
  }
}

resource googleClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'google-client-id'
  parent: keyVault
  properties: {
    value: googleClientId
  }
}

resource jwtSigningKeySecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'jwt-signing-key'
  parent: keyVault
  properties: {
    value: jwtSigningKey
  }
}

resource openAiApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'openai-api-key'
  parent: keyVault
  properties: {
    value: openAiApiKey
  }
}

resource openAiEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  name: 'openai-endpoint'
  parent: keyVault
  properties: {
    value: openAiEndpoint
  }
}

output vaultResourceName string = name
output vaultUri string = keyVault.properties.vaultUri
output appliedTags object = tags
