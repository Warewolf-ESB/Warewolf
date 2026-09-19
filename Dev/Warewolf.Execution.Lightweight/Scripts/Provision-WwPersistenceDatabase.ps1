#Requires -Version 7.0
<#
.SYNOPSIS
    Provisions (or read-only CHECKS) the Azure SQL database the Lightweight engine's Hangfire
    persistence needs, and reports what it found in the BLOCKER / EXPECTED-LATER / REPORT vocabulary
    the Profiler pipelines' preflight mode already uses.

.DESCRIPTION
    WHY THE PIPELINE PROVISIONS THIS AT ALL. Engine persistence is what makes
    ServiceBusReplayAndResultStore's dedupe survive an instance recycle: with persistence off it
    falls back to a process-local ConcurrentDictionary, so a redelivery landing on a different
    instance re-executes the workflow. That was the root cause of the 2026-09-15 re-execution storm
    (7,648 executions for 5,000 messages). Turning it on needs a Hangfire database, and The
    Unlimited's UAT has never had one - so the pipeline creates it rather than making its first real
    run depend on a hand-provisioned resource nobody has been asked for yet.

    CONTROL PLANE ONLY, NEVER T-SQL. This script shells to 'az' and nothing else, because the
    Profiler pipelines run on a self-hosted 'Default' agent documented to carry only PowerShell 7
    and the Azure CLI. There is no SQL client to assume: the repo's own Invoke-WwSqlQuery works by
    shelling to WINDOWS PowerShell 5.1 (Microsoft.Data.SqlClient needs its native SNI companion,
    which will not load in a standalone PS7 process), so it is Windows-only and unavailable here.

    WHICH MAKES THE ADMIN LOGIN THE PIVOT, and it is the check most worth reading. 'az sql db
    create' leaves a database that only the server's ADMINISTRATOR can log into; any other login
    needs a CREATE USER ... FOR LOGIN issued INSIDE the new database, which is T-SQL this script
    cannot run. So the connection string the engine will use must name the server admin. -CheckOnly
    proves or disproves that with 'az sql server show --query administratorLogin' and, when it
    differs, fails as a BLOCKER carrying the exact statement an operator must run once by hand.
    Discovering that during preflight is the entire point: the alternative is a deploy that succeeds
    and an engine that cannot reach its own persistence store.

    THE PERMISSION PROBE IS AN ARM what-if. 'az deployment group what-if' against a minimal
    Microsoft.Sql/servers/databases template is the closest thing to a read-only "may I create
    this?" that Azure offers - it round-trips the request through the same preflight validation a
    real deployment would, without creating anything. It is a strong signal, NOT a guarantee: it can
    pass and a later create still fail on a policy or a quota. Treated accordingly - a failure is a
    BLOCKER, a pass is reported as "permitted (what-if)" rather than as a promise.

.PARAMETER SqlServerFqdn
    REQUIRED. The server as the connection string names it, e.g. 'foo.database.windows.net'. The
    Azure resource is resolved FROM this rather than assumed to be its first label, because the
    resource name and the DNS name can differ and the resource group is not derivable at all.
.PARAMETER DatabaseName
    REQUIRED. The Hangfire catalog to create/check, e.g. 'uatprofilerlw-hangfire'.
.PARAMETER ExpectedAdminLogin
    REQUIRED. The SQL login the engine's DbSource will use. Compared against the server's actual
    administratorLogin - see the DESCRIPTION for why a mismatch is a BLOCKER.
.PARAMETER ResourceGroup
    Optional. Resolved from the server lookup when omitted.
.PARAMETER Edition / ComputeModel / Family / Capacity / MinCapacity / AutoPauseDelayMinutes
    Serverless General Purpose by default. Hangfire's load here is bookkeeping, not workload, and an
    auto-pausing database costs nothing between runs - but note an auto-paused database takes ~30-60s
    to resume, which the engine sees as a slow first persistence call after an idle period.
.PARAMETER CheckOnly
    Read-only. Runs every check and CREATES NOTHING. This is what preflight mode calls.
.PARAMETER SkipFirewallRule
    Do not create/verify the 'allow Azure services' rule. The engine reaches SQL as an Azure service,
    so without that rule (or an equivalent private endpoint) it cannot connect at all.
.PARAMETER SummaryPath
    Where to write the JSON summary. Defaults beside the script, timestamped.
.PARAMETER LoadFunctionsOnly
    Test hook - define the pure helpers and return, touching no cloud.

.EXAMPLE
    # Preflight: prove the pipeline could provision this, change nothing.
    ./Provision-WwPersistenceDatabase.ps1 -SqlServerFqdn 'foo.database.windows.net' `
        -DatabaseName 'uatprofilerlw-hangfire' -ExpectedAdminLogin 'warewolfadmin' -CheckOnly

.EXAMPLE
    # Real run: create the database and the firewall rule if they are absent.
    ./Provision-WwPersistenceDatabase.ps1 -SqlServerFqdn 'foo.database.windows.net' `
        -DatabaseName 'uatprofilerlw-hangfire' -ExpectedAdminLogin 'warewolfadmin'

.NOTES
    Idempotent: an existing database is never re-created or re-scaled, and an existing firewall rule
    is left alone. Nothing here drops or alters an existing database - this script only ever adds.
#>
[CmdletBinding()]
param(
    # NOT [Parameter(Mandatory)] - a mandatory parameter PROMPTS when the script is dot-sourced with
    # -LoadFunctionsOnly, which hangs an unattended test run. Enforced below, after the helpers-only exit.
    [string] $SqlServerFqdn,
    [string] $DatabaseName,
    [string] $ExpectedAdminLogin,
    [string] $ResourceGroup,

    [string] $Edition     = 'GeneralPurpose',
    [string] $ComputeModel = 'Serverless',
    [string] $Family      = 'Gen5',
    [int]    $Capacity    = 1,
    [double] $MinCapacity = 0.5,
    [int]    $AutoPauseDelayMinutes = 60,

    [switch] $CheckOnly,
    [switch] $SkipFirewallRule,
    [string] $SummaryPath,

    [switch] $LoadFunctionsOnly          # test hook — define the pure helpers then return
)

$ErrorActionPreference = 'Stop'

# The rule name Azure's own portal uses for the "Allow Azure services and resources to access this
# server" toggle. Matching it means the portal switch and this script see the same object, instead of
# each creating its own 0.0.0.0 rule.
$script:AzureServicesRuleName = 'AllowAllWindowsAzureIps'

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }
function Write-Bad  { param([string] $Msg) Write-Host "  [x] $Msg" -ForegroundColor Red }

# ═════════════════════════════════════════════════════════════════════════════
# Pure helpers — no cloud call, no file write. Defined ABOVE the
# -LoadFunctionsOnly return so Pester can exercise every branch offline.
# ═════════════════════════════════════════════════════════════════════════════

function New-WwProvisionCheck {
    <#
        One row of the preflight report, in the vocabulary the Profiler pipelines already use:

          BLOCKER        must pass before a real run; a FAIL fails the step.
          EXPECTED-LATER something the first real deploy creates - absent is NORMAL beforehand, so it
                         must never fail a preflight run against a not-yet-provisioned environment.
          REPORT         printed for a human; never fails anything.

        Remediation is carried on the record rather than logged at the point of discovery, so the
        summary JSON a build publishes is self-contained.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('BLOCKER', 'EXPECTED-LATER', 'REPORT')][string] $Class,
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][ValidateSet('PASS', 'FAIL', 'INFO')][string] $Status,
        [string] $Detail,
        [string] $Remediation
    )
    [pscustomobject]@{
        Class       = $Class
        Name        = $Name
        Status      = $Status
        Detail      = $Detail
        Remediation = $Remediation
    }
}

function Get-WwSqlServerFromFqdn {
    <#
        PURE. Picks the server resource whose fullyQualifiedDomainName matches, from whatever
        'az sql server list' returned.

        WHY NOT JUST SPLIT THE HOSTNAME. 'foo.database.windows.net' usually means a resource named
        'foo', but the resource group is not in the name at all, and a server can be reached through
        a DNS alias or a private endpoint whose first label is NOT the resource name. Matching on
        the FQDN the connection string actually uses is the only mapping that cannot be wrong.

        Comparison is case-insensitive and tolerates a trailing dot (a fully-qualified DNS name may
        legitimately carry one). AMBIGUITY IS AN ERROR, not a first-match: two servers answering to
        one FQDN means the caller is not describing the resource it thinks it is.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowNull()] $Servers,
        [Parameter(Mandatory)][string] $Fqdn
    )

    $needle = $Fqdn.Trim().TrimEnd('.').ToLowerInvariant()
    $matched = @(@($Servers) | Where-Object {
        $_ -and $_.fullyQualifiedDomainName -and
        $_.fullyQualifiedDomainName.Trim().TrimEnd('.').ToLowerInvariant() -eq $needle
    })

    if ($matched.Count -eq 0) { return $null }
    if ($matched.Count -gt 1) {
        throw ("Ambiguous SQL server lookup: $($matched.Count) servers report the FQDN '$Fqdn' " +
               "($($matched.name -join ', ')). Pass -ResourceGroup to disambiguate.")
    }
    return $matched[0]
}

function Get-WwAdminLoginCheck {
    <#
        PURE. The pivot check - see the script's DESCRIPTION.

        A database created through the control plane is reachable only by the server administrator
        until someone issues a CREATE USER inside it. This script has no SQL client, so when the
        engine's login is NOT the admin the pipeline cannot finish the job, and saying so during
        preflight (with the statement to run) is far better than a green deploy and an engine that
        cannot reach its persistence store.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string] $ActualAdminLogin,
        [Parameter(Mandatory)][AllowEmptyString()][string] $ExpectedLogin,
        [Parameter(Mandatory)][string] $DatabaseName
    )

    if ([string]::IsNullOrWhiteSpace($ActualAdminLogin)) {
        return New-WwProvisionCheck -Class 'BLOCKER' -Name 'SQL admin login matches the engine login' -Status 'FAIL' `
            -Detail 'The server reported no administratorLogin (it may be Entra-only).' `
            -Remediation ("Confirm how the engine should authenticate. If the server is Entra-only, the DbSource " +
                          "cannot use SQL authentication and persistence needs a different credential model.")
    }

    if ($ActualAdminLogin -eq $ExpectedLogin) {
        return New-WwProvisionCheck -Class 'BLOCKER' -Name 'SQL admin login matches the engine login' -Status 'PASS' `
            -Detail "administratorLogin '$ActualAdminLogin' is the login the engine's DbSource will use, so it is dbo in a newly created database."
    }

    return New-WwProvisionCheck -Class 'BLOCKER' -Name 'SQL admin login matches the engine login' -Status 'FAIL' `
        -Detail ("administratorLogin is '$ActualAdminLogin' but the engine's DbSource will connect as " +
                 "'$ExpectedLogin'. A database created through the control plane grants nothing to that login.") `
        -Remediation ("Run ONCE against [$DatabaseName], connected as '$ActualAdminLogin':`n" +
                      "    CREATE USER [$ExpectedLogin] FOR LOGIN [$ExpectedLogin];`n" +
                      "    ALTER ROLE db_owner ADD MEMBER [$ExpectedLogin];`n" +
                      "db_owner is required, not merely convenient: persistencesettings.json sets " +
                      "PrepareSchemaIfNecessary=true, so the engine CREATES the Hangfire schema on first start.")
}

function Get-WwProvisionVerdict {
    <#
        PURE. Turns the check list into a pass/fail decision.

        ONLY A FAILED BLOCKER FAILS THE RUN. An EXPECTED-LATER failure is the normal state of a
        not-yet-provisioned environment - failing on it would make the very first preflight read as
        a broken pipeline, which is exactly the trap the preflight vocabulary exists to avoid.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowNull()] $Checks)

    $all       = @($Checks)
    $blockers  = @($all | Where-Object { $_.Class -eq 'BLOCKER' -and $_.Status -eq 'FAIL' })
    $expected  = @($all | Where-Object { $_.Class -eq 'EXPECTED-LATER' -and $_.Status -eq 'FAIL' })

    [pscustomobject]@{
        Passed              = ($blockers.Count -eq 0)
        BlockerFailures     = $blockers.Count
        ExpectedLaterAbsent = $expected.Count
        Total               = $all.Count
        Message             = if ($blockers.Count -eq 0) {
            "All $($all.Count) check(s) acceptable; $($expected.Count) item(s) the first real run will create."
        } else {
            "$($blockers.Count) BLOCKER check(s) failed: $(($blockers.Name) -join '; ')"
        }
    }
}

function ConvertTo-WwPersistenceConnectionString {
    <#
        PURE. Builds the connection string the generated DbSource .bite will carry.

        The password is interpolated but NEVER logged by this function, and callers must keep it out
        of transcripts - the summary JSON records the connection string with the password masked.

        Encrypt=True is explicit rather than relied upon: Azure SQL requires TLS regardless, but an
        explicit value keeps the string honest if it is ever pointed at a local SQL Server.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $ServerFqdn,
        [Parameter(Mandatory)][string] $DatabaseName,
        [Parameter(Mandatory)][string] $UserId,
        [Parameter(Mandatory)][AllowEmptyString()][string] $Password,
        [int] $ConnectTimeoutSeconds = 30
    )
    return ("Data Source=$ServerFqdn,1433;Initial Catalog=$DatabaseName;User ID=$UserId;" +
            "Password=$Password;Encrypt=True;Connection Timeout=$ConnectTimeoutSeconds")
}

function Hide-WwConnectionStringPassword {
    <#
        PURE. Masks the password so a connection string can be printed and persisted in a summary.
        Everything else is kept, because the server, catalog and login are exactly what a reader
        needs to confirm the engine is pointed where they think it is.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $ConnectionString)
    return ([regex]::Replace($ConnectionString, '(?i)(password\s*=)[^;]*', '${1}***'))
}

function Get-WwDatabaseSkuName {
    <#
        PURE. The SKU name 'az sql db create --sku' wants, assembled from the edition/family/capacity
        triple so the serverless and provisioned shapes cannot drift apart.

        Serverless SKUs are spelled GP_S_Gen5_<capacity>; provisioned ones GP_Gen5_<capacity>. The
        '_S_' is the whole difference between a database that auto-pauses to zero cost and one that
        bills continuously, so it is derived, never typed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Edition,
        [Parameter(Mandatory)][string] $ComputeModel,
        [Parameter(Mandatory)][string] $Family,
        [Parameter(Mandatory)][int] $Capacity
    )

    $prefix = switch ($Edition) {
        'GeneralPurpose' { 'GP' }
        'BusinessCritical' { 'BC' }
        'Hyperscale' { 'HS' }
        default { throw "Unsupported -Edition '$Edition' for SKU assembly (expected GeneralPurpose, BusinessCritical or Hyperscale)." }
    }
    $serverless = if ($ComputeModel -eq 'Serverless') { '_S' } else { '' }
    return "${prefix}${serverless}_${Family}_${Capacity}"
}

function New-WwSqlDatabaseWhatIfTemplate {
    <#
        PURE. The minimal ARM template the permission probe deploys in what-if mode.

        It describes exactly the resource the real run would create, because what-if only answers
        for what it is given: probing with a different SKU or a different name would prove the
        pipeline may create SOMETHING, not that it may create THIS.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $ServerName,
        [Parameter(Mandatory)][string] $DatabaseName,
        [Parameter(Mandatory)][string] $Location,
        [Parameter(Mandatory)][string] $SkuName,
        [Parameter(Mandatory)][string] $Edition,
        [Parameter(Mandatory)][string] $Family,
        [Parameter(Mandatory)][int] $Capacity,
        [double] $MinCapacity = 0.5,
        [int] $AutoPauseDelayMinutes = 60
    )

    $resource = [ordered]@{
        type       = 'Microsoft.Sql/servers/databases'
        apiVersion = '2022-05-01-preview'
        name       = "$ServerName/$DatabaseName"
        location   = $Location
        sku        = [ordered]@{ name = $SkuName; tier = $Edition; family = $Family; capacity = $Capacity }
        properties = [ordered]@{ autoPauseDelay = $AutoPauseDelayMinutes; minCapacity = $MinCapacity }
    }

    return ([ordered]@{
        '$schema'      = 'https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#'
        contentVersion = '1.0.0.0'
        parameters     = [ordered]@{}
        resources      = @($resource)
    } | ConvertTo-Json -Depth 8)
}

function Test-WwAzAuthorizationFailure {
    <#
        PURE. Does this az output read as "you are not allowed to do that", as opposed to any other
        failure? Distinguishing them matters: an authorization failure is a BLOCKER a human must fix
        in RBAC, while a transient or malformed-input failure is not evidence about permissions at
        all and must not be reported as though it were.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $Output)

    if ([string]::IsNullOrWhiteSpace($Output)) { return $false }
    return [bool]($Output -match '(?i)AuthorizationFailed|does not have authorization|Forbidden|InsufficientPrivileges|RequestDisallowedByPolicy')
}

if ($LoadFunctionsOnly) { return }

# ═════════════════════════════════════════════════════════════════════════════
# Cloud-touching helpers
# ═════════════════════════════════════════════════════════════════════════════

function Invoke-ProvisionAz {
    <#
        Runs 'az' and returns stdout as TEXT, with PowerShell ErrorRecords stripped out.

        THE STRIP IS LOAD-BEARING, not tidiness. Capturing with 2>&1 folds stderr into the pipeline
        as ErrorRecords, and a service principal signed in with --allow-no-subscriptions (no Graph
        read) makes az print 'WARNING: Failed to query ... by invoking Graph API' on stderr while
        still EXITING 0. Build 30759 died exactly there: the command had succeeded and only
        ConvertFrom-Json failed, on a 'W'. Deploy-WwQueueProcessor.ps1 applies the same split.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string[]] $AzArgs,
        [switch] $AllowFail
    )

    $raw = & az @AzArgs 2>&1
    $exit = $LASTEXITCODE
    $stdout = @($raw | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] }) -join "`n"
    $stderr = @($raw | Where-Object { $_ -is [System.Management.Automation.ErrorRecord] } |
                ForEach-Object { $_.ToString() }) -join "`n"

    if ($exit -ne 0 -and -not $AllowFail) {
        throw "az $($AzArgs -join ' ') failed (exit $exit): $stderr$stdout"
    }
    return [pscustomobject]@{ ExitCode = $exit; StdOut = $stdout; StdErr = $stderr }
}

function ConvertFrom-ProvisionAzJson {
    param([Parameter(Mandatory)][AllowEmptyString()][AllowNull()][string] $Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return $null }
    try { return ($Text | ConvertFrom-Json) } catch { return $null }
}

# ═════════════════════════════════════════════════════════════════════════════
# Guards
# ═════════════════════════════════════════════════════════════════════════════

foreach ($required in @(
    @{ N = 'SqlServerFqdn'; V = $SqlServerFqdn }, @{ N = 'DatabaseName'; V = $DatabaseName },
    @{ N = 'ExpectedAdminLogin'; V = $ExpectedAdminLogin }
)) {
    if ([string]::IsNullOrWhiteSpace($required.V)) { throw "-$($required.N) is required." }
}

$stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
if (-not $SummaryPath) {
    $SummaryPath = Join-Path $PSScriptRoot "provision-WwPersistenceDatabase-$stamp.summary.json"
}

$checks = [System.Collections.Generic.List[object]]::new()
$mode   = if ($CheckOnly) { 'CHECK-ONLY (read-only)' } else { 'PROVISION' }

Write-Host ''
Write-Host "== Persistence database - $mode" -ForegroundColor Cyan
Write-Host ("   {0,-22} {1}" -f 'server (fqdn)', $SqlServerFqdn)
Write-Host ("   {0,-22} {1}" -f 'database', $DatabaseName)
Write-Host ("   {0,-22} {1}" -f 'engine login', $ExpectedAdminLogin)

# ── az present and logged in ────────────────────────────────────────────────
$account = ConvertFrom-ProvisionAzJson (Invoke-ProvisionAz -AzArgs @('account', 'show', '-o', 'json') -AllowFail).StdOut
if (-not $account) {
    $checks.Add((New-WwProvisionCheck -Class 'BLOCKER' -Name 'Azure CLI signed in' -Status 'FAIL' `
        -Detail 'az account show returned nothing.' -Remediation 'Run this step under an AzureCLI@2 task, or az login first.'))
    $verdict = Get-WwProvisionVerdict -Checks $checks
    $checks | ForEach-Object { Write-Bad "$($_.Class) $($_.Name): $($_.Detail)" }
    throw $verdict.Message
}
$checks.Add((New-WwProvisionCheck -Class 'REPORT' -Name 'Azure subscription' -Status 'INFO' `
    -Detail "$($account.name) ($($account.id))"))

# ── resolve the server resource from the FQDN ───────────────────────────────
$serverArgs = @('sql', 'server', 'list', '-o', 'json')
if ($ResourceGroup) { $serverArgs = @('sql', 'server', 'list', '--resource-group', $ResourceGroup, '-o', 'json') }
$servers = ConvertFrom-ProvisionAzJson (Invoke-ProvisionAz -AzArgs $serverArgs -AllowFail).StdOut
$server  = Get-WwSqlServerFromFqdn -Servers $servers -Fqdn $SqlServerFqdn

if (-not $server) {
    $checks.Add((New-WwProvisionCheck -Class 'BLOCKER' -Name 'SQL server resolved from FQDN' -Status 'FAIL' `
        -Detail "No SQL server visible to this identity reports the FQDN '$SqlServerFqdn'." `
        -Remediation ("Confirm the server exists in this subscription and that the pipeline's service principal can " +
                      "read it (Reader on the server's resource group is enough for this lookup).")))
    $verdict = Get-WwProvisionVerdict -Checks $checks
    $checks | ForEach-Object { Write-Bad "$($_.Class) $($_.Name): $($_.Detail)" }
    ($checks | ConvertTo-Json -Depth 6) | Set-Content -LiteralPath $SummaryPath -Encoding utf8
    throw $verdict.Message
}

$serverName = $server.name
$serverRg   = if ($ResourceGroup) { $ResourceGroup } else { $server.resourceGroup }
$location   = $server.location
Write-Ok "Resolved '$SqlServerFqdn' -> server '$serverName' (rg '$serverRg', $location)"
$checks.Add((New-WwProvisionCheck -Class 'REPORT' -Name 'SQL server resolved from FQDN' -Status 'INFO' `
    -Detail "$serverName in $serverRg ($location)"))

# ── the pivot: is the engine's login the server admin? ──────────────────────
$checks.Add((Get-WwAdminLoginCheck -ActualAdminLogin ([string]$server.administratorLogin) `
                -ExpectedLogin $ExpectedAdminLogin -DatabaseName $DatabaseName))

# ── does the database already exist? ────────────────────────────────────────
$dbShow = Invoke-ProvisionAz -AzArgs @('sql', 'db', 'show', '--name', $DatabaseName, '--server', $serverName,
                                       '--resource-group', $serverRg, '-o', 'json') -AllowFail
$existingDb = if ($dbShow.ExitCode -eq 0) { ConvertFrom-ProvisionAzJson $dbShow.StdOut } else { $null }

if ($existingDb) {
    $checks.Add((New-WwProvisionCheck -Class 'REPORT' -Name 'Persistence database exists' -Status 'INFO' `
        -Detail "'$DatabaseName' already exists (sku $($existingDb.currentSku.name), status $($existingDb.status)) - it will NOT be re-created or re-scaled."))
} else {
    $checks.Add((New-WwProvisionCheck -Class 'EXPECTED-LATER' -Name 'Persistence database exists' -Status 'FAIL' `
        -Detail "'$DatabaseName' does not exist yet." `
        -Remediation 'The first non-preflight run of this step creates it.'))
}

$skuName = Get-WwDatabaseSkuName -Edition $Edition -ComputeModel $ComputeModel -Family $Family -Capacity $Capacity

# ── permission probe: ARM what-if for exactly the resource we would create ──
if (-not $existingDb) {
    $templatePath = Join-Path ([System.IO.Path]::GetTempPath()) "ww-persistence-whatif-$stamp.json"
    New-WwSqlDatabaseWhatIfTemplate -ServerName $serverName -DatabaseName $DatabaseName -Location $location `
        -SkuName $skuName -Edition $Edition -Family $Family -Capacity $Capacity `
        -MinCapacity $MinCapacity -AutoPauseDelayMinutes $AutoPauseDelayMinutes |
        Set-Content -LiteralPath $templatePath -Encoding utf8
    try {
        $whatIf = Invoke-ProvisionAz -AzArgs @('deployment', 'group', 'what-if', '--resource-group', $serverRg,
                                               '--template-file', $templatePath, '--no-pretty-print', '-o', 'json') -AllowFail
        if ($whatIf.ExitCode -eq 0) {
            $checks.Add((New-WwProvisionCheck -Class 'BLOCKER' -Name 'Permitted to create the database (what-if)' -Status 'PASS' `
                -Detail "ARM what-if accepted a Microsoft.Sql/servers/databases deployment of '$DatabaseName' ($skuName) into '$serverRg'. Strong signal, not a guarantee - a policy or quota can still refuse the real create."))
        }
        elseif (Test-WwAzAuthorizationFailure -Output "$($whatIf.StdErr)`n$($whatIf.StdOut)") {
            $checks.Add((New-WwProvisionCheck -Class 'BLOCKER' -Name 'Permitted to create the database (what-if)' -Status 'FAIL' `
                -Detail "ARM what-if was refused: $($whatIf.StdErr.Trim())" `
                -Remediation ("Grant the pipeline's service principal a role that includes " +
                              "Microsoft.Sql/servers/databases/write on '$serverRg' (SQL DB Contributor is the least " +
                              "privileged built-in role that covers it).")))
        }
        else {
            $checks.Add((New-WwProvisionCheck -Class 'REPORT' -Name 'Permitted to create the database (what-if)' -Status 'INFO' `
                -Detail ("what-if did not complete, and the failure does not read as an authorization problem, so it is " +
                         "NOT evidence either way: $($whatIf.StdErr.Trim())")))
        }
    }
    finally {
        Remove-Item -LiteralPath $templatePath -Force -ErrorAction SilentlyContinue
    }
}

# ── role assignments, for a human to read ───────────────────────────────────
$spId = $account.user.name
if ($spId) {
    $ra = Invoke-ProvisionAz -AzArgs @('role', 'assignment', 'list', '--assignee', $spId,
                                       '--scope', "/subscriptions/$($account.id)/resourceGroups/$serverRg",
                                       '--query', '[].roleDefinitionName', '-o', 'json') -AllowFail
    $roles = ConvertFrom-ProvisionAzJson $ra.StdOut
    $checks.Add((New-WwProvisionCheck -Class 'REPORT' -Name 'Service principal roles on the server resource group' -Status 'INFO' `
        -Detail $(if ($roles) { "${spId}: $(@($roles) -join ', ')" }
                  else { "Could not read role assignments for '$spId' (a principal without Microsoft Graph read cannot list its own roles - this is normal and is why the what-if probe above is the real check)." })))
}

# ── firewall: the engine reaches SQL as an Azure service ────────────────────
if (-not $SkipFirewallRule) {
    $fw = Invoke-ProvisionAz -AzArgs @('sql', 'server', 'firewall-rule', 'show', '--name', $script:AzureServicesRuleName,
                                       '--server', $serverName, '--resource-group', $serverRg, '-o', 'json') -AllowFail
    if ($fw.ExitCode -eq 0) {
        $checks.Add((New-WwProvisionCheck -Class 'REPORT' -Name 'Allow-Azure-services firewall rule' -Status 'INFO' `
            -Detail "'$($script:AzureServicesRuleName)' already present."))
    } else {
        $checks.Add((New-WwProvisionCheck -Class 'EXPECTED-LATER' -Name 'Allow-Azure-services firewall rule' -Status 'FAIL' `
            -Detail "'$($script:AzureServicesRuleName)' is absent; without it (or a private endpoint) the Function App cannot reach this server." `
            -Remediation 'The first non-preflight run of this step creates it.'))
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Report, then act
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ''
foreach ($c in $checks) {
    $line = "$($c.Class.PadRight(14)) $($c.Name): $($c.Detail)"
    switch ($c.Status) {
        'PASS' { Write-Ok $line }
        'FAIL' { if ($c.Class -eq 'BLOCKER') { Write-Bad $line } else { Write-Note $line } }
        default { Write-Step $line }
    }
    if ($c.Remediation -and $c.Status -eq 'FAIL') {
        foreach ($rl in ($c.Remediation -split "`n")) { Write-Host "                 $rl" -ForegroundColor DarkCyan }
    }
}

$verdict = Get-WwProvisionVerdict -Checks $checks
$connectionStringMasked = Hide-WwConnectionStringPassword -ConnectionString (
    ConvertTo-WwPersistenceConnectionString -ServerFqdn $SqlServerFqdn -DatabaseName $DatabaseName `
        -UserId $ExpectedAdminLogin -Password 'placeholder')

$created = $false
if (-not $CheckOnly -and $verdict.Passed) {
    if (-not $existingDb) {
        Write-Step "Creating database '$DatabaseName' ($skuName, min $MinCapacity vCore, auto-pause ${AutoPauseDelayMinutes}m)..."
        $createArgs = @('sql', 'db', 'create', '--name', $DatabaseName, '--server', $serverName,
                        '--resource-group', $serverRg, '--edition', $Edition, '--family', $Family,
                        '--capacity', "$Capacity", '--compute-model', $ComputeModel,
                        '--min-capacity', "$MinCapacity", '--auto-pause-delay', "$AutoPauseDelayMinutes", '-o', 'json')
        Invoke-ProvisionAz -AzArgs $createArgs | Out-Null
        $created = $true
        Write-Ok "Created '$DatabaseName'."
    } else {
        Write-Note "Database '$DatabaseName' already exists - left exactly as it is."
    }

    if (-not $SkipFirewallRule) {
        $fw2 = Invoke-ProvisionAz -AzArgs @('sql', 'server', 'firewall-rule', 'show', '--name', $script:AzureServicesRuleName,
                                            '--server', $serverName, '--resource-group', $serverRg, '-o', 'json') -AllowFail
        if ($fw2.ExitCode -ne 0) {
            Write-Step "Creating firewall rule '$($script:AzureServicesRuleName)' (0.0.0.0 = allow Azure services)..."
            Invoke-ProvisionAz -AzArgs @('sql', 'server', 'firewall-rule', 'create', '--name', $script:AzureServicesRuleName,
                                         '--server', $serverName, '--resource-group', $serverRg,
                                         '--start-ip-address', '0.0.0.0', '--end-ip-address', '0.0.0.0', '-o', 'json') | Out-Null
            Write-Ok "Created '$($script:AzureServicesRuleName)'."
        }
    }
}

$summary = [pscustomobject]@{
    Timestamp              = (Get-Date).ToUniversalTime().ToString('o')
    Mode                   = $mode
    ServerFqdn             = $SqlServerFqdn
    ServerName             = $serverName
    ResourceGroup          = $serverRg
    Location               = $location
    DatabaseName           = $DatabaseName
    Sku                    = $skuName
    EngineLogin            = $ExpectedAdminLogin
    ConnectionStringMasked = $connectionStringMasked
    DatabaseExisted        = [bool]$existingDb
    DatabaseCreated        = $created
    Verdict                = $verdict
    Checks                 = $checks
}
($summary | ConvertTo-Json -Depth 8) | Set-Content -LiteralPath $SummaryPath -Encoding utf8
Write-Host ''
Write-Host "   summary -> $SummaryPath" -ForegroundColor White
Write-Host "   $($verdict.Message)" -ForegroundColor $(if ($verdict.Passed) { 'Green' } else { 'Red' })

if (-not $verdict.Passed) { throw $verdict.Message }
$summary
