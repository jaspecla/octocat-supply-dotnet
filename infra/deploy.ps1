#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrEmpty()]
    [string] $ResourceGroup,

    [ValidateNotNullOrEmpty()]
    [string] $Location = 'eastus2',

    [ValidatePattern('^[a-z][a-z0-9-]{1,14}[a-z0-9]$')]
    [string] $AppName = 'octocat-supply',

    [ValidatePattern('^[A-Za-z0-9_][A-Za-z0-9_.-]{0,127}$')]
    [string] $ImageTag = "demo-$([DateTime]::UtcNow.ToString('yyyyMMddHHmmss'))-$([guid]::NewGuid().ToString('N').Substring(0, 6))"
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Invoke-AzureCli {
    param([string[]] $Arguments)

    $result = & az @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command '$($Arguments[0]) $($Arguments[1])' failed with exit code $LASTEXITCODE."
    }
    return $result
}

Get-Command az -ErrorAction Stop | Out-Null
$account = Invoke-AzureCli -Arguments @('account', 'show', '--output', 'json', '--only-show-errors') | ConvertFrom-Json
Write-Host "Deploying to subscription '$($account.name)' ($($account.id))."
Write-Warning 'This deploys an intentionally vulnerable demo to public HTTPS. Do not use real data. SQLite data and browser sessions are disposable.'

$repositoryRoot = Split-Path -Parent $PSScriptRoot
$template = Join-Path $PSScriptRoot 'main.bicep'
$apiDockerfile = Join-Path $repositoryRoot 'src\OctocatSupply.Api\Dockerfile'
$webDockerfile = Join-Path $repositoryRoot 'src\OctocatSupply.Web\Dockerfile'

Invoke-AzureCli -Arguments @(
    'bicep', 'build', '--file', $template, '--stdout', '--only-show-errors'
) | Out-Null

$groupExists = Invoke-AzureCli -Arguments @(
    'group', 'exists', '--name', $ResourceGroup, '--output', 'tsv', '--only-show-errors'
)
if ($groupExists.ToString().Trim() -eq 'true') {
    $group = Invoke-AzureCli -Arguments @(
        'group', 'show', '--name', $ResourceGroup, '--output', 'json', '--only-show-errors'
    ) | ConvertFrom-Json
    if ($group.location -ne $Location) {
        Write-Host "Keeping resource group in '$($group.location)'; deploying resources in '$Location'."
    }
}
else {
    $group = Invoke-AzureCli -Arguments @(
        'group', 'create', '--name', $ResourceGroup, '--location', $Location,
        '--output', 'json', '--only-show-errors'
    ) | ConvertFrom-Json
}

$deploymentArguments = @(
    'deployment', 'group', 'create', '--resource-group', $ResourceGroup,
    '--template-file', $template, '--output', 'json', '--only-show-errors',
    '--parameters', "appName=$AppName", "location=$Location", "imageTag=$ImageTag"
)

Write-Host 'Provisioning registry, managed identity, Application Insights, logging, and Container Apps environment...'
$infrastructure = Invoke-AzureCli -Arguments ($deploymentArguments + @(
    'deployApplication=false', '--name', "$AppName-infrastructure"
)) | ConvertFrom-Json
$registryName = $infrastructure.properties.outputs.registryName.value
$applicationInsightsName = $infrastructure.properties.outputs.applicationInsightsName.value

Write-Host "Building API and web images in ACR '$registryName' with tag '$ImageTag'..."
Invoke-AzureCli -Arguments @(
    'acr', 'build', '--registry', $registryName,
    '--image', "octocat-supply/api:$ImageTag", '--file', $apiDockerfile,
    '--platform', 'linux/amd64', $repositoryRoot, '--only-show-errors'
) | Out-Host
Invoke-AzureCli -Arguments @(
    'acr', 'build', '--registry', $registryName,
    '--image', "octocat-supply/web:$ImageTag", '--file', $webDockerfile,
    '--platform', 'linux/amd64', $repositoryRoot, '--only-show-errors'
) | Out-Host

Write-Host 'Deploying the application...'
$deployment = Invoke-AzureCli -Arguments ($deploymentArguments + @(
    'deployApplication=true', '--name', "$AppName-app-deployment"
)) | ConvertFrom-Json
$frontendUrl = $deployment.properties.outputs.frontendUrl.value
$containerAppName = $deployment.properties.outputs.containerAppName.value

Write-Host "Checking $frontendUrl ..."
$response = Invoke-WebRequest -Uri $frontendUrl -TimeoutSec 30 -MaximumRetryCount 12 -RetryIntervalSec 10
if ($response.StatusCode -ne 200) {
    throw "Frontend verification failed: HTTP $($response.StatusCode)."
}

[pscustomobject]@{
    FrontendUrl = $frontendUrl
    ContainerAppName = $containerAppName
    RegistryName = $registryName
    ApplicationInsightsName = $applicationInsightsName
    ImageTag = $ImageTag
    ResourceGroupUrl = "https://portal.azure.com/#resource$($group.id)"
}
