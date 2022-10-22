$appSettingsPath = "CLI/appsettings.json"
$appSettingsLocalPath = "CLI/appsettings.local.json"
if (-Not(Test-Path $appSettingsLocalPath))
{
    Write-Output "No local settings to substitute: $appSettingsLocalPath"
    return
}

Write-Output "Copying local settings from: $appSettingsLocalPath tp $appSettingsPath"
$appSettingsJson = Get-Content -Raw $appSettingsPath | ConvertFrom-Json
$appSettingsLocalJson = Get-Content -Raw $appSettingsLocalPath | ConvertFrom-Json

# Copy values
$applicationInsightsConnectionString = $appSettingsLocalJson.ApplicationInsights.ConnectionString
if ($applicationInsightsConnectionString)
{
    $appSettingsJson.ApplicationInsights.ConnectionString = $applicationInsightsConnectionString
}

# Save updaed file
$appSettingsJson | ConvertTo-Json -depth 16 | Set-Content $appSettingsPath