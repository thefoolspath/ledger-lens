[CmdletBinding(SupportsShouldProcess)]
param(
    [switch] $Uninstall
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot)).TrimEnd('\')
$entryPoint = Join-Path $repoRoot 'lg.cmd'
$profilePath = $PROFILE.CurrentUserCurrentHost
$beginMarker = '# >>> LedgerLens LG CLI >>>'
$endMarker = '# <<< LedgerLens LG CLI <<<'
$legacyBeginMarker = '# >>> LedgerLens TheFools CLI >>>'
$legacyEndMarker = '# <<< LedgerLens TheFools CLI <<<'
$escapedRepoRoot = $repoRoot.Replace("'", "''")

if (-not (Test-Path -LiteralPath $entryPoint -PathType Leaf)) {
    throw "LG CLI entry point is missing: $entryPoint"
}

$existingContent = if (Test-Path -LiteralPath $profilePath) {
    [System.IO.File]::ReadAllText($profilePath)
}
else {
    ''
}

$blockPattern = '(?ms)^' + [regex]::Escape($beginMarker) + '.*?^' + [regex]::Escape($endMarker) + '(?:\r?\n)?'
$contentWithoutBlock = [regex]::Replace($existingContent, $blockPattern, '')
$legacyBlockPattern = '(?ms)^' + [regex]::Escape($legacyBeginMarker) + '.*?^' + [regex]::Escape($legacyEndMarker) + '(?:\r?\n)?'
$contentWithoutBlock = [regex]::Replace($contentWithoutBlock, $legacyBlockPattern, '')

if ($Uninstall) {
    if ($contentWithoutBlock -eq $existingContent) {
        Write-Host 'LG PowerShell command is not installed.'
        exit 0
    }

    if ($PSCmdlet.ShouldProcess($profilePath, 'Remove the LG PowerShell function')) {
        [System.IO.File]::WriteAllText($profilePath, $contentWithoutBlock, [System.Text.UTF8Encoding]::new($false))
        Write-Host 'LG PowerShell command was uninstalled. Open a new PowerShell window.'
    }
    exit 0
}

if ($contentWithoutBlock -match '(?im)^\s*function\s+(?:global:)?lg\b') {
    throw "An unrelated lg function already exists in $profilePath. Refusing to overwrite it."
}

$functionBlock = @"
$beginMarker
function global:lg {
    `$ledgerLensRepoRoot = '$escapedRepoRoot'
    `$ledgerLensCurrentDirectory = [System.IO.Path]::GetFullPath((Get-Location).Path).TrimEnd('\')
    `$ledgerLensInsideRepository =
        `$ledgerLensCurrentDirectory.Equals(`$ledgerLensRepoRoot, [System.StringComparison]::OrdinalIgnoreCase) -or
        `$ledgerLensCurrentDirectory.StartsWith(`$ledgerLensRepoRoot + '\', [System.StringComparison]::OrdinalIgnoreCase)

    if (-not `$ledgerLensInsideRepository) {
        throw 'LG is available only inside the LedgerLens repository.'
    }

    & (Join-Path `$ledgerLensRepoRoot 'lg.cmd') @args
}
$endMarker
"@

$newContent = $contentWithoutBlock.TrimEnd("`r", "`n")
if ($newContent.Length -gt 0) {
    $newContent += "`r`n`r`n"
}
$newContent += $functionBlock + "`r`n"

if ($PSCmdlet.ShouldProcess($profilePath, 'Install the repository-scoped LG PowerShell function')) {
    $profileDirectory = Split-Path -Parent $profilePath
    New-Item -ItemType Directory -Force -Path $profileDirectory | Out-Null
    [System.IO.File]::WriteAllText($profilePath, $newContent, [System.Text.UTF8Encoding]::new($false))
    Write-Host 'LG PowerShell command was installed. Open a new PowerShell window, then run: lg help'
}
