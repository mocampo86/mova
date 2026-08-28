<#
.SYNOPSIS
    Post-deployment smoke test for the Mova API health and monitoring endpoints.

.DESCRIPTION
    Validates that the API health endpoints are reachable, return the expected
    JSON schema, and report healthy dependencies. In staging environments that
    run a DEBUG build, the -StressErrorRate switch can force 5xx responses and
    verify that the readiness check eventually turns Unhealthy.

.PARAMETER ApiBaseUrl
    Base URL of the Mova API (e.g. https://mova-api-prod.azurewebsites.net).

.PARAMETER StressErrorRate
    When set, repeatedly calls the DEBUG-only /api/test/throw endpoint and then
    polls /health/ready until it reports Unhealthy or the timeout expires.

.PARAMETER StressRequests
    Number of requests to send to the test exception endpoint. The default is
    one more than the ErrorRateHealthCheck threshold (5 errors per minute).

.PARAMETER ReadinessTimeoutSeconds
    Maximum time to wait for the readiness check to become Unhealthy during a
    stress test.

.EXAMPLE
    .\smoke-test.ps1 -ApiBaseUrl https://mova-api-staging.azurewebsites.net

.EXAMPLE
    .\smoke-test.ps1 -ApiBaseUrl http://localhost:5098 -StressErrorRate
#>
[CmdletBinding()]
param (
    [Parameter()]
    [string] $ApiBaseUrl = 'http://localhost:5098',

    [Parameter()]
    [switch] $StressErrorRate,

    [Parameter()]
    [int] $StressRequests = 26,

    [Parameter()]
    [int] $ReadinessTimeoutSeconds = 30
)

$ErrorActionPreference = 'Stop'

function Invoke-MovaEndpoint {
    param (
        [string] $Path,
        [string] $Method = 'GET',
        [switch] $SuppressWarning
    )

    $uri = "$ApiBaseUrl$Path"
    if (-not $SuppressWarning) {
        Write-Host "$Method $uri"
    }

    try {
        $response = Invoke-WebRequest -Uri $uri -Method $Method -UseBasicParsing -ContentType 'application/json'
        return @{ Status = $response.StatusCode; Body = ($response.Content | ConvertFrom-Json); Error = $null }
    } catch {
        $err = $_
        if ($err.Exception -is [System.Net.WebException] -and $err.Exception.Response) {
            $status = [int]$err.Exception.Response.StatusCode
            $stream = $err.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $body = $reader.ReadToEnd()
            $reader.Close()
            try { $json = $body | ConvertFrom-Json } catch { $json = $body }
            return @{ Status = $status; Body = $json; Error = $err }
        }
        return @{ Status = 0; Body = $null; Error = $err }
    }
}

$failures = 0

# 1. Liveness probe
Write-Host "`n=== Liveness probe ($ApiBaseUrl/health/live) ==="
try {
    $live = Invoke-MovaEndpoint -Path '/health/live'
    if ($live.Status -ne 200) {
        Write-Error "Liveness returned HTTP $($live.Status); expected 200." -ErrorAction Continue
        $failures++
    } elseif ($live.Body.status -ne 'Healthy') {
        Write-Error "Liveness status is $($live.Body.status); expected Healthy." -ErrorAction Continue
        $failures++
    } else {
        Write-Host "PASS: liveness is Healthy (HTTP $($live.Status))."
    }
} catch {
    Write-Error "Liveness probe failed: $_" -ErrorAction Continue
    $failures++
}

# 2. Readiness probe
Write-Host "`n=== Readiness probe ($ApiBaseUrl/health/ready) ==="
try {
    $ready = Invoke-MovaEndpoint -Path '/health/ready'
    if ($ready.Status -ne 200) {
        Write-Error "Readiness returned HTTP $($ready.Status); expected 200." -ErrorAction Continue
        $failures++
    } elseif ($ready.Body.status -ne 'Healthy') {
        Write-Error "Readiness status is $($ready.Body.status); expected Healthy." -ErrorAction Continue
        $failures++
    } else {
        $deps = $ready.Body.dependencies | ForEach-Object { $_.name }
        if ($deps -notcontains 'database' -or $deps -notcontains 'error-rate') {
            Write-Error "Readiness dependencies are incomplete. Found: $deps" -ErrorAction Continue
            $failures++
        } else {
            Write-Host "PASS: readiness is Healthy and includes database and error-rate dependencies."
        }
    }
} catch {
    Write-Error "Readiness probe failed: $_" -ErrorAction Continue
    $failures++
}

# 3. Optional stress test for error-rate readiness behavior
if ($StressErrorRate) {
    Write-Host "`n=== Stress test: error-rate readiness ==="

    $testThrowUri = "$ApiBaseUrl/api/test/throw"
    $throwSupported = $false

    try {
        $firstThrow = Invoke-MovaEndpoint -Path '/api/test/throw' -SuppressWarning
        if ($firstThrow.Status -eq 500) {
            $throwSupported = $true
            Write-Host "DEBUG test exception endpoint is available."
        } elseif ($firstThrow.Status -eq 200) {
            Write-Warning "DEBUG test exception endpoint returned 200; stress test cannot be completed."
        } else {
            Write-Warning "Test exception endpoint returned HTTP $($firstThrow.Status); stress test skipped."
        }
    } catch {
        Write-Warning "Test exception endpoint is not reachable or not supported in this build: $_"
    }

    if ($throwSupported) {
        Write-Host "Sending $StressRequests 5xx requests to $testThrowUri ..."
        for ($i = 0; $i -lt $StressRequests; $i++) {
            $null = Invoke-MovaEndpoint -Path '/api/test/throw' -SuppressWarning
        }

        Write-Host "Polling /health/ready for up to $ReadinessTimeoutSeconds seconds ..."
        $start = Get-Date
        $becameUnhealthy = $false
        while (((Get-Date) - $start).TotalSeconds -lt $ReadinessTimeoutSeconds) {
            $ready = Invoke-MovaEndpoint -Path '/health/ready' -SuppressWarning
            if ($ready.Status -ne 200 -and $ready.Body.status -eq 'Unhealthy') {
                $becameUnhealthy = $true
                $elapsed = ((Get-Date) - $start).TotalSeconds.ToString('F1')
                Write-Host "PASS: readiness turned Unhealthy after $elapsed seconds."
                break
            }
            Start-Sleep -Seconds 1
        }

        if (-not $becameUnhealthy) {
            Write-Error "Readiness did not turn Unhealthy within $ReadinessTimeoutSeconds seconds." -ErrorAction Continue
            $failures++
        }
    } else {
        Write-Warning "Stress test skipped. Build a DEBUG deployment or use the integration test suite for error-rate behavior validation."
    }
}

Write-Host "`n=== Smoke test complete ==="
if ($failures -eq 0) {
    Write-Host "PASS: all checks passed."
    exit 0
} else {
    Write-Error "FAIL: $failures check(s) failed."
    exit 1
}
