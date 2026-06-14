targetScope = 'resourceGroup'

param location string
param name string
@allowed([
  'Free'
  'Standard'
])
param skuName string = 'Free'
param tags object = {}

resource staticWebApp 'Microsoft.Web/staticSites@2024-04-01' = {
  name: name
  location: location
  sku: {
    name: skuName
    tier: skuName
  }
  properties: {
    stagingEnvironmentPolicy: 'Enabled'
    allowConfigFileUpdates: true
  }
  tags: tags
}

output resourceName string = name
output defaultHostname string = staticWebApp.properties.defaultHostname
output appliedTags object = tags
