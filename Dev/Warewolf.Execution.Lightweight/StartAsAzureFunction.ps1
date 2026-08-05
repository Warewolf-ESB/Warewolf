# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
Param(
  [switch]$DoExit,
  [string]$ResourcesPath,
  [string]$FunctionPath,
  [switch]$Cleanup,
  [int]$Port            = 7071,
  [int]$TimeoutSeconds  = 120,
  [int]$PollIntervalSec = 5,

  # Path to the secure.config file to deploy with the function host.
  # When supplied the file is copied to the function directory and
  # WAREWOLF_SECURE_CONFIG is set so both the host and the test ClassInit
  # read the same secret key.
  # When omitted and no secure.config is already in the function directory
  # Public is granted Administrator permissions.
  [string]$SecureConfigPath,

  # Azure Key Vault name from which to retrieve (or create) the JWT HMAC
  # secret key used to sign test tokens.  When supplied the script calls
  # 'az keyvault secret show/set' to persist the key across runs so every
  # CI agent signs tokens with the same secret -- matching what the func
  # host loaded from the generated secure.config.
  # Requires the az CLI to be installed and logged in, with Get/Set
  # permissions on the vault for the calling identity.
  # When empty the script falls back to local key generation.
  [string]$VaultName,
  [string]$SecretName = 'WWExecutionEngineTestSecret',

  # Path to a pre-built 'Warewolf License.secureconfig' to copy into the
  # function directory.  When omitted a test license (Status=Active) is
  # auto-generated so /IsLicensed returns { isLicensed: true }.
  [string]$LicenseConfigPath
)

# -----------------------------------------------------------------------------
# region: Pipeline Logging Helpers
# -----------------------------------------------------------------------------

function Write-PipelineSection ([string]$Message) {
    Write-Host ""
    Write-Host "##[section]$Message"
}

function Write-PipelineWarning ([string]$Message) {
    Write-Host "##vso[task.logissue type=warning]$Message"
}

function Write-PipelineError ([string]$Message) {
    Write-Host "##vso[task.logissue type=error]$Message"
}

function Fail-Pipeline ([string]$Message) {
    Write-PipelineError $Message
    Write-Host "##vso[task.complete result=Failed;]$Message"

    # Dump last 50 lines of func output to help diagnose failures
    $outLog = "$PSScriptRoot\TestResults\AzureFunctionOutput.txt"
    $errLog = "$PSScriptRoot\TestResults\AzureFunctionError.txt"
    if (Test-Path $outLog) {
        Write-Host "--- AzureFunctionOutput.txt (last 50 lines) ---"
        Get-Content $outLog -ErrorAction SilentlyContinue | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
    }
    if (Test-Path $errLog) {
        Write-Host "--- AzureFunctionError.txt (last 50 lines) ---"
        Get-Content $errLog -ErrorAction SilentlyContinue | Select-Object -Last 50 | ForEach-Object { Write-Host $_ }
    }
    exit 1
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Secure-config helpers
# -----------------------------------------------------------------------------
#
# Implements the same AES-CBC algorithm as SecurityEncryption.cs so that a
# portable test secure.config can be generated in pure PowerShell without
# requiring a running Warewolf server.
#
# Fixed key material mirrors SecurityEncryption constants exactly so that
# SecureConfigLoader.ReadConfig can decrypt the file on any machine.
# -----------------------------------------------------------------------------

function Invoke-SecurityEncrypt {
    <#
    .SYNOPSIS
        Encrypts $PlainText using the same AES-CBC algorithm as SecurityEncryption.cs.
    #>
    param([string]$PlainText)

    $initVectorBytes = [System.Text.Encoding]::ASCII.GetBytes("@1B2c3D4e5F6g7H8")
    $saltValueBytes  = [System.Text.Encoding]::ASCII.GetBytes("s@1tValue")
    $plainTextBytes  = [System.Text.Encoding]::UTF8.GetBytes($PlainText)

    # PasswordDeriveBytes with SHA1 / 2 iterations — matches SecurityEncryption.cs
    $password = New-Object System.Security.Cryptography.PasswordDeriveBytes(
        "Pas5pr@se", $saltValueBytes, "SHA1", 2)
    $keyBytes = $password.GetBytes(32)   # 256-bit key

    # Aes.Create() is the .NET 8 replacement for the deprecated RijndaelManaged;
    # with a 128-bit block (default), CBC mode, and zero padding it is identical.
    $aes         = [System.Security.Cryptography.Aes]::Create()
    $aes.Mode    = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::Zeros

    $encryptor  = $aes.CreateEncryptor($keyBytes, $initVectorBytes)
    $memStream  = [System.IO.MemoryStream]::new()
    $cryptoStream = [System.Security.Cryptography.CryptoStream]::new(
        $memStream, $encryptor, [System.Security.Cryptography.CryptoStreamMode]::Write)

    $cryptoStream.Write($plainTextBytes, 0, $plainTextBytes.Length)
    $cryptoStream.FlushFinalBlock()
    $cipherBytes = $memStream.ToArray()   # capture before Dispose

    $cryptoStream.Dispose()
    $memStream.Dispose()
    $aes.Dispose()

    return [Convert]::ToBase64String($cipherBytes)
}

function Invoke-SecurityDecrypt {
    <#
    .SYNOPSIS
        Decrypts a Base64 AES-CBC ciphertext produced by SecurityEncryption.cs.
        Returns the original plain-text string.  If the input is not valid Base64
        it is returned unchanged (i.e. it was never encrypted).
    #>
    param([string]$CipherText)

    if ([string]::IsNullOrEmpty($CipherText)) { return $CipherText }

    try   { $cipherBytes = [Convert]::FromBase64String($CipherText) }
    catch { return $CipherText }   # not Base64 → already plain text

    $initVectorBytes = [System.Text.Encoding]::ASCII.GetBytes('@1B2c3D4e5F6g7H8')
    $saltValueBytes  = [System.Text.Encoding]::ASCII.GetBytes('s@1tValue')

    $password = New-Object System.Security.Cryptography.PasswordDeriveBytes(
        'Pas5pr@se', $saltValueBytes, 'SHA1', 2)
    $keyBytes = $password.GetBytes(32)

    $aes         = [System.Security.Cryptography.Aes]::Create()
    $aes.Mode    = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::Zeros

    # Mirrors SecurityEncryption.Decrypt: MemoryStream wraps cipher bytes
    # (writable, position 0); CryptoStream overwrites them with plain bytes.
    $decryptor  = $aes.CreateDecryptor($keyBytes, $initVectorBytes)
    $memStream  = [System.IO.MemoryStream]::new($cipherBytes)
    $cryptoStream = [System.Security.Cryptography.CryptoStream]::new(
        $memStream, $decryptor, [System.Security.Cryptography.CryptoStreamMode]::Write)

    $cryptoStream.Write($cipherBytes, 0, $cipherBytes.Length)
    $cryptoStream.FlushFinalBlock()
    $plainBytes = $memStream.ToArray()

    $cryptoStream.Dispose()
    $memStream.Dispose()
    $aes.Dispose()

    return [System.Text.Encoding]::UTF8.GetString($plainBytes).TrimEnd([char]0)
}

function New-LicenseConfig {
    <#
    .SYNOPSIS
        Writes an encrypted 'Warewolf License.secureconfig' with Status=Active
        to $OutputPath.  All values are pre-encrypted with SecurityEncryption so
        SubscriptionConfig loads them directly without needing to auto-re-encrypt.
    #>
    param(
        [Parameter(Mandatory)][string]$OutputPath,
        [string]$CustomerId      = '',
        [string]$SubscriptionId  = '',
        [string]$Status          = 'Active',
        [string]$PlanId          = 'developer',
        [string]$SubscriptionKey = 'Dev2-test-account',
        [string]$SiteName        = 'warewolf-test',
        [string]$StopExecutions  = 'False'
    )

    function Enc([string]$v) { if ([string]::IsNullOrEmpty($v)) { $v } else { Invoke-SecurityEncrypt -PlainText $v } }

    $xml = [System.Xml.XmlDocument]::new()
    $dec = $xml.CreateXmlDeclaration('1.0', 'utf-8', $null)
    $xml.AppendChild($dec) | Out-Null
    $root = $xml.CreateElement('subscriptionSettings')
    $xml.AppendChild($root) | Out-Null

    $entries = [ordered]@{
        CustomerId          = Enc $CustomerId
        SubscriptionId      = Enc $SubscriptionId
        Status              = Enc $Status
        PlanId              = Enc $PlanId
        SubscriptionKey     = Enc $SubscriptionKey
        SubscriptionSiteName = Enc $SiteName
        StopExecutions      = Enc $StopExecutions
    }

    foreach ($kv in $entries.GetEnumerator()) {
        $node = $xml.CreateElement('add')
        $node.SetAttribute('key',   $kv.Key)
        $node.SetAttribute('value', $kv.Value)
        $root.AppendChild($node) | Out-Null
    }

    $xws             = [System.Xml.XmlWriterSettings]::new()
    $xws.Indent      = $true
    $xws.IndentChars = '  '
    $xws.Encoding    = [System.Text.UTF8Encoding]::new($false)
    $writer = [System.Xml.XmlWriter]::Create($OutputPath, $xws)
    $xml.Save($writer)
    $writer.Flush(); $writer.Dispose()

    Write-Host "  status  : $Status"
    Write-Host "  path    : $OutputPath"
}

function New-TestSecureConfig {
    <#
    .SYNOPSIS
        Generates a minimal encrypted secure.config for integration tests and
        writes it to $OutputPath.  Returns the path.

    .DESCRIPTION
        Permissions mirror the SecurityHttpTests config:
          Warewolf Administrators  — global full access (default admin group)
          Azure Functions Users    — global full access
          Public                   — no global View; resource-specific View on
                                     "Hello World" only
    #>
    param(
        [string]$OutputPath,
        # When supplied this key is used directly (e.g. retrieved from Key Vault).
        # When omitted a random HMAC-SHA256 key is generated.
        [string]$SecretKey
    )

    if (-not $SecretKey) {
        $hmac      = [System.Security.Cryptography.HMACSHA256]::new()
        $SecretKey = [Convert]::ToBase64String($hmac.Key)
        $hmac.Dispose()
    }
    $secretKey = $SecretKey

    # Minimal SecuritySettingsTO JSON — SecureConfigLoader only reads
    # SecretKey and WindowsGroupPermissions, so no $type annotations needed.
    $settings = [ordered]@{
        SecretKey                     = $secretKey
        WindowsGroupPermissions       = @(
            [ordered]@{ WindowsGroup = "Warewolf Administrators"; IsServer = $true;  ResourceID = "00000000-0000-0000-0000-000000000000"; ResourceName = "";            View = $true;  Execute = $true;  Contribute = $true;  DeployTo = $true;  DeployFrom = $true;  Administrator = $true  }
            [ordered]@{ WindowsGroup = "Azure Functions Users";   IsServer = $true;  ResourceID = "00000000-0000-0000-0000-000000000000"; ResourceName = "";            View = $true;  Execute = $true;  Contribute = $false; DeployTo = $false; DeployFrom = $false; Administrator = $false }
            [ordered]@{ WindowsGroup = "Public";                  IsServer = $true;  ResourceID = "00000000-0000-0000-0000-000000000000"; ResourceName = "";            View = $false; Execute = $false; Contribute = $false; DeployTo = $false; DeployFrom = $false; Administrator = $false }
            [ordered]@{ WindowsGroup = "Public";                  IsServer = $false; ResourceID = [System.Guid]::NewGuid().ToString();    ResourceName = "Hello World"; View = $true;  Execute = $true;  Contribute = $false; DeployTo = $false; DeployFrom = $false; Administrator = $false }
        )
        CacheTimeout                  = "00:00:00"
    }

    $json      = ConvertTo-Json $settings -Depth 5 -Compress
    $encrypted = Invoke-SecurityEncrypt -PlainText $json
    [System.IO.File]::WriteAllText($OutputPath, $encrypted)

    Write-Host "  secretKey : $secretKey"
    Write-Host "  path      : $OutputPath"

    return $OutputPath
}

function Get-OrSet-VaultSecretKey {
    <#
    .SYNOPSIS
        Retrieves the JWT HMAC secret key from Key Vault.
        Creates and stores a new random key on first call (idempotent).

    .DESCRIPTION
        Secret value format matches the experiment script:
          { "version": 1, "keyId": "<guid>", "key": "<base64>", "created": "<ISO-8601>" }

        A raw base64 value (no JSON wrapper) is also accepted for
        hand-created secrets.

    .OUTPUTS
        Base64-encoded HMAC-SHA256 key string.
    #>
    param(
        [Parameter(Mandatory)][string]$VaultName,
        [Parameter(Mandatory)][string]$SecretName
    )

    # Verify az CLI is available and logged in before hitting Key Vault.
    $azAccount = az account show --query name -o tsv 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-PipelineWarning "az CLI not logged in or not available (exit $LASTEXITCODE): $azAccount"
        Write-PipelineWarning "Key Vault '$VaultName' unreachable — falling back to a fresh random key (single-run only; key will NOT be persisted)."
        $hmac = [System.Security.Cryptography.HMACSHA256]::new()
        $key  = [Convert]::ToBase64String($hmac.Key)
        $hmac.Dispose()
        Write-Host "  vault key : fallback (az CLI unavailable)"
        return $key
    }
    Write-Host "  vault key : az account = $azAccount"

    # Try to retrieve an existing secret.
    $existing = az keyvault secret show `
        --vault-name $VaultName `
        --name       $SecretName `
        --query value -o tsv 2>&1
    $vaultExitCode = $LASTEXITCODE

    if ($vaultExitCode -ne 0) {
        Write-PipelineWarning "Key Vault secret show failed (exit $vaultExitCode): $existing"
        Write-PipelineWarning "Vault '$VaultName' / secret '$SecretName' could not be read — generating a fresh key (single-run only)."
        $hmac = [System.Security.Cryptography.HMACSHA256]::new()
        $key  = [Convert]::ToBase64String($hmac.Key)
        $hmac.Dispose()
        Write-Host "  vault key : fallback (vault read error)"
        return $key
    }

    if ($existing) {
        # 1. Try well-formed JSON.
        try {
            $km = $existing | ConvertFrom-Json -ErrorAction Stop
            Write-Host "  vault key : retrieved (keyId=$($km.keyId), created=$($km.created))"
            return $km.key
        } catch { }

        # 2. Repair malformed JSON (unquoted keys/values produced by some tools).
        #    Matches the ConvertFrom-KeyMaterial repair in the experiment script.
        try {
            $repaired = $existing -replace '([\{,])\s*([a-zA-Z_]\w*)\s*:', '$1"$2":'
            $repaired = $repaired  -replace ':\s*(?!")([^,\}]+)',           ':"$1"'
            $km = $repaired | ConvertFrom-Json -ErrorAction Stop
            Write-Host "  vault key : retrieved (repaired JSON, keyId=$($km.keyId))"
            return $km.key
        } catch { }

        # 3. Raw base64 secret (no JSON wrapper).
        Write-Host "  vault key : retrieved (raw secret)"
        return $existing
    }

    # No secret found -- generate and store a new key.
    Write-Host "  vault key : not found -- generating new key..."
    $hmac = [System.Security.Cryptography.HMACSHA256]::new()
    $key  = [Convert]::ToBase64String($hmac.Key)
    $hmac.Dispose()

    $km = [ordered]@{
        version = 1
        keyId   = [System.Guid]::NewGuid().ToString()
        key     = $key
        created = (Get-Date -Format 'o')
    }

    $setResult = az keyvault secret set `
        --vault-name $VaultName `
        --name       $SecretName `
        --value      ($km | ConvertTo-Json -Compress) `
        --output none 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-PipelineWarning "Key Vault secret set failed (exit $LASTEXITCODE): $setResult"
        Write-PipelineWarning "Key generated but NOT persisted in '$VaultName' — this run will work but the key won't survive to the next run."
    } else {
        Write-Host "  vault key : stored in '$VaultName' / '$SecretName'"
    }
    return $key
}
# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 1. Kill any existing func host processes (-Cleanup switch)
# -----------------------------------------------------------------------------

if ($Cleanup.IsPresent) {
    Write-PipelineSection "Cleanup: stopping existing func processes..."
    $FuncProcesses = Get-Process "func" -ErrorAction SilentlyContinue
    if ($FuncProcesses) {
        $FuncProcesses | Stop-Process -Force
        Start-Sleep 2
        Write-Host "Stopped $($FuncProcesses.Count) existing func process(es)."
    } else {
        Write-Host "No existing func processes found."
    }
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 2. Kill any stale process occupying the target port
# -----------------------------------------------------------------------------

Write-PipelineSection "Checking port $Port availability..."

$IsLinux = $PSVersionTable.Platform -eq "Unix"

if ($IsLinux) {
    # Linux: use lsof to find and kill whatever owns the port
    $stalePids = & bash -c "lsof -ti tcp:$Port 2>/dev/null"
    if ($stalePids) {
        Write-PipelineWarning "Port $Port is in use by PID(s): $stalePids - killing..."
        & bash -c "kill -9 $stalePids 2>/dev/null"
        Start-Sleep 2
        Write-Host "Port $Port cleared."
    } else {
        Write-Host "Port $Port is free."
    }
} else {
    # Windows: use netstat to find the PID and kill it
    $netstatLine = netstat -ano | Select-String ":$Port\s+.*LISTENING"
    if ($netstatLine) {
        $stalePid = ($netstatLine -split '\s+')[-1]
        Write-PipelineWarning "Port $Port is in use by PID $stalePid - killing..."
        try {
            Stop-Process -Id $stalePid -Force -ErrorAction Stop
            Start-Sleep 2
            Write-Host "Port $Port cleared."
        } catch {
            Write-PipelineWarning "Could not kill PID $stalePid : $_"
        }
    } else {
        Write-Host "Port $Port is free."
    }
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 3. Install Azure Functions Core Tools v4 via npm if missing
# -----------------------------------------------------------------------------

Write-PipelineSection "Checking Azure Functions Core Tools (func)..."

$FuncExe     = $null
$FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue

if ($FuncCommand) {
    $FuncExe = $FuncCommand.Source
    $FuncVer = & func --version 2>&1
    Write-Host "func already installed: $FuncExe (v$FuncVer)"
} else {
    Write-Host "func not found - installing azure-functions-core-tools@4 via npm..."

    $npmCommand = Get-Command "npm" -ErrorAction SilentlyContinue
    if (-not $npmCommand) {
        Fail-Pipeline "npm is not available on this agent. Cannot auto-install Azure Functions Core Tools."
    }

    # --unsafe-perm is required on Linux agents running as root (common in pipelines)
    & npm install -g azure-functions-core-tools@4 --unsafe-perm true
    if ($LASTEXITCODE -ne 0) {
        Fail-Pipeline "npm install of azure-functions-core-tools@4 failed (exit code $LASTEXITCODE)."
    }

    # Re-resolve after install
    $FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue
    if (-not $FuncCommand) {
        # npm global bin may not yet be in PATH - try to add it dynamically
        $npmGlobalBin = & npm bin -g 2>/dev/null
        if ($npmGlobalBin -and (Test-Path "$npmGlobalBin/func")) {
            $env:PATH = "$npmGlobalBin$([System.IO.Path]::PathSeparator)$env:PATH"
            $FuncCommand = Get-Command "func" -ErrorAction SilentlyContinue
        }
    }

    if (-not $FuncCommand) {
        Fail-Pipeline "func still not found after npm install. Ensure npm global bin directory is in PATH."
    }

    $FuncExe = $FuncCommand.Source
    $FuncVer = & func --version 2>&1
    Write-Host "func installed successfully: $FuncExe (v$FuncVer)"
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Resolve host.json directory
# -----------------------------------------------------------------------------

Write-PipelineSection "Resolving Function project directory..."

if ($FunctionPath -and (Test-Path "$FunctionPath\host.json")) {
    $FuncDir = $FunctionPath
} elseif (Test-Path "$PSScriptRoot\host.json") {
    $FuncDir = $PSScriptRoot
} elseif (Test-Path "$PSScriptRoot\Warewolf.Execution.Lightweight\host.json") {
    $FuncDir = "$PSScriptRoot\Warewolf.Execution.Lightweight"
} else {
    Fail-Pipeline "Cannot find host.json. Use -FunctionPath to specify the Azure Functions directory."
}

Write-Host "Function directory: $FuncDir"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Pre-flight - verify required assemblies are present
# -----------------------------------------------------------------------------
#
# The dotnet-isolated worker process is spawned by the func host and probes
# for its dependencies in $FuncDir.  If any required assembly is absent the
# worker crashes immediately with a FileNotFoundException and the host enters
# an infinite restart loop - burning the entire timeout with no useful output.
# Catching this here gives a fast, actionable failure instead.
#
# -----------------------------------------------------------------------------

Write-PipelineSection "Pre-flight: checking required assemblies in '$FuncDir'..."

# Each entry is @{ File = "name.dll"; MinVersion = [version]"x.y.z.w" }
# MinVersion is the minimum acceptable assembly version (inclusive).
$RequiredAssemblies = @(
    @{ File = "Google.Protobuf.dll"; MinVersion = [version]"3.25.2.0" }
)

$missingAssemblies  = @()
$wrongVersionAsms   = @()

foreach ($req in $RequiredAssemblies) {
    $path = "$FuncDir\$($req.File)"

    if (-not (Test-Path $path)) {
        Write-PipelineError "  [MISSING] $($req.File)"
        $missingAssemblies += $req.File
        continue
    }

    $asmName  = [System.Reflection.AssemblyName]::GetAssemblyName($path)
    $actual   = $asmName.Version
    $required = $req.MinVersion

    if ($actual -lt $required) {
        Write-PipelineError ("  [WRONG VERSION] $($req.File) - found $actual, need >= $required")
        $wrongVersionAsms += "$($req.File) (found $actual, need >= $required)"
    } else {
        Write-Host "  [OK] $($req.File) - $actual"
    }
}

if ($missingAssemblies.Count -gt 0) {
    Fail-Pipeline ("One or more required assemblies are missing from '$FuncDir': " +
        ($missingAssemblies -join ", ") +
        ".  The dotnet-isolated worker cannot start without them.  " +
        "Ensure each is an explicit <PackageReference> in the project file so MSBuild copies it to the output directory.")
}

if ($wrongVersionAsms.Count -gt 0) {
    Fail-Pipeline ("One or more assemblies in '$FuncDir' are the wrong version: " +
        ($wrongVersionAsms -join "; ") +
        ".  Another project in the solution is likely overwriting the correct DLL with an older transitive copy.  " +
        "Pin the required version in Directory.Build.props so every project resolves the same version.")
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Copy workflow resources if requested
# -----------------------------------------------------------------------------

if ($ResourcesPath) {
    Write-PipelineSection "Copying resources from '$ResourcesPath'..."
    $TargetResourcesDir = "$FuncDir\Resources"
    if (!(Test-Path $TargetResourcesDir)) {
        New-Item -ItemType Directory -Path $TargetResourcesDir | Out-Null
    }
    if (Test-Path "$ResourcesPath\Resources") {
        Copy-Item -Path "$ResourcesPath\Resources\*" -Destination $TargetResourcesDir -Recurse -Force
    } elseif (Test-Path "$PSScriptRoot\Resources - $ResourcesPath\Resources") {
        Copy-Item -Path "$PSScriptRoot\Resources - $ResourcesPath\Resources\*" -Destination $TargetResourcesDir -Recurse -Force
    } elseif (Test-Path "$PSScriptRoot\..\Resources - $ResourcesPath\Resources") {
        Copy-Item -Path "$PSScriptRoot\..\Resources - $ResourcesPath\Resources\*" -Destination $TargetResourcesDir -Recurse -Force
    } else {
        Write-PipelineWarning "Resources path not found for '$ResourcesPath'"
    }
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Setup secure.config
# -----------------------------------------------------------------------------

Write-PipelineSection "Setting up secure.config..."

if ($SecureConfigPath) {
    if (-not (Test-Path $SecureConfigPath)) {
        Fail-Pipeline "SecureConfigPath '$SecureConfigPath' not found."
    }
    Copy-Item $SecureConfigPath -Destination "$FuncDir\secure.config" -Force
    $env:WAREWOLF_SECURE_CONFIG = "$FuncDir\secure.config"
    Write-Host "  source  : $SecureConfigPath (supplied)"
} elseif ($VaultName) {
    Write-Host "  Resolving JWT secret key from Key Vault '$VaultName' / secret '$SecretName'..."
    $vaultKey = Get-OrSet-VaultSecretKey -VaultName $VaultName -SecretName $SecretName
    New-TestSecureConfig -OutputPath "$FuncDir\secure.config" -SecretKey $vaultKey | Out-Null
    $env:WAREWOLF_SECURE_CONFIG = "$FuncDir\secure.config"
    Write-Host "  source  : Key Vault '$VaultName' / secret '$SecretName'"
} else {
    # No external config supplied: generate (or regenerate) a test config.
    # Always regenerate so the SecretKey is fresh and non-empty — a pre-existing
    # file may have SecretKey="" (e.g. a real Warewolf server install), which
    # causes both the func worker and the test runner to each auto-generate
    # their own random key, making every valid-JWT test fail with 401.
    if (Test-Path "$FuncDir\secure.config") {
        Write-Host "  Pre-existing secure.config found — regenerating to ensure non-empty SecretKey."
    } else {
        Write-Host "  No secure.config found - generating test config..."
    }
    New-TestSecureConfig -OutputPath "$FuncDir\secure.config" | Out-Null
    $env:WAREWOLF_SECURE_CONFIG = "$FuncDir\secure.config"
    Write-Host "  source  : generated"
}

Write-Host "  WAREWOLF_SECURE_CONFIG=$env:WAREWOLF_SECURE_CONFIG"

# Propagate to the Azure DevOps pipeline so subsequent steps (test runner) inherit it.
Write-Host "##vso[task.setvariable variable=WAREWOLF_SECURE_CONFIG]$env:WAREWOLF_SECURE_CONFIG"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Setup Warewolf License.secureconfig
# -----------------------------------------------------------------------------

Write-PipelineSection "Setting up Warewolf License.secureconfig..."

$LicenseFile = Join-Path $FuncDir "Warewolf License.secureconfig"

if ($LicenseConfigPath) {
    if (-not (Test-Path -LiteralPath $LicenseConfigPath)) {
        Fail-Pipeline "LicenseConfigPath '$LicenseConfigPath' not found."
    }
    Copy-Item -LiteralPath $LicenseConfigPath -Destination $LicenseFile -Force
    Write-Host "  source  : $LicenseConfigPath (supplied)"
} else {
    New-LicenseConfig -OutputPath $LicenseFile
    Write-Host "  source  : generated"
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# Write local.settings.json with WAREWOLF_SECURE_CONFIG so the dotnet-isolated worker
# reliably receives it.  The func CLI (Node.js) reads local.settings.json and passes
# its Values entries to the worker process; plain env-var inheritance through the
# Node.js → dotnet worker process boundary is not guaranteed on all CI agents.
$localSettings = [ordered]@{
    IsEncrypted = $false
    Values      = [ordered]@{
        AzureWebJobsStorage      = if ($env:AzureWebJobsStorage) { $env:AzureWebJobsStorage } else { "" }
        FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
        WAREWOLF_SECURE_CONFIG   = $env:WAREWOLF_SECURE_CONFIG
    }
}
$localSettings | ConvertTo-Json -Depth 3 | Set-Content (Join-Path $FuncDir "local.settings.json") -Encoding UTF8
Write-Host "  local.settings.json written with WAREWOLF_SECURE_CONFIG=$env:WAREWOLF_SECURE_CONFIG"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Disable concurrency snapshot / host health monitor in the deployed host.json
# -----------------------------------------------------------------------------
#
# host.json's dynamicConcurrencyEnabled/snapshotPersistenceEnabled, and the
# (host.json-implicit, default-on) host health monitor, each acquire a
# "primary host" lease at startup and can force the WebJobs Script Host to
# restart itself once fully up (observed as "Host lock lease acquired..."
# followed by "Restarting host." in warewolf-server.log). With worker
# indexing enabled (see worker.config.json), that restart hits a known
# azure-functions-host bug where the dotnet-isolated worker channel started
# at the webhost level is not shut down before the new host re-requests the
# same function loads, throwing "Unable to load Function '<name>'. A
# function with the id '<id>' name already exists." and permanently 500-ing
# every request for the rest of the run (see
# Azure/azure-functions-dotnet-worker#2124, Azure/azure-functions-host#9851
# -- fixed upstream only for the "unhealthy host" restart path, not for this
# lease/specialization-style restart).
#
# An AzureFunctionsJobHost__concurrency__dynamicConcurrencyEnabled=false /
# ...__healthMonitor__enabled=false local.settings.json override was tried
# first but did NOT take effect (ConcurrencyOptions/HostHealthMonitorOptions
# still logged as enabled) -- these options are apparently bound before, or
# independently of, the env-var configuration layer func.exe applies. Patch
# the deployed host.json directly instead, which we've already confirmed
# (via the routePrefix fix) reliably takes effect. This only touches the
# copy in $FuncDir (the shared CI test-publish output); it does not modify
# Warewolf.Execution.Lightweight's source-controlled, production host.json.
$hostJsonPath = Join-Path $FuncDir "host.json"
$hostJson = Get-Content $hostJsonPath -Raw | ConvertFrom-Json
if (-not $hostJson.concurrency) {
    $hostJson | Add-Member -MemberType NoteProperty -Name concurrency -Value ([pscustomobject]@{})
}
$hostJson.concurrency | Add-Member -MemberType NoteProperty -Name dynamicConcurrencyEnabled -Value $false -Force
$hostJson.concurrency | Add-Member -MemberType NoteProperty -Name snapshotPersistenceEnabled -Value $false -Force
if (-not $hostJson.healthMonitor) {
    $hostJson | Add-Member -MemberType NoteProperty -Name healthMonitor -Value ([pscustomobject]@{})
}
$hostJson.healthMonitor | Add-Member -MemberType NoteProperty -Name enabled -Value $false -Force
$hostJson | ConvertTo-Json -Depth 10 | Set-Content $hostJsonPath -Encoding UTF8
Write-Host "  host.json patched: concurrency.dynamicConcurrencyEnabled=false, healthMonitor.enabled=false (avoids known worker-indexing restart bug)"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Ensure TestResults directory exists
# -----------------------------------------------------------------------------

if (!(Test-Path "$PSScriptRoot\TestResults")) {
    New-Item -ItemType Directory "$PSScriptRoot\TestResults" | Out-Null
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Resolve how to invoke func (handles .ps1 / .cmd / .exe wrappers)
# -----------------------------------------------------------------------------

Write-PipelineSection "Starting Azure Functions host..."

# For HTTP-only functions, AzureWebJobsStorage is not required at runtime.
# Override to empty string if not already set so the host starts without Azurite.
if (!$env:AzureWebJobsStorage) {
    $env:AzureWebJobsStorage = ""
}

# Also set FUNCTIONS_WORKER_RUNTIME as a process env var so the host starts
# non-interactively in environments where local.settings.json may not be present.
if (!$env:FUNCTIONS_WORKER_RUNTIME) {
    $env:FUNCTIONS_WORKER_RUNTIME = "dotnet-isolated"
}

# IMPORTANT: Clear WAREWOLF_SECURE_CONFIG from the process environment before
# starting func so the Azure Functions Core Tools CLI does NOT log "Skipping
# '...' from local settings as it's already defined in current environment
# variables" and instead delivers it to the dotnet-isolated worker via the
# official local.settings.json mechanism.
#
# When WAREWOLF_SECURE_CONFIG is already in the process env the func CLI skips
# injecting it from local.settings.json.  Whether the worker then inherits the
# env-var through normal process inheritance is not guaranteed — on some CI
# agents the Node.js→dotnet worker process boundary does not reliably carry
# custom env vars.  Removing it here forces the func CLI to read the value from
# local.settings.json and pass it to the worker via the reliable path.
#
# We restore it after Start-Process so the test runner (vstest.console.exe,
# which is spawned later in the same PowerShell session) still inherits it.
$_savedSecureConfig = $env:WAREWOLF_SECURE_CONFIG
[System.Environment]::SetEnvironmentVariable("WAREWOLF_SECURE_CONFIG", $null, "Process")

Write-Host "  func    : $FuncExe"
Write-Host "  root    : $FuncDir"
Write-Host "  port    : $Port"
Write-Host "  timeout : ${TimeoutSeconds}s"

# Start-Process requires a Win32 executable as FilePath.
# func is often a .ps1 or .cmd wrapper installed by npm on Windows;
# on Linux it is a plain binary - switch on extension accordingly.
$ext = [System.IO.Path]::GetExtension($FuncExe).ToLower()
switch ($ext) {
    '.ps1' {
        $StartFilePath = (Get-Command powershell.exe -ErrorAction SilentlyContinue).Source
        if (-not $StartFilePath) { $StartFilePath = "pwsh" }
        $StartArgs = "-NoProfile -NoLogo -File `"$FuncExe`" start --port $Port"
    }
    '.cmd' {
        $StartFilePath = "$env:ComSpec"
        $StartArgs     = "/c `"$FuncExe`" start --port $Port"
    }
    default {
        $StartFilePath = $FuncExe
        $StartArgs     = "start --port $Port"
    }
}

$FuncProcess = Start-Process `
    -FilePath    $StartFilePath `
    -ArgumentList $StartArgs `
    -WorkingDirectory $FuncDir `
    -RedirectStandardOutput "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" `
    -RedirectStandardError  "$PSScriptRoot\TestResults\AzureFunctionError.txt" `
    -PassThru

if (-not $FuncProcess) {
    Fail-Pipeline "Start-Process did not return a process object - func failed to launch."
}

# Restore WAREWOLF_SECURE_CONFIG now that func has been started.
# The test runner (vstest.console.exe) spawned later in this PowerShell session
# inherits this value so SecurityHttpTests.ClassInit can load the same secret
# key the func worker loaded from local.settings.json.
$env:WAREWOLF_SECURE_CONFIG = $_savedSecureConfig

Write-Host "func host launched. PID: $($FuncProcess.Id)"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: 4. Wait for host to be ready - HTTP health probe with timeout
# -----------------------------------------------------------------------------
#
# We use /admin/host/ping (returns HTTP 200 only when the host is fully
# initialised and ready to serve requests) rather than a raw TCP probe.
# A TCP connect can succeed briefly while the host is still loading or
# crashing, producing a false-positive that causes all tests to fail.
#
# -----------------------------------------------------------------------------

Write-PipelineSection "Waiting for Azure Functions host to be ready on port $Port (timeout: ${TimeoutSeconds}s)..."

$pingUrl = "http://localhost:$Port/admin/host/ping"
$elapsed = 0
$Ready   = $false

while ($elapsed -lt $TimeoutSeconds -and !$Ready) {
    Start-Sleep $PollIntervalSec
    $elapsed += $PollIntervalSec

    # Detect early exit before timeout
    if ($FuncProcess.HasExited) {
        Fail-Pipeline "Azure Functions host exited unexpectedly after ${elapsed}s (exit code $($FuncProcess.ExitCode))."
    }

    # HTTP health probe - only 200 means the host is fully ready
    try {
        $response = Invoke-WebRequest -Uri $pingUrl -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
        if ($response.StatusCode -eq 200) {
            $Ready = $true
            Write-Host "  [${elapsed}s] $pingUrl -> $($response.StatusCode) [OK]"
        } else {
            Write-Host "  [${elapsed}s] $pingUrl -> $($response.StatusCode) (not ready yet)"
        }
    } catch {
        Write-Host "  [${elapsed}s] $pingUrl - not ready yet: $($_.Exception.Message)"
    }
}

if (!$Ready) {
    Fail-Pipeline "Azure Functions host did not respond on $pingUrl within ${TimeoutSeconds}s."
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Success - print summary
# -----------------------------------------------------------------------------

Write-Host ""
Write-Host "##[section]Azure Functions host is ready [OK]"
Write-Host "  url     : http://localhost:$Port"
Write-Host "  pid     : $($FuncProcess.Id)"
Write-Host "  elapsed : ${elapsed}s"
Write-Host "  exited  : $($FuncProcess.HasExited)"

Write-Host ""
Write-Host "--- AzureFunctionOutput.txt (last 20 lines) ---"
Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -ErrorAction SilentlyContinue | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
Write-Host "--- AzureFunctionError.txt (last 20 lines) ---"
Get-Content "$PSScriptRoot\TestResults\AzureFunctionError.txt"  -ErrorAction SilentlyContinue | Select-Object -Last 20 | ForEach-Object { Write-Host $_ }
Write-Host "-----------------------------------------------"

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------


# -----------------------------------------------------------------------------
# region: Tail log (interactive / non-pipeline mode)
# -----------------------------------------------------------------------------

if (!$DoExit.IsPresent) {
    Get-Content "$PSScriptRoot\TestResults\AzureFunctionOutput.txt" -Wait
}

# -----------------------------------------------------------------------------
# endregion
# -----------------------------------------------------------------------------
