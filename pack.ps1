<#
.SYNOPSIS
    Builds, tests, packs, and optionally installs or publishes the NetVerticalSlice.Templates package.

.PARAMETER Test
    Runs the integration test suite before packing.

.PARAMETER InstallLocal
    Installs the generated package locally into dotnet new for testing.

.PARAMETER Push
    Pushes the package to NuGet.org (requires ApiKey).

.PARAMETER ApiKey
    NuGet API Key for publishing.
#>
param(
    [switch]$Test,
    [switch]$InstallLocal,
    [switch]$Push,
    [string]$ApiKey
)

$ErrorActionPreference = "Stop"

Write-Host "==> Cleaning previous output..." -ForegroundColor Cyan
if (Test-Path "templates/bin/Release") {
    Remove-Item "templates/bin/Release/*.nupkg" -Force -ErrorAction SilentlyContinue
}

if ($Test) {
    Write-Host "==> Running template tests..." -ForegroundColor Cyan
    dotnet test templates/TemplateTests/TemplateTests.csproj -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Template tests failed."
    }
}

Write-Host "==> Packing NetVerticalSlice.Templates..." -ForegroundColor Cyan
dotnet pack templates/NetVerticalSlice.Templates.csproj -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet pack failed."
}

$nupkg = Get-ChildItem "templates/bin/Release/*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($null -eq $nupkg) {
    Write-Error "No package was produced."
}

Write-Host "==> Package created successfully: $($nupkg.FullName)" -ForegroundColor Green

if ($InstallLocal) {
    Write-Host "==> Installing package locally into dotnet template engine..." -ForegroundColor Cyan
    dotnet new install $nupkg.FullName --force
    Write-Host "==> Installed! List of VSA templates:" -ForegroundColor Green
    dotnet new list vsa
}

if ($Push) {
    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        Write-Error "ApiKey parameter is required when -Push is specified."
    }
    Write-Host "==> Publishing $($nupkg.Name) to NuGet.org..." -ForegroundColor Cyan
    dotnet nuget push $nupkg.FullName --api-key $ApiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
}
