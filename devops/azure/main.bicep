@description('Name of the existing Application Insights resource that receives Mova telemetry.')
param applicationInsightsName string

@description('Resource group of the existing Application Insights resource. Defaults to the deployment resource group.')
param applicationInsightsResourceGroup string = resourceGroup().name

@description('Name of the existing Azure App Service that hosts the Mova API.')
param appServiceName string

@description('Resource group of the existing App Service. Defaults to the deployment resource group.')
param appServiceResourceGroup string = resourceGroup().name

@description('Email address that receives alert notifications.')
param alertEmailAddress string

@description('Optional webhook URI that receives alert notifications (Slack, PagerDuty, etc.).')
param alertWebhookUri string = ''

@description('Server-error threshold: number of errors per minute before the alert fires. Must match the API ErrorRateHealthCheck:MaxErrorRatePerMinute threshold.')
param errorRateThreshold int = 5

@description('Number of minutes over which server errors are counted. Must match the API ErrorRateTracker:EvaluationWindow duration.')
param errorRateEvaluationWindowMinutes int = 5

@description('Azure region for resources. Defaults to the deployment resource group location.')
param location string = resourceGroup().location

@description('Optional resource tags.')
param tags object = {}

module monitoring 'monitoring.bicep' = {
  name: 'movaMonitoring'
  params: {
    applicationInsightsName: applicationInsightsName
    applicationInsightsResourceGroup: applicationInsightsResourceGroup
    appServiceName: appServiceName
    alertEmailAddress: alertEmailAddress
    alertWebhookUri: alertWebhookUri
    errorRateThreshold: errorRateThreshold
    errorRateEvaluationWindowMinutes: errorRateEvaluationWindowMinutes
    location: location
    tags: tags
  }
}

module appServiceHealthCheck 'app-service-health-check.bicep' = {
  name: 'movaHealthCheck'
  scope: resourceGroup(appServiceResourceGroup)
  params: {
    appServiceName: appServiceName
  }
}

output actionGroupId string = monitoring.outputs.actionGroupId
output serverErrorAlertId string = monitoring.outputs.serverErrorAlertId
output readinessAlertId string = monitoring.outputs.readinessAlertId
