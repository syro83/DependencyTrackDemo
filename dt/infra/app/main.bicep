// dt/infra/app/main.bicep
// App layer: deploys Dependency-Track API server and frontend as Azure Container Apps.
//
// Why Container Apps instead of App Service:
//   - Native Azure Files volume mounts (needed to persist /data across restarts)
//   - Scale-to-zero for demo cost savings
//   - HTTPS out of the box without extra ingress config
//
// Requires baseName + postfix + containerSubnetId from the foundation layer, dbUrl + credentials
// from the data layer, and the storage account name + file share name from the data layer.
// The storage account key is resolved internally via an 'existing' resource reference —
// it never needs to be passed as a pipeline parameter.
// The backend Container App runs inside snet-containers and reaches PostgreSQL over the private
// VNet connection — no database traffic crosses the public internet.

targetScope = 'resourceGroup'

@description('Short environment identifier, used for resource tagging.')
param env string

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Base name passed from the foundation layer output.')
param baseName string

@description('Postfix passed from the foundation layer output, used for tagging.')
param postfix string

@description('Clean JDBC URL without credentials, from the data layer dbUrl output. Format: jdbc:postgresql://host:5432/db?sslmode=require')
param dbUrl string

@description('PostgreSQL administrator username, from the data layer dbAdminUser output.')
param dbAdminUser string

@description('PostgreSQL administrator password. Same value used in the data pipeline.')
@secure()
param dbAdminPassword string

@description('Storage account name from the foundation layer output. Used to resolve the Azure Files key internally.')
param storageAccountName string

@description('Azure Files share name from the data layer vulnerabilityShareName output.')
param vulnerabilityShareName string

@description('Subnet resource ID for the Container Apps environment, from the foundation layer containerSubnetId output.')
param containerSubnetId string

@description('Backend container image repository, e.g. dependencytrack/apiserver')
param backendImage string = 'dependencytrack/apiserver'

@description('Backend container image tag.')
param backendImageTag string = 'latest'

@description('Frontend container image repository, e.g. dependencytrack/frontend')
param frontendImage string = 'dependencytrack/frontend'

@description('Frontend container image tag.')
param frontendImageTag string = 'latest'

var backendAppName = '${baseName}-api'
var frontendAppName = '${baseName}-ui'
var tags = { environment: env, postfix: postfix }

// Resolve the storage account key internally — avoids passing it as a pipeline secret
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// Consumption-plan Container Apps environment, integrated into snet-containers.
// internal: false keeps the environment publicly reachable so the frontend URL works without a private DNS workaround.
resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${baseName}-env'
  location: location
  tags: tags
  properties: {
    vnetConfiguration: {
      infrastructureSubnetId: containerSubnetId
      internal: false
    }
  }
}

// Register the Azure Files share with the Container Apps environment so containers can mount it
resource envStorage 'Microsoft.App/managedEnvironments/storages@2024-03-01' = {
  parent: containerAppEnv
  name: 'vulnerability-data'
  properties: {
    azureFile: {
      accountName: storageAccount.name
      accountKey: storageAccount.listKeys().keys[0].value
      shareName: vulnerabilityShareName
      accessMode: 'ReadWrite'
    }
  }
}

// Backend API server — external ingress so the frontend (and operators) can reach it directly.
// For a production setup, switch to internal ingress and let the frontend proxy calls via nginx.
resource backendApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: backendAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
      }
    }
    template: {
      containers: [
        {
          name: 'apiserver'
          image: '${backendImage}:${backendImageTag}'
          resources: {
            // DT API server runs on the JVM; 2 Gi is the practical minimum
            cpu: json('1.0')
            memory: '2Gi'
          }
          env: [
            // Tell DT to use an external PostgreSQL database instead of the embedded H2
            { name: 'ALPINE_DATABASE_MODE', value: 'external' }
            { name: 'ALPINE_DATABASE_DRIVER', value: 'org.postgresql.Driver' }
            // Clean JDBC URL — credentials are supplied via separate env vars below
            { name: 'ALPINE_DATABASE_URL', value: dbUrl }
            { name: 'ALPINE_DATABASE_USERNAME', value: dbAdminUser }
            { name: 'ALPINE_DATABASE_PASSWORD', value: dbAdminPassword }
            // Connection pool — reduced from production defaults for demo cost
            { name: 'ALPINE_DATABASE_POOL_ENABLED', value: 'true' }
            { name: 'ALPINE_DATABASE_POOL_MAX_SIZE', value: '10' }
            { name: 'ALPINE_DATABASE_POOL_MIN_IDLE', value: '2' }
            // Worker threads — reduced from production defaults for demo cost
            { name: 'ALPINE_WORKER_THREADS', value: '2' }
            { name: 'ALPINE_WORKER_THREAD_MULTIPLIER', value: '4' }
            // Data directory backed by the Azure Files volume mounted below
            { name: 'ALPINE_DATA_DIRECTORY', value: '/data' }
          ]
          volumeMounts: [
            {
              volumeName: 'vulnerability-data'
              mountPath: '/data'
            }
          ]
        }
      ]
      volumes: [
        {
          name: 'vulnerability-data'
          storageType: 'AzureFile'
          // Must match the name of the managedEnvironments/storages resource above
          storageName: 'vulnerability-data'
        }
      ]
      scale: {
        minReplicas: 1 // Keep at least 1 so vulnerability mirrors are always running
        maxReplicas: 1 // Single replica is fine for demo
      }
    }
  }
  dependsOn: [envStorage]
}

resource frontendApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: frontendAppName
  location: location
  tags: tags
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
      }
    }
    template: {
      containers: [
        {
          name: 'frontend'
          image: '${frontendImage}:${frontendImageTag}'
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            // nginx in the DT frontend container proxies API calls to this URL
            { name: 'API_BASE_URL', value: 'https://${backendApp.properties.configuration.ingress.fqdn}' }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
    }
  }
}

output backendUrl string = 'https://${backendApp.properties.configuration.ingress.fqdn}'
output frontendUrl string = 'https://${frontendApp.properties.configuration.ingress.fqdn}'
