targetScope = 'resourceGroup'

param environmentName string
param location string
param staticWebAppName string
param staticWebAppDefaultHostname string
param appServicePlanName string
param appServiceName string
param appServiceDefaultHostname string
param postgresServerName string
param postgresDatabaseName string
param postgresServerFqdn string
param keyVaultName string
param keyVaultUri string
param tags object = {}

var effectiveTags = union(
  {
    application: 'WhatsForDinner'
    environment: environmentName
    managedBy: 'bicep'
  },
  tags
)

output deploymentEnvironment object = {
  name: environmentName
  location: location
  resourceGroupName: resourceGroup().name
}

output staticWebAppName string = staticWebAppName
output staticWebAppUrl string = 'https://${staticWebAppDefaultHostname}'
output appServicePlanName string = appServicePlanName
output appServiceName string = appServiceName
output appServiceUrl string = 'https://${appServiceDefaultHostname}'
output postgresServerName string = postgresServerName
output postgresDatabaseName string = postgresDatabaseName
output postgresServerFqdn string = postgresServerFqdn
output keyVaultName string = keyVaultName
output keyVaultUri string = keyVaultUri
output effectiveTags object = effectiveTags
