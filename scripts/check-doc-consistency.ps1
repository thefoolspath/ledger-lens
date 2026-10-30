[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure {
    param([Parameter(Mandatory)][string] $Message)
    $failures.Add($Message)
}

$planIndexPath = Join-Path $repoRoot 'docs\plans\README.md'
$planIndex = Get-Content -Raw -LiteralPath $planIndexPath -Encoding UTF8
$activePlanDirectory = Join-Path $repoRoot 'docs\plans\active'

Get-ChildItem -LiteralPath $activePlanDirectory -Filter '*.md' -File | ForEach-Object {
    $expectedLink = "active/$($_.Name)"
    if ($planIndex.IndexOf($expectedLink, [System.StringComparison]::Ordinal) -lt 0) {
        Add-Failure "Active plan is missing from docs/plans/README.md: $($_.Name)"
    }
}

$prohibitedPhrases = @(
    @{ Path = 'AGENTS.md'; Text = 'planning documentation only' },
    @{ Path = 'docs\README.md'; Text = 'has no production implementation' },
    @{ Path = 'docs\README.md'; Text = 'Source code and automated tests, once they exist' },
    @{ Path = 'README.md'; Text = 'Manual Simulation desktop/mobile visual QA and the remaining Portfolio Core milestones are still pending' },
    @{ Path = 'docs\product\EXECUTIVE_SUMMARY.md'; Text = 'The next delivery is the manual portfolio vertical slice' },
    @{ Path = 'docs\architecture\DOTNET_APPLICATION_ARCHITECTURE.md'; Text = 'Future vertical slices use' },
    @{ Path = 'docs\architecture\ASPIRE_TOPOLOGY.md'; Text = 'Connectivity shell now; product UI later' },
    @{ Path = 'docs\project\LOCAL_DEVELOPMENT_TOOLING.md'; Text = 'future Angular workspace' },
    @{ Path = 'docs\research\PRE_IMPLEMENTATION_RESEARCH_GATE.md'; Text = 'No production implementation should start' }
)

foreach ($rule in $prohibitedPhrases) {
    $path = Join-Path $repoRoot $rule.Path
    $content = Get-Content -Raw -LiteralPath $path -Encoding UTF8
    if ($content.IndexOf($rule.Text, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Failure "Stale central-state phrase remains in $($rule.Path): $($rule.Text)"
    }
}

$identityPlanPath = Join-Path $repoRoot 'docs\plans\active\0006-identity-access-google-oidc-and-ui-resources.md'
$identityPlan = Get-Content -Raw -LiteralPath $identityPlanPath -Encoding UTF8
if ($identityPlan.IndexOf('Design-only proposal; implementation is not authorized', [System.StringComparison]::Ordinal) -lt 0) {
    Add-Failure 'Plan 0006 must remain explicitly design-only until owner reprioritization.'
}

$projectState = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'docs\project\PROJECT_STATE.md') -Encoding UTF8
$projectDateMatch = [regex]::Match($projectState, 'Last reviewed:\s*(?<date>\d{4}-\d{2}-\d{2})')
if (-not $projectDateMatch.Success) {
    Add-Failure 'PROJECT_STATE.md is missing a Last reviewed date.'
}
else {
    $projectDate = [datetime]::ParseExact($projectDateMatch.Groups['date'].Value, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
    foreach ($relativePath in @('docs\README.md', 'docs\plans\README.md', 'docs\product\EXECUTIVE_SUMMARY.md')) {
        $content = Get-Content -Raw -LiteralPath (Join-Path $repoRoot $relativePath) -Encoding UTF8
        $dateMatch = [regex]::Match($content, 'Last reviewed:\s*(?<date>\d{4}-\d{2}-\d{2})')
        if (-not $dateMatch.Success) {
            Add-Failure "$relativePath is missing a Last reviewed date."
            continue
        }

        $reviewDate = [datetime]::ParseExact($dateMatch.Groups['date'].Value, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
        if ($reviewDate -lt $projectDate) {
            Add-Failure "$relativePath review date is older than PROJECT_STATE.md."
        }
    }
}

$markdownFiles = @(
    Get-ChildItem -LiteralPath $repoRoot -Filter '*.md' -File
    Get-ChildItem -LiteralPath (Join-Path $repoRoot 'docs') -Filter '*.md' -File -Recurse
)

foreach ($file in $markdownFiles) {
    $lines = Get-Content -LiteralPath $file.FullName -Encoding UTF8
    for ($lineIndex = 0; $lineIndex -lt $lines.Count; $lineIndex++) {
        foreach ($match in [regex]::Matches($lines[$lineIndex], '(?<!!)\[[^\]]*\]\((?<target>[^)]+)\)')) {
            $target = $match.Groups['target'].Value.Trim()
            if ($target.StartsWith('<') -and $target.Contains('>')) {
                $target = $target.Substring(1, $target.IndexOf('>') - 1)
            }
            elseif ($target.Contains(' ')) {
                $target = ([regex]::Split($target, '\s+', 2))[0]
            }

            if ($target -match '^(?:https?:|mailto:|#|codex:)' -or [string]::IsNullOrWhiteSpace($target)) {
                continue
            }

            $pathPart = ($target -split '#', 2)[0]
            if ([string]::IsNullOrWhiteSpace($pathPart)) {
                continue
            }

            $decodedPath = [uri]::UnescapeDataString($pathPart)
            $resolvedPath = [System.IO.Path]::GetFullPath((Join-Path $file.DirectoryName $decodedPath))
            if (-not (Test-Path -LiteralPath $resolvedPath)) {
                $relativeFile = $file.FullName.Substring($repoRoot.Length).TrimStart('\')
                Add-Failure "Broken Markdown link at ${relativeFile}:$($lineIndex + 1) -> $target"
            }
        }
    }
}

if ($failures.Count -gt 0) {
    $failures | ForEach-Object { Write-Host "ERROR: $_" -ForegroundColor Red }
    Write-Host "Documentation consistency failed with $($failures.Count) error(s)." -ForegroundColor Red
    exit 1
}

Write-Host "Documentation consistency passed: $($markdownFiles.Count) Markdown files, $((Get-ChildItem -LiteralPath $activePlanDirectory -Filter '*.md' -File).Count) active plans." -ForegroundColor Green
