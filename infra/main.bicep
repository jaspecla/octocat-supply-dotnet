targetScope = 'resourceGroup'

@description('Lowercase application name, 3-16 characters. Use letters, digits, and internal hyphens.')
@minLength(3)
@maxLength(16)
param appName string = 'octocat-supply'

@description('Azure region for all resources.')
param location string = resourceGroup().location

@description('Tag shared by the API and web images built in ACR.')
@minLength(1)
@maxLength(128)
param imageTag string

@description('Set false to provision the registry before building images, then redeploy with true.')
param deployApplication bool = true

var suffix = uniqueString(resourceGroup().id, appName)
var resourceName = '${appName}-${suffix}'
var tags = {
  application: appName
  purpose: 'disposable-demo'
}

resource registry 'Microsoft.ContainerRegistry/registries@2025-04-01' = {
  name: 'acr${suffix}'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    anonymousPullEnabled: false
    publicNetworkAccess: 'Enabled'
    policies: {
      azureADAuthenticationAsArmPolicy: {
        status: 'enabled'
      }
    }
  }
}

resource pullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2024-11-30' = {
  name: '${resourceName}-pull'
  location: location
  tags: tags
}

var acrPullRoleId = subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource acrPull 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, pullIdentity.id, acrPullRoleId)
  scope: registry
  properties: {
    roleDefinitionId: acrPullRoleId
    principalId: pullIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource logs 'Microsoft.OperationalInsights/workspaces@2025-02-01' = {
  name: '${resourceName}-logs'
  location: location
  tags: tags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

resource environment 'Microsoft.App/managedEnvironments@2025-01-01' = {
  name: '${resourceName}-env'
  location: location
  tags: tags
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logs.properties.customerId
        sharedKey: logs.listKeys().primarySharedKey
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

module application 'modules/container-app.bicep' = if (deployApplication) {
  name: '${appName}-application'
  params: {
    name: resourceName
    location: location
    tags: tags
    environmentId: environment.id
    registryServer: registry.properties.loginServer
    pullIdentityId: pullIdentity.id
    imageTag: imageTag
  }
  dependsOn: [
    acrPull
  ]
}

output registryName string = registry.name
output registryLoginServer string = registry.properties.loginServer
output containerAppName string = resourceName
output frontendUrl string = application.?outputs.frontendUrl ?? ''
