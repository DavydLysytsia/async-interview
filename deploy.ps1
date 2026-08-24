# One-shot Azure deployment for Async Interview Profile.
# Creates the web app (first run only), applies settings from server/AsyncInterview.Api/.env,
# publishes the server (with the built SPA in wwwroot), and zip-deploys it.
# Requirements: az CLI logged in, .NET SDK, client already built into wwwroot.

$ErrorActionPreference = "Stop"

$Rg   = "bvc-group2-backend-rg"   # existing resource group (hosts the free F1 plan)
$Plan = "bvc-group2-plan"         # existing F1 Linux plan — F1 quota allows no second plan
$App  = "async-interview-davyd"
$ProjDir = Join-Path $PSScriptRoot "server\AsyncInterview.Api"

# 1. Create the web app on the existing plan (skipped if it already exists)
$exists = az webapp show -g $Rg -n $App --query name -o tsv 2>$null
if (-not $exists) {
    Write-Host "Creating web app $App..."
    az webapp create -g $Rg -p $Plan -n $App --runtime "DOTNETCORE:10.0" -o none
} else {
    Write-Host "Web app $App already exists - updating it."
}
$AppHost = az webapp show -g $Rg -n $App --query defaultHostName -o tsv
Write-Host "Host: https://$AppHost"

# 2. App settings from local .env (values never printed)
$envFile = Join-Path $ProjDir ".env"
$envMap = @{}
foreach ($line in Get-Content $envFile) {
    if ($line -match "^\s*([A-Z_]+)\s*=\s*(.*)$") { $envMap[$Matches[1]] = $Matches[2].Trim() }
}
if (-not $envMap["GOOGLE_CLIENT_ID"] -or -not $envMap["GOOGLE_CLIENT_SECRET"]) {
    throw "GOOGLE_CLIENT_ID / GOOGLE_CLIENT_SECRET missing from $envFile"
}
Write-Host "Applying app settings..."
az webapp config appsettings set -g $Rg -n $App -o none --settings `
    ("GOOGLE_CLIENT_ID=" + $envMap["GOOGLE_CLIENT_ID"]) `
    ("GOOGLE_CLIENT_SECRET=" + $envMap["GOOGLE_CLIENT_SECRET"]) `
    ("APP_BASE_URL=https://" + $AppHost) `
    "DEV_FAKE_AUTH=false" `
    "DB_PATH=/home/data/app.db"

# 3. Publish and zip
Write-Host "Publishing..."
Push-Location $ProjDir
dotnet publish -c Release -o publish --nologo -v quiet
if (-not (Test-Path "publish\wwwroot\index.html")) {
    Pop-Location
    throw "SPA missing from publish\wwwroot - run 'npm run build' in client/ first."
}
Compress-Archive -Path "publish\*" -DestinationPath "app.zip" -Force
Pop-Location

# 4. Deploy
Write-Host "Deploying zip (F1 cold deploys can take a few minutes)..."
az webapp deploy -g $Rg -n $App --src-path (Join-Path $ProjDir "app.zip") --type zip -o none

# 5. Health check
Write-Host "Waiting for the app to come up..."
$healthy = $false
for ($i = 0; $i -lt 40; $i++) {
    try {
        $r = Invoke-RestMethod "https://$AppHost/api/health" -TimeoutSec 5
        if ($r.ok) { $healthy = $true; break }
    } catch { Start-Sleep -Seconds 6 }
}
if ($healthy) {
    Write-Host ""
    Write-Host "DEPLOYED AND HEALTHY: https://$AppHost"
    Write-Host "Remaining manual step: add the two redirect URIs for this host in Google Cloud."
} else {
    Write-Host "Deployed, but /api/health did not respond yet. Check logs:"
    Write-Host "  az webapp log tail -g $Rg -n $App"
}
