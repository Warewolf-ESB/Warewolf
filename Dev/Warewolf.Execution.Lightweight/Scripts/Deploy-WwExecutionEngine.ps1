#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end deployment orchestrator for the Warewolf Workflow Execution
    Engine Azure Function App (Warewolf.Execution.Lightweight).

.DESCRIPTION
    A THIN ORCHESTRATOR.  It provisions Azure infrastructure, configures auth,
    stages + encrypts the package contents, applies the engine's environment
    variables, and deploys an ALREADY-PUBLISHED package to the Function App.

    It does NOT build/publish the project.  You publish once yourself, e.g.:

        dotnet publish Warewolf.Execution.Lightweight.csproj -c Release -o D:\ExecutionEngine\Publish

    then point this script at that folder (or a .zip of it) via -PublishPath.

    Flow:
        Phase 0    Pre-flight   - az CLI + login, resolve subscription/tenant
        Phase 0.5  PLAN         - resolve EVERY decision (targeting, publish source,
                                  feature toggles, all env-var values, secure.config
                                  classification, encryption/Key Vault, Elasticsearch),
                                  print a full masked summary, and ask once to proceed
                                  BEFORE anything is created or changed
        Phase 1    Infra        - resource group, storage, Function App, App Insights
        Phase 2    Auth         - Entra ID + Easy Auth (Configure-WwExecutionAuth.ps1)
        Phase 3    Stage        - secure.config (validate / auto-encrypt plaintext),
                                  workflow resources (+ optional WFAES encryption),
                                  Elasticsearch source (+ WFAES encryption),
                                  environment variables
        Phase 4    Deploy       - publish the package dir to the Function App
                                  (func, falling back to az zip-deploy)
        Phase 5    Verify       - endpoint banner + optional HTTP probe

    A timestamped transcript log and a masked *.summary.json are written for
    every real (non -DryRun) run.

    Inputs follow a "params first, prompt if missing" model.  REQUIRED targeting
    parameters have NO defaults (breaking change — see NOTES).

.PARAMETER SubscriptionId
    Azure subscription GUID.  Resolved from `az account show` when omitted.

.PARAMETER TenantId
    Microsoft Entra tenant GUID.  Resolved from `az account show` when omitted.

.PARAMETER ResourceGroup
    Resource group to create / use.  REQUIRED (no default).

.PARAMETER Location
    Azure region (e.g. southafricanorth).  REQUIRED (no default).

.PARAMETER StorageAccount
    Storage account name (3-24 lowercase chars).  REQUIRED (no default).

.PARAMETER AppName
    Function App name (globally unique).  REQUIRED (no default).

.PARAMETER PublishPath
    Path to the ALREADY-PUBLISHED package: either a folder, or a .zip of the
    publish output (extracted to a sibling folder, which becomes the package
    directory).  REQUIRED (no default).

.PARAMETER PublishMethod
    Auto (default) | Zip | Func.  Auto and Zip both use az zip-deploy (config-zip),
    the correct method for the PRE-BUILT publish artifact this orchestrator deploys.
    Func uses 'func azure functionapp publish --dotnet-isolated --no-build' and is
    ADVANCED/opt-in only — 'func' expects a project source directory and fails on a
    pre-built package ("Can't determine project language… Worker runtime 'None'").

.PARAMETER AppInsightsName
    Application Insights resource name.  Default: <AppName>-ai.

.PARAMETER EnableAppInsights
    Provision Application Insights and enable worker telemetry
    (ENABLEAPPLICATIONINSIGHTS=true + WAREWOLF_APPINSIGHTS_CONNECTION_STRING).

.PARAMETER SkipAuthProvisioning
    Skip Entra ID + Easy Auth provisioning.

.PARAMETER AuthConfigPath
    JSON file describing GroupPermissions and UserAssignments for
    Configure-WwExecutionAuth.ps1.

.PARAMETER SecureConfigPath
    Path to the secure.config to deploy.  An already-AES-encrypted file is
    validated (must be decryptable by the engine) and staged as-is; a PLAINTEXT
    JSON file is validated then AES-encrypted automatically before staging.

.PARAMETER WorkflowsSourcePath
    Folder of workflow .bite resource files, staged into <PublishDir>/Resources.

.PARAMETER EncryptResources
    OPTIONAL — default FALSE.  When set, this run encrypts ALL deployable sources
    (workflow .bite ConnectionStrings, the Elasticsearch source, and any others)
    from plain/DPAPI to WFAES via the Key Vault key.  Encrypt ONCE: on subsequent
    deploys leave this off — the sources are already encrypted and are staged
    as-is.  (You must still pass -KeyVaultName/-KeyVaultSecretName for already-
    encrypted sources so the engine can decrypt them at runtime — see below.)

.PARAMETER VerifyDecryption
    OPTIONAL — default FALSE.  After encryption, verify IN MEMORY that the engine's
    Key Vault key decrypts every value (no plaintext is written to disk).  Only
    meaningful together with -EncryptResources.

.PARAMETER KeyVaultName
    Key Vault the engine uses to decrypt WFAES sources at runtime.  REQUIRED when
    -EncryptResources is set, and also whenever your deployed sources are already
    WFAES-encrypted (so the runtime app settings + managed-identity role are wired).

.PARAMETER KeyVaultSecretName
    Key Vault secret holding the AES key material.  REQUIRED whenever -KeyVaultName
    is in play (no default — you must name the secret explicitly).

.PARAMETER GenerateNewKey
    Generate a fresh AES key (Encrypt-Config -GenerateKeys); implied for a new vault.

.PARAMETER EnableElasticsearch
    Enable Elasticsearch logging.  Requires -ElasticsearchSourcePath and Key Vault
    (the source's ConnectionString is WFAES-encrypted before deploy).

.PARAMETER ElasticsearchSourcePath
    Path to the user-supplied Elasticsearch source.  The file MUST be named
    exactly 'ElasticsearchLoggingSource.bite' (the engine reads that exact path).

.PARAMETER EnableConsoleLogging
    Set ENABLECONSOLELOGGING=true.

.PARAMETER ExecutionLogLevel
    EXECUTIONLOGLEVEL value (TRACE|DEBUG|INFO|WARN|ERROR|FATAL|OFF).  Default INFO.
    Drives the engine's console + App Insights logging in code (the isolated worker
    does NOT read host.json).

.PARAMETER AlignHostJsonLogLevel
    OPT-IN.  Also rewrite the published host.json logLevel (default + Warewolf.*) to
    the EXECUTIONLOGLEVEL-mapped MEL level.  NOT required for the engine's own
    logging — it only tunes the Functions HOST process verbosity.  Default: off.

.PARAMETER LicenseCheckEnabled
    WAREWOLF_LICENSE_CHECK_ENABLED (engine default: true).

.PARAMETER LicenseConfigPath
    Path to a 'Warewolf License.secureconfig' file.  When supplied it is copied to
    the publish root as 'Warewolf License.secureconfig' (the engine's license gate
    reads it from there).  Prompted interactively; OPTIONAL — if omitted, the
    license check (default ON) may fail at startup.  See
    https://warewolf.io/knowledge-base/articles/security-encryption/.

.PARAMETER StructuredLogs
    STRUCTURED_LOGS — JSON console output (default true).

.PARAMETER LogDir
    Directory for the transcript + summary.  Default: <PublishDir>\..\deploy-logs.

.PARAMETER NonInteractive
    Never prompt; missing required values cause an early, clear throw.

.PARAMETER DryRun
    Print every mutating action without executing it.  Read-only probes still run;
    no transcript/summary is written.

.NOTES
    BREAKING CHANGE: -ResourceGroup, -Location, -StorageAccount, -AppName and
    -PublishPath (and -KeyVaultSecretName when encrypting) no longer have defaults.
    Non-interactive callers MUST pass them explicitly.

    FIXED / NON-CONFIGURABLE env vars: ASPNETCORE_ENVIRONMENT is always
    'Production'; BYPASS_SECURE_CONFIG, WAREWOLF_SUPER_ADMIN_ENABLED and
    SkipFailureToRetrieveSecret are always 'false' and are no longer exposed as
    parameters.  The development-only DEBUG_* settings have also been removed, and
    WEBSITE_INSTANCE_ID / AZURE_FUNCTIONS_ENVIRONMENT remain platform-managed.

    Prerequisites: PowerShell 7+, Azure CLI (az, logged in), and — only when
    publishing via func — Azure Functions Core Tools.  DPAPI-encrypted .bite
    sources can only be re-encrypted on the machine that created them.
#>

[CmdletBinding()]
param(
    # ── Targeting (REQUIRED — no defaults) ───────────────────────────────────
    [string] $SubscriptionId,
    [string] $TenantId,
    [string] $ResourceGroup,
    [string] $Location,
    [string] $StorageAccount,
    [string] $AppName,

    # ── Publish source (REQUIRED — folder or .zip) ───────────────────────────
    [string] $PublishPath,
    [ValidateSet('Auto', 'Func', 'Zip')]
    [string] $PublishMethod = 'Auto',

    # ── Application Insights ─────────────────────────────────────────────────
    [string] $AppInsightsName,
    [nullable[bool]] $EnableAppInsights,

    # ── Auth provisioning ────────────────────────────────────────────────────
    [switch] $SkipAuthProvisioning,
    [string] $AuthConfigPath,

    # ── Package inputs ───────────────────────────────────────────────────────
    [string] $SecureConfigPath,
    [string] $WorkflowsSourcePath,
    [string] $LicenseConfigPath,          # 'Warewolf License.secureconfig'; prompted, optional

    # ── Encryption / Key Vault ───────────────────────────────────────────────
    [nullable[bool]] $EncryptResources,
    [switch] $VerifyDecryption,           # OPT-IN: in-memory decrypt verification (default off)
    [string] $KeyVaultName,
    [string] $KeyVaultSecretName,        # no default — name the secret explicitly
    [switch] $GenerateNewKey,

    # ── Elasticsearch logging ────────────────────────────────────────────────
    [nullable[bool]] $EnableElasticsearch,
    [string] $ElasticsearchSourcePath,

    # ── Suspend/resume persistence (Hangfire) ─────────────────────────────────
    # The resume route reads Config.Persistence; the DbSource ConnectionString is
    # WFAES-encrypted exactly like the Elasticsearch source. Both files are PROMPTED
    # when persistence is enabled and a path is not passed.
    [nullable[bool]] $EnablePersistence,
    [string] $PersistenceSettingsPath,       # persistencesettings.json
    [string] $PersistenceDbSourcePath,       # persistencesettingsdbsource.bite

    # ── ExecutionEngineJobProcessor (optional companion deploy) ───────────────
    # When -DeployJobProcessor, after the engine deploy this calls
    # Deploy-WwJobProcessor.ps1 for the poller/reaper Function App, passing the
    # shared context (subscription/tenant/RG/location/Key Vault/persistence pair);
    # the child prompts for anything not supplied here.
    # JobProcessorPublishPath MUST be a SEPARATE publish output from the engine's
    # (it is a different csproj / different Function App); the plan phase resolves
    # it and fails loudly if it collides with the engine PublishPath.
    [switch] $DeployJobProcessor,
    [string] $JobProcessorAppName,
    [string] $JobProcessorPublishPath,
    [string] $JobProcessorStorageAccount,
    [string] $EngineResumeScope,             # MI token scope the processor uses (api://<engine-app-id>/.default)

    # ── RabbitMQ queue triggers (optional companion deploy) ───────────────────
    # When -DeployRabbitMqTriggers, after the engine deploy this calls
    # Deploy-WwQueueProcessor.ps1 ONCE PER TRIGGER FILE resolved from
    # -QueueTriggerPath / -QueueTriggerFilePath / -QueueTriggerManifestPath, so one
    # command provisions the engine AND a Container App per queue trigger. Each app is
    # autoscaled 0..Concurrency by the KEDA rabbitmq scaler (maxReplicas is derived from
    # the trigger's Concurrency; the KEDA target from its Prefetch).
    # QueueProcessorPublishPath MUST be a SEPARATE publish output from both the engine's
    # and the JobProcessor's; the plan phase resolves it and fails loudly on a collision.
    # Docs: docs/Deploy-EndToEnd-Runbook.md section 8.
    [switch] $DeployRabbitMqTriggers,
    [string] $QueueTriggerPath,                       # folder of trigger .bite files
    [string] $QueueTriggerFilter = '*.bite',
    [string] $QueueTriggerFilePath,                   # or exactly one file
    [string] $QueueTriggerManifestPath,               # or a manifest with per-trigger overrides
    [string] $QueueSourcePath,                        # folder holding {QueueSourceId}.bite
    [string] $AcaEnvironment,
    [string] $AcrName,
    [string] $QueueProcessorPublishPath,
    [string] $QueueProcessorImage,                    # or reuse a pre-built digest
    [string] $QueueEngineResourceAppId,               # engine Entra app id for the worker's token
    [string] $RabbitMqSecretUri,                      # Key Vault secret holding the broker URI (KEDA)
    [ValidateSet('Elastic', 'Fixed', 'Warm')]
    [string] $QueueScalingMode = 'Elastic',
    [switch] $ContinueOnQueueTriggerError,

    # ── Other logging / feature env vars ─────────────────────────────────────
    [nullable[bool]] $EnableConsoleLogging,
    [ValidateSet('TRACE', 'DEBUG', 'INFO', 'WARN', 'ERROR', 'FATAL', 'OFF')]
    [string] $ExecutionLogLevel = 'INFO',
    [nullable[bool]] $LicenseCheckEnabled,
    [nullable[bool]] $StructuredLogs,
    [switch] $AlignHostJsonLogLevel,      # opt-in: also rewrite host.json logLevel (host-process only)

    # ── Logging output ───────────────────────────────────────────────────────
    [string] $LogDir,

    # ── Control ──────────────────────────────────────────────────────────────
    [switch] $NonInteractive,
    [switch] $DryRun,

    # Test hook: when dot-sourced with -LoadFunctionsOnly, the script defines its
    # helper functions and returns BEFORE any cloud/filesystem action, so the
    # Pester suite (Deploy-WwExecutionEngine.Tests.ps1) can unit-test the helpers.
    [switch] $LoadFunctionsOnly
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ════════════════════════════════════════════════════════════════════════════
# Helpers
# ════════════════════════════════════════════════════════════════════════════

function Write-Phase {
    param([string] $Title)
    Write-Host ''
    Write-Host ('═' * 76) -ForegroundColor Cyan
    Write-Host "  $Title" -ForegroundColor Cyan
    Write-Host ('═' * 76) -ForegroundColor Cyan
}

function Write-Step { param([string] $Msg) Write-Host "  -> $Msg" -ForegroundColor White }
function Write-Ok   { param([string] $Msg) Write-Host "  [+] $Msg" -ForegroundColor Green }
function Write-Note { param([string] $Msg) Write-Host "  [-] $Msg" -ForegroundColor DarkYellow }

function Test-CommandExists {
    param([string] $Name)
    return [bool](Get-Command $Name -ErrorAction SilentlyContinue)
}

function Confirm-Yes {
    <#
        Prompt for a yes/no with a default.  In -NonInteractive mode the default
        is returned without prompting so unattended runs never block.
    #>
    param([string] $Message, [bool] $DefaultYes = $true)
    if ($NonInteractive) { return $DefaultYes }
    $suffix = if ($DefaultYes) { '[Y/n]' } else { '[y/N]' }
    $answer = Read-Host "  $Message $suffix"
    if ([string]::IsNullOrWhiteSpace($answer)) { return $DefaultYes }
    return $answer -imatch '^y'
}

function Read-Required {
    <#
        Return $Current if set; otherwise prompt (interactive) or throw
        (non-interactive) for a value the pipeline cannot proceed without.
    #>
    param([string] $Name, [string] $Current, [string] $Hint)
    if (-not [string]::IsNullOrWhiteSpace($Current)) { return $Current }
    if ($NonInteractive) {
        throw "Required value '$Name' was not supplied. Pass -$Name <value> (running with -NonInteractive)."
    }
    $hintText = if ($Hint) { " ($Hint)" } else { '' }
    do {
        $v = Read-Host "  Enter $Name$hintText"
    } while ([string]::IsNullOrWhiteSpace($v))
    return $v.Trim()
}

function Resolve-Toggle {
    <#
        Resolve a tri-state [nullable[bool]] feature flag:
          * explicit $true/$false  -> used as-is
          * $null + interactive     -> prompt (with $Default)
          * $null + non-interactive -> $Default
    #>
    param([string] $Name, [nullable[bool]] $Current, [bool] $Default, [string] $Prompt)
    if ($null -ne $Current) { return [bool]$Current }
    if ($NonInteractive)    { return $Default }
    $msg = if ($Prompt) { $Prompt } else { "Enable $Name" }
    return (Confirm-Yes $msg $Default)
}

function Get-MaskedValue {
    <#
        Mask a value for display/logging.  Secret settings show only a short hint.
    #>
    param([string] $Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    if ($Value.Length -le 6) { return '******' }
    return ('{0}…(masked, len={1})' -f $Value.Substring(0, 3), $Value.Length)
}

function Format-AzArgsForLog {
    <#
        Render an az argument array as a single line, masking secret values so
        they never reach the console, the -DryRun echo, a thrown error, or the
        transcript.  A NAME=VALUE token whose NAME contains SECRET/PASSWORD/TOKEN/
        CONNECTION is redacted — except names ending in _NAME (e.g.
        KEYVAULT_SECRET_NAME, which is a name, not a secret).  Values following a
        --password/--client-secret/--secret flag are masked too.
    #>
    param([string[]] $Arguments)
    $flagRegex = '(?i)^(--password|--client-secret|--secret)$'
    # A NAME=VALUE setting is a secret only when the name's trailing token is
    # PASSWORD/SECRET/TOKEN/KEY (preceded by '_' or start) or it contains
    # CONNECTION[_]STRING.  This redacts e.g. DEBUG_PRINCIPAL_TOKEN and
    # *_SECRET, but NOT flag-like names such as SkipFailureToRetrieveSecret or
    # KEYVAULT_SECRET_NAME.
    $secretNameRegex = '(?i)((^|_)(PASSWORD|SECRET|TOKEN|KEY)$|CONNECTION_?STRING)'
    $rendered  = New-Object System.Collections.Generic.List[string]
    $maskNext  = $false
    foreach ($a in $Arguments) {
        if ($maskNext)            { $rendered.Add('***REDACTED***'); $maskNext = $false; continue }
        if ($a -match $flagRegex) { $rendered.Add($a); $maskNext = $true; continue }
        if ($a -match '^([A-Za-z0-9_.\-]+)=(.+)$') {
            $name = $Matches[1]
            if ($name -match $secretNameRegex) { $rendered.Add("$name=***REDACTED***") } else { $rendered.Add($a) }
            continue
        }
        $rendered.Add($a)
    }
    return ($rendered -join ' ')
}

function Invoke-Az {
    <#
        Thin wrapper over the az CLI.
          -Mutating  : changes cloud state; skipped (echoed) under -DryRun.
          -AllowFail : non-zero exit returns $null instead of throwing (probes).
        Logged/echoed args are secret-redacted via Format-AzArgsForLog.
    #>
    param(
        [Parameter(Mandatory)][string[]] $Args,
        [switch] $Mutating,
        [switch] $AllowFail
    )
    if ($Mutating -and $DryRun) {
        Write-Host "      [DRYRUN] az $(Format-AzArgsForLog $Args)" -ForegroundColor DarkGray
        return $null
    }
    $out = & az @Args 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($AllowFail) { return $null }
        throw "az CLI failed ($LASTEXITCODE): az $(Format-AzArgsForLog $Args)`n$($out | Out-String)"
    }
    return $out
}

function Invoke-ChildScript {
    <#
        Invoke a sibling script via the call operator, honouring -DryRun by
        printing the planned call.
    #>
    param(
        [Parameter(Mandatory)][string] $Path,
        [hashtable] $Parameters = @{},
        [string] $Label
    )
    $name = if ($Label) { $Label } else { Split-Path $Path -Leaf }
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required sibling script not found: $Path"
    }
    if ($DryRun) {
        $rendered = ($Parameters.GetEnumerator() | ForEach-Object { "-$($_.Key)" }) -join ' '
        Write-Host "      [DRYRUN] & '$name' $rendered" -ForegroundColor DarkGray
        return
    }
    Write-Step "Invoking $name"
    & $Path @Parameters
    if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) {
        throw "$name exited with code $LASTEXITCODE."
    }
}

function Initialize-SecureConfigCrypto {
    <#
        Compile (once) a self-contained replica of
        Dev2.Services.Security.SecurityEncryption so the script can classify,
        validate, and AES-encrypt a secure.config exactly as the engine reads it.

        MUST stay byte-compatible with
        Dev/Dev2.Infrastructure/Services/Security/SecurityEncryption.cs
        (fixed passphrase/salt/IV, PBKDF1 via PasswordDeriveBytes, AES-256-CBC,
        zero padding).  AES == Rijndael with the default 128-bit block, so Aes
        is used in place of the obsolete RijndaelManaged.
    #>
    if ('WwSecureConfigCrypto' -as [type]) { return }
    Add-Type -Language CSharp -TypeDefinition @'
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class WwSecureConfigCrypto
{
    const string InitVector = "@1B2c3D4e5F6g7H8";
    const string PassPhrase = "Pas5pr@se";
    const string SaltValue  = "s@1tValue";
    const string HashAlgorithm = "SHA1";
    const int PasswordIterations = 2;
    const int KeySize = 256;

    static byte[] DeriveKey()
    {
        var salt = Encoding.ASCII.GetBytes(SaltValue);
#pragma warning disable 612,618
        var pdb = new PasswordDeriveBytes(PassPhrase, salt, HashAlgorithm, PasswordIterations);
        return pdb.GetBytes(KeySize / 8);
#pragma warning restore 612,618
    }

    public static string Encrypt(string plainText)
    {
        var iv   = Encoding.ASCII.GetBytes(InitVector);
        var data = Encoding.UTF8.GetBytes(plainText);
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;
        aes.KeySize = KeySize;
        using var enc = aes.CreateEncryptor(DeriveKey(), iv);
        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
        }
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        var iv   = Encoding.ASCII.GetBytes(InitVector);
        var data = Convert.FromBase64String(cipherText);
        using var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;
        aes.KeySize = KeySize;
        using var dec = aes.CreateDecryptor(DeriveKey(), iv);
        using var ms = new MemoryStream(data);
        using var cs = new CryptoStream(ms, dec, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs, Encoding.UTF8);
        return sr.ReadToEnd();
    }
}
'@
}

function Test-SecureConfig {
    <#
        Classify a secure.config file and validate that the engine will be able
        to load it.  Returns a PSCustomObject:
            Kind  = 'Plaintext' | 'Encrypted' | 'Invalid'
            Valid = $true when the (decrypted) content is a usable config

        A config is "usable" when it parses as JSON containing SecretKey and/or
        WindowsGroupPermissions — the shape SecureConfigLoader deserialises.
    #>
    param([Parameter(Mandatory)][string] $Path)
    Initialize-SecureConfigCrypto

    $raw = Get-Content -LiteralPath $Path -Raw

    function Test-IsUsableJson([string] $text) {
        $clean = $text.TrimEnd([char]0).Trim()
        # Cheap pre-check: only attempt a JSON parse when the content actually
        # looks like JSON. Encrypted (Base64) / binary content would otherwise make
        # ConvertFrom-Json throw a caught-but-transcript-logged TerminatingError.
        if (-not ($clean.StartsWith('{') -or $clean.StartsWith('['))) { return $false }
        try { $obj = $clean | ConvertFrom-Json } catch { return $false }
        if ($null -eq $obj) { return $false }
        $names = @($obj.PSObject.Properties.Name)
        return ($names -contains 'SecretKey') -or ($names -contains 'WindowsGroupPermissions')
    }

    # 1. Plaintext JSON (the engine's TryDecrypt passes it through unchanged).
    if (Test-IsUsableJson $raw) {
        return [pscustomobject]@{ Kind = 'Plaintext'; Valid = $true }
    }

    # 2. AES-encrypted — must decrypt to usable JSON with the engine's fixed key.
    try {
        $decrypted = [WwSecureConfigCrypto]::Decrypt($raw)
        if (Test-IsUsableJson $decrypted) {
            return [pscustomobject]@{ Kind = 'Encrypted'; Valid = $true }
        }
        return [pscustomobject]@{ Kind = 'Encrypted'; Valid = $false }
    } catch {
        return [pscustomobject]@{ Kind = 'Invalid'; Valid = $false }
    }
}

function Protect-SecureConfig {
    <#
        AES-encrypt a plaintext secure.config (using the engine's fixed key) and
        write the Base64 result to $OutPath with no trailing newline.
    #>
    param([Parameter(Mandatory)][string] $InPath, [Parameter(Mandatory)][string] $OutPath)
    Initialize-SecureConfigCrypto
    $raw       = Get-Content -LiteralPath $InPath -Raw
    $encrypted = [WwSecureConfigCrypto]::Encrypt($raw)
    [System.IO.File]::WriteAllText($OutPath, $encrypted, (New-Object System.Text.UTF8Encoding $false))
}

function Convert-ToMelLevel {
    <#
        Map the engine's Dev2 EXECUTIONLOGLEVEL to the Microsoft.Extensions.Logging
        level used by host.json (mirrors LoggingConfiguration.MelMinimumLevel).
    #>
    param([Parameter(Mandatory)][string] $ExecutionLogLevel)
    switch ($ExecutionLogLevel.ToUpperInvariant()) {
        'TRACE' { 'Trace' }
        'DEBUG' { 'Debug' }
        'INFO'  { 'Information' }
        'WARN'  { 'Warning' }
        'ERROR' { 'Error' }
        'FATAL' { 'Critical' }
        'OFF'   { 'None' }
        default { 'Information' }
    }
}

function Read-ExecutionLogLevel {
    <#
        Prompt for EXECUTIONLOGLEVEL (validated against the Dev2 level set).
        Returns $Current unchanged in -NonInteractive mode or on empty input.
    #>
    param([string] $Current = 'INFO')
    if ($NonInteractive) { return $Current }
    $levels = @('TRACE', 'DEBUG', 'INFO', 'WARN', 'ERROR', 'FATAL', 'OFF')
    while ($true) {
        $v = Read-Host "  EXECUTIONLOGLEVEL ($($levels -join '/')) [$Current]"
        if ([string]::IsNullOrWhiteSpace($v)) { return $Current }
        $u = $v.Trim().ToUpperInvariant()
        if ($levels -contains $u) { return $u }
        Write-Host "    (invalid; choose one of $($levels -join ', '))" -ForegroundColor Yellow
    }
}

function Update-HostJsonLogLevel {
    <#
        Align host.json's logging.logLevel with the chosen EXECUTIONLOGLEVEL:
        sets "default" and every "Warewolf.Execution.Lightweight.*" category to
        the mapped MEL level, leaving Microsoft/Host framework categories intact.
        Returns the mapped level.  No-op (returns the level) when host.json has no
        logging.logLevel section.
    #>
    param([Parameter(Mandatory)][string] $HostJsonPath, [Parameter(Mandatory)][string] $ExecutionLogLevel)
    $mel  = Convert-ToMelLevel $ExecutionLogLevel
    $json = Get-Content -LiteralPath $HostJsonPath -Raw | ConvertFrom-Json
    $logging = $json.PSObject.Properties['logging']
    if (-not $logging -or -not $logging.Value.PSObject.Properties['logLevel']) { return $mel }
    $ll = $logging.Value.logLevel
    $ll.default = $mel
    foreach ($name in @($ll.PSObject.Properties.Name)) {
        if ($name -like 'Warewolf.Execution.Lightweight.*') { $ll.$name = $mel }
    }
    $json | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $HostJsonPath -Encoding UTF8
    return $mel
}

function Save-DeploySummary {
    <#
        Write the run summary to $summaryPath — CRASH-SAFELY and INCREMENTALLY.
        Called after every phase AND from the outer catch, so that NO MATTER WHERE
        the run stops (including a mid-phase failure such as the Phase 4 deploy) the
        rollback always has an authoritative record of what THIS run created vs.
        found pre-existing.  Writes atomically (temp file -> Move-Item) so a crash
        during the write itself cannot leave a truncated/corrupt summary.

        Status: in-progress | completed | failed.  lastPhase + error let an operator
        (and the rollback) recognise a partial/failed run.

        Reads the run-state script variables in scope at call time; all of them are
        initialised before the first call, so this is safe under Set-StrictMode.
    #>
    param(
        [ValidateSet('in-progress', 'completed', 'failed')]
        [string] $Status = 'in-progress',
        [string] $ErrorMessage
    )
    if ([string]::IsNullOrWhiteSpace($summaryPath)) { return }

    $maskedSettings = [ordered]@{}
    foreach ($k in $appSettings.Keys) {
        $maskedSettings[$k] = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    }
    $aiConnMasked = if ($enableAppInsights) {
        if ([string]::IsNullOrEmpty($aiConnectionString)) { '<pending — created/read on a real run>' }
        elseif ($aiConnectionString -like '<pending*')     { $aiConnectionString }
        else                                               { Get-MaskedValue $aiConnectionString }
    } else { $null }

    $summary = [ordered]@{
        timestampUtc    = (Get-Date).ToUniversalTime().ToString('o')
        status          = $Status
        lastPhase       = $script:DeployLastPhase
        error           = $ErrorMessage
        dryRun          = [bool]$DryRun
        runId           = $runId
        resourceTags    = $ResourceTags
        created         = $created
        subscriptionId  = $SubscriptionId
        tenantId        = $TenantId
        resourceGroup   = $ResourceGroup
        location        = $Location
        storageAccount  = $StorageAccount
        appName         = $AppName
        appInsightsName = ($enableAppInsights ? $AppInsightsName : $null)
        appInsightsConnectionString = $aiConnMasked
        endpoint        = $baseUrl
        publishPath     = $PublishPath
        publishDir      = $StagingDir
        publishMethod   = $PublishMethod
        appInsights     = $enableAppInsights
        authProvisioned = (-not $SkipAuthProvisioning)
        entraAppDisplayName = ($SkipAuthProvisioning ? $null : "$AppName-auth")
        secureConfig    = ($SecureConfigPath ? $secureConfigKind : 'none')
        licenseConfig   = ($LicenseConfigPath ? 'staged' : 'none')
        workflowsSource = $WorkflowsSourcePath
        encryptResources = $doEncryptResources
        verifyDecryption = [bool]$VerifyDecryption
        elasticsearch   = $enableEs
        persistence     = $enablePersistence
        persistenceSettings = ($enablePersistence ? $PersistenceSettingsPath : $null)
        persistenceDbSource = ($enablePersistence ? $PersistenceDbSourcePath : $null)
        deployJobProcessor  = [bool]$DeployJobProcessor
        jobProcessorAppName = ($DeployJobProcessor ? $JobProcessorAppName : $null)
        deployRabbitMqTriggers = [bool]$DeployRabbitMqTriggers
        queueProcessorApps  = ($DeployRabbitMqTriggers ? $script:QueueProcessorApps : $null)
        keyVault        = ($kvRequired ? @{ name = $KeyVaultName; secret = $KeyVaultSecretName } : $null)
        appSettings     = $maskedSettings
    }
    # Atomic write: render to a temp sibling, then replace — a crash mid-write
    # can never leave the rollback a half-written summary.
    $tmp = "$summaryPath.tmp"
    $summary | ConvertTo-Json -Depth 6 | Set-Content -Path $tmp -Encoding UTF8
    Move-Item -LiteralPath $tmp -Destination $summaryPath -Force
}

# ════════════════════════════════════════════════════════════════════════════
# Path resolution
# ════════════════════════════════════════════════════════════════════════════

$ScriptDir   = $PSScriptRoot

$ConfigureAuthScript = Join-Path $ScriptDir 'Configure-WwExecutionAuth.ps1'
$AppInsightsScript   = Join-Path $ScriptDir 'Setup-ApplicationInsights.ps1'
$EncryptScript       = Join-Path $ScriptDir 'Encrypt-Config.ps1'
$WorkflowIndexScript = Join-Path $ScriptDir 'Generate-WorkflowIndex.ps1'
$AuthOutputPath      = Join-Path $ScriptDir 'Configure-WwExecutionAuth.output.json'

$ElasticsearchBiteName = 'ElasticsearchLoggingSource.bite'

# Suspend/resume persistence pair (staged into Settings\ exactly like the ES source).
$PersistenceSettingsName = 'persistencesettings.json'
$PersistenceDbSourceName = 'persistencesettingsdbsource.bite'

# Companion JobProcessor deploy (invoked only when -DeployJobProcessor).
$JobProcessorScript = Join-Path $ScriptDir 'Deploy-WwJobProcessor.ps1'

# Companion QueueProcessor deploy (invoked only when -DeployRabbitMqTriggers), once per
# resolved trigger file.
$QueueProcessorScript = Join-Path $ScriptDir 'Deploy-WwQueueProcessor.ps1'

# Test hook: stop here when only the helper functions are wanted (Pester).
if ($LoadFunctionsOnly) { return }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0 — Pre-flight
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0  Pre-flight'

if (-not (Test-CommandExists 'az')) {
    throw 'Azure CLI (az) is not installed or not on PATH. Install: https://aka.ms/installazurecliwindows'
}

# NOTE: capture first, then parse — piping a $null (failed/not-logged-in) result
# straight into ConvertFrom-Json throws "Cannot bind argument ... because it is null".
$accountJson = Invoke-Az @('account', 'show', '-o', 'json') -AllowFail
$account = if ($accountJson) { $accountJson | ConvertFrom-Json } else { $null }
if (-not $account) {
    throw "Not logged in to Azure CLI. Run 'az login' first."
}
Write-Ok "Azure account: $($account.user.name) | Subscription: $($account.name)"

if (-not $SubscriptionId) { $SubscriptionId = $account.id }
if (-not $TenantId)       { $TenantId       = $account.tenantId }

# ════════════════════════════════════════════════════════════════════════════
# Phase 0.5 — PLAN  (resolve every decision; no cloud/state change yet)
# ════════════════════════════════════════════════════════════════════════════

Write-Phase 'Phase 0.5  Plan (resolve all settings before any change)'

# Run identity + tags so Rollback-WwExecutionEngine.ps1 can target ONLY what this
# run created (and never a pre-existing resource).  $created records, per resource,
# whether THIS run created it ($true) or found it already present ($false).
$runId        = "wwx-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$ResourceTags = @("wwx-test-run=$runId", 'wwx-purpose=exec-engine-live-test')
$created      = [ordered]@{}

# ── Targeting (required) ──────────────────────────────────────────────────────
$AppName        = Read-Required -Name 'AppName'        -Current $AppName        -Hint 'globally-unique Function App name'
$ResourceGroup  = Read-Required -Name 'ResourceGroup'  -Current $ResourceGroup
$Location       = Read-Required -Name 'Location'       -Current $Location        -Hint 'e.g. southafricanorth'
$StorageAccount = Read-Required -Name 'StorageAccount' -Current $StorageAccount  -Hint '3-24 lowercase chars'
if (-not $AppInsightsName) { $AppInsightsName = "$AppName-ai" }

# ── Publish source (folder or .zip) ───────────────────────────────────────────
$PublishPath = Read-Required -Name 'PublishPath' -Current $PublishPath -Hint 'folder or .zip of the Release publish output'
if (-not (Test-Path -LiteralPath $PublishPath)) {
    throw "PublishPath not found: $PublishPath"
}
$publishItem      = Get-Item -LiteralPath $PublishPath
$publishIsZip     = (-not $publishItem.PSIsContainer) -and ($publishItem.Extension -ieq '.zip')
if ($publishItem.PSIsContainer) {
    $PublishDir = $publishItem.FullName
} elseif ($publishIsZip) {
    $PublishDir = Join-Path $publishItem.DirectoryName $publishItem.BaseName
} else {
    throw "PublishPath must be a folder or a .zip file: $PublishPath"
}

if (-not $LogDir) { $LogDir = Join-Path (Split-Path $PublishDir -Parent) 'deploy-logs' }

# ── Feature toggles ────────────────────────────────────────────────────────────
$enableAppInsights   = Resolve-Toggle -Name 'EnableAppInsights'   -Current $EnableAppInsights   -Default $true  -Prompt 'Provision + enable Application Insights?'
$enableConsole       = Resolve-Toggle -Name 'EnableConsoleLogging' -Current $EnableConsoleLogging -Default $true  -Prompt 'Enable console logging?'
$enableEs            = Resolve-Toggle -Name 'EnableElasticsearch' -Current $EnableElasticsearch -Default $false -Prompt 'Enable Elasticsearch logging?'
$enablePersistence   = Resolve-Toggle -Name 'EnablePersistence'   -Current $EnablePersistence   -Default $false -Prompt 'Enable suspend/resume persistence (Hangfire) — stage the persistence settings pair?'
$doEncryptResources  = Resolve-Toggle -Name 'EncryptResources'    -Current $EncryptResources    -Default $false -Prompt 'Encrypt ALL sources (workflows + Elasticsearch + others) now? (encrypt once; leave off if already encrypted)'
$licenseCheck        = Resolve-Toggle -Name 'LicenseCheckEnabled' -Current $LicenseCheckEnabled  -Default $true  -Prompt 'Enable license/subscription check?'
$structuredLogs      = Resolve-Toggle -Name 'StructuredLogs'      -Current $StructuredLogs       -Default $true  -Prompt 'Structured (JSON) console logs?'

# Security toggles below are intentionally NOT exposed: they are always disabled.
#   BYPASS_SECURE_CONFIG / WAREWOLF_SUPER_ADMIN_ENABLED / SkipFailureToRetrieveSecret.

# EXECUTIONLOGLEVEL — prompt when not explicitly supplied (drives both the app
# setting and the host.json logLevel alignment below).
if (-not $PSBoundParameters.ContainsKey('ExecutionLogLevel')) {
    $ExecutionLogLevel = Read-ExecutionLogLevel -Current $ExecutionLogLevel
}

# ── Key Vault requirement (decoupled from "this run encrypts") ──────────────────
# KV wiring (app settings + the Function App MI's "Secrets User" role) is needed
# whenever a vault is in play, so the engine can DECRYPT WFAES sources at runtime:
#   * -EncryptResources -> this run (re)encrypts; the vault is mandatory, and
#   * -KeyVaultName supplied (without encrypting) -> sources are ALREADY encrypted
#     and the engine still needs the vault at runtime.
# Encryption itself (dev "Secrets Officer" role + key generation + verification) is
# gated separately on $doEncryptResources.
$kvRequired = $doEncryptResources -or (-not [string]::IsNullOrWhiteSpace($KeyVaultName))
if ($kvRequired) {
    $KeyVaultName       = Read-Required -Name 'KeyVaultName'       -Current $KeyVaultName       -Hint 'Key Vault the engine decrypts WFAES sources with (3-24 chars)'
    $KeyVaultSecretName = Read-Required -Name 'KeyVaultSecretName' -Current $KeyVaultSecretName -Hint 'AES key secret name'
}

# ── Elasticsearch source (user-supplied, exact filename) ───────────────────────
if ($enableEs) {
    $ElasticsearchSourcePath = Read-Required -Name 'ElasticsearchSourcePath' -Current $ElasticsearchSourcePath -Hint "path to $ElasticsearchBiteName"
    if (-not (Test-Path -LiteralPath $ElasticsearchSourcePath -PathType Leaf)) {
        throw "ElasticsearchSourcePath not found (must be a file): $ElasticsearchSourcePath"
    }
    $esLeaf = Split-Path $ElasticsearchSourcePath -Leaf
    if ($esLeaf -ine $ElasticsearchBiteName) {
        throw "Elasticsearch source must be named exactly '$ElasticsearchBiteName' (the engine reads that exact path); got '$esLeaf'."
    }
}

# ── Persistence settings pair (prompted when enabled; exact filenames) ─────────
if ($enablePersistence) {
    $PersistenceSettingsPath = Read-Required -Name 'PersistenceSettingsPath' -Current $PersistenceSettingsPath -Hint "path to $PersistenceSettingsName"
    if (-not (Test-Path -LiteralPath $PersistenceSettingsPath -PathType Leaf)) {
        throw "PersistenceSettingsPath not found (must be a file): $PersistenceSettingsPath"
    }
    $psLeaf = Split-Path $PersistenceSettingsPath -Leaf
    if ($psLeaf -ine $PersistenceSettingsName) {
        throw "Persistence settings must be named exactly '$PersistenceSettingsName' (the engine reads that exact path); got '$psLeaf'."
    }
    $PersistenceDbSourcePath = Read-Required -Name 'PersistenceDbSourcePath' -Current $PersistenceDbSourcePath -Hint "path to $PersistenceDbSourceName"
    if (-not (Test-Path -LiteralPath $PersistenceDbSourcePath -PathType Leaf)) {
        throw "PersistenceDbSourcePath not found (must be a file): $PersistenceDbSourcePath"
    }
    $dbLeaf = Split-Path $PersistenceDbSourcePath -Leaf
    if ($dbLeaf -ine $PersistenceDbSourceName) {
        throw "Persistence DbSource must be named exactly '$PersistenceDbSourceName' (the engine reads that exact path); got '$dbLeaf'."
    }
}

# ── JobProcessor companion — its publish output MUST differ from the engine's ──
# The processor is a SEPARATE Function App built from a DIFFERENT csproj
# (Warewolf.Execution.EngineJobProcessor). Sharing a publish/upload directory with
# the engine would zip the engine's binaries and upload them to the processor app —
# a silently-wrong deploy. Resolve + validate the processor publish path up-front so
# it fails at PLAN time (before the engine is even deployed), not deep in the child.
if ($DeployJobProcessor) {
    $JobProcessorPublishPath = Read-Required -Name 'JobProcessorPublishPath' -Current $JobProcessorPublishPath -Hint 'folder or .zip of the JobProcessor Release publish output — MUST differ from the engine PublishPath'
    if (-not (Test-Path -LiteralPath $JobProcessorPublishPath)) {
        throw "JobProcessorPublishPath not found: $JobProcessorPublishPath"
    }
    $jpItem = Get-Item -LiteralPath $JobProcessorPublishPath
    $jpDir  =
        if     ($jpItem.PSIsContainer)         { $jpItem.FullName }
        elseif ($jpItem.Extension -ieq '.zip') { Join-Path $jpItem.DirectoryName $jpItem.BaseName }
        else   { throw "JobProcessorPublishPath must be a folder or a .zip file: $JobProcessorPublishPath" }
    $engineFull = ([System.IO.Path]::GetFullPath($PublishDir)).TrimEnd('\','/')
    $jpFull     = ([System.IO.Path]::GetFullPath($jpDir)).TrimEnd('\','/')
    if ($jpFull -ieq $engineFull) {
        throw ("JobProcessorPublishPath resolves to the SAME directory as the engine PublishPath ('$engineFull'). " +
               "The processor is a different Function App built from Warewolf.Execution.EngineJobProcessor and MUST publish to its own directory. " +
               "Publish it separately, e.g. dotnet publish Warewolf.Execution.EngineJobProcessor -c Release -o <different-path>.")
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Plan-time validation for the RabbitMQ queue-trigger companion deploy.
# Runs BEFORE the engine is deployed so an operator sees the whole fan-out (and any
# mistake) up front rather than after the engine is already live.
# ─────────────────────────────────────────────────────────────────────────────
$script:QueueTriggerFiles  = @()
$script:QueueProcessorApps = @()

if ($DeployRabbitMqTriggers) {
    if (-not (Test-Path -LiteralPath $QueueProcessorScript)) {
        throw "Deploy-WwQueueProcessor.ps1 not found at '$QueueProcessorScript'."
    }

    $modeCount = @([bool]$QueueTriggerPath, [bool]$QueueTriggerFilePath, [bool]$QueueTriggerManifestPath |
                   Where-Object { $_ }).Count
    if ($modeCount -gt 1) {
        throw ('-QueueTriggerPath, -QueueTriggerFilePath and -QueueTriggerManifestPath are mutually ' +
               'exclusive; pass exactly one.')
    }

    # Resolve the trigger set now, so "zero triggers" fails at plan time.
    if ($QueueTriggerFilePath) {
        if (-not (Test-Path -LiteralPath $QueueTriggerFilePath)) {
            throw "QueueTriggerFilePath not found: '$QueueTriggerFilePath'."
        }
        $script:QueueTriggerFiles = @((Get-Item -LiteralPath $QueueTriggerFilePath).FullName)
    }
    elseif ($QueueTriggerManifestPath) {
        if (-not (Test-Path -LiteralPath $QueueTriggerManifestPath)) {
            throw "QueueTriggerManifestPath not found: '$QueueTriggerManifestPath'."
        }
        $qpManifest = Get-Content -LiteralPath $QueueTriggerManifestPath -Raw | ConvertFrom-Json
        $script:QueueTriggerFiles = @($qpManifest.triggers | ForEach-Object { $_.file })
        if ($script:QueueTriggerFiles.Count -eq 0) {
            throw "Trigger manifest '$QueueTriggerManifestPath' declares no triggers."
        }
    }
    else {
        $QueueTriggerPath = Read-Required -Name 'QueueTriggerPath' -Current $QueueTriggerPath `
                                          -Hint 'folder containing the queue-trigger .bite files'
        if (-not (Test-Path -LiteralPath $QueueTriggerPath)) {
            throw "QueueTriggerPath not found: '$QueueTriggerPath'."
        }
        $script:QueueTriggerFiles = @(Get-ChildItem -LiteralPath $QueueTriggerPath -Filter $QueueTriggerFilter -File |
                                      Sort-Object Name | Select-Object -ExpandProperty FullName)
        if ($script:QueueTriggerFiles.Count -eq 0) {
            throw ("No trigger files matching '$QueueTriggerFilter' were found in '$QueueTriggerPath'. " +
                   'Refusing to run a queue-trigger deploy that would deploy nothing.')
        }
    }

    # An unsubstituted release token means maxReplicas would be derived from a token, so the
    # app would silently get the wrong capacity. Fail here, not in the child.
    foreach ($qpFile in $script:QueueTriggerFiles) {
        $qpRaw = Get-Content -LiteralPath $qpFile -Raw
        if ($qpRaw -match '#\{') {
            throw ("Trigger file '$qpFile' still contains an unsubstituted release token ('#{...'). " +
                   'The release pipeline must substitute Concurrency before the deploy runs.')
        }
    }

    $QueueSourcePath = Read-Required -Name 'QueueSourcePath' -Current $QueueSourcePath `
                                     -Hint 'folder holding the {QueueSourceId}.bite RabbitMQ source files'
    $AcaEnvironment  = Read-Required -Name 'AcaEnvironment' -Current $AcaEnvironment `
                                     -Hint 'Container Apps environment for the queue workers'

    # Publish-path isolation: the worker is a different project (and a Linux container), so
    # sharing a publish directory with the engine or the JobProcessor would ship the wrong bits.
    if (-not $QueueProcessorImage) {
        $QueueProcessorPublishPath = Read-Required -Name 'QueueProcessorPublishPath' `
            -Current $QueueProcessorPublishPath `
            -Hint 'folder of the Warewolf.Execution.QueueProcessor publish output (or pass -QueueProcessorImage)'
        if (-not (Test-Path -LiteralPath $QueueProcessorPublishPath)) {
            throw "QueueProcessorPublishPath not found: $QueueProcessorPublishPath"
        }
        $AcrName = Read-Required -Name 'AcrName' -Current $AcrName -Hint 'Azure Container Registry name'

        $qpFull = ([System.IO.Path]::GetFullPath($QueueProcessorPublishPath)).TrimEnd('\', '/')
        $engineFullForQp = ([System.IO.Path]::GetFullPath($PublishDir)).TrimEnd('\', '/')
        if ($qpFull -ieq $engineFullForQp) {
            throw ("QueueProcessorPublishPath resolves to the SAME directory as the engine PublishPath " +
                   "('$engineFullForQp'). The queue worker is a different project " +
                   '(Warewolf.Execution.QueueProcessor) and MUST publish to its own directory.')
        }
        if ($DeployJobProcessor -and $JobProcessorPublishPath) {
            $jpFullForQp = ([System.IO.Path]::GetFullPath($JobProcessorPublishPath)).TrimEnd('\', '/')
            if ($qpFull -ieq $jpFullForQp) {
                throw ("QueueProcessorPublishPath resolves to the SAME directory as " +
                       "JobProcessorPublishPath ('$jpFullForQp'). Each app must publish separately.")
            }
        }
    }
}

# ── secure.config classification (validate now, before any change) ─────────────
$secureConfigKind = $null
if ($SecureConfigPath) {
    if (-not (Test-Path -LiteralPath $SecureConfigPath -PathType Leaf)) {
        throw "SecureConfigPath not found: $SecureConfigPath"
    }
    $sc = Test-SecureConfig -Path $SecureConfigPath
    $secureConfigKind = $sc.Kind
    if ($sc.Kind -eq 'Invalid' -or (-not $sc.Valid)) {
        throw ("secure.config at '$SecureConfigPath' cannot be loaded by the engine: it is neither valid " +
               "plaintext JSON nor AES-decryptable to a usable config (needs SecretKey / WindowsGroupPermissions). " +
               "Re-export it from Warewolf Studio or supply a valid file.")
    }
}

# ── Warewolf License.secureconfig (prompted; optional — copied to the publish root) ──
# https://warewolf.io/knowledge-base/articles/security-encryption/
if (-not $LicenseConfigPath -and -not $NonInteractive) {
    $resp = Read-Host "  Path to 'Warewolf License.secureconfig' (Enter to skip)"
    if (-not [string]::IsNullOrWhiteSpace($resp)) { $LicenseConfigPath = $resp.Trim() }
}
if ($LicenseConfigPath -and -not (Test-Path -LiteralPath $LicenseConfigPath -PathType Leaf)) {
    throw "LicenseConfigPath not found (must be a file): $LicenseConfigPath"
}

# ── Build the environment-variable (app settings) map ──────────────────────────
$appSettings = [ordered]@{}
$secretSettingNames = New-Object System.Collections.Generic.HashSet[string]

# ASPNETCORE_ENVIRONMENT is FIXED to Production (never user-configurable).
$appSettings['ASPNETCORE_ENVIRONMENT']        = 'Production'
$appSettings['EXECUTIONLOGLEVEL']             = $ExecutionLogLevel
$appSettings['ENABLECONSOLELOGGING']          = ($enableConsole ? 'true' : 'false')
$appSettings['STRUCTURED_LOGS']               = ($structuredLogs ? 'true' : 'false')
$appSettings['ENABLEAPPLICATIONINSIGHTS']     = ($enableAppInsights ? 'true' : 'false')
$appSettings['ENABLEELASTICSEARCHLOGGING']    = ($enableEs ? 'true' : 'false')
$appSettings['WAREWOLF_LICENSE_CHECK_ENABLED'] = ($licenseCheck ? 'true' : 'false')
# NOTE: BYPASS_SECURE_CONFIG, WAREWOLF_SUPER_ADMIN_ENABLED and
# SkipFailureToRetrieveSecret are deliberately NOT set here (not shown in logs/
# summary, not created on the Function App). The engine defaults them to
# disabled/false when absent; an admin can add them manually in the Function App
# only if a specific override is ever required.

if ($kvRequired) {
    $appSettings['AZURE_KEYVAULT_NAME']           = $KeyVaultName
    $appSettings['KEYVAULT_SECRET_NAME']          = $KeyVaultSecretName
}

# ── Settings summary + single confirmation (item 8) ────────────────────────────
Write-Host ''
Write-Host '  ── Resolved deployment settings ──────────────────────────────────────' -ForegroundColor White
Write-Host ("    {0,-28}: {1}" -f 'SubscriptionId', $SubscriptionId)
Write-Host ("    {0,-28}: {1}" -f 'TenantId', $TenantId)
Write-Host ("    {0,-28}: {1}" -f 'ResourceGroup', $ResourceGroup)
Write-Host ("    {0,-28}: {1}" -f 'Location', $Location)
Write-Host ("    {0,-28}: {1}" -f 'StorageAccount', $StorageAccount)
Write-Host ("    {0,-28}: {1}" -f 'AppName', $AppName)
Write-Host ("    {0,-28}: {1}" -f 'PublishPath', $PublishPath)
Write-Host ("    {0,-28}: {1}" -f 'PublishDir', ($publishIsZip ? "$PublishDir  (extracted from zip)" : $PublishDir))
Write-Host ("    {0,-28}: {1}" -f 'PublishMethod', $PublishMethod)
Write-Host ("    {0,-28}: {1}" -f 'App Insights', ($enableAppInsights ? "enabled ($AppInsightsName)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'Auth provisioning', ($SkipAuthProvisioning ? 'skipped' : 'enabled'))
Write-Host ("    {0,-28}: {1}" -f 'secure.config', ($SecureConfigPath ? "$SecureConfigPath [$secureConfigKind]" : '(none — open access)'))
Write-Host ("    {0,-28}: {1}" -f 'License config', ($LicenseConfigPath ? $LicenseConfigPath : '(none — license check may fail)'))
Write-Host ("    {0,-28}: {1}" -f 'Workflows source', ($WorkflowsSourcePath ? $WorkflowsSourcePath : '(none)'))
Write-Host ("    {0,-28}: {1}" -f 'Encrypt sources (this run)', ($doEncryptResources ? 'YES (workflows + ES + others)' : 'no (staged as-is; assumed already encrypted)'))
Write-Host ("    {0,-28}: {1}" -f 'Verify decryption', ($doEncryptResources ? ($VerifyDecryption ? 'yes (in-memory)' : 'no') : 'n/a'))
Write-Host ("    {0,-28}: {1}" -f 'Elasticsearch logging', ($enableEs ? "enabled ($ElasticsearchSourcePath)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'Persistence (Hangfire)', ($enablePersistence ? "enabled ($PersistenceDbSourcePath)" : 'disabled'))
Write-Host ("    {0,-28}: {1}" -f 'Deploy JobProcessor', ($DeployJobProcessor ? "yes -> Deploy-WwJobProcessor.ps1$($JobProcessorAppName ? " ($JobProcessorAppName)" : '')" : 'no'))
if ($DeployJobProcessor) {
    Write-Host ("    {0,-28}: {1}" -f 'JobProcessor PublishPath', "$JobProcessorPublishPath  (separate from engine PublishDir)")
}
Write-Host ("    {0,-28}: {1}" -f 'Deploy RabbitMQ triggers', ($DeployRabbitMqTriggers ? "yes -> Deploy-WwQueueProcessor.ps1 ($($script:QueueTriggerFiles.Count) trigger(s): $((($script:QueueTriggerFiles | ForEach-Object { [System.IO.Path]::GetFileNameWithoutExtension($_) }) -join ', ')))" : 'no'))
if ($DeployRabbitMqTriggers) {
    Write-Host ("    {0,-28}: {1}" -f 'QueueProcessor image', ($QueueProcessorImage ? $QueueProcessorImage : "build from $QueueProcessorPublishPath -> $AcrName"))
    Write-Host ("    {0,-28}: {1}" -f 'ACA environment', $AcaEnvironment)
    Write-Host ("    {0,-28}: {1}" -f 'Queue scaling mode', $QueueScalingMode)
}
if ($kvRequired) {
    $kvPurpose = $doEncryptResources ? 'encrypt now + runtime decrypt' : 'runtime decrypt of already-encrypted sources'
    Write-Host ("    {0,-28}: {1}" -f 'Key Vault', "$KeyVaultName / secret '$KeyVaultSecretName' ($kvPurpose)")
}
Write-Host ("    {0,-28}: {1}" -f 'LogDir', $LogDir)
Write-Host ("    {0,-28}: {1}" -f 'Run tag', "wwx-test-run=$runId  (rollback targets this tag only)")
Write-Host ("    {0,-28}: {1}" -f 'DryRun', $DryRun)
Write-Host ''
Write-Host '  ── Environment variables (App Settings) to apply ─────────────────────' -ForegroundColor White
foreach ($k in $appSettings.Keys) {
    $shown = if ($secretSettingNames.Contains($k)) { Get-MaskedValue $appSettings[$k] } else { $appSettings[$k] }
    Write-Host ("    {0,-32}= {1}" -f $k, $shown)
}
if ($enableAppInsights) {
    Write-Host ("    {0,-32}= {1}" -f 'WAREWOLF_APPINSIGHTS_CONNECTION_STRING', '(auto-read from the App Insights resource)')
}
if (-not $SkipAuthProvisioning) {
    Write-Host '    WAREWOLF_ENTRA_TENANT_ID/AUDIENCE/CLIENT_ID = (set by Configure-WwExecutionAuth.ps1)'
}
Write-Host ''

if (-not (Confirm-Yes 'Proceed with this deployment?' $true)) {
    Write-Note 'Aborted by user.'
    return
}

# ════════════════════════════════════════════════════════════════════════════
# Begin work — start the transcript log + summary (BOTH dry-run and real runs).
# A dry-run produces the same logs + summary as a real run (dry-run files carry a
# .dryrun. infix and the summary's "dryRun": true flag) so the rollback can be
# exercised from a dry-run summary.
# ════════════════════════════════════════════════════════════════════════════

$runStamp     = Get-Date -Format 'yyyyMMdd-HHmmss'
$transcriptOn = $false
if (-not (Test-Path -LiteralPath $LogDir)) { New-Item -ItemType Directory -Path $LogDir -Force | Out-Null }
$logInfix    = if ($DryRun) { 'dryrun.' } else { '' }
$logFile     = Join-Path $LogDir "deploy-WwExecutionEngine-$runStamp.${logInfix}log"
$summaryPath = Join-Path $LogDir "deploy-WwExecutionEngine-$runStamp.${logInfix}summary.json"
try { Start-Transcript -Path $logFile -Append | Out-Null; $transcriptOn = $true; Write-Ok "Logging to $logFile" }
catch { Write-Note "Transcript not started (an outer transcript may be active): $($_.Exception.Message)" }

# State Save-DeploySummary needs at ANY failure point (StrictMode-safe defaults).
# $StagingDir is finalised in Phase 3; the rest are refined as the run proceeds.
$baseUrl                = "https://$AppName.azurewebsites.net"
$StagingDir             = $PublishDir
$aiConnectionString     = $null
$script:DeployLastPhase = 'Phase 0.5  Plan'
$script:DeployStatus    = 'in-progress'

# Write an initial summary IMMEDIATELY — before the first mutating action — so even
# an instant failure in Phase 1 leaves the rollback an authoritative (if early)
# record of intent. Subsequent phases overwrite it with the latest created-map.
Save-DeploySummary -Status 'in-progress'

try {
    Invoke-Az @('account', 'set', '--subscription', $SubscriptionId) -Mutating | Out-Null

    # ════════════════════════════════════════════════════════════════════════
    # Phase 1 — Infrastructure
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 1  Infrastructure (resource group, storage, function app, App Insights)'
    $script:DeployLastPhase = 'Phase 1  Infrastructure'

    # 1.1 Resource group
    # NOTE on the created-map: we record created[x]=$true the moment we DECIDE to
    # create (before the mutating call), not after it returns. So if a create
    # half-succeeds and then throws, the rollback still sees it as a candidate.
    # The rollback re-verifies existence AND the run tag before deleting, so a
    # resource that never actually got created is harmlessly skipped, and a
    # pre-existing one (created=$false) is never touched.
    $rgExists = (Invoke-Az @('group', 'exists', '--name', $ResourceGroup)) -join ''
    if ($rgExists -eq 'true') {
        Write-Ok "Resource group '$ResourceGroup' already exists."
        $created['resourceGroup'] = $false
    } else {
        $created['resourceGroup'] = $true   # intent recorded BEFORE creating (crash-safe rollback)
        Write-Step "Creating resource group '$ResourceGroup' in '$Location'"
        Invoke-Az (@('group', 'create', '--name', $ResourceGroup, '--location', $Location, '--tags') + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Resource group created.'
    }

    # 1.2 Storage account
    $stExists = Invoke-Az @('storage', 'account', 'show', '--name', $StorageAccount, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($stExists) {
        Write-Ok "Storage account '$StorageAccount' already exists."
        $created['storageAccount'] = $false
    } else {
        $created['storageAccount'] = $true   # intent recorded BEFORE creating
        Write-Step "Creating storage account '$StorageAccount'"
        Invoke-Az (@(
            'storage', 'account', 'create',
            '--name', $StorageAccount, '--resource-group', $ResourceGroup, '--location', $Location,
            '--sku', 'Standard_LRS', '--kind', 'StorageV2', '--min-tls-version', 'TLS1_2', '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Storage account created.'
    }

    # 1.3 Function App (Consumption Y1, .NET 8 isolated, Functions v4)
    $appExists = Invoke-Az @('functionapp', 'show', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail
    if ($appExists) {
        Write-Ok "Function App '$AppName' already exists."
        $created['functionApp'] = $false
    } else {
        $created['functionApp'] = $true   # intent recorded BEFORE creating
        Write-Step "Creating Function App '$AppName' (Consumption Y1, dotnet-isolated 8, Functions v4)"
        Invoke-Az (@(
            'functionapp', 'create',
            '--name', $AppName, '--resource-group', $ResourceGroup, '--consumption-plan-location', $Location,
            '--storage-account', $StorageAccount, '--runtime', 'dotnet-isolated', '--runtime-version', '8',
            '--functions-version', '4', '--https-only', 'true', '--os-type', 'Windows', '--tags'
        ) + $ResourceTags) -Mutating | Out-Null
        Write-Ok 'Function App created.'
    }

    # 1.4 Application Insights
    $aiConnectionString = $null
    if ($enableAppInsights) {
        # Record created-vs-reused BEFORE the child script runs so rollback never
        # deletes an App Insights component that pre-existed this run.
        $aiPreExists = [bool](Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '-o', 'json') -AllowFail)
        $created['appInsights'] = (-not $aiPreExists)   # intent recorded BEFORE provisioning
        Invoke-ChildScript -Path $AppInsightsScript -Label 'Setup-ApplicationInsights.ps1' -Parameters @{
            ResourceGroup   = $ResourceGroup
            FunctionAppName = $AppName
            Location        = $Location
            AppInsightsName = $AppInsightsName
        }
        if ($created['appInsights'] -and -not $DryRun) {
            Invoke-Az (@('resource', 'tag', '--resource-group', $ResourceGroup, '--name', $AppInsightsName, '--resource-type', 'microsoft.insights/components', '--tags') + $ResourceTags) -Mutating | Out-Null
        }
        # Resolve the live connection string for the summary (WAREWOLF_APPINSIGHTS_
        # CONNECTION_STRING is set on the app by Setup-ApplicationInsights.ps1). On a
        # dry-run where the component does not yet exist, fall back to a placeholder.
        $aiConnectionString = (Invoke-Az @('monitor', 'app-insights', 'component', 'show', '--app', $AppInsightsName, '--resource-group', $ResourceGroup, '--query', 'connectionString', '-o', 'tsv') -AllowFail) -join ''
        if ([string]::IsNullOrWhiteSpace($aiConnectionString)) { $aiConnectionString = '<pending — created/read on a real run>' }
        if ($DryRun) {
            Write-Note ("Application Insights '{0}': dry-run made NO change. A real run would {1}, then set WAREWOLF_APPINSIGHTS_CONNECTION_STRING + ENABLEAPPLICATIONINSIGHTS=true." -f $AppInsightsName, ($aiPreExists ? 'REUSE this existing component' : 'CREATE it (not present)'))
        } else {
            Write-Ok ("Application Insights configured ({0} existing component)." -f ($aiPreExists ? 'reused' : 'created'))
        }
    } else {
        Write-Note 'Application Insights disabled.'
    }

    Save-DeploySummary -Status 'in-progress'   # persist created-map after infrastructure

    # ════════════════════════════════════════════════════════════════════════
    # Phase 2 — Auth provisioning (Entra ID + Easy Auth)
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 2  Auth provisioning (Entra ID + Easy Auth)'
    $script:DeployLastPhase = 'Phase 2  Auth provisioning'

    if ($SkipAuthProvisioning) {
        Write-Note 'Auth provisioning skipped (-SkipAuthProvisioning).'
        $created['entraApp'] = $false
    } else {
        # PROBE BEFORE PROVISIONING. Configure-WwExecutionAuth.ps1 is idempotent and happily REUSES an
        # existing registration, so "auth provisioning ran" never meant "this run created the app".
        #
        # This flag is not cosmetic. Rollback-WwExecutionEngine.ps1 treats created.entraApp = true as
        # OWNERSHIP and deletes the registration (Rollback:346-347, :406) - and a directory object
        # survives resource-group teardown, so it is the one artefact whose removal reaches outside the
        # deployment. Setting it unconditionally meant every redeploy of an EXISTING engine wrote a
        # summary arming a later rollback to destroy an app registration it did not create, taking down
        # every client that authenticates against it. Observed 2026-08-11: an in-place redeploy of
        # wwengine-e2e-th2teq reported "CREATED Entra app registration" for an app created days earlier.
        $entraDisplayName = "$AppName-auth"
        $entraPreExists   = $true   # fail-safe default: never ARM a delete on an unproven assumption

        $existingApp = Invoke-Az @('ad', 'app', 'list', '--display-name', $entraDisplayName,
                                   '--only-show-errors', '-o', 'json') -AllowFail
        if ($null -eq $existingApp) {
            Write-Note ("Could not determine whether Entra app '$entraDisplayName' already exists " +
                        '(the Graph probe failed). Recording it as PRE-EXISTING so rollback will not ' +
                        'delete it; pass -IncludeEntraApp to Rollback-WwExecutionEngine.ps1 if this run ' +
                        'really did create it.')
        } else {
            try {
                $entraPreExists = @(($existingApp | Out-String | ConvertFrom-Json)).Count -gt 0
            } catch {
                Write-Note "Entra app probe returned unparseable JSON; treating '$entraDisplayName' as pre-existing."
            }
        }

        $created['entraApp'] = -not $entraPreExists
        $created['entraAppDisplayName'] = $entraDisplayName
        Write-Ok ("Entra app registration '$entraDisplayName': " +
                  ($entraPreExists ? 'PRE-EXISTING (reused; teardown will NOT delete it)'
                                   : 'not present, will be created by this run'))
        $groupPermissions = @{}
        $userAssignments  = @()
        if ($AuthConfigPath) {
            if (-not (Test-Path -LiteralPath $AuthConfigPath)) { throw "AuthConfigPath not found: $AuthConfigPath" }
            Write-Step "Loading auth config from '$AuthConfigPath'"
            $authCfg = Get-Content -LiteralPath $AuthConfigPath -Raw | ConvertFrom-Json -AsHashtable
            if ($authCfg.ContainsKey('GroupPermissions') -and $authCfg.GroupPermissions) { $groupPermissions = [hashtable]$authCfg.GroupPermissions }
            if ($authCfg.ContainsKey('UserAssignments') -and $authCfg.UserAssignments)   { $userAssignments  = @($authCfg.UserAssignments) }
            Write-Ok ("Auth config loaded: {0} group(s), {1} user assignment(s)." -f $groupPermissions.Count, $userAssignments.Count)
        } else {
            Write-Note 'No -AuthConfigPath supplied; Configure-WwExecutionAuth.ps1 will provision auth with empty group/user maps.'
        }

        Invoke-ChildScript -Path $ConfigureAuthScript -Label 'Configure-WwExecutionAuth.ps1' -Parameters @{
            SubscriptionId    = $SubscriptionId
            TenantId          = $TenantId
            ResourceGroupName = $ResourceGroup
            FunctionAppName   = $AppName
            GroupPermissions  = $groupPermissions
            UserAssignments   = $userAssignments
            NonInteractive    = $true
            SkipSmokeTest     = $true
        }
        if (-not $DryRun -and (Test-Path -LiteralPath $AuthOutputPath)) {
            $authOut = Get-Content -LiteralPath $AuthOutputPath -Raw | ConvertFrom-Json
            Write-Ok "Entra app provisioned. ClientId: $($authOut.ClientId)  Audience: $($authOut.Audience)"
        }
    }

    Save-DeploySummary -Status 'in-progress'   # persist created-map after auth provisioning

    # ════════════════════════════════════════════════════════════════════════
    # Phase 3 — Stage package, encrypt, apply environment variables
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 3  Stage package, encrypt, apply environment variables'
    $script:DeployLastPhase = 'Phase 3  Stage package'

    # 3.0 Resolve the STAGING directory — a FRESH, dedicated copy under the OS temp
    # dir that we prepare and (on a real run) upload as the final artifact. The
    # operator's publish OUTPUT is NEVER mutated, so the zip is built cleanly on every
    # run and the engine + JobProcessor always stage in SEPARATE directories.
    #   real run -> removed after a successful upload (Phase 4).
    #   dry run  -> kept as the inspectable preview artifact (path printed at the end).
    $stageSuffix = if ($DryRun) { '-dryrun' } else { '' }
    $StagingDir  = Join-Path ([System.IO.Path]::GetTempPath()) "wwexecutionengine-stage-$AppName-$runStamp$stageSuffix"
    Write-Step "$($DryRun ? 'Dry-run: building preview artifact' : 'Staging deploy artifact') in '$StagingDir' (your publish output is left untouched)"
    if (Test-Path -LiteralPath $StagingDir) { Remove-Item -LiteralPath $StagingDir -Recurse -Force }
    New-Item -ItemType Directory -Path $StagingDir -Force | Out-Null
    if ($publishIsZip) {
        Expand-Archive -LiteralPath $PublishPath -DestinationPath $StagingDir -Force
    } else {
        Copy-Item -Path (Join-Path $PublishDir '*') -Destination $StagingDir -Recurse -Force
    }
    Write-Ok "Publish output copied to staging dir '$StagingDir'."
    if (-not (Test-Path -LiteralPath $StagingDir)) {
        throw "Staging directory '$StagingDir' does not exist after resolution."
    }

    # 3.0b OPTIONAL host.json logLevel alignment (-AlignHostJsonLogLevel).
    # NOT needed for the engine's own logging: the isolated worker's console + App
    # Insights pipelines are driven by EXECUTIONLOGLEVEL in code (Program.cs) and do
    # NOT read host.json. This only tunes the Functions HOST process log verbosity.
    if ($AlignHostJsonLogLevel) {
        $hostJsonPath = Join-Path $StagingDir 'host.json'
        $melLevel     = Convert-ToMelLevel $ExecutionLogLevel
        if (Test-Path -LiteralPath $hostJsonPath) {
            Write-Step "Aligning host.json logLevel (default + Warewolf.*) -> '$melLevel'"
            Update-HostJsonLogLevel -HostJsonPath $hostJsonPath -ExecutionLogLevel $ExecutionLogLevel | Out-Null
            Write-Ok 'host.json logLevel aligned.'
        } else {
            Write-Note "host.json not found in staging dir; skipping logLevel alignment."
        }
    } else {
        Write-Note 'host.json logLevel alignment skipped (worker logging is code-driven; pass -AlignHostJsonLogLevel to also tune the host process).'
    }

    # 3.1 secure.config — encrypted: stage as-is; plaintext: AES-encrypt automatically.
    # (Staging is a local file op — performed in BOTH dry-run and real, into $StagingDir.)
    if ($SecureConfigPath) {
        $secureDest = Join-Path $StagingDir 'secure.config'
        if ($secureConfigKind -eq 'Encrypted') {
            Write-Step 'Staging already-encrypted secure.config (validated decryptable; not re-encrypted)'
            Copy-Item -LiteralPath $SecureConfigPath -Destination $secureDest -Force
        } else {
            Write-Step 'Encrypting plaintext secure.config (AES) and staging'
            Protect-SecureConfig -InPath $SecureConfigPath -OutPath $secureDest
        }
        Write-Ok 'secure.config staged.'
    } else {
        Write-Note 'No secure.config supplied; engine runs in open-access mode.'
    }

    # 3.1b Warewolf License.secureconfig — copy to the staging root (license gate).
    if ($LicenseConfigPath) {
        $licenseDest = Join-Path $StagingDir 'Warewolf License.secureconfig'
        Write-Step "Staging license -> '$licenseDest'"
        Copy-Item -LiteralPath $LicenseConfigPath -Destination $licenseDest -Force
        Write-Ok 'Warewolf License.secureconfig staged.'
    } else {
        Write-Note 'No license supplied; engine license check (default ON) may fail at startup.'
    }

    # 3.2 Workflow resources.
    $publishResources = Join-Path $StagingDir 'Resources'
    if ($WorkflowsSourcePath) {
        if (-not (Test-Path -LiteralPath $WorkflowsSourcePath)) { throw "WorkflowsSourcePath not found: $WorkflowsSourcePath" }
        Write-Step "Staging workflow resources -> '$publishResources'"
        if (-not (Test-Path -LiteralPath $publishResources)) { New-Item -ItemType Directory -Path $publishResources -Force | Out-Null }
        Copy-Item -Path (Join-Path $WorkflowsSourcePath '*') -Destination $publishResources -Recurse -Force
        Write-Ok 'Workflow resources staged.'
    } else {
        Write-Note 'No workflow source supplied.'
    }

    # 3.3 Key Vault setup. KV is wired whenever $kvRequired (so the engine can
    # DECRYPT at runtime). The dev-side "Secrets Officer" role + key generation are
    # added ONLY when THIS run encrypts ($doEncryptResources).
    if ($kvRequired) {
        $vaultExists = [bool](Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '-o', 'json') -AllowFail)
        $created['keyVault'] = (-not $vaultExists)   # intent recorded BEFORE creating
        if (-not $vaultExists) {
            if (-not $doEncryptResources) {
                throw ("Key Vault '$KeyVaultName' does not exist, and -EncryptResources was not set. " +
                       "A vault is required so the engine can decrypt already-encrypted sources at runtime. " +
                       "Run once with -EncryptResources to create the vault + key, or supply an existing vault.")
            }
            Write-Step "Creating Key Vault '$KeyVaultName' (RBAC authorization)"
            Invoke-Az (@('keyvault', 'create', '--name', $KeyVaultName, '--resource-group', $ResourceGroup, '--location', $Location, '--enable-rbac-authorization', 'true', '--tags') + $ResourceTags) -Mutating | Out-Null
            $GenerateNewKey = $true
        }
        Write-Step 'Enabling system-assigned managed identity on the Function App'
        $idJson = Invoke-Az @('functionapp', 'identity', 'assign', '--name', $AppName, '--resource-group', $ResourceGroup, '-o', 'json') -Mutating
        $funcPrincipalId = if ($DryRun) { '<dryrun-principal-id>' } else { ($idJson | ConvertFrom-Json).principalId }
        $vaultId = if ($DryRun) { '<dryrun-vault-id>' } else { (Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '--query', 'id', '-o', 'tsv')) -join '' }

        # ALWAYS: the engine's managed identity must read the key at runtime.
        Write-Step 'Assigning RBAC: Key Vault Secrets User -> Function App managed identity'
        Invoke-Az @('role', 'assignment', 'create', '--role', 'Key Vault Secrets User', '--assignee', $funcPrincipalId, '--scope', $vaultId) -Mutating | Out-Null

        # ONLY when encrypting now: the operator needs Secrets Officer to read/write the key.
        if ($doEncryptResources) {
            $devOid = if ($DryRun) { '<dryrun-dev-oid>' } else { (Invoke-Az @('ad', 'signed-in-user', 'show', '--query', 'id', '-o', 'tsv')) -join '' }
            Write-Step 'Assigning RBAC: Key Vault Secrets Officer -> current user (for encryption)'
            Invoke-Az @('role', 'assignment', 'create', '--role', 'Key Vault Secrets Officer', '--assignee', $devOid, '--scope', $vaultId) -Mutating | Out-Null
            if (-not $DryRun) { Write-Note 'RBAC propagation can take ~1-2 min before the secret can be written.'; Start-Sleep -Seconds 30 }
        }
    }

    # Decide whether REAL encryption can run now (only relevant when encrypting;
    # encryption can't be faked — faked output would not decrypt):
    #   real run -> yes (KV exists/created; key generated on first encrypt)
    #   dry run  -> only when the KV AND the secret already exist (a dry-run never
    #               creates the vault or writes a key); otherwise encryption is
    #               deferred to the real run and the source is staged in plaintext.
    $keyReachable = $false
    if ($doEncryptResources) {
        if (-not $DryRun) {
            $keyReachable = $true
        } elseif ([bool](Invoke-Az @('keyvault', 'show', '--name', $KeyVaultName, '-o', 'json') -AllowFail)) {
            $keyReachable = [bool](Invoke-Az @('keyvault', 'secret', 'show', '--vault-name', $KeyVaultName, '--name', $KeyVaultSecretName, '-o', 'json') -AllowFail)
        }
    }

    # Local helper: encrypt a path, then OPTIONALLY (-VerifyDecryption) prove the
    # engine's key decrypts it — verification is IN MEMORY (no plaintext on disk).
    function Invoke-EncryptAndVerify {
        param([string] $TargetPath, [string] $Label)
        $encryptParams = @{ FilePath = $TargetPath; VaultName = $KeyVaultName; SecretName = $KeyVaultSecretName; NonInteractive = $true; NoBackup = $true }
        if ($GenerateNewKey) { $encryptParams['GenerateKeys'] = $true }
        Write-Step "Encrypting ($Label) via Key Vault '$KeyVaultName'"
        & $EncryptScript @encryptParams
        if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Encrypt-Config.ps1 ($Label) failed ($LASTEXITCODE)." }
        if ($VerifyDecryption) {
            # Round-trip verify IN MEMORY — never writes plaintext to disk (-VerifyOnly).
            & $EncryptScript -FilePath $TargetPath -VaultName $KeyVaultName -SecretName $KeyVaultSecretName -VerifyOnly -NonInteractive
            if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Encrypt-Config.ps1 (verify $Label) failed ($LASTEXITCODE)." }
            Write-Ok "Verified (in-memory) the engine's Key Vault key decrypts: $Label"
        } else {
            Write-Note "Decrypt verification skipped for $Label (pass -VerifyDecryption to enable)."
        }
        # First encryption generated the key; subsequent calls must reuse it.
        $script:GenerateNewKey = $false
    }

    # 3.4 Workflow resources — encrypt ONLY when -EncryptResources is set; otherwise
    # the staged sources are left AS-IS (assumed already encrypted on a prior run).
    if (-not $doEncryptResources) {
        Write-Note 'Source encryption disabled (-EncryptResources off) — workflow resources staged AS-IS (assumed already encrypted).'
    } elseif (-not (Test-Path -LiteralPath $publishResources)) {
        Write-Note "Encryption requested but no Resources folder at '$publishResources'; nothing to encrypt."
    } elseif (-not $keyReachable) {
        Write-Note 'Workflow resources staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
    } else {
        Write-Step 'Encrypting workflow resource connection strings (WFAES via Key Vault)'
        Invoke-EncryptAndVerify -TargetPath $publishResources -Label 'workflow resources'
        Write-Ok 'Workflow resources encrypted.'
    }

    # 3.5 Elasticsearch source — ALWAYS staged when enabled; encrypted ONLY when
    # -EncryptResources is set (otherwise staged as-is / assumed already encrypted).
    if ($enableEs) {
        $settingsDir  = Join-Path $StagingDir 'Settings'
        $esDest       = Join-Path $settingsDir $ElasticsearchBiteName
        Write-Step "Staging '$ElasticsearchBiteName' -> '$esDest'"
        if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }
        Copy-Item -LiteralPath $ElasticsearchSourcePath -Destination $esDest -Force
        if (-not $doEncryptResources) {
            Write-Note 'Elasticsearch source staged AS-IS (source encryption disabled; assumed already encrypted).'
        } elseif (-not $keyReachable) {
            Write-Note 'Elasticsearch source staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
        } else {
            Write-Step 'Encrypting Elasticsearch source connection string (WFAES via Key Vault)'
            Invoke-EncryptAndVerify -TargetPath $esDest -Label 'Elasticsearch source'
            Write-Ok 'Elasticsearch source encrypted.'
        }
    }

    # 3.5b Persistence settings pair (suspend/resume). persistencesettings.json is
    # staged AS-IS (flags only); the DbSource ConnectionString is WFAES-encrypted with
    # the SAME pass as the Elasticsearch source — encrypted ONLY when -EncryptResources
    # (otherwise staged as-is / assumed already encrypted). The engine's resume route
    # (and the JobProcessor) decrypt it at runtime through the shared AesDecryptHook.
    if ($enablePersistence) {
        $settingsDir = Join-Path $StagingDir 'Settings'
        if (-not (Test-Path -LiteralPath $settingsDir)) { New-Item -ItemType Directory -Path $settingsDir -Force | Out-Null }

        $psDest = Join-Path $settingsDir $PersistenceSettingsName
        Write-Step "Staging '$PersistenceSettingsName' -> '$psDest'"
        Copy-Item -LiteralPath $PersistenceSettingsPath -Destination $psDest -Force

        $dbDest = Join-Path $settingsDir $PersistenceDbSourceName
        Write-Step "Staging '$PersistenceDbSourceName' -> '$dbDest'"
        Copy-Item -LiteralPath $PersistenceDbSourcePath -Destination $dbDest -Force
        if (-not $doEncryptResources) {
            Write-Note 'Persistence DbSource staged AS-IS (source encryption disabled; assumed already encrypted).'
        } elseif (-not $keyReachable) {
            Write-Note 'Persistence DbSource staged UNENCRYPTED — Key Vault key not reachable; encryption deferred to a real run.'
        } else {
            Write-Step 'Encrypting persistence DbSource connection string (WFAES via Key Vault)'
            Invoke-EncryptAndVerify -TargetPath $dbDest -Label 'persistence DbSource'
            Write-Ok 'Persistence DbSource encrypted.'
        }
    }

    # 3.6 Workflow index — generate workflow-index.json over the STAGED Resources so
    # it ships INSIDE the publish zip built in Phase 4. The engine reads it at
    # startup for O(1) lookups and falls back to a disk scan only when absent
    # (WorkflowIndex.cs). Generated AFTER staging + encryption (encryption renames
    # nothing, so keys are identical). Pure LOCAL file op — runs in BOTH dry-run
    # (into the preview artifact) and real, mirroring resource staging above.
    if (Test-Path -LiteralPath $publishResources) {
        if (Test-Path -LiteralPath $WorkflowIndexScript) {
            $workflowIndexPath = Join-Path $publishResources 'workflow-index.json'
            Write-Step "Generating workflow index -> '$workflowIndexPath'"
            & $WorkflowIndexScript -ResourcesDir $publishResources -OutputPath $workflowIndexPath | Out-Null
            if (Test-Path -LiteralPath $workflowIndexPath) {
                $wfIndexCount = @(([System.IO.File]::ReadAllText($workflowIndexPath) | ConvertFrom-Json).PSObject.Properties).Count
                Write-Ok "workflow-index.json generated ($wfIndexCount entr$(if ($wfIndexCount -eq 1) { 'y' } else { 'ies' })) — bundled into the publish zip."
            } else {
                Write-Note 'No .bite/.xml workflows under Resources — no index written (engine will disk-scan at startup).'
            }
        } else {
            Write-Note "Generate-WorkflowIndex.ps1 not found at '$WorkflowIndexScript'; skipping index (engine will disk-scan at startup)."
        }
    } else {
        Write-Note 'No staged Resources folder — workflow index generation skipped.'
    }

    # 3.7 Apply environment variables (App Settings).
    Write-Step 'Applying environment variables (App Settings)'
    $settingsArgs = @($appSettings.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" })
    Invoke-Az (@('functionapp', 'config', 'appsettings', 'set', '--name', $AppName, '--resource-group', $ResourceGroup, '--settings') + $settingsArgs + @('-o', 'none')) -Mutating | Out-Null
    Write-Ok "$($appSettings.Count) app setting(s) applied."

    Save-DeploySummary -Status 'in-progress'   # persist created-map + staging dir before deploy

    # ════════════════════════════════════════════════════════════════════════
    # Phase 4 — Deploy the package directory to the Function App
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 4  Deploy package to Function App'
    $script:DeployLastPhase = 'Phase 4  Deploy package'

    # We deploy an ALREADY-PUBLISHED artifact, so az zip-deploy (config-zip) is the
    # correct, robust method. 'func azure functionapp publish' expects a PROJECT
    # source dir it can language-detect/build and fails on a pre-built package
    # ("Can't determine project language… Worker runtime cannot be 'None'"), so it
    # is NOT used by Auto — only when explicitly requested via -PublishMethod Func.
    $useFunc = $false
    switch ($PublishMethod) {
        'Func' {
            if (-not (Test-CommandExists 'func')) { throw "PublishMethod 'Func' requires Azure Functions Core Tools (func). Install: https://aka.ms/azfunc-install" }
            $useFunc = $true
        }
        'Auto' { $useFunc = $false }   # pre-built artifact -> zip-deploy
        'Zip'  { $useFunc = $false }
    }

    if ($useFunc) {
        # Advanced/opt-in: pass the language + --no-build so func does not try to
        # detect/build the pre-built package.
        if ($DryRun) {
            Write-Host "      [DRYRUN] (cd '$StagingDir') func azure functionapp publish $AppName --dotnet-isolated --no-build" -ForegroundColor DarkGray
        } else {
            Push-Location $StagingDir
            try {
                & func azure functionapp publish $AppName --dotnet-isolated --no-build
                if ($LASTEXITCODE -ne 0) { throw "func publish failed ($LASTEXITCODE)." }
            } finally { Pop-Location }
            Write-Ok 'Deployed via func.'
        }
    } else {
        $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) "wwexecution-$AppName-$runStamp.zip"
        if ($DryRun) {
            Write-Host "      [DRYRUN] Compress-Archive '$StagingDir\*' -> '$zipPath'" -ForegroundColor DarkGray
            Write-Host "      [DRYRUN] az functionapp deployment source config-zip --name $AppName --resource-group $ResourceGroup --src '$zipPath'" -ForegroundColor DarkGray
        } else {
            if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force }
            Write-Step 'Creating deployment package'
            Compress-Archive -Path (Join-Path $StagingDir '*') -DestinationPath $zipPath -Force
            try {
                Invoke-Az @('functionapp', 'deployment', 'source', 'config-zip', '--name', $AppName, '--resource-group', $ResourceGroup, '--src', $zipPath) -Mutating | Out-Null
                Write-Ok 'Deployed via az zip-deploy.'
            } finally { if (Test-Path -LiteralPath $zipPath) { Remove-Item -LiteralPath $zipPath -Force } }
        }
    }

    # Real run: the staging copy has been uploaded — remove it (it lives under the OS
    # temp dir, not the publish tree). The dry-run preview is intentionally KEPT.
    if (-not $DryRun -and (Test-Path -LiteralPath $StagingDir)) {
        Remove-Item -LiteralPath $StagingDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Ok "Staging dir removed (publish output was never modified)."
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 5 — Verify
    # ════════════════════════════════════════════════════════════════════════
    Write-Phase 'Phase 5  Verify'
    $script:DeployLastPhase = 'Phase 5  Verify'

    Write-Ok "Function App endpoint: $baseUrl"
    Write-Host "    Discovery (apis.json): $baseUrl/apis.json" -ForegroundColor Gray

    if ($DryRun) {
        Write-Note 'DryRun: skipping HTTP probe.'
    } elseif (Confirm-Yes 'Run a quick HTTP probe against /apis.json now?' $true) {
        try {
            $resp = Invoke-WebRequest -Uri "$baseUrl/apis.json" -Method GET -MaximumRedirection 0 -SkipHttpErrorCheck
            Write-Host "    GET /apis.json -> $([int]$resp.StatusCode)" -ForegroundColor Green
            Write-Note 'A cold start can take a moment; retry if you see 5xx immediately after deploy.'
        } catch {
            Write-Note "Probe could not complete: $($_.Exception.Message)"
        }
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 6 — (optional) ExecutionEngineJobProcessor companion deploy
    # ════════════════════════════════════════════════════════════════════════
    if ($DeployJobProcessor) {
        Write-Phase 'Phase 6  Deploy ExecutionEngineJobProcessor (companion)'
        $script:DeployLastPhase = 'Phase 6  JobProcessor'

        if (-not (Test-Path -LiteralPath $JobProcessorScript)) {
            throw "Deploy-WwJobProcessor.ps1 not found at '$JobProcessorScript'."
        }

        # Pass the shared context; Deploy-WwJobProcessor.ps1 prompts (interactively) for
        # anything omitted here — including its own AppName / PublishPath / StorageAccount.
        # The DbSource is re-staged + (re-)encrypted by the child from the SAME operator
        # source file, so it stands alone even if run separately later.
        $jpParams = [ordered]@{
            SubscriptionId          = $SubscriptionId
            TenantId                = $TenantId
            ResourceGroup           = $ResourceGroup
            Location                = $Location
            EngineResumeBaseUrl     = $baseUrl
            PersistenceSettingsPath = $PersistenceSettingsPath
            PersistenceDbSourcePath = $PersistenceDbSourcePath
            EncryptResources        = [bool]$doEncryptResources
        }
        if ($JobProcessorAppName)        { $jpParams['AppName']        = $JobProcessorAppName }
        if ($JobProcessorPublishPath)    { $jpParams['PublishPath']    = $JobProcessorPublishPath }
        if ($JobProcessorStorageAccount) { $jpParams['StorageAccount'] = $JobProcessorStorageAccount }
        if ($EngineResumeScope)          { $jpParams['EngineResumeScope'] = $EngineResumeScope }
        if ($kvRequired) {
            $jpParams['KeyVaultName']       = $KeyVaultName
            $jpParams['KeyVaultSecretName'] = $KeyVaultSecretName
        }
        if ($VerifyDecryption)  { $jpParams['VerifyDecryption']  = $true }
        if ($enableAppInsights) { $jpParams['EnableAppInsights'] = $true }
        if ($NonInteractive)    { $jpParams['NonInteractive']    = $true }
        if ($DryRun)            { $jpParams['DryRun']            = $true }

        Invoke-ChildScript -Path $JobProcessorScript -Label 'Deploy-WwJobProcessor.ps1' -Parameters $jpParams
        Write-Ok 'JobProcessor companion deploy invoked.'
        Write-Note 'Reminder: grant the JobProcessor MI the engine role Warewolf_JobProcessor (see runbook).'
    }

    # ════════════════════════════════════════════════════════════════════════
    # Phase 7 — (optional) RabbitMQ QueueProcessors, one Container App per trigger
    # ════════════════════════════════════════════════════════════════════════
    if ($DeployRabbitMqTriggers) {
        Write-Phase 'Phase 7  Deploy RabbitMQ QueueProcessors (companion, one per trigger)'
        $script:DeployLastPhase = 'Phase 7  QueueProcessors'

        # The engine is deployed by now, so its URL is known and can be handed to every worker.
        $queueEngineBaseUrl = "https://$AppName.azurewebsites.net"

        # Prefer an explicitly supplied app id; otherwise derive it from the resume scope
        # (api://<app-id>/.default) that the JobProcessor path already uses. If neither is
        # available the child prompts, rather than guessing an audience.
        $queueEngineAppId = $QueueEngineResourceAppId
        if (-not $queueEngineAppId -and $EngineResumeScope -match 'api://([^/]+)/') {
            $queueEngineAppId = $Matches[1]
        }

        $qpFailures = 0

        foreach ($qpTriggerFile in $script:QueueTriggerFiles) {
            $qpLabel = [System.IO.Path]::GetFileNameWithoutExtension($qpTriggerFile)

            $qpParams = [ordered]@{
                ResourceGroup                 = $ResourceGroup
                Location                      = $Location
                AcaEnvironment                = $AcaEnvironment
                TriggerFilePath               = $qpTriggerFile
                QueueSourcePath               = $QueueSourcePath
                EngineBaseUrl                 = $queueEngineBaseUrl
                ScalingMode                   = $QueueScalingMode
                ExecutionLogLevel             = $ExecutionLogLevel
                # Always pass the tenant explicitly. The worker acquires an app-only engine token
                # with its managed identity, and a BLANK tenant is legal only for a
                # system-assigned MI - anywhere else the credential chain fails with
                # 'Invalid tenant id provided', which looks like a missing app role rather than
                # missing config. $TenantId is already resolved from `az account show` above.
                EngineTenantId                = $TenantId
            }

            if ($queueEngineAppId)          { $qpParams['EngineResourceAppId'] = $queueEngineAppId }
            if ($EngineResumeScope)          { $qpParams['EngineScope']         = $EngineResumeScope }
            if ($QueueProcessorImage)        { $qpParams['Image']               = $QueueProcessorImage }
            if ($QueueProcessorPublishPath)  { $qpParams['PublishPath']         = $QueueProcessorPublishPath }
            if ($AcrName)                    { $qpParams['AcrName']             = $AcrName }
            if ($RabbitMqSecretUri)          { $qpParams['RabbitMqSecretUri']   = $RabbitMqSecretUri }
            if ($kvRequired) {
                $qpParams['KeyVaultName']       = $KeyVaultName
                $qpParams['KeyVaultSecretName'] = $KeyVaultSecretName
                $qpParams['EncryptStagedSettings'] = $true
            }
            if ($enableAppInsights) { $qpParams['EnableAppInsights'] = $true }
            if ($NonInteractive)    { $qpParams['NonInteractive']    = $true }
            if ($DryRun)            { $qpParams['DryRun']            = $true }

            try {
                Invoke-ChildScript -Path $QueueProcessorScript `
                    -Label "Deploy-WwQueueProcessor.ps1 ($qpLabel)" -Parameters $qpParams
                $script:QueueProcessorApps += @{ trigger = $qpLabel; file = $qpTriggerFile; status = 'invoked' }
            }
            catch {
                $qpFailures++
                $script:QueueProcessorApps += @{
                    trigger = $qpLabel; file = $qpTriggerFile; status = 'failed'; error = $_.Exception.Message
                }
                Write-Note "QueueProcessor deploy failed for '$qpLabel': $($_.Exception.Message)"

                if (-not $ContinueOnQueueTriggerError) {
                    throw ("QueueProcessor deploy failed for '$qpLabel' and -ContinueOnQueueTriggerError " +
                           'was not supplied, so the remaining triggers were skipped. The engine deploy ' +
                           'itself completed successfully.')
                }
            }
        }

        Write-Ok "QueueProcessor companion deploy invoked for $($script:QueueTriggerFiles.Count) trigger(s)."
        Write-Note 'Reminder: grant EACH QueueProcessor MI the engine role Warewolf_QueueProcessor, and add'
        Write-Note 'a PER-WORKFLOW Execute row to secure.config for each trigger workflow (runbook section 8).'

        if ($qpFailures -gt 0) {
            Write-Note "$qpFailures trigger deploy(s) failed - see the summary JSON."
        }
    }

    # ── Final summary — completed status ─────────────────────────────────────
    # (Incremental in-progress summaries were already written after each phase;
    # this records the terminal 'completed' state for BOTH dry-run and real runs.)
    Save-DeploySummary -Status 'completed'
    Write-Ok "Summary written to $summaryPath"

    Write-Phase ($DryRun ? 'Dry-run complete (no cloud changes made)' : 'Deployment complete')
    Write-Host "  App      : $AppName" -ForegroundColor White
    Write-Host "  RG       : $ResourceGroup" -ForegroundColor White
    Write-Host "  Endpoint : $baseUrl" -ForegroundColor White
    Write-Host "  Artifact : $($DryRun ? "$StagingDir  (dry-run preview)" : 'staged under OS temp, uploaded, then removed (publish output untouched)')" -ForegroundColor White
    Write-Host "  Summary  : $summaryPath" -ForegroundColor White
    if (-not $SkipAuthProvisioning -and (Test-Path -LiteralPath $AuthOutputPath)) {
        Write-Host "  Auth out : $AuthOutputPath" -ForegroundColor White
    }

    # ── Resources manipulated, with reachable URLs ───────────────────────────
    # Driven by the SAME `created` map that Rollback-WwExecutionEngine.ps1 consumes, so what is
    # printed here and what teardown will remove can never disagree. Pre-existing resources are
    # listed as REUSED precisely so nobody mistakes them for things this run owns.
    # NOTE the local names: $created is the run's ownership MAP (consumed by the rollback script) and
    # must not be shadowed here.
    $verb = $DryRun ? 'WOULD CREATE' : 'CREATED'
    Write-Host ''
    Write-Host '  -- Resources manipulated --' -ForegroundColor Cyan
    $createdList = [System.Collections.Generic.List[string]]::new()
    $reusedList  = [System.Collections.Generic.List[string]]::new()
    $entraName   = $created['entraAppDisplayName']
    if (-not $entraName) { $entraName = "$AppName-auth" }
    foreach ($pair in @(
        @{ Key = 'resourceGroup';  Kind = 'Resource group';         Name = $ResourceGroup }
        @{ Key = 'storageAccount'; Kind = 'Storage account';        Name = $StorageAccount }
        @{ Key = 'functionApp';    Kind = 'Function App';           Name = $AppName }
        @{ Key = 'appInsights';    Kind = 'App Insights';           Name = $AppInsightsName }
        @{ Key = 'keyVault';       Kind = 'Key Vault';              Name = $KeyVaultName }
        @{ Key = 'entraApp';       Kind = 'Entra app registration'; Name = $entraName }
    )) {
        if (-not $pair.Name) { continue }
        if ($created[$pair.Key]) { $createdList.Add("$($pair.Kind): $($pair.Name)") }
        else                     { $reusedList.Add("$($pair.Kind): $($pair.Name)") }
    }
    if ($createdList.Count) {
        Write-Host "     $verb" -ForegroundColor Green
        $createdList | ForEach-Object { Write-Host "       $_" -ForegroundColor Green }
    }
    if ($reusedList.Count) {
        Write-Host '     REUSED (pre-existing - teardown will NOT remove these)' -ForegroundColor Gray
        $reusedList | ForEach-Object { Write-Host "       $_" -ForegroundColor Gray }
    }
    Write-Host '     UPDATED' -ForegroundColor Cyan
    Write-Host "       Function App settings: $($appSettings.Count) applied to $AppName" -ForegroundColor Cyan
    if (-not $SkipAuthProvisioning) { Write-Host "       Easy Auth + Entra app roles on $AppName" -ForegroundColor Cyan }
    Write-Host ''
    Write-Host '     ENDPOINTS' -ForegroundColor Blue
    Write-Host "       Engine     : $baseUrl" -ForegroundColor Blue
    Write-Host "       Discovery  : $baseUrl/apis.json" -ForegroundColor Blue
    Write-Host "       Public     : $baseUrl/Public/{workflow}.json     (anonymous)" -ForegroundColor Blue
    Write-Host "       Secure     : $baseUrl/Secure/{workflow}.json     (Entra JWT)" -ForegroundColor Blue
    Write-Host "       Services   : $baseUrl/Services/{workflow}.json   (function key)" -ForegroundColor Blue
    Write-Host "       Portal     : https://portal.azure.com/#@/resource/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Web/sites/$AppName" -ForegroundColor Blue
    Write-Host ''
}
catch {
    # CRASH-SAFE summary: whatever phase we failed in, persist the latest created-map
    # so Rollback-WwExecutionEngine.ps1 can still tear down exactly what THIS run
    # created (and nothing it found pre-existing). Then re-throw so the failure and
    # its non-zero exit propagate exactly as before.
    $script:DeployStatus = 'failed'
    $errMsg = $_.Exception.Message
    Write-Note "Deployment FAILED in '$script:DeployLastPhase': $errMsg"
    try {
        Save-DeploySummary -Status 'failed' -ErrorMessage $errMsg
        Write-Note "Failure summary written to $summaryPath (use it with Rollback-WwExecutionEngine.ps1 -SummaryPath)."
    } catch {
        Write-Note "Could not write failure summary: $($_.Exception.Message)"
    }
    throw
}
finally {
    if ($transcriptOn) { try { Stop-Transcript | Out-Null } catch { } }
}
