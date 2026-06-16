// dt/infra/data/main.bicep
// Data layer: PostgreSQL Flexible Server (VNet-injected), database, and Azure Files share.
// Requires baseName + storageAccountName + vnetId + dbSubnetId from the foundation layer output.
// PostgreSQL is deployed inside snet-db — no public internet access.
// A Private DNS zone lets the DT backend resolve the server FQDN over the private network.

targetScope = 'resourceGroup'

@description('Short environment identifier, used for resource tagging.')
param env string

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Base name passed from the foundation layer output.')
param baseName string

@description('PostgreSQL administrator username.')
param dbAdminUser string

@description('PostgreSQL administrator password. Store as a secret pipeline variable — not in source.')
@secure()
param dbAdminPassword string

@description('Storage account name from the foundation layer output. The Azure Files share for vulnerability data will be created inside this account.')
param storageAccountName string

@description('VNet resource ID from the foundation layer output. Used to link the Private DNS zone.')
param vnetId string

@description('Subnet resource ID for the PostgreSQL delegation, from the foundation layer dbSubnetId output.')
param dbSubnetId string

@description('Name of the PostgreSQL database to create.')
param dbName string = 'dependencytrack'

// Reference the storage account created in the foundation layer
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Private DNS zone required for VNet-injected PostgreSQL Flexible Server.
// Allows the DT API server to resolve the PostgreSQL FQDN via the private IP inside the VNet.
resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: 'privatelink.postgres.database.azure.com'
  location: 'global'
  tags: { environment: env }
}

// Link the DNS zone to the VNet so all resources inside the VNet can resolve it
resource dnsZoneVnetLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${baseName}-pg-dns-link'
  location: 'global'
  properties: {
    virtualNetwork: { id: vnetId }
    registrationEnabled: false
  }
}

resource fileService 'Microsoft.Storage/storageAccounts/fileServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// Mounted to the DT API server container at /data via ALPINE_DATA_DIRECTORY.
// Stores mirrored vulnerability feeds (NVD, OSV, etc.) so they survive container restarts.
resource vulnerabilityDataShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2023-01-01' = {
  parent: fileService
  name: 'vulnerability-data'
  properties: {
    shareQuota: 50 // GB — sufficient for all DT vulnerability mirrors
  }
}

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: '${baseName}-pg'
  location: location
  tags: { environment: env }
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: dbAdminUser
    administratorLoginPassword: dbAdminPassword
    storage: {
      storageSizeGB: 32 // Minimum allowed value
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    authConfig: {
      activeDirectoryAuth: 'Disabled'
      passwordAuth: 'Enabled'
    }
    network: {
      // VNet injection: server gets a private IP on snet-db; public internet access is disabled
      delegatedSubnetResourceId: dbSubnetId
      privateDnsZoneArmResourceId: privateDnsZone.id
    }
  }
  // DNS zone VNet link must exist before the server can register its private IP in DNS
  dependsOn: [dnsZoneVnetLink]
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgresServer
  name: dbName
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// JDBC connection string with embedded credentials — plain text for demo only
var dbConnectionString = 'jdbc:postgresql://${postgresServer.properties.fullyQualifiedDomainName}:5432/${dbName}?sslmode=require&user=${dbAdminUser}&password=${dbAdminPassword}'

// Clean JDBC URL without credentials — passed to ALPINE_DATABASE_URL; credentials go in separate env vars.
var dbUrl = 'jdbc:postgresql://${postgresServer.properties.fullyQualifiedDomainName}:5432/${dbName}?sslmode=require'

output dbServerName string = postgresServer.name
output dbName string = database.name
// dbUrl does not contain credentials; safe to use as a non-secret output
output dbUrl string = dbUrl
output dbAdminUser string = dbAdminUser
// Kept for convenience when the caller prefers a single JDBC URL with credentials embedded
@secure()
output dbConnectionString string = dbConnectionString
output vulnerabilityShareName string = vulnerabilityDataShare.name
