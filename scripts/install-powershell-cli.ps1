[CmdletBinding(SupportsShouldProcess)]
param(
    [switch] $Uninstall
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot)).TrimEnd('\')
$entryPoint = Join-Path $repoRoot 'thefools.cmd'
$profilePath = $PROFILE.CurrentUserCurrentHost
$beginMarker = '# >>> LedgerLens TheFools CLI >>>'
$endMarker = '# <<< LedgerLens TheFools CLI <<<'
$escapedRepoRoot = $repoRoot.Replace("'", "''")

if (-not (Test-Path -LiteralPath $entryPoint -PathType Leaf)) {
    throw "TheFools CLI entry point is missing: $entryPoint"
}

$existingContent = if (Test-Path -LiteralPath $profilePath) {
    [System.IO.File]::ReadAllText($profilePath)
}
else {
    ''
}

$blockPattern = '(?ms)^' + [regex]::Escape($beginMarker) + '.*?^' + [regex]::Escape($endMarker) + '(?:\r?\n)?'
$contentWithoutBlock = [regex]::Replace($existingContent, $blockPattern, '')

if ($Uninstall) {
    if ($contentWithoutBlock -eq $existingContent) {
        Write-Host 'TheFools PowerShell command is not installed.'
        exit 0
    }

    if ($PSCmdlet.ShouldProcess($profilePath, 'Remove the TheFools PowerShell function')) {
        [System.IO.File]::WriteAllText($profilePath, $contentWithoutBlock, [System.Text.UTF8Encoding]::new($false))
        Write-Host 'TheFools PowerShell command was uninstalled. Open a new PowerShell window.'
    }
    exit 0
}

if ($contentWithoutBlock -match '(?im)^\s*function\s+(?:global:)?thefools\b') {
    throw "An unrelated thefools function already exists in $profilePath. Refusing to overwrite it."
}

$functionBlock = @"
$beginMarker
function global:thefools {
    `$theFoolsRepoRoot = '$escapedRepoRoot'
    `$theFoolsCurrentDirectory = [System.IO.Path]::GetFullPath((Get-Location).Path).TrimEnd('\')
    `$theFoolsInsideRepository =
        `$theFoolsCurrentDirectory.Equals(`$theFoolsRepoRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        `$theFoolsCurrentDirectory.StartsWith(`$theFoolsRepoRoot + '\', [System.StringComparison]::OrdinalIgnoreCase)

    if (-not `$theFoolsInsideRepository) {
        throw 'TheFools is available only inside the LedgerLens repository.'
    }

    & (Join-Path `$theFoolsRepoRoot 'thefools.cmd') @args
}
$endMarker
"@

$newContent = $contentWithoutBlock.TrimEnd("`r", "`n")
if ($newContent.Length -gt 0) {
    $newContent += "`r`n`r`n"
}
$newContent += $functionBlock + "`r`n"

if ($PSCmdlet.ShouldProcess($profilePath, 'Install the repository-scoped TheFools PowerShell function')) {
    $profileDirectory = Split-Path -Parent $profilePath
    New-Item -ItemType Directory -Force -Path $profileDirectory | Out-Null
    [System.IO.File]::WriteAllText($profilePath, $newContent, [System.Text.UTF8Encoding]::new($false))
    Write-Host 'TheFools PowerShell command was installed. Open a new PowerShell window, then run: thefools help'
}
