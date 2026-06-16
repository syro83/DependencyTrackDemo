// dth/infra/app/main.bicep
// App layer: deploys the DependencyTrackHelper API as an Azure Container App.
//
// The API is a lightweight .NET service that wraps the Dependency-Track REST API.
// It does not need a VNet because the DT backend already has external (public) ingress.
//
// Reads the DT backend URL from the dtBackendUrl parameter and injects it as
// DependencyTrack__BaseUrl, which overrides the appsettings.json value at runtime.

targetScope = 'resourceGroup'

@description('Short environment identifier, used for resource tagging.')
param env string

@description('Azure region for all resources. Defaults to the resource group location.')
param location string = resourceGroup().location

@description('Base name passed from the foundation layer output.')
param baseName string

@description('Postfix passed from the foundation layer output, used for tagging.')
param postfix string

@description('DT API server public URL from the app-outputs blob. Injected as DependencyTrack__BaseUrl.')
param dtBackendUrl string

@description('DT frontend public URL from the app-outputs blob. Stored as env var for reference.')
param dtFrontendUrl string

@description('Container registry server, e.g. myacr.azurecr.io')
param containerRegistryServer string

@description('Container registry username for image pull.')
param containerRegistryUsername string

@description('Container registry password for image pull.')
@secure()
param containerRegistryPassword string

@description('Container image name, e.g. dth-api')
param containerImageName string = 'dth-api'

@description('Container image tag.')
param containerImageTag string = 'latest'

var appName = '${baseName}-dth-api'
var tags = { environment: env, postfix: postfix }

// Consumption-plan Container Apps environment without VNet integration.
// The DT backend is publicly reachable, so no private network routing is needed.
resource containerAppEnv 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: '${baseName}-dth-env'
  location: location
  tags: tags
  properties: {}
}

resource dthApiApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
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
      registries: [
        {
          server: containerRegistryServer
          username: containerRegistryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: containerRegistryPassword
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'dth-api'
          image: '${containerRegistryServer}/${containerImageName}:${containerImageTag}'
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: [
            { name: 'ASPNETCORE_ENVIRONMENT', value: 'Prod' }
            // Override DependencyTrack.BaseUrl from appsettings.Prod.json with the live DT API URL
            { name: 'DependencyTrack__BaseUrl', value: dtBackendUrl }
            // Stored for reference; available to future features that need to link back to the UI
            { name: 'DependencyTrack__FrontendUrl', value: dtFrontendUrl }
          ]
        }
      ]
      scale: {
        minReplicas: 0 // Scale to zero when idle — demo cost saving
        maxReplicas: 1
      }
    }
  }
}

output dthApiUrl string = 'https://${dthApiApp.properties.configuration.ingress.fqdn}'
