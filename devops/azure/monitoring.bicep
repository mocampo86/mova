@description('Name of the existing Application Insights resource that receives Mova telemetry.')
param applicationInsightsName string

@description('Resource group of the existing Application Insights resource. Defaults to the deployment resource group.')
param applicationInsightsResourceGroup string = resourceGroup().name

@description('Name of the existing Azure App Service that hosts the Mova API. Used for naming the alert resources.')
param appServiceName string

@description('Email address that receives alert notifications.')
param alertEmailAddress string

@description('Optional webhook URI that receives alert notifications (Slack, PagerDuty, etc.).')
param alertWebhookUri string = ''

@description('Server-error threshold: number of errors per minute before the alert fires. Must match the API ErrorRateHealthCheck:MaxErrorRatePerMinute threshold.')
param errorRateThreshold int = 5

@description('Number of minutes over which server errors are counted. Must match the API ErrorRateTracker:EvaluationWindow duration.')
param errorRateEvaluationWindowMinutes int = 5

@description('Azure region for the alert-rule resources. Defaults to the deployment resource group location.')
param location string = resourceGroup().location

@description('Optional resource tags.')
param tags object = {}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: applicationInsightsName
  scope: resourceGroup(applicationInsightsResourceGroup)
}

resource actionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: '${appServiceName}-mova-alerts'
  location: 'Global'
  tags: tags
  properties: {
    groupShortName: 'MovaAlerts'
    enabled: true
    emailReceivers: [
      {
        name: 'OperatorEmail'
        emailAddress: alertEmailAddress
        useCommonAlertSchema: true
      }
    ]
    webhookReceivers: empty(alertWebhookUri) ? [] : [
      {
        name: 'OperatorWebhook'
        serviceUri: alertWebhookUri
        useCommonAlertSchema: true
      }
    ]
  }
}

resource serverErrorAlert 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: '${appServiceName}-server-errors'
  location: location
  tags: tags
  properties: {
    displayName: 'Mova server error rate'
    description: 'Fires when the API returns 5xx responses at a rate of ${errorRateThreshold} or more per minute over the last ${errorRateEvaluationWindowMinutes} minutes.'
    severity: 1
    enabled: true
    evaluationFrequency: 'PT1M'
    scopes: [
      applicationInsights.id
    ]
    windowSize: 'PT${errorRateEvaluationWindowMinutes}M'
    targetResourceTypes: [
      'Microsoft.Insights/components'
    ]
    criteria: {
      allOf: [
        {
          query: '''requests
| where resultCode >= 500
| where timestamp > ago(${errorRateEvaluationWindowMinutes}m)
| summarize ErrorCount = count()
| extend RatePerMinute = ErrorCount / ${errorRateEvaluationWindowMinutes}
| project RatePerMinute'''
          timeAggregation: 'Average'
          metricMeasureColumn: 'RatePerMinute'
          operator: 'GreaterThan'
          threshold: errorRateThreshold
          dimensions: []
          resourceIdColumn: '_ResourceId'
        }
      ]
    }
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
    autoMitigate: true
    muteActionsDuration: null
  }
}

resource readinessAlert 'Microsoft.Insights/scheduledQueryRules@2023-12-01' = {
  name: '${appServiceName}-readiness-failures'
  location: location
  tags: tags
  properties: {
    displayName: 'Mova readiness probe failures'
    description: 'Fires when /health/ready returns a non-2xx response, indicating the API is not ready to serve traffic.'
    severity: 1
    enabled: true
    evaluationFrequency: 'PT1M'
    scopes: [
      applicationInsights.id
    ]
    windowSize: 'PT5M'
    targetResourceTypes: [
      'Microsoft.Insights/components'
    ]
    criteria: {
      allOf: [
        {
          query: '''requests
| where url contains "/health/ready" and resultCode >= 400
| summarize FailureCount = count()'''
          timeAggregation: 'Total'
          metricMeasureColumn: 'FailureCount'
          operator: 'GreaterThan'
          threshold: 0
          dimensions: []
          resourceIdColumn: '_ResourceId'
        }
      ]
    }
    actions: {
      actionGroups: [
        actionGroup.id
      ]
    }
    autoMitigate: true
    muteActionsDuration: null
  }
}

output actionGroupId string = actionGroup.id
output serverErrorAlertId string = serverErrorAlert.id
output readinessAlertId string = readinessAlert.id
