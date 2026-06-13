targetScope = 'resourceGroup'

param location string
param serverName string
param databaseName string
param administratorLogin string
@secure()
param administratorLoginPassword string
param skuName string = 'Standard_B1ms'
param skuTier string = 'Burstable'
param version string = '16'
param storageSizeGb int = 32
param backupRetentionDays int = 7
param firewallStartIp string = '0.0.0.0'
param firewallEndIp string = '255.255.255.255'
param tags object = {}

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: serverName
  location: location
  sku: {
    name: skuName
    tier: skuTier
  }
  properties: {
    // Public access is an explicit personal-project choice. TLS and firewall rules
    // remain declared in IaC so connectivity is reproducible across redeployments.
    createMode: 'Create'
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
    version: version
    storage: {
      storageSizeGB: storageSizeGb
    }
    backup: {
      backupRetentionDays: backupRetentionDays
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Enabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
  }
  tags: tags
}

resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  name: databaseName
  parent: postgresServer
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource openFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  name: 'client-access'
  parent: postgresServer
  properties: {
    startIpAddress: firewallStartIp
    endIpAddress: firewallEndIp
  }
}

output serverResourceName string = serverName
output databaseResourceName string = databaseName
output fullyQualifiedDomainName string = postgresServer.properties.fullyQualifiedDomainName
output appliedTags object = tags
