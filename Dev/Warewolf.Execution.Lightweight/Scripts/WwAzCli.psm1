#Requires -Version 7.0
<#
.SYNOPSIS
    Shared az CLI retry-on-transient policy for the Lightweight deployment scripts.

.DESCRIPTION
    ONE definition of "this az failure is transient, wait and try again", used by
    Deploy-WwExecutionEngine.ps1 (Invoke-Az), Setup-ApplicationInsights.ps1,
    Configure-WwExecutionAuth.ps1, Configure-WwExecutionAuth-Clients.ps1 and
    Cleanup-WwExecutionAuth.ps1.  Each script keeps its own wrapper (its own log
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

Export-ModuleMember -Function Test-WwAzTransientError, Invoke-WwAzWithRetry
