<# 
    In order to build a [local] publishable version of this CLI, 
    We must:
        1. copy all settings from appsettings.local.json to appsettings.json
        2. rebuild the solution in Release configuration
        
    The rebuild in Release configuration will automatically publish the package to the local machine (see CLI.csproj targets)
    Note. Since this script could be changing the contents of the appsettings.json file with values
    that we don't want committed to source control, we try to revert those changes after the build 
 #>
$PublishConfiguration = "Release"
$appSettingsPath = "CLI/appsettings.json"
$appSettingsLocalPath = "CLI/appsettings.local.json"
if (-Not(Test-Path $appSettingsLocalPath))
{
    Write-Output "PublishLocally: No local settings to substitute: $appSettingsLocalPath"
    return
}

# Detect already staged changes to appsettings.json
[string[]]$stagedAppSettingsFiles = git status --short $appSettingsPath
$appSettingsAlreadyStaged = ($stagedAppSettingsFiles.Count -gt 0)
if ($appSettingsAlreadyStaged)
{
    Write-Output "PublishLocally: Detected that '$appSettingsPath' already has staged changes"
}
else
{
    Write-Output "PublishLocally: Detected that '$appSettingsPath' has no existing staged changes"
}

# Read settings to copy
Write-Output "PublishLocally: Copying local settings from: $appSettingsLocalPath to $appSettingsPath"
$appSettingsJson = Get-Content -Raw $appSettingsPath | ConvertFrom-Json
$appSettingsLocalJson = Get-Content -Raw $appSettingsLocalPath | ConvertFrom-Json

# Copy values
$applicationInsightsConnectionString = $appSettingsLocalJson.ApplicationInsights.ConnectionString
if ($applicationInsightsConnectionString)
{
    $appSettingsJson.ApplicationInsights.ConnectionString = $applicationInsightsConnectionString
}

# Save updated file
$appSettingsJson | ConvertTo-Json -depth 16 | Set-Content $appSettingsPath


# Rebuild for release
& dotnet build --configuration $PublishConfiguration


# Revert staged changes to appsettings.json 
if (-not$appSettingsAlreadyStaged)
{
    [string[]]$stagedAppSettingsFiles = git status --short $appSettingsPath
    $appSettingsNowStaged = ($stagedAppSettingsFiles.Count -gt 0)
    if ($appSettingsNowStaged)
    {
        Write-Output "PublishLocally: Detected newly staged changes in '$appSettingsPath'"
        git reset HEAD $appSettingsPath
        git restore $appSettingsPath
        Write-Output "PublishLocally: Reverted staged changes in '$appSettingsPath'"
    }
    else
    {
        Write-Output "PublishLocally: no changes were made to '$appSettingsPath' to restore"
    }
}
else
{
    Write-Output "PublishLocally: '$appSettingsPath' was already changed before script ran. Leaving it staged"
}