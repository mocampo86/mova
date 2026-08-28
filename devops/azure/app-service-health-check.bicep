@description('Name of the existing Azure App Service that hosts the Mova API. Must exist in the resource group targeted by the deployment.')
param appServiceName string

@description('Health check path used by the App Service load balancer to determine instance health.')
param healthCheckPath string = '/health/ready'

resource appService 'Microsoft.Web/sites@2022-03-01' existing = {
  name: appServiceName
}

resource siteConfig 'Microsoft.Web/sites/config@2022-03-01' = {
  parent: appService
  name: 'web'
  properties: {
    healthCheckPath: healthCheckPath
  }
}

output healthCheckPath string = healthCheckPath
