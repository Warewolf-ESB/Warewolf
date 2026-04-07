<#
.SYNOPSIS
    Encrypts Warewolf source .bite files for deployment to Azure Function.
    Replaces DPAPI-encrypted (or plain-text) ConnectionString attributes with
    AES-256-GCM encrypted equivalents backed by an Azure Key Vault secret.

.DESCRIPTION
    Designed for developer machines running Windows + PowerShell 7+.

    FIRST RUN
      Generates a 256-bit AES key, stores it in Key Vault as secret
      "dp-keyring-v1" (or the value of -SecretName), then encrypts all
      matching .bite files.

    SUBSEQUENT RUNS
      Retrieves the existing key from Key Vault and re-encrypts. No new
      key is generated (idempotent for the same set of files).

    KEY ROTATION  (pass -KeyRotate)
      Generates a NEW key, stores it in Key Vault as a new secret version,
      then encrypts all provided files. Files must be supplied in their
      original (DPAPI or plain-text) form — see docs/KeyRotationRunbook.md.

    ENCRYPTION FORMAT  (applied to the ConnectionString XML attribute value)
      WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}

      • Not valid base64 on its own (contains "::"), so DpapiWrapper.CanBeDecrypted
        returns false without the AES hook — safe on machines without the key.
      • Authenticated encryption: any tampering is detected at decrypt time.

    BACKUP
      Original files are backed up as <filename>.bite.bak before modification.
      Delete backups after verifying deployment. They may still contain
      DPAPI-encrypted values which are machine-bound and lower-risk, but
      treat them as sensitive nonetheless.

.PARAMETER FilePath
    Path to a single .bite file, or a folder that is searched recursively
    for .bite files.  Files that do not contain a <Source> element with a
    ConnectionString attribute are silently skipped.

.PARAMETER VaultName
    Name of the Azure Key Vault (e.g. "kv-warewolf-prod").

.PARAMETER SecretName
    Name of the secret in Key Vault.  Default: "dp-keyring-v1".

.PARAMETER FunctionApp
    Azure Function App name.  Required when -UploadToAzure is set.

.PARAMETER ResourceGroup
    Resource group containing the Function App.  Required when -UploadToAzure is set.

.PARAMETER UploadToAzure
    When specified, deploys the encrypted resource folder to the Function App
    via a zip-deploy using the az CLI.

.PARAMETER KeyRotate
    Generates a NEW key and re-encrypts all files.
    ⚠ Provide files in original (DPAPI or plain) form — see KeyRotationRunbook.md.

.PARAMETER Decrypt
    Decrypts WFAES:: (and DPAPI) ConnectionString attributes back to plain text.
    Writes output as <filename>.decrypted.bite alongside the original (which is
    not modified).  Cannot be combined with -KeyRotate or -UploadToAzure.

.EXAMPLE
    # First-time setup: encrypt all .bite files and upload
    .\Encrypt-Config.ps1 `
        -FilePath      "C:\Warewolf\Resources" `
        -VaultName     "kv-warewolf-prod" `
        -FunctionApp   "func-warewolf-prod" `
        -ResourceGroup "rg-warewolf-prod" `
        -UploadToAzure

.EXAMPLE
    # Encrypt a single file (no upload)
    .\Encrypt-Config.ps1 `
        -FilePath  "C:\Warewolf\Resources\Sources\MyDb.bite" `
        -VaultName "kv-warewolf-prod"

.EXAMPLE
    # Key rotation
    .\Encrypt-Config.ps1 `
        -FilePath      "C:\Warewolf\OriginalResources" `
        -VaultName     "kv-warewolf-prod" `
        -KeyRotate `
        -UploadToAzure `
        -FunctionApp   "func-warewolf-prod" `
        -ResourceGroup "rg-warewolf-prod"

.EXAMPLE
    # Decrypt .bite files for inspection or recovery
    .\Encrypt-Config.ps1 `
        -FilePath  "C:\Warewolf\Resources" `
        -VaultName "kv-warewolf-prod" `
        -Decrypt

.NOTES
    Requires: PowerShell 7+, az CLI (logged in), Windows (for DPAPI detection).
#>

#Requires -Version 7.0

[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)]
    [string] $FilePath,

    [Parameter(Mandatory)]
    [string] $VaultName,

    [string] $SecretName = 'dp-keyring-v1',

    [string] $FunctionApp,

    [string] $ResourceGroup,

    [switch] $UploadToAzure,

    [switch] $KeyRotate,

    [switch] $Decrypt
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Decrypt -and $KeyRotate)     { Write-Host '[!] -Decrypt and -KeyRotate cannot be used together.'     -ForegroundColor Red; exit 1 }
if ($Decrypt -and $UploadToAzure) { Write-Host '[!] -Decrypt and -UploadToAzure cannot be used together.' -ForegroundColor Red; exit 1 }

# ── Constants ──────────────────────────────────────────────────────────────────
$WFAES_PREFIX    = 'WFAES::'
$AES_KEY_BYTES   = 32
$SECRET_VERSION  = 1

# ── Console helpers ────────────────────────────────────────────────────────────
function Write-Step  ([string]$msg) { Write-Host "[*] $msg" -ForegroundColor Cyan    }
function Write-OK    ([string]$msg) { Write-Host "[+] $msg" -ForegroundColor Green   }
function Write-Skip  ([string]$msg) { Write-Host "[-] $msg" -ForegroundColor Yellow  }
function Write-Fail  ([string]$msg) { Write-Host "[!] $msg" -ForegroundColor Red     }

# ── Compile a small C# AES-GCM helper to avoid Span<T> reflection issues ──────
Add-Type -Language CSharp -TypeDefinition @'
using System;
using System.Security.Cryptography;
using System.Text;

public static class WfAesHelper
{
    private const int NonceSize = 12;
    private const int TagSize   = 16;

    /// <summary>
    /// Encrypts <paramref name="plainText"/> (UTF-8) with AES-256-GCM.
    /// Returns raw bytes: [12 nonce][ciphertext][16 tag].
    /// </summary>
    public static byte[] Encrypt(byte[] key, string plainText)
    {
        var plain      = Encoding.UTF8.GetBytes(plainText);
        var nonce      = new byte[NonceSize];
        var ciphertext = new byte[plain.Length];
        var tag        = new byte[TagSize];

        RandomNumberGenerator.Fill(nonce);

        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plain, ciphertext, tag);

        // Layout: [12 nonce][ciphertext][16 tag]
        var result = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce,      0, result, 0,                              NonceSize);
        Buffer.BlockCopy(ciphertext, 0, result, NonceSize,                      ciphertext.Length);
        Buffer.BlockCopy(tag,        0, result, NonceSize + ciphertext.Length,  TagSize);
        return result;
    }

    /// <summary>
    /// Decrypts AES-256-GCM payload (layout: [12 nonce][ciphertext][16 tag]).
    /// Used during key rotation to unwrap existing WFAES:: values.
    /// </summary>
    public static string Decrypt(byte[] key, byte[] data)
    {
        if (data.Length < NonceSize + TagSize)
            throw new CryptographicException("Payload too short.");

        var nonce      = new byte[NonceSize];
        var tag        = new byte[TagSize];
        var ciphertext = new byte[data.Length - NonceSize - TagSize];
        var plaintext  = new byte[ciphertext.Length];

        Buffer.BlockCopy(data, 0,                              nonce,      0, NonceSize);
        Buffer.BlockCopy(data, NonceSize,                      ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(data, NonceSize + ciphertext.Length,  tag,        0, TagSize);

        using var aes = new AesGcm(key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        return Encoding.UTF8.GetString(plaintext);
    }
}
'@

# ── DPAPI detection ────────────────────────────────────────────────────────────
# DpapiWrapper uses DataProtectionScope.LocalMachine + Unicode encoding.
function Test-IsDpapiEncrypted ([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value)) { return $false }
    if ($value.Length % 4 -ne 0)             { return $false }
    try {
        $bytes = [Convert]::FromBase64String($value)
        [System.Security.Cryptography.ProtectedData]::Unprotect(
            $bytes, $null,
            [System.Security.Cryptography.DataProtectionScope]::LocalMachine
        ) | Out-Null
        return $true
    } catch { return $false }
}

function Invoke-DpapiDecrypt ([string]$cipher) {
    $bytes = [Convert]::FromBase64String($cipher)
    $plain = [System.Security.Cryptography.ProtectedData]::Unprotect(
                 $bytes, $null,
                 [System.Security.Cryptography.DataProtectionScope]::LocalMachine)
    # DpapiWrapper uses Unicode (UTF-16 LE) encoding
    return [System.Text.Encoding]::Unicode.GetString($plain)
}

# ── AES helpers ────────────────────────────────────────────────────────────────
function Invoke-WfAesEncrypt ([byte[]]$keyBytes, [string]$plainText) {
    $raw = [WfAesHelper]::Encrypt($keyBytes, $plainText)
    return $WFAES_PREFIX + [Convert]::ToBase64String($raw)
}

function Invoke-WfAesDecrypt ([byte[]]$keyBytes, [string]$encryptedValue) {
    $b64  = $encryptedValue.Substring($WFAES_PREFIX.Length)
    $data = [Convert]::FromBase64String($b64)
    return [WfAesHelper]::Decrypt($keyBytes, $data)
}

function Test-IsWfAesEncrypted ([string]$value) {
    return $value.StartsWith($WFAES_PREFIX)
}

# Repair + parse key-material JSON from Key Vault.
# Uses try/catch so the repair only runs when ConvertFrom-Json actually fails.
# Handles two known malformed formats written by legacy/external tools:
#   1. Unquoted GUID only:  {"version":1,"keyId":26d979c7-...,"key":"..."}
#   2. Fully unquoted:      {version:1,keyId:26d979c7-...,key:...,created:...}
function ConvertFrom-KeyMaterial ([string]$json) {
    try {
        return $json | ConvertFrom-Json
    } catch {
        # Step 1 — quote all unquoted property names:  {version: → {"version":
        $repaired = $json -replace '([\{,])\s*([a-zA-Z_]\w*)\s*:', '$1"$2":'
        # Step 2 — quote all unquoted property values:  :"value" already quoted values
        #           are skipped by the (?!") negative lookahead.
        #           [^,\}]+ stops at the next comma or closing brace so datetime
        #           values containing colons are captured as a single token.
        $repaired = $repaired -replace ':\s*(?!")([^,\}]+)', ':"$1"'

        if ($repaired -ceq $json) {
            Write-Fail "Key material JSON could not be parsed. Raw value: $json"
            throw
        }
        Write-Skip 'Note: Key Vault secret was not valid JSON (unquoted keys/values) — auto-repaired.'
        try {
            return $repaired | ConvertFrom-Json
        } catch {
            Write-Fail "Key material JSON repair failed. Repaired attempt: $repaired"
            throw
        }
    }
}

# ── Prerequisite checks ────────────────────────────────────────────────────────
Write-Step 'Checking prerequisites...'

if (-not (Get-Command 'az' -ErrorAction SilentlyContinue)) {
    Write-Fail 'Azure CLI (az) not found. Install from https://aka.ms/installazurecliwindows'
    exit 1
}

$azCtx = az account show 2>$null | ConvertFrom-Json
if (-not $azCtx) {
    Write-Fail 'Not logged in to Azure. Run: az login'
    exit 1
}
Write-OK "Azure context: $($azCtx.user.name) | Subscription: $($azCtx.name)"

# ── Step 1: Resolve key material from Key Vault ────────────────────────────────
Write-Step "Resolving key material from Key Vault '$VaultName' / secret '$SecretName'..."

$keyMaterialJson = $null
$oldKeyBytes     = $null

if (-not $KeyRotate) {
    # Normal run: try to fetch existing key.
    $existingJson = az keyvault secret show `
        --vault-name $VaultName `
        --name $SecretName `
        --query value -o tsv 2>$null

    if ($existingJson) {
        $keyMaterialJson = $existingJson
        $km = ConvertFrom-KeyMaterial $keyMaterialJson
        Write-OK "Retrieved existing key (KeyId=$($km.keyId), Created=$($km.created))"
    } else {
        if ($Decrypt) { Write-Fail "Secret '$SecretName' not found in '$VaultName' — cannot decrypt."; exit 1 }
        Write-Skip "Secret '$SecretName' not found — will create new key on first run."
    }
} else {
    # Key rotation: fetch the current key so we can decrypt existing WFAES:: values.
    Write-Step 'Key rotation requested — fetching current key to decrypt existing values...'
    $currentJson = az keyvault secret show `
        --vault-name $VaultName `
        --name $SecretName `
        --query value -o tsv 2>$null

    if ($currentJson) {
        $oldKm       = ConvertFrom-KeyMaterial $currentJson
        $oldKeyBytes = [Convert]::FromBase64String($oldKm.key)
        Write-OK "Current (old) key fetched for rotation (KeyId=$($oldKm.keyId))"
    } else {
        Write-Skip 'No existing secret — generating first key (no old values to migrate).'
    }
    $keyMaterialJson = $null   # Force new key generation below.
}

# Generate new key if needed.
if (-not $keyMaterialJson) {
    if ($Decrypt) { Write-Fail 'Cannot decrypt: no key material found in Key Vault.'; exit 1 }
    Write-Step 'Generating new 256-bit AES key material...'
    $rawKey    = [byte[]]::new($AES_KEY_BYTES)
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($rawKey)

    $newKm = [ordered]@{
        version = $SECRET_VERSION
        keyId   = [Guid]::NewGuid().ToString()
        key     = [Convert]::ToBase64String($rawKey)
        created = (Get-Date -Format 'o')
    }
    $keyMaterialJson = $newKm | ConvertTo-Json -Compress

    Write-Step "Storing key in Key Vault '$VaultName'..."
    az keyvault secret set `
        --vault-name $VaultName `
        --name $SecretName `
        --value $keyMaterialJson `
        --output none

    Write-OK "Key stored as secret '$SecretName' in '$VaultName'"

    # Wipe raw key bytes from memory immediately after storage.
    [Array]::Clear($rawKey, 0, $rawKey.Length)
}

# Decode the active key bytes.
$km       = ConvertFrom-KeyMaterial $keyMaterialJson
$keyBytes = [Convert]::FromBase64String($km.key)

if ($keyBytes.Length -ne $AES_KEY_BYTES) {
    Write-Fail "Invalid key length: expected $AES_KEY_BYTES bytes, got $($keyBytes.Length). Re-run without -KeyRotate to regenerate."
    exit 1
}

# ── Step 2: Collect target .bite files ────────────────────────────────────────
Write-Step "Collecting .bite files from '$FilePath'..."

$files = @()
if (Test-Path -LiteralPath $FilePath -PathType Leaf) {
    if ([System.IO.Path]::GetExtension($FilePath) -ieq '.bite') {
        $files = @($FilePath)
    } else {
        Write-Fail "'$FilePath' is not a .bite file."
        exit 1
    }
} elseif (Test-Path -LiteralPath $FilePath -PathType Container) {
    $files = Get-ChildItem -LiteralPath $FilePath -Recurse -Filter '*.bite' |
             Select-Object -ExpandProperty FullName
} else {
    Write-Fail "Path '$FilePath' not found."
    exit 1
}

Write-OK "Found $($files.Count) .bite file(s)"

# ── Step 3: Process each file ──────────────────────────────────────────────────
$processedCount  = 0
$skippedCount    = 0
$errorCount      = 0
$processedFiles  = [System.Collections.Generic.List[string]]::new()

foreach ($file in $files) {
    $leafName = Split-Path $file -Leaf
    try {
        $content = Get-Content -LiteralPath $file -Raw -Encoding UTF8

        # Guard: must contain <Source> element with a ConnectionString attribute.
        if ($content -notmatch '<Source\b' -or $content -notmatch '\bConnectionString\s*=') {
            Write-Skip "  SKIP (no <Source ConnectionString=...>): $leafName"
            $skippedCount++
            continue
        }

        [xml]$xml   = $content
        $sources    = $xml.SelectNodes('//Source[@ConnectionString]')

        if ($sources.Count -eq 0) {
            Write-Skip "  SKIP (XPath found no matching nodes): $leafName"
            $skippedCount++
            continue
        }

        $modified = $false
        foreach ($src in $sources) {
            $rawValue = $src.GetAttribute('ConnectionString')
            if ([string]::IsNullOrWhiteSpace($rawValue)) { continue }

            $plainValue = $rawValue

            if ($Decrypt) {
                # ── Decrypt mode: WFAES or DPAPI → plain text ─────────────
                if (Test-IsWfAesEncrypted $rawValue) {
                    Write-Host "    [WFAES] Decrypting: $leafName" -ForegroundColor DarkCyan
                    $plainValue = Invoke-WfAesDecrypt $keyBytes $rawValue
                } elseif (Test-IsDpapiEncrypted $rawValue) {
                    Write-Host "    [DPAPI] Decrypting: $leafName" -ForegroundColor DarkCyan
                    $plainValue = Invoke-DpapiDecrypt $rawValue
                } else {
                    Write-Skip "    [SKIP] Already plain-text (not encrypted): $leafName"
                    continue
                }
                $src.SetAttribute('ConnectionString', $plainValue)
                $modified = $true
            } else {
                # ── Encrypt mode ───────────────────────────────────────────
                if (Test-IsDpapiEncrypted $rawValue) {
                    # ── DPAPI → plain-text ─────────────────────────────────────
                    Write-Host "    [DPAPI] Decrypting: $leafName" -ForegroundColor DarkCyan
                    $plainValue = Invoke-DpapiDecrypt $rawValue

                } elseif (Test-IsWfAesEncrypted $rawValue) {
                    # ── Existing WFAES:: value ─────────────────────────────────
                    if (-not $KeyRotate) {
                        Write-Skip "    [SKIP] Already WFAES-encrypted (use -KeyRotate to rotate): $leafName"
                        continue
                    }
                    if ($oldKeyBytes) {
                        # Key rotation: decrypt with old key, then re-encrypt with new key.
                        Write-Host "    [ROTATE] Re-encrypting: $leafName" -ForegroundColor DarkMagenta
                        $plainValue = Invoke-WfAesDecrypt $oldKeyBytes $rawValue
                    } else {
                        Write-Skip "    [SKIP] WFAES-encrypted but no old key available for rotation: $leafName"
                        continue
                    }
                }
                # else: plain-text connection string — encrypt directly.

                $encryptedValue = Invoke-WfAesEncrypt -keyBytes $keyBytes -plainText $plainValue
                $src.SetAttribute('ConnectionString', $encryptedValue)
                $modified = $true
            }
        }

        if (-not $modified) {
            $skippedCount++
            continue
        }

        # ── Write modified XML ─────────────────────────────────────────────
        $settings = [System.Xml.XmlWriterSettings]::new()
        $settings.Indent             = $true
        $settings.IndentChars        = '  '
        $settings.Encoding           = [System.Text.Encoding]::UTF8
        $settings.OmitXmlDeclaration = $false
        $settings.NewLineHandling    = [System.Xml.NewLineHandling]::Replace

        if ($Decrypt) {
            # Write decrypted content to a new .decrypted.bite file (original unchanged).
            $baseName   = [System.IO.Path]::GetFileNameWithoutExtension($file)
            $outputPath = Join-Path (Split-Path $file -Parent) "$baseName.decrypted.bite"
            $writer = [System.Xml.XmlWriter]::Create($outputPath, $settings)
            try   { $xml.Save($writer) }
            finally { $writer.Dispose() }
            Write-OK "  Decrypted: $leafName  →  $([System.IO.Path]::GetFileName($outputPath))"
        } else {
            # Backup original, then overwrite with encrypted content.
            $backupPath = "$file.bak"
            Copy-Item -LiteralPath $file -Destination $backupPath -Force
            $writer = [System.Xml.XmlWriter]::Create($file, $settings)
            try   { $xml.Save($writer) }
            finally { $writer.Dispose() }
            Write-OK "  Encrypted: $leafName  (backup: $($backupPath | Split-Path -Leaf))"
        }
        $processedCount++
        $processedFiles.Add($file)

    } catch {
        Write-Fail "  ERROR processing '$leafName': $_"
        $errorCount++
    }
}

# ── Wipe key bytes from memory ─────────────────────────────────────────────────
[Array]::Clear($keyBytes, 0, $keyBytes.Length)
if ($oldKeyBytes) { [Array]::Clear($oldKeyBytes, 0, $oldKeyBytes.Length) }

# ── Step 4: Summary ────────────────────────────────────────────────────────────
Write-Host ''
Write-Host ('─' * 55) -ForegroundColor DarkGray
Write-OK   ($Decrypt ? 'Decryption complete' : 'Encryption complete')
Write-Host "  Processed : $processedCount" -ForegroundColor Green
Write-Host "  Skipped   : $skippedCount"   -ForegroundColor Yellow
if ($errorCount -gt 0) {
    Write-Host "  Errors    : $errorCount" -ForegroundColor Red
} else {
    Write-Host "  Errors    : 0" -ForegroundColor DarkGray
}
Write-Host ('─' * 55) -ForegroundColor DarkGray

if ($errorCount -gt 0) {
    Write-Fail 'One or more files failed. Review errors above before deploying.'
    exit 1
}

# ── Step 5: Upload to Azure Function App ───────────────────────────────────────
if (-not $Decrypt -and $UploadToAzure -and $processedFiles.Count -gt 0) {
    if (-not $FunctionApp -or -not $ResourceGroup) {
        Write-Fail '-FunctionApp and -ResourceGroup are required when using -UploadToAzure'
        exit 1
    }

    Write-Step "Packaging and uploading resources to Function App '$FunctionApp'..."

    # Create a temp zip of the parent resources folder so directory structure is preserved.
    $resourcesRoot = Split-Path ($processedFiles[0]) -Parent
    $zipPath       = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "wf-resources-$(Get-Date -Format 'yyyyMMddHHmmss').zip")

    Compress-Archive -Path "$resourcesRoot\*" -DestinationPath $zipPath -Force

    az functionapp deploy `
        --resource-group $ResourceGroup `
        --name           $FunctionApp `
        --src-path       $zipPath `
        --type           static `
        --target-path    "site/wwwroot/Resources" `
        --output none

    Remove-Item -LiteralPath $zipPath -Force
    Write-OK "Resources uploaded to '$FunctionApp'"
}

Write-Host ''
Write-OK ($Decrypt
    ? 'Done. Review .decrypted.bite files and delete them after use — they contain plain-text credentials.'
    : 'Done. Remember to delete .bite.bak backups after verifying the deployment.')
