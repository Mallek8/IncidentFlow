$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location (Join-Path $root "frontend")

if (-not (Test-Path "node_modules")) {
  npm install
}

Write-Host "Starting IncidentFlow frontend on http://localhost:5173 ..."
npm run dev
