// dt/infra/foundation/main.bicep
// Foundation layer: creates shared storage and establishes the naming postfix.
// Deploy this first. Pass its outputs (baseName, postfix, storage info) to the data and app layers.
//
// Storage account purpose:
//   - Blob container 'config': pipeline config hand-off (JSON files)

targetScope = 'resourceGroup'

@description('Short environment identifier, e.g. dev / test / prod. Keep to 4 chars or fewer to stay within storage account name limits.')
param env string

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Prefix used when building resource base names.')
param basePrefix string = 'dt'

// Stable 6-character deterministic postfix derived from the resource group resource ID.
// This avoids naming collisions across deployments without needing a manual value.
var postfix = substring(uniqueString(resourceGroup().id), 0, 6)
var baseName = '${basePrefix}${env}${postfix}'
var containerRegistryName = 'acr${baseName}'

// Storage account name must be lowercase alphanumeric only, 3–24 chars.
// 'st' + basePrefix(2) + env(≤4) + postfix(6) = max 14 chars — well within limit.
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: 'st${baseName}'
  location: location
  tags: { environment: env }
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// Azure Container Registry used for application image builds and pulls.
resource containerRegistry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: containerRegistryName
  location: location
  tags: { environment: env }
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// --- Blob storage (pipeline config hand-off) ---

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-01-01' = {
  parent: storageAccount
  name: 'default'
}

// Container used to hand off JSON configuration files between pipeline stages
resource configContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  parent: blobService
  name: 'config'
  properties: {
    publicAccess: 'None'
  }
}

// VNet with two delegated subnets. Inline subnet definitions avoid needing separate subnet resources.
// snet-containers: /23 — minimum for Container Apps is /27; /23 gives headroom.
// snet-db:         /24 — minimum for PostgreSQL VNet injection is /28; /24 is clean and readable.
resource vnet 'Microsoft.Network/virtualNetworks@2023-11-01' = {
  name: '${baseName}-vnet'
  location: location
  tags: { environment: env }
  properties: {
    addressSpace: {
      addressPrefixes: ['10.0.0.0/22']
    }
    subnets: [
      {
        name: 'snet-containers'
        properties: {
          addressPrefix: '10.0.0.0/23'
          delegations: [
            {
              name: 'del-containerApps'
              properties: { serviceName: 'Microsoft.App/environments' }
            }
          ]
        }
      }
      {
        name: 'snet-db'
        properties: {
          addressPrefix: '10.0.2.0/24'
          delegations: [
            {
              name: 'del-postgres'
              properties: { serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers' }
            }
          ]
        }
      }
    ]
  }
}

// Plain-text connection string — demo only
var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'

output storageAccountName string = storageAccount.name
@secure()
output storageConnectionString string = storageConnectionString
output configContainerName string = configContainer.name
output postfix string = postfix
output baseName string = baseName
output vnetId string = vnet.id
// subnets[0] = snet-containers, subnets[1] = snet-db (order matches the inline array above)
output containerSubnetId string = vnet.properties.subnets[0].id
output dbSubnetId string = vnet.properties.subnets[1].id
output containerRegistryName string = containerRegistry.name
output containerRegistryServer string = containerRegistry.properties.loginServer
