#Requires -Version 7.0
<#
.SYNOPSIS
    Shared az CLI retry-on-transient policy for the Lightweight deployment scripts.

.DESCRIPTION
    ONE definition of "this az failure is transient, wait and try again", used by
    Deploy-WwExecutionEngine.ps1 (Invoke-Az), Setup-ApplicationInsights.ps1,
    Configure-WwExecutionAuth.ps1, Configure-WwExecutionAuth-Clients.ps1 and
    Cleanup-WwExecutionAuth.ps1 - and, through Invoke-WwAz, by every inline `az` step in the
    Dev/.azure pipelines (build 30844 lost a whole run to one ARM 503 in such a step).
    Each script keeps its own wrapper (its own log
    format, secret redaction and throw/-AllowFail contract); only the pattern and
    the retry loop live here, so a new transient signature is added in one place.

    The loop does NOT call az itself: the caller passes -Invoke, a script block
    created in the CALLER's scope that runs the az command.  That matters for the
    Pester suites, which shim `az` as a function in the test scope — a module
    function calling `& az` directly would resolve past that shim to the real CLI.
#>

Set-StrictMode -Version Latest

# Patterns that mean "the request never reached its handler, or the handler was
# overloaded".  Deliberately conservative: real errors (BadRequest, Forbidden,
# NotFound, Conflict, validation) must NOT match, so they still fail immediately.
$script:WwAzTransientPattern = (@(
    'Connection aborted',
    'ConnectionResetError',
    '10054',                         # WSAECONNRESET
    '10053',                         # WSAECONNABORTED
    '10060',                         # WSAETIMEDOUT
    'Read timed out',
    'ReadTimeoutError',
    'ConnectTimeoutError',
    'SSLEOFError',
    'SSL.*EOF occurred in violation',
    'Max retries exceeded',
    'temporary failure in name resolution',
    'Could not resolve host',
    'getaddrinfo failed',
    'TooManyRequests',
    '"code":\s*"429"',
    ' 429 ',
    ' 500 Internal',
    ' 502 Bad Gateway',
    ' 503 ',
    'ServiceUnavailable',
    # ARM's front door answers an outage with an HTML page, which az surfaces as
    # "ERROR: Service Unavailable(<!DOCTYPE ...>Our services aren't available right now...".
    # Neither ' 503 ' nor 'ServiceUnavailable' matches that (build 30801).
    'Service Unavailable',
    "services aren't available right now",
    ' 504 Gateway',
    'GatewayTimeout',
    'Gateway Timeout',
    'BadGateway',
    'Bad Gateway'
) -join '|')

$script:WwAzDefaultDelaySeconds = @(3, 7, 15, 30)

function Test-WwAzTransientError {
    <#
        $true when az output carries a known-transient signature (see the pattern above).
    #>
    param([AllowNull()][AllowEmptyString()][string] $Output)
    if ([string]::IsNullOrEmpty($Output)) { return $false }
    return ($Output -match $script:WwAzTransientPattern)
}

function Invoke-WwAzWithRetry {
    <#
        Runs -Invoke (which must run ONE az command and return its merged output) and
        retries it while the failure is transient, up to -MaxAttempts.

        Never throws for an az failure: returns
            [pscustomobject]@{ Output; ExitCode; Attempts; Transient }
        and leaves $global:LASTEXITCODE at the final exit code, so callers keep their
        own contract (throw, -AllowFail, or an existing `$LASTEXITCODE -ne 0` check).

        -OnRetry is called before each wait as & $OnRetry $attempt $MaxAttempts $wait.
        It is given no az output on purpose: that output can echo the arguments,
        and some arguments are secrets.

        Default waits 3+7+15+30 = 55s across 5 attempts before giving up.
    #>
    param(
        [Parameter(Mandatory)][scriptblock] $Invoke,
        [int] $MaxAttempts = 5,
        [int[]] $DelaySeconds = $script:WwAzDefaultDelaySeconds,
        [scriptblock] $OnRetry
    )
    if ($MaxAttempts -lt 1) { $MaxAttempts = 1 }
    if (-not $DelaySeconds -or $DelaySeconds.Count -eq 0) { $DelaySeconds = @(0) }

    $output = $null; $exit = 0; $transient = $false
    for ($attempt = 1; $attempt -le $MaxAttempts; $attempt++) {
        $global:LASTEXITCODE = 0
        $output = & $Invoke
        $exit   = $global:LASTEXITCODE
        if ($null -eq $exit) { $exit = 0 }
        if ($exit -eq 0) {
            return [pscustomobject]@{ Output = $output; ExitCode = 0; Attempts = $attempt; Transient = $false }
        }

        $transient = Test-WwAzTransientError -Output ($output | Out-String)
        if (-not $transient -or $attempt -ge $MaxAttempts) { break }

        $wait = $DelaySeconds[[Math]::Min($attempt - 1, $DelaySeconds.Count - 1)]
        if ($OnRetry) { & $OnRetry $attempt $MaxAttempts $wait }
        if ($wait -gt 0) { Start-Sleep -Seconds $wait }
    }

    $global:LASTEXITCODE = $exit
    return [pscustomobject]@{ Output = $output; ExitCode = $exit; Attempts = $attempt; Transient = $transient }
}

function Get-WwAzExecutable {
    # The real az CLI, never an alias or function of the same name - Invoke-WwAz is normally
    # reached THROUGH an `az` alias, so resolving plain `az` here would recurse. Separate so the
    # Pester suite can mock it.
    $cmd = Get-Command -Name az -CommandType Application -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $cmd) { throw 'az CLI not found on PATH.' }
    return $cmd.Source
}

function Invoke-WwAz {
    <#
        Drop-in replacement for a bare `az ...` call in the pipelines' inline steps:

            Import-Module '<Scripts>\WwAzCli.psm1' -Force
            Set-Alias -Name az -Value Invoke-WwAz -Scope Script

        after which every `az ...` line in the step retries on a transient failure, unchanged.
        Behaves like the bare call it replaces: returns stdout only (so `$x = az ... -o tsv` and
        `az ... | ConvertFrom-Json` see what they always saw), writes the final attempt's stderr
        to the host, and leaves $LASTEXITCODE at the final exit code for the step's own check.

        No param block on purpose: every argument, including --flags, passes through as $args.
        Consequence: a caller's `2>$null` no longer hides az's stderr, because it is written to
        the host rather than the error stream (writing it as an error would throw under the
        steps' $ErrorActionPreference = 'Stop').
    #>
    $exe    = Get-WwAzExecutable
    $azArgs = $args
    # Name the command in retry messages by its leading subcommand words only - never an
    # option or its value, which may be a secret (az login -p ...).
    $label  = @($azArgs | Select-Object -First 3 | ForEach-Object { [string]$_ })
    $cut    = [Array]::FindIndex([string[]]$label, [Predicate[string]] { param($a) $a.StartsWith('-') })
    if ($cut -eq 0) { $label = @() } elseif ($cut -gt 0) { $label = $label[0..($cut - 1)] }
    $label  = (@('az') + $label) -join ' '

    $ErrorActionPreference = 'Continue'   # native stderr must never become terminating here
    $result = Invoke-WwAzWithRetry -Invoke { & $exe @azArgs 2>&1 } -OnRetry {
        param($attempt, $max, $wait)
        Write-Host "##[warning]$label failed transiently (attempt $attempt/$max); retrying in ${wait}s."
    }

    foreach ($item in @($result.Output)) {
        if ($item -is [System.Management.Automation.ErrorRecord]) { Write-Host $item.ToString() }
        else { $item }
    }
    $global:LASTEXITCODE = $result.ExitCode
}

Export-ModuleMember -Function Test-WwAzTransientError, Invoke-WwAzWithRetry, Invoke-WwAz
