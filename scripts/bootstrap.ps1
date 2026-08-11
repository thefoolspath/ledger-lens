[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$manifestPath = Join-Path $repoRoot 'toolchain.json'
$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$cacheDir = Join-Path $repoRoot '.cache\downloads'
$dotnetDir = Join-Path $repoRoot '.dotnet'
$toolsDir = Join-Path $repoRoot '.tools'
$aspireDir = Join-Path $repoRoot ([string]$manifest.aspire.installDirectory)
$nodeDir = Join-Path $toolsDir 'node'

$env:DOTNET_CLI_HOME = Join-Path $repoRoot '.cache\dotnet-home'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'

New-Item -ItemType Directory -Force -Path $cacheDir, $toolsDir | Out-Null

function Get-VerifiedDownload {
    param(
        [Parameter(Mandatory)] [string] $Uri,
        [Parameter(Mandatory)] [string] $Destination,
        [Parameter(Mandatory)] [string] $Sha256
    )

    if (Test-Path -LiteralPath $Destination) {
        $existingHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $Destination).Hash
        if ($existingHash -ieq $Sha256) {
            return
        }

        Remove-Item -Force -LiteralPath $Destination
    }

    Write-Host "Downloading $Uri"
    Invoke-WebRequest -UseBasicParsing -Uri $Uri -OutFile $Destination
    $downloadedHash = (Get-FileHash -Algorithm SHA256 -LiteralPath $Destination).Hash
    if ($downloadedHash -ine $Sha256) {
        Remove-Item -Force -LiteralPath $Destination
        throw "Checksum mismatch for $Uri. The downloaded file was removed."
    }
}

function Reset-Directory {
    param([Parameter(Mandatory)] [string] $Path)

    $resolvedRoot = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd('\')
    $resolvedTarget = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not $resolvedTarget.StartsWith($resolvedRoot + '\', [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to modify a directory outside the LedgerLens repository: $resolvedTarget"
    }

    $longTarget = '\\?\' + $resolvedTarget
    if ([System.IO.Directory]::Exists($longTarget)) {
        [System.IO.Directory]::Delete($longTarget, $true)
    }
    New-Item -ItemType Directory -Force -Path $resolvedTarget | Out-Null
}

function Remove-SafeDirectory {
    param([Parameter(Mandatory)] [string] $Path)

    $resolvedRoot = [System.IO.Path]::GetFullPath($repoRoot).TrimEnd('\')
    $resolvedTarget = [System.IO.Path]::GetFullPath($Path).TrimEnd('\')
    if (-not $resolvedTarget.StartsWith($resolvedRoot + '\', [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove a directory outside the LedgerLens repository: $resolvedTarget"
    }

    $longTarget = '\\?\' + $resolvedTarget
    if ([System.IO.Directory]::Exists($longTarget)) {
        [System.IO.Directory]::Delete($longTarget, $true)
    }
}

$dotnetExe = Join-Path $dotnetDir 'dotnet.exe'
$dotnetSdkSentinel = Join-Path $dotnetDir "sdk\$($manifest.dotnet.version)\Sdks\Microsoft.NET.Sdk\codestyle\cs\build\Microsoft.CodeAnalysis.CSharp.CodeStyle.targets"
$dotnetReady = $false
if (Test-Path -LiteralPath $dotnetExe) {
    $dotnetReady = ((& $dotnetExe --version) -eq [string]$manifest.dotnet.version) -and (Test-Path -LiteralPath $dotnetSdkSentinel)
}

if (-not $dotnetReady) {
    $installScript = Join-Path $cacheDir 'dotnet-install.ps1'
    Get-VerifiedDownload -Uri $manifest.dotnet.installScriptUrl -Destination $installScript -Sha256 $manifest.dotnet.installScriptSha256
    Reset-Directory -Path $dotnetDir
    Write-Host "Installing .NET SDK $($manifest.dotnet.version) into $dotnetDir"
    $shortDrive = $null
    foreach ($letter in [char[]]'ZYXWVUT') {
        $candidate = "${letter}:"
        if (-not (Test-Path "$candidate\")) {
            $shortDrive = $candidate
            break
        }
    }
    if (-not $shortDrive) {
        throw '.NET restore needs one unused temporary drive letter between T: and Z: to avoid the Windows path-length limit.'
    }

    try {
        & subst.exe $shortDrive $repoRoot
        if ($LASTEXITCODE -ne 0) {
            throw "Could not create the temporary $shortDrive drive alias for .NET restore."
        }
        & $installScript -Version $manifest.dotnet.version -InstallDir "$shortDrive\.dotnet" -NoPath
        if ($LASTEXITCODE -ne 0) {
            throw ".NET SDK installation failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ($shortDrive) {
            & subst.exe $shortDrive /D | Out-Null
        }
    }

    if (-not (Test-Path -LiteralPath $dotnetSdkSentinel)) {
        throw ".NET SDK installation is incomplete; required SDK file is missing: $dotnetSdkSentinel"
    }
}

$resolvedDotnetVersion = & $dotnetExe --version
if ($resolvedDotnetVersion -ne [string]$manifest.dotnet.version) {
    throw "Expected .NET SDK $($manifest.dotnet.version), but local dotnet reported $resolvedDotnetVersion."
}

$aspireExe = Join-Path $aspireDir 'aspire.cmd'
$aspireReady = $false
if (Test-Path -LiteralPath $aspireExe) {
    $aspireVersionOutput = (& $aspireExe --version 2>&1 | Out-String).Trim()
    $aspireReady = $aspireVersionOutput -match [regex]::Escape([string]$manifest.aspire.version)
}

if (-not $aspireReady) {
    Reset-Directory -Path $aspireDir
    Write-Host "Installing Aspire CLI $($manifest.aspire.version) into $aspireDir"
    $shortDrive = $null
    foreach ($letter in [char[]]'ZYXWVUT') {
        $candidate = "${letter}:"
        if (-not (Test-Path "$candidate\")) {
            $shortDrive = $candidate
            break
        }
    }
    if (-not $shortDrive) {
        throw 'Aspire restore needs one unused temporary drive letter between T: and Z: to avoid the Windows path-length limit.'
    }

    try {
        & subst.exe $shortDrive $repoRoot
        if ($LASTEXITCODE -ne 0) {
            throw "Could not create the temporary $shortDrive drive alias for Aspire restore."
        }
        $shortAspireDir = "$shortDrive\$($manifest.aspire.installDirectory)"
        & $dotnetExe tool install $manifest.aspire.packageId --version $manifest.aspire.version --tool-path $shortAspireDir
        if ($LASTEXITCODE -ne 0) {
            throw "Aspire CLI installation failed with exit code $LASTEXITCODE."
        }
    }
    finally {
        if ($shortDrive) {
            & subst.exe $shortDrive /D | Out-Null
        }
    }
}

$nodeExe = Join-Path $nodeDir 'node.exe'
$nodeReady = $false
if (Test-Path -LiteralPath $nodeExe) {
    $nodeReady = ((& $nodeExe --version) -eq "v$($manifest.node.version)")
}

if (-not $nodeReady) {
    $nodeArchiveName = "node-v$($manifest.node.version)-win-x64.zip"
    $nodeArchive = Join-Path $cacheDir $nodeArchiveName
    $nodeStage = Join-Path $toolsDir '.node-installing'
    Get-VerifiedDownload -Uri $manifest.node.archiveUrl -Destination $nodeArchive -Sha256 $manifest.node.archiveSha256
    Reset-Directory -Path $nodeStage
    Expand-Archive -LiteralPath $nodeArchive -DestinationPath $nodeStage -Force
    $expandedNodeDir = Join-Path $nodeStage "node-v$($manifest.node.version)-win-x64"
    if (-not (Test-Path -LiteralPath (Join-Path $expandedNodeDir 'node.exe'))) {
        throw "The Node archive did not contain the expected win-x64 directory."
    }
    if (Test-Path -LiteralPath $nodeDir) {
        Remove-SafeDirectory -Path $nodeDir
    }
    Move-Item -LiteralPath $expandedNodeDir -Destination $nodeDir
    Remove-SafeDirectory -Path $nodeStage
}

$resolvedNodeVersion = & $nodeExe --version
$npmCmd = Join-Path $nodeDir 'npm.cmd'
$resolvedNpmVersion = & $npmCmd --version
if ($resolvedNodeVersion -ne "v$($manifest.node.version)") {
    throw "Expected Node $($manifest.node.version), but local node reported $resolvedNodeVersion."
}
if ($resolvedNpmVersion -ne [string]$manifest.node.npmVersion) {
    throw "Expected npm $($manifest.node.npmVersion), but local npm reported $resolvedNpmVersion."
}

Write-Host ''
Write-Host 'LedgerLens project-local toolchain is ready:'
Write-Host "  .NET   $resolvedDotnetVersion"
Write-Host "  Aspire $((& $aspireExe --version 2>&1 | Out-String).Trim())"
Write-Host "  Node   $resolvedNodeVersion"
Write-Host "  npm    $resolvedNpmVersion"
Write-Host 'Use .\scripts\dev.ps1 doctor to verify the environment.'
