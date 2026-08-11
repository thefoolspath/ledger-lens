[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('help', '--help', '-h', 'bootstrap', 'doctor', 'restore', 'build', 'test', 'run', 'dotnet', 'aspire', 'node', 'npm', 'ng')]
    [string] $Command = 'help',

    [Parameter(Position = 1, ValueFromRemainingArguments)]
    [string[]] $Arguments
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$manifest = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'toolchain.json') | ConvertFrom-Json
$dotnetExe = Join-Path $repoRoot '.dotnet\dotnet.exe'
$aspireExe = Join-Path $repoRoot '.a\aspire.cmd'
$nodeExe = Join-Path $repoRoot '.tools\node\node.exe'
$npmCmd = Join-Path $repoRoot '.tools\node\npm.cmd'
$webRoot = Join-Path $repoRoot 'web\ledgerlens-web'
$env:PATH = "$(Split-Path -Parent $nodeExe);$env:PATH"

function Assert-Tool {
    param([string] $Path, [string] $Name)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Name is not restored. Run .\scripts\bootstrap.ps1 from the LedgerLens directory."
    }
}

function Invoke-LocalTool {
    param([string] $Path, [string[]] $ToolArguments)
    & $Path @ToolArguments
    exit $LASTEXITCODE
}

if ($Command -in @('help', '--help', '-h')) {
    @'
TheFools - LedgerLens development CLI

Usage:
  thefools <command> [arguments]

Commands:
  bootstrap   Restore the pinned project-local toolchain
  doctor      Verify local tools and Docker
  restore     Restore .NET and Angular dependencies
  build       Build the .NET solution and Angular application
  test        Run .NET and Angular tests
  run         Start the complete Aspire application graph
  dotnet      Run the project-local .NET CLI
  aspire      Run the project-local Aspire CLI
  node        Run the project-local Node.js executable
  npm         Run the project-local npm CLI
  ng          Run the project-local Angular CLI
  help        Show this help

Examples:
  thefools run
  thefools test
  thefools dotnet --version
'@ | Write-Host
    exit 0
}

if ($Command -eq 'bootstrap') {
    & (Join-Path $PSScriptRoot 'bootstrap.ps1') @Arguments
    exit $LASTEXITCODE
}

if ($Command -eq 'doctor') {
    Assert-Tool $dotnetExe '.NET SDK'
    Assert-Tool $aspireExe 'Aspire CLI'
    Assert-Tool $nodeExe 'Node.js'
    Assert-Tool $npmCmd 'npm'

    $failures = [System.Collections.Generic.List[string]]::new()
    $dotnetVersion = & $dotnetExe --version
    $aspireVersion = (& $aspireExe --version 2>&1 | Out-String).Trim()
    $nodeVersion = & $nodeExe --version
    $npmVersion = & $npmCmd --version
    $dotnetSdkSentinel = Join-Path $repoRoot ".dotnet\sdk\$($manifest.dotnet.version)\Sdks\Microsoft.NET.Sdk\codestyle\cs\build\Microsoft.CodeAnalysis.CSharp.CodeStyle.targets"

    if ($dotnetVersion -ne [string]$manifest.dotnet.version) { $failures.Add(".NET expected $($manifest.dotnet.version), found $dotnetVersion") }
    if ($aspireVersion -notmatch [regex]::Escape([string]$manifest.aspire.version)) { $failures.Add("Aspire expected $($manifest.aspire.version), found $aspireVersion") }
    if ($nodeVersion -ne "v$($manifest.node.version)") { $failures.Add("Node expected $($manifest.node.version), found $nodeVersion") }
    if ($npmVersion -ne [string]$manifest.node.npmVersion) { $failures.Add("npm expected $($manifest.node.npmVersion), found $npmVersion") }
    if (-not (Test-Path -LiteralPath $dotnetSdkSentinel)) { $failures.Add(".NET SDK is incomplete; missing $dotnetSdkSentinel") }

    docker version | Out-Host
    if ($LASTEXITCODE -ne 0) { $failures.Add('Docker client/server check failed') }
    docker compose version | Out-Host
    if ($LASTEXITCODE -ne 0) { $failures.Add('Docker Compose check failed') }

    Write-Host ''
    Write-Host "Local .NET:   $dotnetVersion"
    Write-Host "Local Aspire: $aspireVersion"
    Write-Host "Local Node:   $nodeVersion"
    Write-Host "Local npm:    $npmVersion"

    if ($failures.Count -gt 0) {
        $failures | ForEach-Object { Write-Host "ERROR: $_" -ForegroundColor Red }
        exit 1
    }

    Write-Host 'LedgerLens tooling doctor passed.'
    exit 0
}

switch ($Command) {
    'restore' {
        Assert-Tool $dotnetExe '.NET SDK'
        Assert-Tool $npmCmd 'npm'
        & $dotnetExe restore (Join-Path $repoRoot 'LedgerLens.slnx')
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        & $npmCmd ci --prefix $webRoot
        exit $LASTEXITCODE
    }
    'build' {
        Assert-Tool $dotnetExe '.NET SDK'
        Assert-Tool $npmCmd 'npm'
        & $dotnetExe build (Join-Path $repoRoot 'LedgerLens.slnx') --no-restore
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        & $npmCmd run build --prefix $webRoot
        exit $LASTEXITCODE
    }
    'test' {
        Assert-Tool $dotnetExe '.NET SDK'
        Assert-Tool $npmCmd 'npm'
        & $dotnetExe test (Join-Path $repoRoot 'LedgerLens.slnx') --no-restore
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        & $npmCmd test --prefix $webRoot -- --watch=false
        exit $LASTEXITCODE
    }
    'run' {
        Assert-Tool $dotnetExe '.NET SDK'
        & $dotnetExe run --project (Join-Path $repoRoot 'src\LedgerLens.AppHost\LedgerLens.AppHost.csproj') @Arguments
        exit $LASTEXITCODE
    }
    'dotnet' {
        Assert-Tool $dotnetExe '.NET SDK'
        Invoke-LocalTool $dotnetExe $Arguments
    }
    'aspire' {
        Assert-Tool $aspireExe 'Aspire CLI'
        Invoke-LocalTool $aspireExe $Arguments
    }
    'node' {
        Assert-Tool $nodeExe 'Node.js'
        Invoke-LocalTool $nodeExe $Arguments
    }
    'npm' {
        Assert-Tool $npmCmd 'npm'
        Invoke-LocalTool $npmCmd $Arguments
    }
    'ng' {
        Assert-Tool $nodeExe 'Node.js'
        $ngScript = Join-Path $webRoot 'node_modules\@angular\cli\bin\ng.js'
        Assert-Tool $ngScript 'Angular CLI'
        $ngArguments = @($ngScript) + $Arguments
        Invoke-LocalTool $nodeExe $ngArguments
    }
}
