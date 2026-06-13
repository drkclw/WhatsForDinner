targetScope = 'resourceGroup'

// This stack deliberately excludes Azure OpenAI provisioning. Existing OpenAI
// resources are referenced only through configuration values supplied at deploy time.

@description('Logical environment name for this single Azure deployment target.')
param environmentName string = 'prod'

@description('Azure region used for resource deployment.')
param location string = resourceGroup().location

@description('Short prefix applied to resource names.')
@minLength(2)
param resourcePrefix string = 'wfd'

@description('Static Web Apps resource name.')
param staticWebAppName string = '${resourcePrefix}-${environmentName}-web'

@description('App Service Plan resource name.')
param appServicePlanName string = '${resourcePrefix}-${environmentName}-plan'

@description('Backend App Service resource name.')
param appServiceName string = '${resourcePrefix}-${environmentName}-api'

@description('PostgreSQL Flexible Server resource name.')
param postgresServerName string = '${resourcePrefix}-${environmentName}-pg'

@description('Primary PostgreSQL database name.')
param postgresDatabaseName string = 'whatsfordinner'

@description('Key Vault resource name.')
param keyVaultName string = '${resourcePrefix}-${environmentName}-kv'

@description('Azure App Service plan SKU name.')
param appServicePlanSkuName string = 'B1'

@description('Azure App Service plan SKU tier.')
param appServicePlanSkuTier string = 'Basic'

@description('Static Web Apps SKU tier.')
@allowed([
  'Free'
  'Standard'
])
param staticWebAppSku string = 'Free'

@description('PostgreSQL Flexible Server SKU name.')
param postgresSkuName string = 'Standard_B1ms'

@description('PostgreSQL Flexible Server SKU tier.')
param postgresSkuTier string = 'Burstable'

@description('PostgreSQL major version.')
@allowed([
  '13'
  '14'
  '15'
  '16'
  '17'
])
param postgresVersion string = '16'

@description('PostgreSQL admin username.')
param postgresAdministratorLogin string = 'wfdadmin'

@secure()
@description('PostgreSQL admin password stored into Key Vault and used for initial server provisioning.')
param postgresAdministratorPassword string

@description('PostgreSQL storage size in GB.')
param postgresStorageSizeGb int = 32

@description('PostgreSQL backup retention in days.')
param postgresBackupRetentionDays int = 7

@description('Start IP address for the primary PostgreSQL firewall rule.')
param postgresFirewallStartIp string = '0.0.0.0'

@description('End IP address for the primary PostgreSQL firewall rule.')
param postgresFirewallEndIp string = '255.255.255.255'

@description('Frontend origin allowed by backend CORS in deployed environments.')
param frontendAllowedOrigin string = 'https://example.azurestaticapps.net'

@description('Google client ID used by the backend authentication flow.')
param googleClientId string = ''

@secure()
@description('JWT signing key used by the backend authentication flow.')
param jwtSigningKey string

@secure()
@description('Azure OpenAI API key already provisioned outside this stack.')
param openAiApiKey string

@description('Azure OpenAI endpoint already provisioned outside this stack.')
param openAiEndpoint string = ''

@description('Azure OpenAI model name used by the backend.')
param openAiModel string = 'gpt-4o-mini'

@description('Azure OpenAI timeout in seconds.')
param openAiTimeoutSeconds int = 90

@description('Common tags applied to all resources.')
param tags object = {}

var effectiveTags = union(
  {
    application: 'WhatsForDinner'
    environment: environmentName
    managedBy: 'bicep'
  },
  tags
)

module keyVault './modules/key-vault.bicep' = {
  name: 'keyvault-${environmentName}'
  params: {
    location: location
    name: keyVaultName
    tenantId: subscription().tenantId
    postgresAdministratorLogin: postgresAdministratorLogin
    postgresAdministratorPassword: postgresAdministratorPassword
    postgresConnectionString: 'Host=${postgresServerName}.postgres.database.azure.com;Database=${postgresDatabaseName};Username=${postgresAdministratorLogin};Password=${postgresAdministratorPassword};SSL Mode=Require;Trust Server Certificate=false'
    googleClientId: googleClientId
    jwtSigningKey: jwtSigningKey
    openAiApiKey: openAiApiKey
    openAiEndpoint: openAiEndpoint
    tags: effectiveTags
  }
}

module appService './modules/app-service.bicep' = {
  name: 'appservice-${environmentName}'
  params: {
    location: location
    planName: appServicePlanName
    planSkuName: appServicePlanSkuName
    planSkuTier: appServicePlanSkuTier
    appName: appServiceName
    keyVaultName: keyVaultName
    keyVaultUri: keyVault.outputs.vaultUri
    allowedOrigin: frontendAllowedOrigin
    jwtIssuer: 'whatsfordinner-api'
    jwtAudience: 'whatsfordinner-spa'
    jwtExpiryDays: 30
    openAiModel: openAiModel
    openAiTimeoutSeconds: openAiTimeoutSeconds
    tags: effectiveTags
  }
}

module postgres './modules/postgres-flex.bicep' = {
  name: 'postgres-${environmentName}'
  params: {
    location: location
    serverName: postgresServerName
    databaseName: postgresDatabaseName
    administratorLogin: postgresAdministratorLogin
    administratorLoginPassword: postgresAdministratorPassword
    skuName: postgresSkuName
    skuTier: postgresSkuTier
    version: postgresVersion
    storageSizeGb: postgresStorageSizeGb
    backupRetentionDays: postgresBackupRetentionDays
    firewallStartIp: postgresFirewallStartIp
    firewallEndIp: postgresFirewallEndIp
    tags: effectiveTags
  }
}

module staticWebApp './modules/static-web-app.bicep' = {
  name: 'staticwebapp-${environmentName}'
  params: {
    location: location
    name: staticWebAppName
    skuName: staticWebAppSku
    tags: effectiveTags
  }
}

module outputsContract './modules/monitoring-and-outputs.bicep' = {
  name: 'outputs-${environmentName}'
  params: {
    environmentName: environmentName
    location: location
    staticWebAppName: staticWebAppName
    staticWebAppDefaultHostname: staticWebApp.outputs.defaultHostname
    appServicePlanName: appServicePlanName
    appServiceName: appServiceName
    appServiceDefaultHostname: appService.outputs.defaultHostname
    postgresServerName: postgresServerName
    postgresDatabaseName: postgresDatabaseName
    postgresServerFqdn: postgres.outputs.fullyQualifiedDomainName
    keyVaultName: keyVaultName
    keyVaultUri: keyVault.outputs.vaultUri
    tags: effectiveTags
  }
}

output deploymentEnvironment object = outputsContract.outputs.deploymentEnvironment
output staticWebAppName string = outputsContract.outputs.staticWebAppName
output staticWebAppUrl string = outputsContract.outputs.staticWebAppUrl
output appServicePlanName string = outputsContract.outputs.appServicePlanName
output appServiceName string = outputsContract.outputs.appServiceName
output appServiceUrl string = outputsContract.outputs.appServiceUrl
output postgresServerName string = outputsContract.outputs.postgresServerName
output postgresDatabaseName string = outputsContract.outputs.postgresDatabaseName
output postgresServerFqdn string = outputsContract.outputs.postgresServerFqdn
output keyVaultName string = outputsContract.outputs.keyVaultName
output keyVaultUri string = outputsContract.outputs.keyVaultUri
output effectiveTags object = outputsContract.outputs.effectiveTags
