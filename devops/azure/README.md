# Mova Azure monitoring and health-check configuration

This directory contains versioned Bicep templates that deploy the operational monitoring and alerting resources required by EPIC-10.

## Files

| File | Purpose |
|---|---|
| `main.bicep` | Orchestrates the monitoring and health-check modules. |
| `main.bicepparam.example` | Example parameter file. Copy to `main.bicepparam` and supply real values. |
| `monitoring.bicep` | Creates an Azure Monitor action group and two alert rules: server-error rate and readiness probe failures. |
| `app-service-health-check.bicep` | Configures the Azure App Service health-check probe to point at `/health/ready`. |

## What is deployed

- **Action group** (`<appServiceName>-mova-alerts`) with an email receiver and an optional webhook receiver.
- **Server-error rate alert** (`<appServiceName>-server-errors`) that fires when the number of 5xx responses in the configured window exceeds the per-minute threshold.
- **Readiness probe alert** (`<appServiceName>-readiness-failures`) that fires when `/health/ready` returns a non-2xx status code.
- **App Service health-check path** set to `/health/ready` so the platform can remove unhealthy instances from load balancing.

The server-error threshold and evaluation window default to 5 errors per minute over 5 minutes, matching the API's `ErrorRateHealthCheck:MaxErrorRatePerMinute` and `ErrorRateTracker:EvaluationWindow` defaults in `src/Mova.Api/appsettings.json`.

## Parameters

| Parameter | Default | Notes |
|---|---|---|
| `applicationInsightsName` | *required* | Name of the existing Application Insights resource linked to the Mova API. |
| `applicationInsightsResourceGroup` | deployment resource group | Resource group containing the Application Insights resource. |
| `appServiceName` | *required* | Name of the existing Azure App Service that hosts the Mova API. |
| `appServiceResourceGroup` | deployment resource group | Resource group containing the App Service. The health-check module is deployed to this resource group. |
| `alertEmailAddress` | *required* | Email address that receives alert notifications. |
| `alertWebhookUri` | `''` | Optional webhook URI (Slack, PagerDuty, etc.). |
| `errorRateThreshold` | `5` | Errors per minute that trigger the server-error alert. |
| `errorRateEvaluationWindowMinutes` | `5` | Length of the evaluation window in minutes. |
| `location` | deployment resource group location | Region for the alert-rule resources. |
| `tags` | `{}` | Optional tags applied to created resources. |

## Deployment

Copy the example parameter file and edit it for the target environment:

```powershell
cp devops/azure/main.bicepparam.example devops/azure/main.bicepparam
```

Deploy to an Azure resource group. The alert rule and action group are created in the target resource group; the App Service health-check configuration is applied to the App Service in its own resource group if different.

```powershell
$rg = 'mova-prod-rg'
az group create --name $rg --location eastus
az deployment group create `
  --resource-group $rg `
  --template-file devops/azure/main.bicep `
  --parameters devops/azure/main.bicepparam
```

To validate syntax without deploying:

```powershell
az bicep build --file devops/azure/main.bicep
```

## Alert rule details

### Server-error rate alert

The rule queries the Application Insights `requests` table:

```kusto
requests
| where resultCode >= 500
| where timestamp > ago(5m)
| summarize ErrorCount = count()
| extend RatePerMinute = ErrorCount / 5
| project RatePerMinute
```

It fires when `RatePerMinute` is greater than `errorRateThreshold` (default 5). This mirrors the internal `ErrorRateHealthCheck` calculation.

### Readiness alert

The rule queries the Application Insights `requests` table:

```kusto
requests
| where url contains "/health/ready" and resultCode >= 400
| summarize FailureCount = count()
```

It fires on any `/health/ready` failure in the last 5 minutes. This complements the App Service health-check probe and the load balancer readiness routing.

## Smoke testing

After deployment, run the smoke test from `devops/scripts/smoke-test.ps1` to verify that the health endpoints are reachable and return the expected schema. In a staging environment with a DEBUG build, you can use the `-StressErrorRate` switch to force 5xx responses and verify that `/health/ready` eventually reports `Unhealthy`.

See `devops/scripts/smoke-test.ps1` for usage.
