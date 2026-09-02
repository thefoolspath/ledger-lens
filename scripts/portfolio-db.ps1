[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('help', 'scaffold', 'migration', 'script', 'check')]
    [string] $Command = 'help',

    [Parameter(Position = 1)]
    [string] $Argument,

    [Parameter(Position = 2)]
    [string] $To,

    [Parameter(Position = 3)]
    [string] $Output
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$dotnetExe = Join-Path $repoRoot '.dotnet\dotnet.exe'
$efExe = Join-Path $repoRoot '.tools\dotnet-ef\dotnet-ef.exe'
$databaseProject = Join-Path $repoRoot 'src\Services\PortfolioCore\LedgerLens.PortfolioCore.Database\LedgerLens.PortfolioCore.Database.csproj'
$migrationsProject = Join-Path $repoRoot 'src\Services\PortfolioCore\LedgerLens.PortfolioCore.Migrations\LedgerLens.PortfolioCore.Migrations.csproj'
$migrationsDirectory = Join-Path (Split-Path -Parent $migrationsProject) 'Migrations'
$connectionVariable = 'ConnectionStrings__portfolio-db'
$allowedTables = @(
    'user_profiles',
    'portfolios',
    'investment_accounts',
    'cash_ledger_entries',
    'simulation_accounts',
    'simulation_trade_drafts',
    'simulation_trade_entries',
    'simulation_valuation_snapshots'
)

$env:DOTNET_ROOT = Join-Path $repoRoot '.dotnet'
$env:DOTNET_CLI_HOME = Join-Path $repoRoot '.cache\dotnet-home'
$env:NUGET_PACKAGES = Join-Path $repoRoot '.n'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
$env:DOTNET_NOLOGO = '1'
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE = '1'

function Assert-Tool {
    param([Parameter(Mandatory)][string] $Path, [Parameter(Mandatory)][string] $Label)

    if (-not (Test-Path -LiteralPath $Path)) {
        throw "$Label is missing. Run 'lg bootstrap' first."
    }
}

function Invoke-Ef {
    param([Parameter(Mandatory)][string[]] $Arguments)

    & $efExe @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet-ef failed with exit code $LASTEXITCODE."
    }
}

function Assert-SafeScaffoldSource {
    $connectionString = [Environment]::GetEnvironmentVariable($connectionVariable)
    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        throw "Set $connectionVariable in the current process. Secrets must not be stored in tracked files."
    }

    $hostMatch = [regex]::Match($connectionString, '(?:Host|Server)\s*=\s*([^;]+)', 'IgnoreCase')
    if (-not $hostMatch.Success) {
        throw 'The Portfolio Core connection string must contain Host or Server.'
    }

    $hostName = $hostMatch.Groups[1].Value.Trim()
    if ($hostName -in @('localhost', '127.0.0.1', '::1')) {
        return
    }

    $approvedHost = [Environment]::GetEnvironmentVariable('LEDGERLENS_DB_SYNC_APPROVED_HOST')
    if ([string]::IsNullOrWhiteSpace($approvedHost) -or
        -not [string]::Equals($hostName, $approvedHost.Trim(), [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to scaffold from '$hostName'. Set LEDGERLENS_DB_SYNC_APPROVED_HOST to that exact nonproduction host after approval."
    }
}

function Get-CommonEfArguments {
    return @(
        '--project', $migrationsProject,
        '--startup-project', $migrationsProject,
        '--context', 'PortfolioCoreDbContext',
        '--no-build'
    )
}

function Assert-MigrationHasNoDestructiveOperations {
    param([Parameter(Mandatory)][string] $MigrationName)

    $migrationFile = Get-ChildItem -LiteralPath $migrationsDirectory -Filter "*_$MigrationName.cs" |
        Where-Object { $_.Name -notlike '*.Designer.cs' } |
        Sort-Object Name -Descending |
        Select-Object -First 1
    if ($null -eq $migrationFile) {
        throw "Could not locate the generated migration '$MigrationName'."
    }

    $source = Get-Content -LiteralPath $migrationFile.FullName -Raw
    $unsafeOperations = @([regex]::Matches(
        $source,
        'migrationBuilder\.(DropTable|DropColumn|AlterColumn|Sql)\s*(?:<[^>]+>)?\s*\(') |
        ForEach-Object { $_.Groups[1].Value } |
        Sort-Object -Unique)
    if ($unsafeOperations.Count -gt 0) {
        $operations = $unsafeOperations -join ', '
        throw "Migration '$MigrationName' contains review-gated operations: $operations. The files were kept for manual correction and review."
    }
}

Assert-Tool -Path $dotnetExe -Label '.NET SDK'
Assert-Tool -Path $efExe -Label 'EF Core CLI'

if ($Command -eq 'help') {
    @'
Portfolio Core hybrid database tooling

Usage:
  lg db scaffold
  lg db migration <MigrationName>
  lg db check
  lg db script [<from-migration>] [<to-migration>] [<output-path>]

Required environment:
  ConnectionStrings__portfolio-db       Source/design-time PostgreSQL connection.
  LEDGERLENS_DB_SYNC_APPROVED_HOST      Exact approved host for non-local scaffolding.

Safety:
  - Scaffold reads only the eight explicitly allowlisted Portfolio Core-owned tables and never scaffolds EF history.
  - Generated code is isolated in the Database project; Domain/Application are untouched.
  - Migration creation stops on DropTable, DropColumn, AlterColumn, or raw SQL for review.
  - This tool never applies migrations and never inserts EF migration-history rows.
'@ | Write-Host
    exit 0
}

switch ($Command) {
    'scaffold' {
        Assert-SafeScaffoldSource
        $arguments = @(
            'dbcontext', 'scaffold',
            'Name=ConnectionStrings:portfolio-db',
            'Npgsql.EntityFrameworkCore.PostgreSQL',
            '--project', $databaseProject,
            '--startup-project', $migrationsProject,
            '--context', 'PortfolioCoreDbContext',
            '--context-dir', '.',
            '--output-dir', 'Models',
            '--namespace', 'LedgerLens.PortfolioCore.Database.Models',
            '--context-namespace', 'LedgerLens.PortfolioCore.Database',
            '--no-onconfiguring',
            '--force'
        )
        foreach ($table in $allowedTables) {
            $arguments += @('--table', $table)
        }
        Invoke-Ef -Arguments $arguments
        & $dotnetExe build $databaseProject --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw 'The scaffolded Database project does not compile.'
        }
        Write-Host 'Scaffold completed. Review the Database project diff before creating a migration.' -ForegroundColor Green
    }
    'migration' {
        $Name = $Argument
        if ([string]::IsNullOrWhiteSpace($Name) -or $Name -notmatch '^[A-Za-z][A-Za-z0-9_]*$') {
            throw 'Provide a migration name containing only letters, digits, and underscores, starting with a letter.'
        }
        & $dotnetExe build $migrationsProject --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw 'Build failed before migration generation.'
        }
        $arguments = @('migrations', 'add', $Name) + (Get-CommonEfArguments) + @('--output-dir', 'Migrations')
        Invoke-Ef -Arguments $arguments
        Assert-MigrationHasNoDestructiveOperations -MigrationName $Name
        Write-Host "Migration '$Name' created. Run 'lg db check' and generate reviewed SQL before deployment." -ForegroundColor Green
    }
    'check' {
        & $dotnetExe build $migrationsProject --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw 'Build failed before the model-change check.'
        }
        Invoke-Ef -Arguments (@('migrations', 'has-pending-model-changes') + (Get-CommonEfArguments))
        Write-Host 'The EF model matches the committed ModelSnapshot.' -ForegroundColor Green
    }
    'script' {
        $From = $Argument
        & $dotnetExe build $migrationsProject --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw 'Build failed before SQL generation.'
        }
        if ([string]::IsNullOrWhiteSpace($Output)) {
            $artifactDirectory = Join-Path $repoRoot 'artifacts\database\portfolio-core'
            New-Item -ItemType Directory -Force -Path $artifactDirectory | Out-Null
            $Output = Join-Path $artifactDirectory 'portfolio-core-migrations.sql'
        }
        elseif (-not [System.IO.Path]::IsPathRooted($Output)) {
            $Output = Join-Path $repoRoot $Output
        }

        $arguments = @('migrations', 'script')
        if (-not [string]::IsNullOrWhiteSpace($From)) { $arguments += $From }
        if (-not [string]::IsNullOrWhiteSpace($To)) { $arguments += $To }
        if ([string]::IsNullOrWhiteSpace($From) -and [string]::IsNullOrWhiteSpace($To)) {
            $arguments += '--idempotent'
        }
        $arguments += (Get-CommonEfArguments)
        $arguments += @('--output', $Output)
        Invoke-Ef -Arguments $arguments
        Write-Host "Reviewable SQL written to $Output" -ForegroundColor Green
    }
}
