param name string
param location string
param tags object
param environmentId string
param registryServer string
param pullIdentityId string
param imageTag string

resource app 'Microsoft.App/containerApps@2025-01-01' = {
  name: name
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${pullIdentityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    workloadProfileName: 'Consumption'
    configuration: {
      activeRevisionsMode: 'Single'
      registries: [
        {
          server: registryServer
          identity: pullIdentityId
        }
      ]
      ingress: {
        external: true
        allowInsecure: false
        targetPort: 8080
        transport: 'auto'
      }
    }
    template: {
      // One replica keeps the disposable SQLite database and Blazor circuits local.
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      containers: [
        {
          name: 'api'
          image: '${registryServer}/octocat-supply/api:${imageTag}'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8081'
            }
            {
              name: 'DbFile'
              value: '/app/data/app.db'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: [
            {
              type: 'Startup'
              tcpSocket: {
                port: 8081
              }
              periodSeconds: 5
              failureThreshold: 60
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/api/products'
                port: 8081
                scheme: 'HTTP'
              }
              periodSeconds: 10
              timeoutSeconds: 5
            }
          ]
        }
        {
          name: 'web'
          image: '${registryServer}/octocat-supply/web:${imageTag}'
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: '8080'
            }
            {
              name: 'ApiBaseUrl'
              value: 'http://localhost:8081/api/'
            }
          ]
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: [
            {
              type: 'Startup'
              tcpSocket: {
                port: 8080
              }
              periodSeconds: 5
              failureThreshold: 60
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/'
                port: 8080
                scheme: 'HTTP'
              }
              periodSeconds: 10
              timeoutSeconds: 5
            }
          ]
        }
      ]
    }
  }
}

output frontendUrl string = 'https://${app.properties.configuration.ingress.fqdn}'
