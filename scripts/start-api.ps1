$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

$env:ASPNETCORE_ENVIRONMENT = "Development"
Write-Host "Starting IncidentFlow API on http://localhost:5088 ..."
dotnet run --project src/IncidentFlow.API --launch-profile http
