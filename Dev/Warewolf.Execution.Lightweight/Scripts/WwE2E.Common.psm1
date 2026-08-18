# Warewolf E2E verification harness - shared helpers.
#
# Extracted so New-WwE2EStaging.ps1, Invoke-WwE2EVerification.ps1 and their Pester tests share ONE
# implementation of the things that are easy to get wrong. Every non-obvious rule below was measured
# on the 2026-08-06 end-to-end run; see docs/Deploy-E2E-Execution-Summary.md for the evidence.

#Requires -Version 7.0
Set-StrictMode -Version Latest

# ═════════════════════════════════════════════════════════════════════════════
# Console output (same glyphs/colours as Deploy-WwQueueProcessor.ps1)
# ═════════════════════════════════════════════════════════════════════════════

function Write-E2EPhase {
    param([Parameter(Mandatory)][string] $Title)
    Write-Host ''
    Write-Host ('=' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('=' * 76) -ForegroundColor Cyan
}

function Write-E2EStep { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-E2EOk   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-E2ENote { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }
function Write-E2EBad  { param([string] $Msg) Write-Host "  [x] $Msg" -ForegroundColor Red }

function Write-E2ECriterion {
    <#
        One acceptance-criterion line. $State is Pass/Fail/Skip/Info.
    #>
    param(
        [Parameter(Mandatory)][int] $Number,
        [Parameter(Mandatory)][string] $Text,
        [ValidateSet('Pass', 'Fail', 'Skip', 'Info')][string] $State = 'Info',
        [string] $Detail
    )
    $glyph, $colour = switch ($State) {
        'Pass' { '[PASS]', 'Green' }
        'Fail' { '[FAIL]', 'Red' }
        'Skip' { '[SKIP]', 'DarkYellow' }
        default { '[ -- ]', 'Gray' }
    }
    Write-Host ("  {0} {1,2}. {2}" -f $glyph, $Number, $Text) -ForegroundColor $colour
    if ($Detail) { Write-Host "          $Detail" -ForegroundColor DarkGray }
}

# ═════════════════════════════════════════════════════════════════════════════
# Azure CLI
# ═════════════════════════════════════════════════════════════════════════════

function Invoke-E2EAzJson {
    <#
        Runs `az ... -o json` and returns the deserialised object.

        DELIBERATELY REFUSES --query. On Windows `az` is a .cmd shim, and cmd.exe mangles JMESPath
        filter/projection syntax: a query containing [?...] or {...} arrives unquoted and fails with
        "].{name:name was unexpected at this time." - even when the PowerShell string itself is
        correctly quoted. Cost several failed calls on the 2026-08-06 run before being pinned down.
        Filter and project in PowerShell instead; it is also easier to read.

        CAPTURES STDERR INSTEAD OF DISCARDING IT. The previous implementation ran `2>$null`, so a
        failure only ever threw "az ... failed with exit code N" - the actual az error text (an
        auth failure, ResourceNotFound, a transient control-plane blip, ...) was thrown away and
        unrecoverable even from the pipeline log. This is exactly what made a live CI failure
        (2026-08-18, `containerapp env show` on an environment that a deploy step 20 minutes
        earlier had just confirmed existed) undiagnosable: the environment was real, so the
        failure was almost certainly transient, but there was no way to tell from "exit code 1"
        alone. Mirrors Deploy-WwQueueProcessor.ps1's Invoke-Az, which already splits stdout from
        stderr for this reason (see its comment) and puts the real text in the throw.

        ALSO RETRIES the same narrow set of transient transport errors Invoke-Az retries -
        connection resets, timeouts, throttling - observed against this subscription. These calls
        hit the identical Azure CLI/network path, so the same flakiness applies. A genuine
        ResourceNotFound does not match the pattern, so -AllowFail probes still return $null on the
        first attempt instead of stalling through retries.
    #>
    param(
        [Parameter(Mandatory)][string[]] $AzArgs,
        [switch] $AllowFail
    )
    if ($AzArgs -contains '--query') {
        throw 'Invoke-E2EAzJson does not accept --query (cmd.exe mangles JMESPath). Filter in PowerShell.'
    }

    $transient = 'Connection aborted|ConnectionResetError|10054|Read timed out|timed out|' +
                 'ServiceUnavailable|Gateway Time-?out|TooManyRequests|Too many requests|' +
                 'ServerTimeout|temporarily unavailable'
    $maxTries = 4

    for ($try = 1; $try -le $maxTries; $try++) {
        $raw = & az @AzArgs -o json 2>&1
        $exit = $LASTEXITCODE
        # stderr (az extension warnings etc) must not reach the returned VALUE - split it out the
        # same way Invoke-Az does, then reassemble only the true stdout lines before parsing.
        $stdout = @($raw | Where-Object { $_ -isnot [System.Management.Automation.ErrorRecord] })

        if ($exit -eq 0) {
            if ($stdout.Count -eq 0) { return $null }
            return ($stdout | ConvertFrom-Json)
        }

        $errText = ($raw | ForEach-Object { "$_" }) -join ' '
        if ($try -lt $maxTries -and $errText -match $transient) {
            Start-Sleep -Seconds (5 * $try)
            continue
        }

        if ($AllowFail) { return $null }
        throw "az $($AzArgs -join ' ') failed with exit code ${exit}: $errText"
    }
}

# ═════════════════════════════════════════════════════════════════════════════
# Resource ledger - what this run created / updated / deleted, and where to reach it
# ═════════════════════════════════════════════════════════════════════════════

$script:E2EResources = [System.Collections.Generic.List[object]]::new()

function Reset-E2EResourceLedger { $script:E2EResources = [System.Collections.Generic.List[object]]::new() }

function Add-E2EResource {
    <#
        Records one resource the run touched. Call it AT THE MOMENT of the change, never in a batch at
        the end - the ledger's whole purpose is to survive a mid-run failure, and a summary that is
        only assembled on the happy path is worthless exactly when it is needed.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('Created','Updated','Deleted','Reused','Failed')][string] $Action,
        [Parameter(Mandatory)][string] $Kind,
        [Parameter(Mandatory)][string] $Name,
        [string] $Scope,
        [string] $Url,
        [string] $Detail
    )
    $script:E2EResources.Add([pscustomobject]@{
        Order  = $script:E2EResources.Count + 1
        Time   = (Get-Date).ToString('HH:mm:ss')
        Action = $Action
        Kind   = $Kind
        Name   = $Name
        Scope  = $Scope
        Url    = $Url
        Detail = $Detail
    })
}

function Get-E2EResourceLedger { , $script:E2EResources }

function Write-E2EResourceSummary {
    <#
        Prints the resource ledger. Designed to be called from a finally block so it appears on
        SUCCESS AND ON FAILURE - a run that dies half-way is precisely when you need to know what it
        already created and how to reach it.
    #>
    [CmdletBinding()]
    param(
        [string] $Title = 'Resources manipulated by this run',
        [switch] $Failed
    )

    Write-E2EPhase $Title
    $items = @($script:E2EResources)
    if ($items.Count -eq 0) {
        Write-E2ENote 'No resources were created, updated or deleted.'
        return
    }

    if ($Failed) {
        Write-E2EBad 'THE RUN DID NOT COMPLETE. Everything below already exists and must be cleaned up before a retry with the same names.'
    }

    $colour = @{ Created = 'Green'; Updated = 'Cyan'; Deleted = 'Yellow'; Reused = 'Gray'; Failed = 'Red' }
    foreach ($group in @('Created','Updated','Deleted','Reused','Failed')) {
        $rows = @($items | Where-Object { $_.Action -eq $group })
        if ($rows.Count -eq 0) { continue }
        Write-Host ''
        Write-Host ("  {0} ({1})" -f $group.ToUpper(), $rows.Count) -ForegroundColor $colour[$group]
        foreach ($r in $rows) {
            Write-Host ("    {0,-22} {1}" -f $r.Kind, $r.Name) -ForegroundColor $colour[$group]
            if ($r.Scope)  { Write-Host ("        scope : {0}" -f $r.Scope)  -ForegroundColor DarkGray }
            if ($r.Detail) { Write-Host ("        {0}" -f $r.Detail)         -ForegroundColor DarkGray }
            if ($r.Url)    { Write-Host ("        URL   : {0}" -f $r.Url)    -ForegroundColor Blue }
        }
    }

    $urls = @($items | Where-Object { $_.Url -and $_.Action -ne 'Deleted' })
    if ($urls.Count -gt 0) {
        Write-Host ''
        Write-Host '  ENDPOINTS' -ForegroundColor Blue
        foreach ($u in $urls) { Write-Host ("    {0,-30} {1}" -f $u.Name, $u.Url) -ForegroundColor Blue }
    }
    Write-Host ''
}

function Invoke-E2EChildScript {
    <#
        Runs a sibling deployment script with StrictMode DISABLED for that call.

        Set-StrictMode is inherited by child scopes, so a caller running `-Version Latest` silently
        imposes it on every script it invokes. The deploy scripts were not written under StrictMode and
        legitimately read properties that may be absent, or `.Count` off a value that can be scalar or
        null. Measured 2026-08-10: this aborted Deploy-WwQueueProcessor.ps1 inside its own preflight
        with "The property 'Count' cannot be found on this object" - after the engine had already been
        deployed, leaving a half-finished run.

        The scriptblock creates a child scope where StrictMode is off; the invoked script inherits THAT.
        $LASTEXITCODE propagates out unchanged, so callers keep checking it as normal.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Path,
        [hashtable] $Arguments = @{}
    )
    if (-not (Test-Path -LiteralPath $Path)) { throw "Script not found: '$Path'" }
    & {
        Set-StrictMode -Off
        & $Path @Arguments
    }
}

function Get-E2EAzContext {
    $a = Invoke-E2EAzJson -AzArgs @('account', 'show')
    if (-not $a) { throw 'Not signed in to Azure. Run: az login' }
    [pscustomobject]@{
        SubscriptionId = $a.id
        Subscription   = $a.name
        TenantId       = $a.tenantId
        User           = $a.user.name
    }
}

function New-E2ERunSuffix {
    <#
        6 lowercase alphanumerics, so two reviewers can run concurrently without colliding.
        Azure Function App and storage names are GLOBALLY unique, and `az containerapp create` on an
        existing name silently UPDATES that app - so a collision would reconfigure someone else's
        deployment rather than fail. The suffix is what makes the run disposable.
    #>
    param([int] $Length = 6)
    $alphabet = 'abcdefghijklmnopqrstuvwxyz0123456789'
    -join (1..$Length | ForEach-Object { $alphabet[(Get-Random -Maximum $alphabet.Length)] })
}

function Get-E2EReplicaCount {
    <#
        Replica count for a Container App. Uses no --query (see Invoke-E2EAzJson).
        NOTE a forced revision restart transiently reports MORE than maxReplicas, because ACA starts
        replacements while the old replicas drain (observed 5 against max=3). That is not a scale-rule
        violation.
    #>
    param(
        [Parameter(Mandatory)][string] $AppName,
        [Parameter(Mandatory)][string] $ResourceGroup
    )
    $r = Invoke-E2EAzJson -AzArgs @('containerapp', 'replica', 'list', '--name', $AppName, '-g', $ResourceGroup) -AllowFail
    return @($r).Count
}

function Get-E2EContainerApps {
    param(
        [Parameter(Mandatory)][string] $ResourceGroup,
        [string] $NamePrefix
    )
    $all = @(Invoke-E2EAzJson -AzArgs @('containerapp', 'list', '-g', $ResourceGroup) -AllowFail)
    if ($NamePrefix) { $all = @($all | Where-Object { $_.name -like "$NamePrefix*" }) }
    # Null-safe throughout. Under Set-StrictMode -Version Latest, reaching through a property that is
    # absent - `identity` on an app with no managed identity, or `scale` on a bare template - THROWS.
    # An earlier version did exactly that and its caller swallowed the error, so a queue-contention
    # check reported "no contention" when it had in fact never run: a false pass, which is worse than
    # the error it hid.
    function Get-Prop { param($Object, [string[]] $Path)
        $cur = $Object
        foreach ($seg in $Path) {
            if ($null -eq $cur) { return $null }
            $prop = $cur.PSObject.Properties[$seg]
            if (-not $prop) { return $null }
            $cur = $prop.Value
        }
        return $cur
    }

    $all | ForEach-Object {
        $containers = @(Get-Prop $_ @('properties','template','containers'))
        [pscustomobject]@{
            Name        = $_.name
            MinReplicas = Get-Prop $_ @('properties','template','scale','minReplicas')
            MaxReplicas = Get-Prop $_ @('properties','template','scale','maxReplicas')
            # Filter nulls: @($null) is a ONE-ELEMENT array containing $null, so a scale block with no
            # rules would otherwise yield a single null "rule" that throws on first property access.
            Rules       = @(Get-Prop $_ @('properties','template','scale','rules') | Where-Object { $null -ne $_ })
            Image       = if ($containers.Count) { Get-Prop $containers[0] @('image') } else { $null }
            Cpu         = if ($containers.Count) { Get-Prop $containers[0] @('resources','cpu') } else { $null }
            PrincipalId = Get-Prop $_ @('identity','principalId')
            Revision    = Get-Prop $_ @('properties','latestRevisionName')
            EnvId       = Get-Prop $_ @('properties','environmentId')
        }
    }
}

function Get-E2EQueueConsumers {
    <#
        Every Container App in the group whose `rabbitmq` scale rule targets one of $QueueName.

        This is the check that catches the most damaging kind of false success: a second app consuming
        the same queue competes for messages, and if it forwards to a stopped or older engine it
        DEAD-LETTERS AND ACKS its share - so the queue still drains, the app still scales back to zero,
        and the run looks clean while half the messages never executed.

        Throws rather than returning empty on failure, so a caller can never mistake "could not check"
        for "nothing found".
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $ResourceGroup,
        [Parameter(Mandatory)][string[]] $QueueName,
        [string] $ExcludeNamePrefix
    )
    $hits = [System.Collections.Generic.List[object]]::new()
    foreach ($app in (Get-E2EContainerApps -ResourceGroup $ResourceGroup)) {
        if ($ExcludeNamePrefix -and $app.Name -like "$ExcludeNamePrefix*") { continue }
        foreach ($rule in @($app.Rules | Where-Object { $null -ne $_ })) {
            $type = $null; $meta = $null
            if ($rule.PSObject.Properties['custom']) {
                $custom = $rule.custom
                if ($custom.PSObject.Properties['type'])     { $type = $custom.type }
                if ($custom.PSObject.Properties['metadata']) { $meta = $custom.metadata }
            }
            if ($type -ne 'rabbitmq' -or -not $meta) { continue }
            $qn = if ($meta.PSObject.Properties['queueName']) { $meta.queueName } else { $null }
            if ($qn -and ($QueueName -contains $qn)) {
                $hits.Add([pscustomobject]@{ App = $app.Name; Queue = $qn; MinReplicas = $app.MinReplicas; MaxReplicas = $app.MaxReplicas })
            }
        }
    }
    return $hits
}

# ═════════════════════════════════════════════════════════════════════════════
# Engine tokens
# ═════════════════════════════════════════════════════════════════════════════

function Get-E2EEngineToken {
    <#
        Acquires a bearer token for the engine's Entra app via CLIENT CREDENTIALS.

        WHY NOT `az account get-access-token --resource api://<appId>`: on a FRESHLY CREATED app
        registration that fails with

            AADSTS65001: The user or administrator has not consented to use the application with ID
            '04b07795-8ddb-461a-bbee-02f9e1bf7b46' named 'Microsoft Azure CLI'

        because the Azure CLI client is not in the new app's preAuthorizedApplications. It only works
        on registrations where consent happened previously, which is why the runbook's documented
        command succeeds on long-lived apps and fails on every first deployment.

        The Easy Auth client secret that Configure-WwExecutionAuth.ps1 stores in the Function App
        setting MICROSOFT_PROVIDER_AUTHENTICATION_SECRET lets the app authenticate AS ITSELF, needing
        no consent and no directory change.

        The resulting token carries NO app roles. That is sufficient for any workflow that resolves
        against the global permission map, because the Public group's server-level Execute is always
        OR'd into the effective permissions - measured: 200 on both 'Hello World' and
        'rabbit/RabbitProcess'.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $TenantId,
        [Parameter(Mandatory)][string] $EngineAppId,
        [string] $ClientSecret,
        [string] $FunctionAppName,
        [string] $ResourceGroup
    )

    if (-not $ClientSecret) {
        if (-not $FunctionAppName -or -not $ResourceGroup) {
            throw 'Get-E2EEngineToken needs -ClientSecret, or -FunctionAppName + -ResourceGroup to read it.'
        }
        $settings = Invoke-E2EAzJson -AzArgs @('functionapp', 'config', 'appsettings', 'list',
                                               '-n', $FunctionAppName, '-g', $ResourceGroup)
        $ClientSecret = ($settings | Where-Object { $_.name -eq 'MICROSOFT_PROVIDER_AUTHENTICATION_SECRET' }).value
        if (-not $ClientSecret) {
            throw "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET not found on '$FunctionAppName'. Was Easy Auth provisioned?"
        }
    }

    $body = @{
        grant_type    = 'client_credentials'
        client_id     = $EngineAppId
        client_secret = $ClientSecret
        scope         = "api://$EngineAppId/.default"
    }
    $resp = Invoke-RestMethod -Method Post -ContentType 'application/x-www-form-urlencoded' `
        -Uri "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token" -Body $body
    return $resp.access_token
}

function Invoke-E2EEngineRoute {
    <#
        Calls an engine route and returns status + body, never throwing on a non-2xx: the E2E checks
        depend on reading 401/500 deliberately.

        There is NO /api prefix - host.json sets extensions.http.routePrefix to "". Verified live:
        /Public/Hello World.json -> 200, /api/Public/Hello World.json -> 404.
    #>
    param(
        [Parameter(Mandatory)][string] $BaseUrl,
        [Parameter(Mandatory)][string] $Route,
        [string] $Token,
        [int] $TimeoutSec = 150,
        # Treat a client-side timeout as success-with-unknown-result instead of throwing. Used when the
        # call is deliberately fire-and-forget - e.g. publishing a large burst that must still be
        # in flight when the drain test restarts the revision. The engine COMPLETES the workflow
        # server-side after the client disconnects (measured 2026-08-06: a publish whose client was
        # killed still delivered all 36 messages), so abandoning the response loses nothing.
        [switch] $IgnoreTimeout
    )
    $headers = @{}
    if ($Token) { $headers['Authorization'] = "Bearer $Token" }
    try {
        $r = Invoke-WebRequest "$($BaseUrl.TrimEnd('/'))/$($Route.TrimStart('/'))" `
            -Headers $headers -SkipHttpErrorCheck -TimeoutSec $TimeoutSec
    } catch [System.Net.Http.HttpRequestException], [System.Threading.Tasks.TaskCanceledException],
            [System.OperationCanceledException] {
        if (-not $IgnoreTimeout) { throw }
        return [pscustomobject]@{ StatusCode = 0; Body = ''; Compact = "(client timeout after ${TimeoutSec}s - request abandoned deliberately; the engine continues server-side)"; TimedOut = $true }
    } catch {
        # Invoke-WebRequest surfaces a timeout as a plain RuntimeException in some hosts, so match on text.
        if ($IgnoreTimeout -and $_.Exception.Message -match 'Timeout|canceled|cancelled') {
            return [pscustomobject]@{ StatusCode = 0; Body = ''; Compact = "(client timeout after ${TimeoutSec}s - request abandoned deliberately; the engine continues server-side)"; TimedOut = $true }
        }
        throw
    }
    [pscustomobject]@{
        StatusCode = [int]$r.StatusCode
        Body       = [string]$r.Content
        Compact    = ([string]$r.Content -replace '\s+', ' ').Trim()
        TimedOut   = $false
    }
}

function Get-E2EWorkflowRoute {
    <#
        Turns a trigger's WorkflowName ('rabbit\RabbitProcess') into an escaped route segment
        ('rabbit/RabbitProcess'), escaping each segment separately so a workflow in a folder - or one
        with a space, like 'Hello World' - resolves.
    #>
    param([Parameter(Mandatory)][string] $WorkflowName)
    $parts = ($WorkflowName -replace '\\', '/') -split '/'
    ($parts | ForEach-Object { [uri]::EscapeDataString($_) }) -join '/'
}

# ═════════════════════════════════════════════════════════════════════════════
# RabbitMQ (AMQP) - topology + depth
# ═════════════════════════════════════════════════════════════════════════════

$script:RabbitLoaded = $false

function Initialize-E2ERabbitClient {
    <#
        Loads RabbitMQ.Client out of a QueueProcessor publish directory.

        THE TRAP: RabbitMQ.Client 7.x needs System.Threading.RateLimiting, which is NOT in the worker
        publish output - and that absence is CORRECT, because the assembly ships in the
        Microsoft.AspNetCore.App shared framework that the container's aspnet:8.0 base image provides.
        PowerShell only loads the BASE runtime, so the dependency has to be sourced from the installed
        shared framework. Without it the failure is thoroughly misleading:

            BrokerUnreachableException: None of the specified endpoints were reachable
              -> FileNotFoundException: Could not load ... 'System.Threading.RateLimiting'

        which reads as a dead broker rather than a missing DLL.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $PublishPath)

    if ($script:RabbitLoaded) { return }

    $dll = Join-Path $PublishPath 'RabbitMQ.Client.dll'
    if (-not (Test-Path -LiteralPath $dll)) {
        throw "RabbitMQ.Client.dll not found in '$PublishPath'. Publish Warewolf.Execution.QueueProcessor first."
    }

    $aspNetRoot = 'C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App'
    $aspNet = $null
    if (Test-Path -LiteralPath $aspNetRoot) {
        $aspNet = (Get-ChildItem $aspNetRoot -Directory -ErrorAction SilentlyContinue |
                   Where-Object Name -like '8.*' | Sort-Object Name | Select-Object -Last 1).FullName
    }
    if (-not $aspNet) {
        throw ("Microsoft.AspNetCore.App 8.x shared framework not found under '$aspNetRoot'. " +
               'It supplies System.Threading.RateLimiting, which RabbitMQ.Client 7.x requires.')
    }

    $probeDirs = @($PublishPath, $aspNet)
    [System.Runtime.Loader.AssemblyLoadContext]::Default.add_Resolving({
        param($ctx, $name)
        foreach ($d in $probeDirs) {
            $candidate = Join-Path $d "$($name.Name).dll"
            if (Test-Path -LiteralPath $candidate) { return $ctx.LoadFromAssemblyPath($candidate) }
        }
        return $null
    }.GetNewClosure())

    Add-Type -Path (Join-Path $aspNet 'System.Threading.RateLimiting.dll')
    Add-Type -Path $dll
    $script:RabbitLoaded = $true
}

function New-E2EChannel {
    <#
        Opens a channel on an existing connection.

        IChannel.CreateChannelAsync REQUIRES a CreateChannelOptions argument, but that type does not
        expose a static ::Default in every 7.x build. Without Set-StrictMode a missing static silently
        evaluates to $null - which happens to work, because the client accepts a null options object -
        so ad-hoc scripts appear fine while this module (StrictMode Latest) throws
        "The property 'Default' cannot be found on this object". Resolve it reflectively and fall back
        to $null so both shapes work.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)] $Connection, $CancellationToken)

    if ($null -eq $CancellationToken) { $CancellationToken = [System.Threading.CancellationToken]::None }
    $opts = $null
    $prop = [RabbitMQ.Client.CreateChannelOptions].GetProperty('Default', [System.Reflection.BindingFlags]'Public,Static')
    if ($prop) { $opts = $prop.GetValue($null) }
    return $Connection.CreateChannelAsync($opts, $CancellationToken).GetAwaiter().GetResult()
}

function New-E2EBrokerSession {
    <#
        Opens a connection + channel. Returns an object with Connection/Channel/Ct; call
        Close-E2EBrokerSession when done (IChannel.CloseAsync has no single-argument overload, so the
        session disposes the connection instead).

        -AmqpUri accepts amqp://user:pass@host:port/vhost. An empty path means the default vhost '/'.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $AmqpUri,
        [string] $ClientName = 'wwqp-e2e-harness'
    )
    $uri = [Uri]$AmqpUri
    $userInfo = $uri.UserInfo.Split(':', 2)
    if ($userInfo.Count -lt 2) { throw 'AmqpUri must include user:password.' }

    $vhost = if ($uri.AbsolutePath -in @('', '/')) { '/' } else { $uri.AbsolutePath.TrimStart('/') }

    $factory = [RabbitMQ.Client.ConnectionFactory]::new()
    $factory.HostName    = $uri.Host
    $factory.Port        = $uri.Port
    $factory.UserName    = $userInfo[0]
    $factory.Password    = $userInfo[1]
    $factory.VirtualHost = $vhost

    $ct = [System.Threading.CancellationToken]::None
    $conn = $factory.CreateConnectionAsync($ClientName, $ct).GetAwaiter().GetResult()
    $chan = New-E2EChannel -Connection $conn -CancellationToken $ct

    [pscustomobject]@{
        Connection = $conn
        Channel    = $chan
        Ct         = $ct
        Host       = $uri.Host
        Port       = $uri.Port
        User       = $userInfo[0]
        VirtualHost= $vhost
        Describe   = "amqp://$($userInfo[0])@$($uri.Host):$($uri.Port)/"   # never includes the password
    }
}

function Close-E2EBrokerSession {
    param([Parameter(Mandatory)] $Session)
    if ($Session -and $Session.Connection) { $Session.Connection.Dispose() }
}

function Initialize-E2EQueueTopology {
    <#
        Declares the exchange + queue + binding a Warewolf queue-trigger needs.

        WHY THIS EXISTS AT ALL: PublishRabbitMQActivity cannot create them. It calls
        ExchangeDeclarePassive first; a 404 CLOSES the channel, and the catch block then issues the
        ACTIVE declare on that same dead channel, throwing AlreadyClosedException. The queue declare
        below it has the identical flaw. So a first publish to a new queue fails with

            Already closed: ... code=404, text='NOT_FOUND - no exchange '<queue>' in vhost '/''

        and nothing is created. (RabbitMqDeadLetterPublisher.cs names this activity as the broken
        reference it deliberately avoids copying.) Tracked as S11.

        SHAPE: the exchange is DIRECT and named after the queue, and the binding uses an EMPTY routing
        key, because PublishRabbitMQActivity publishes BasicPublish(exchange: <queueName>,
        routingKey: ""). Once both passive declares succeed it sets newExchangeOrQueue = false and
        never calls QueueBind - so an exchange+queue WITHOUT the binding silently discards every
        message. The binding is mandatory, not optional.

        DURABILITY: non-durable by default, matching PublishRabbitMQActivity's IsDurable=False, so the
        objects vanish on a broker restart and a trigger declaring Durable=true still consumes them -
        the message pump performs no QueueDeclare, so it asserts nothing.

        Dead-letter queues need NO exchange or binding: RabbitMqDeadLetterPublisher publishes to the
        default exchange ("") with routingKey = the DLQ name, and creates the DLQ itself, passive-first
        on a fresh channel.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Session,
        [Parameter(Mandatory)][string] $QueueName,
        [bool] $Durable = $false
    )
    $ch = $Session.Channel
    $ct = $Session.Ct

    $ch.ExchangeDeclareAsync($QueueName, 'direct', $Durable, $false, $null, $false, $false, $ct).GetAwaiter().GetResult() | Out-Null
    $ok = $ch.QueueDeclareAsync($QueueName, $Durable, $false, $false, $null, $false, $false, $ct).GetAwaiter().GetResult()
    $ch.QueueBindAsync($QueueName, $QueueName, '', $null, $false, $ct).GetAwaiter().GetResult() | Out-Null

    [pscustomobject]@{
        Queue        = $QueueName
        Exchange     = $QueueName
        RoutingKey   = ''
        Durable      = $Durable
        MessageCount = $ok.MessageCount
    }
}

function Get-E2EQueueDepth {
    <#
        Passive queue declare -> ready-message and consumer counts, or $null when the queue is absent.

        MessageCount is READY messages only. Measured on 2026-08-06: publish -> 1; hold the message
        unacked -> 0; Nack(requeue) -> 1. This is exactly the number the KEDA rabbitmq scaler reads
        with mode=QueueLength protocol=amqp, so IN-FLIGHT WORK IS INVISIBLE TO THE SCALER and a
        long-running message can be scaled away underneath itself. The drain path is the only
        protection; keep EngineTimeout <= ShutdownGrace < TerminationGracePeriod.

        A failed passive declare closes the channel, so each probe runs on a FRESH channel.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Session,
        [Parameter(Mandatory)][string] $QueueName
    )
    $ct = $Session.Ct
    $ch = New-E2EChannel -Connection $Session.Connection -CancellationToken $ct
    try {
        $ok = $ch.QueueDeclarePassiveAsync($QueueName, $ct).GetAwaiter().GetResult()
        return [pscustomobject]@{ Queue = $QueueName; Exists = $true; Messages = [int]$ok.MessageCount; Consumers = [int]$ok.ConsumerCount }
    } catch {
        return [pscustomobject]@{ Queue = $QueueName; Exists = $false; Messages = 0; Consumers = 0 }
    }
}

function Test-E2EExchangeExists {
    param(
        [Parameter(Mandatory)] $Session,
        [Parameter(Mandatory)][string] $ExchangeName
    )
    $ct = $Session.Ct
    $ch = New-E2EChannel -Connection $Session.Connection -CancellationToken $ct
    try { $ch.ExchangeDeclarePassiveAsync($ExchangeName, $ct).GetAwaiter().GetResult() | Out-Null; return $true }
    catch { return $false }
}

function Test-E2EUnackedVisibility {
    <#
        Measures whether the scaler's data source counts unacked messages, on a THROWAWAY queue so no
        worker competes for the probe message. Returns 'ReadyOnly' or 'ReadyPlusUnacked'.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Session,
        [string] $ProbeQueue = 'wwe2e-unacked-probe'
    )
    $ct = $Session.Ct
    $ch = $Session.Channel
    $ch.QueueDeclareAsync($ProbeQueue, $false, $false, $false, $null, $false, $false, $ct).GetAwaiter().GetResult() | Out-Null
    try {
        $props = [RabbitMQ.Client.BasicProperties]::new()
        $body  = [ReadOnlyMemory[byte]][Text.Encoding]::UTF8.GetBytes('probe')
        $ch.BasicPublishAsync('', $ProbeQueue, $false, $props, $body, $ct).GetAwaiter().GetResult() | Out-Null
        Start-Sleep -Seconds 2

        $before = (Get-E2EQueueDepth -Session $Session -QueueName $ProbeQueue).Messages
        $got = $ch.BasicGetAsync($ProbeQueue, $false, $ct).GetAwaiter().GetResult()   # autoAck=false -> unacked
        if (-not $got) { throw "Probe message was not retrievable from '$ProbeQueue'." }
        Start-Sleep -Seconds 2
        $during = (Get-E2EQueueDepth -Session $Session -QueueName $ProbeQueue).Messages

        $ch.BasicNackAsync($got.DeliveryTag, $false, $true, $ct).GetAwaiter().GetResult() | Out-Null
        Start-Sleep -Seconds 2
        $after = (Get-E2EQueueDepth -Session $Session -QueueName $ProbeQueue).Messages

        [pscustomobject]@{
            Verdict       = if ($during -eq 0) { 'ReadyOnly' } else { 'ReadyPlusUnacked' }
            BeforeConsume = $before
            WhileUnacked  = $during
            AfterNack     = $after
        }
    } finally {
        try { $ch.QueueDeleteAsync($ProbeQueue, $false, $false, $false, $ct).GetAwaiter().GetResult() | Out-Null } catch { }
    }
}

function Remove-E2EQueueTopology {
    <#
        Deletes a queue, its same-named exchange, and optionally its dead-letter queue.
        NOTE the arities: QueueDeleteAsync(queue, ifUnused, ifEmpty, noWait, ct) takes FIVE arguments
        and ExchangeDeleteAsync(exchange, ifUnused, noWait, ct) takes FOUR - both differ from the
        obvious guess, and a wrong arity fails at teardown time when it is least welcome.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)] $Session,
        [Parameter(Mandatory)][string] $QueueName,
        [string] $DeadLetterQueue
    )
    $ch = $Session.Channel
    $ct = $Session.Ct
    $removed = [System.Collections.Generic.List[string]]::new()

    foreach ($q in @($QueueName, $DeadLetterQueue) | Where-Object { $_ }) {
        try { $ch.QueueDeleteAsync($q, $false, $false, $false, $ct).GetAwaiter().GetResult() | Out-Null; $removed.Add("queue:$q") } catch { }
    }
    try { $ch.ExchangeDeleteAsync($QueueName, $false, $false, $ct).GetAwaiter().GetResult() | Out-Null; $removed.Add("exchange:$QueueName") } catch { }
    return $removed
}

function Resolve-E2EBrokerUri {
    <#
        Derives an amqp:// URI from a RabbitMQ source .bite, for the harness's own broker-side checks
        (topology, queue depth, the unacked probe). Returns $null when the value cannot be read here.

        Three encodings appear in the wild:

          PLAINTEXT  HostName=..;Port=..;UserName=..;Password=..;VirtualHost=/     -> parsed directly
          DPAPI      base64 blob starting 'AQAAANCMnd8B'                            -> unprotected locally
          WFAES      'WFAES::...'                                                  -> NOT attempted here

        DPAPI is Windows- and machine/user-scoped, so it only decrypts on the machine that created it -
        which is also why such a source can never be read inside the Linux worker and must be converted
        to WFAES at deploy time. Note the DPAPI payload is UTF-16, not UTF-8: decoding it as UTF-8
        yields text with a NUL between every character.

        WFAES is deliberately not decrypted: that needs the Key Vault key, and Encrypt-Config.ps1 owns
        that path. Callers should treat $null as "supply -AmqpUri explicitly".
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $SourceBitePath)

    if (-not (Test-Path -LiteralPath $SourceBitePath)) { return $null }
    try { $xml = [xml](Get-Content -LiteralPath $SourceBitePath -Raw) } catch { return $null }
    $cs = $xml.Source.ConnectionString
    if (-not $cs) { return $null }

    if ($cs.StartsWith('WFAES::', [StringComparison]::OrdinalIgnoreCase)) { return $null }

    if ($cs -notmatch '(?i)HostName=') {
        # Assume DPAPI: base64 blob. CurrentUser scope first, then LocalMachine.
        $plain = $null
        foreach ($scope in @('CurrentUser', 'LocalMachine')) {
            try {
                $bytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
                    [Convert]::FromBase64String($cs), $null,
                    [System.Security.Cryptography.DataProtectionScope]::$scope)
                $plain = [Text.Encoding]::Unicode.GetString($bytes)     # UTF-16, not UTF-8
                if ($plain -notmatch '(?i)HostName=') { $plain = [Text.Encoding]::UTF8.GetString($bytes) }
                break
            } catch { continue }
        }
        if (-not $plain) { return $null }
        $cs = $plain
    }

    $parts = @{}
    foreach ($kv in ($cs -split ';')) {
        if ($kv -match '^\s*([^=]+)=(.*)$') { $parts[$Matches[1].Trim()] = $Matches[2].Trim() }
    }
    if (-not $parts['HostName']) { return $null }

    $port  = if ($parts['Port']) { $parts['Port'] } else { '5672' }
    $vhost = if ($parts['VirtualHost'] -and $parts['VirtualHost'] -ne '/') { $parts['VirtualHost'] } else { '' }
    $user  = [uri]::EscapeDataString(($parts['UserName'] ?? ''))
    $pass  = [uri]::EscapeDataString(($parts['Password'] ?? ''))
    return "amqp://${user}:${pass}@$($parts['HostName']):$port/$vhost"
}

# ═════════════════════════════════════════════════════════════════════════════
# Trigger files
# ═════════════════════════════════════════════════════════════════════════════

function Read-E2ETrigger {
    <#
        Parses a queue-trigger .bite and surfaces the fields that drive deployment and scaling.
        Prefetch of 0/blank/invalid is coerced to 1, matching the worker.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Path)

    $t = Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json
    $prefetch = 1
    if ($null -ne $t.PSObject.Properties['Prefetch'] -and [int]$t.Prefetch -ge 1) { $prefetch = [int]$t.Prefetch }

    $durable = $false
    $opt = $t.Options | Where-Object { $_.Name -eq 'Durable' } | Select-Object -First 1
    if ($opt) { $durable = [bool]$opt.Value }

    $dlqDurable = $false
    $dopt = $t.DeadLetterOptions | Where-Object { $_.Name -eq 'Durable' } | Select-Object -First 1
    if ($dopt) { $dlqDurable = [bool]$dopt.Value }

    $firstInput = $t.Inputs | Select-Object -First 1

    [pscustomobject]@{
        Path            = $Path
        File            = Split-Path $Path -Leaf
        TriggerId       = $t.TriggerId
        Name            = $t.Name
        QueueName       = $t.QueueName
        WorkflowName    = $t.WorkflowName
        Concurrency     = [int]$t.Concurrency      # -> maxReplicas
        Prefetch        = $prefetch
        QueueSourceId   = $t.QueueSourceId
        QueueSinkId     = $t.QueueSinkId
        DeadLetterQueue = $t.DeadLetterQueue
        Durable         = $durable
        DlqDurable      = $dlqDurable
        MapEntireMessage= [bool]$t.MapEntireMessage
        FirstInputName  = if ($firstInput) { $firstInput.Name } else { $null }
    }
}

function Test-E2ETriggerSupported {
    <#
        Flags trigger shapes that the harness cannot verify end to end.

        An '@'-prefixed single input with MapEntireMessage makes the forwarder POST
        multipart/form-data. Whether that binds depends on the ENGINE BUILD: multipart support was
        added to WorkflowFunctionHelper.ParseMultipartAsync, and the worker's old startup guard for
        this shape was removed at the same time. A worker pointed at an engine built BEFORE that change
        dead-letters every such message while appearing to process normally, and nothing detects it at
        runtime. Treated as a warning, not a hard failure, because it is a property of the deployed
        engine rather than of the trigger.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)] $Trigger)

    $warnings = [System.Collections.Generic.List[string]]::new()

    if ($Trigger.MapEntireMessage -and $Trigger.FirstInputName -and $Trigger.FirstInputName.StartsWith('@')) {
        $warnings.Add(("Trigger '{0}' maps the entire message to the '@'-prefixed input '{1}', which is sent as " +
                       'multipart/form-data. Verify the TARGET ENGINE binds multipart, or every message will be ' +
                       'dead-lettered while appearing to process normally.') -f $Trigger.Name, $Trigger.FirstInputName)
    }
    if ($Trigger.Prefetch -gt 1 -and $Trigger.Concurrency -gt 0) {
        $warnings.Add(("Trigger '{0}' has Prefetch={1}: {2} message(s) park inside one replica where the scaler " +
                       'cannot see them, delaying scale-out and widening the redelivery window on drain. ' +
                       'Prefetch is settable ONLY in the trigger .bite.') -f $Trigger.Name, $Trigger.Prefetch, ($Trigger.Prefetch - 1))
    }
    if ($Trigger.Concurrency -le 0) {
        $warnings.Add("Trigger '$($Trigger.Name)' has Concurrency=0, which means DISABLED: it deploys as min=max=0 with no scale rule.")
    }
    return $warnings
}

# ═════════════════════════════════════════════════════════════════════════════
# Evidence - Log Analytics
# ═════════════════════════════════════════════════════════════════════════════

function Invoke-E2ELogAnalytics {
    <#
        Runs a KQL query against the ACA environment's workspace.

        Use SINGLE quotes for string literals inside the KQL. Double quotes survive PowerShell but are
        mangled on the way through az.cmd, which surfaces as a JSON parse failure rather than a query
        error.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $WorkspaceCustomerId,
        [Parameter(Mandatory)][string] $Query
    )

    # FLATTEN TO ONE LINE. A multi-line query passed through the Windows `az` .cmd shim loses every
    # line after the first, and - because the first line is the table name - the call SUCCEEDS while
    # silently returning the whole unfiltered table. Measured 2026-08-10 on the same query:
    #   multi-line -> 5782 rows with all 23 raw columns (every pipeline stage discarded)
    #   one line   -> 2 rows with the expected summarize columns
    # That failure mode is worse than an error: `summarize` columns come back absent, so counts read as
    # zero, while a `| where ... | project Log_s` query happily matches across the ENTIRE table and
    # inflates its result. Same root cause as the --query JMESPath mangling in Invoke-E2EAzJson.
    #
    # KQL is whitespace-insensitive, so joining lines is safe - but `//` line comments would swallow
    # the remainder, so they must not appear in any query passed here.
    if ($Query -match '(?m)^\s*//') { throw 'KQL passed to Invoke-E2ELogAnalytics must not contain // comments; they break when the query is flattened to one line.' }
    $flat = (($Query -split "`r?`n" | ForEach-Object { $_.Trim() } | Where-Object { $_ }) -join ' ')

    Invoke-E2EAzJson -AzArgs @('monitor', 'log-analytics', 'query', '--workspace', $WorkspaceCustomerId,
                               '--analytics-query', $flat) -AllowFail
}

function Get-E2EExecutionStats {
    <#
        Per-app execution counts and the median duration T, from the CONTAINER logs.

        NOT from App Insights: the worker's Dev2Logger lines ('Queue execution succeeded',
        'durationMs=', 'Drain of ...') never reach App Insights `traces` - only the HttpClient
        DEPENDENCY telemetry does. Measured: the traces query returned 0 rows while
        ContainerAppConsoleLogs_CL returned all 42 executions. Log Analytics also spans EVERY replica,
        which a single-replica `az containerapp logs show` cannot. Tracked as S12.

        DistinctBodies is the loss/duplication check: executions == distinctBodies means every message
        ran exactly once. A drained queue alone proves nothing, because the worker's contract is
        2xx -> ack and non-2xx -> dead-letter AND ack, so both outcomes drain the queue.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $WorkspaceCustomerId,
        [Parameter(Mandatory)][string] $AppNamePrefix,
        [int] $LookbackHours = 4
    )
    $q = @"
ContainerAppConsoleLogs_CL
| where TimeGenerated > ago(${LookbackHours}h)
| where ContainerAppName_s startswith '$AppNamePrefix'
| extend d = toint(extract('durationMs=([0-9]+)', 1, Log_s))
| extend body = extract('body=(.+)`$', 1, Log_s)
| summarize starting=countif(Log_s has 'Queue execution starting'),
            succeeded=countif(Log_s has 'Queue execution succeeded'),
            failed=countif(Log_s has 'Queue execution failed'),
            deadLettered=countif(Log_s has 'Dead-lettered'),
            preconditionFailed=countif(Log_s has 'PRECONDITION_FAILED'),
            medianMs=percentile(d, 50), minMs=min(d), maxMs=max(d),
            distinctBodies=dcountif(body, isnotempty(body))
  by ContainerAppName_s
"@
    Invoke-E2ELogAnalytics -WorkspaceCustomerId $WorkspaceCustomerId -Query $q
}

function Get-E2EStartupEvidence {
    <#
        The cold-start lines that prove the workload actually ran.

        `healthState: Healthy` at minReplicas=0 proves NOTHING - no container has started, so nothing
        about the image has been exercised. Three separate startup failures hid behind a Healthy
        revision during this migration: exit 150 (runtime:8.0 base image lacking
        Microsoft.AspNetCore.App), exit 2 (Settings/ staged then never added to the build context), and
        'no such file or directory' (a Windows publish has no Linux apphost).

        The catalog line MUST precede the loader line: sources are read once at startup and cached.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $WorkspaceCustomerId,
        [Parameter(Mandatory)][string] $AppNamePrefix,
        [int] $LookbackHours = 2
    )
    $q = @"
ContainerAppConsoleLogs_CL
| where TimeGenerated > ago(${LookbackHours}h)
| where ContainerAppName_s startswith '$AppNamePrefix'
| where Log_s has_any ('Source catalog cached', 'loading triggers from', 'resolved trigger',
                       'Consuming queue', 'ENGINE__TENANTID', 'PRECONDITION_FAILED',
                       'Drain of', 'Cancelled consumer', 'still in flight', 'NOT using TLS')
| project TimeGenerated, ContainerAppName_s, Log_s
| order by TimeGenerated asc
"@
    Invoke-E2ELogAnalytics -WorkspaceCustomerId $WorkspaceCustomerId -Query $q
}

function Get-E2EEngineDependencies {
    <#
        App Insights `dependencies` - the authoritative record of the worker's calls INTO the engine,
        with result codes. This is the check that distinguishes success from dead-lettering, which a
        drained queue cannot.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $AppInsightsName,
        [Parameter(Mandatory)][string] $ResourceGroup,
        [int] $LookbackHours = 4
    )
    $q = "dependencies | where timestamp > ago(${LookbackHours}h) | where name has 'json' " +
         '| summarize calls=count() by resultCode, tostring(success), name | order by name asc'
    Invoke-E2EAzJson -AzArgs @('monitor', 'app-insights', 'query', '--app', $AppInsightsName,
                               '-g', $ResourceGroup, '--analytics-query', $q) -AllowFail
}

Export-ModuleMember -Function `
    New-E2EChannel, Invoke-E2EChildScript, `
    Add-E2EResource, Get-E2EResourceLedger, Write-E2EResourceSummary, Reset-E2EResourceLedger, `
    Write-E2EPhase, Write-E2EStep, Write-E2EOk, Write-E2ENote, Write-E2EBad, Write-E2ECriterion, `
    Invoke-E2EAzJson, Get-E2EAzContext, New-E2ERunSuffix, Get-E2EReplicaCount, Get-E2EContainerApps, `
    Get-E2EQueueConsumers, `
    Get-E2EEngineToken, Invoke-E2EEngineRoute, Get-E2EWorkflowRoute, `
    Initialize-E2ERabbitClient, New-E2EBrokerSession, Close-E2EBrokerSession, `
    Initialize-E2EQueueTopology, Get-E2EQueueDepth, Test-E2EExchangeExists, `
    Test-E2EUnackedVisibility, Remove-E2EQueueTopology, Resolve-E2EBrokerUri, `
    Read-E2ETrigger, Test-E2ETriggerSupported, `
    Invoke-E2ELogAnalytics, Get-E2EExecutionStats, Get-E2EStartupEvidence, Get-E2EEngineDependencies

