<#
.SYNOPSIS
    Encrypts Warewolf source .bite files for deployment to Azure Function.
    Replaces DPAPI-encrypted (or plain-text) ConnectionString attributes with
    AES-256-GCM encrypted equivalents backed by an Azure Key Vault secret.

.DESCRIPTION
    Designed for developer machines running Windows + PowerShell 7+.

    ENCRYPTION  (default behaviour)
      Retrieves the AES key from Key Vault using -SecretName and encrypts all
      matching .bite files.  If the secret is not found the script exits with
      an error — use -GenerateKeys to create a new key first.
      Already WFAES-encrypted files are skipped; if any are found a warning
      with the count is shown at the end (use -GenerateKeys to rotate them).
      Prompts for an optional backup of all .bite files before modifying them.

    GENERATE KEYS  (pass -GenerateKeys)
      Generates a NEW 256-bit AES key, stores it in Key Vault as a new secret
      version under -SecretName, then encrypts all provided files.
      Files that are already WFAES-encrypted are first decrypted with the
      existing key from Key Vault, then re-encrypted with the new key.
      Plain-text or DPAPI-encrypted files are encrypted directly.

    ENCRYPTION FORMAT  (applied to the ConnectionString XML attribute value)
      WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}

      • Not valid base64 on its own (contains "::"), so DpapiWrapper.CanBeDecrypted
        returns false without the AES hook — safe on machines without the key.
      • Authenticated encryption: any tampering is detected at decrypt time.

    BACKUP
      Before encrypting, the script prompts whether to back up all .bite files.
      If confirmed, files are copied to a timestamped directory (default: source
      path with datetime stamp appended, e.g. resources_2024-01-15-14-30-45-12)
      preserving subdirectory structure.  A custom path may be entered at the
      prompt.  Files keep their .bite extension in the backup.

.PARAMETER FilePath
    Path to a single .bite file, or a folder that is searched recursively
    for .bite files.  Files that do not contain a <Source> element with a
    ConnectionString attribute are silently skipped.

.PARAMETER VaultName
    Name of the Azure Key Vault (e.g. "kv-warewolf-prod").

.PARAMETER SecretName
    Name of the secret in Key Vault (e.g. "dp-keyring-v1").  Required — no default.

.PARAMETER GenerateKeys
    Generates a NEW AES-256 key, stores it in Key Vault under -SecretName, and
    (re-)encrypts all files.  Already WFAES-encrypted files are decrypted with
    the existing Key Vault key before being re-encrypted with the new key.

.PARAMETER Decrypt
    Decrypts WFAES:: (and DPAPI) ConnectionString attributes back to plain text.
    Prompts for an output directory (default: source path with "_decrypted_" and
    a datetime stamp appended, e.g. resources_decrypted_2024-01-15-14-30-45-12).
    Original files are not modified.  Cannot be combined with -GenerateKeys.

.EXAMPLE
    # First-time setup: generate key and encrypt all .bite files
    .\Encrypt-Config.ps1 `
        -FilePath     "C:\Warewolf\Resources" `
        -VaultName    "kv-warewolf-prod" `
        -SecretName   "dp-keyring-v1" `
        -GenerateKeys

.EXAMPLE
    # Encrypt files using the existing key from Key Vault
    .\Encrypt-Config.ps1 `
        -FilePath   "C:\Warewolf\Resources\Sources\MyDb.bite" `
        -VaultName  "kv-warewolf-prod" `
        -SecretName "dp-keyring-v1"

.EXAMPLE
    # Rotate key: generate a new key and re-encrypt all files
    .\Encrypt-Config.ps1 `
        -FilePath     "C:\Warewolf\Resources" `
        -VaultName    "kv-warewolf-prod" `
        -SecretName   "dp-keyring-v1" `
        -GenerateKeys

.EXAMPLE
    # Decrypt .bite files for inspection or recovery
    .\Encrypt-Config.ps1 `
        -FilePath   "C:\Warewolf\Resources" `
        -VaultName  "kv-warewolf-prod" `
        -SecretName "dp-keyring-v1" `
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

    [Parameter(Mandatory)]
    [string] $SecretName,

    [switch] $GenerateKeys,

    [switch] $Decrypt
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($Decrypt -and $GenerateKeys) { Write-Host '[!] -Decrypt and -GenerateKeys cannot be used together.' -ForegroundColor Red; exit 1 }

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

if ($GenerateKeys) {
    Write-Host ''
    Write-Host '[!] WARNING: -GenerateKeys will generate a NEW AES-256 key in Key Vault and' -ForegroundColor Yellow
    Write-Host '    re-encrypt ALL matching .bite files. Existing WFAES-encrypted files will' -ForegroundColor Yellow
    Write-Host '    be decrypted with the current key, then re-encrypted with the new one.'   -ForegroundColor Yellow
    Write-Host '    This cannot be undone without the previous key. Ensure a backup exists.'  -ForegroundColor Yellow
    Write-Host ''

    # GenerateKeys: fetch the current key first so we can re-encrypt existing WFAES:: values.
    Write-Step '-GenerateKeys requested — fetching current key to decrypt existing values...'
    $currentJson = az keyvault secret show `
        --vault-name $VaultName `
        --name $SecretName `
        --query value -o tsv 2>$null

    if ($currentJson) {
        $oldKm       = ConvertFrom-KeyMaterial $currentJson
        $oldKeyBytes = [Convert]::FromBase64String($oldKm.key)
        Write-OK "Current (old) key fetched (KeyId=$($oldKm.keyId))"
    } else {
        Write-Skip 'No existing secret found — new key will be generated with no old values to migrate.'
    }

    Write-Step 'Generating new 256-bit AES key material...'
    $rawKey = [byte[]]::new($AES_KEY_BYTES)
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($rawKey)

    $newKm = [ordered]@{
        version = $SECRET_VERSION
        keyId   = [Guid]::NewGuid().ToString()
        key     = [Convert]::ToBase64String($rawKey)
        created = (Get-Date -Format 'o')
    }
    $keyMaterialJson = $newKm | ConvertTo-Json -Compress

    Write-Step "Storing new key in Key Vault '$VaultName'..."
    az keyvault secret set `
        --vault-name $VaultName `
        --name $SecretName `
        --value $keyMaterialJson `
        --output none

    Write-OK "New key stored as secret '$SecretName' in '$VaultName'"

    # Wipe raw key bytes from memory immediately after storage.
    [Array]::Clear($rawKey, 0, $rawKey.Length)
} else {
    # Normal run: fetch existing key from Key Vault. Key generation requires -GenerateKeys.
    $existingJson = az keyvault secret show `
        --vault-name $VaultName `
        --name $SecretName `
        --query value -o tsv 2>$null

    if ($existingJson) {
        $keyMaterialJson = $existingJson
        $km = ConvertFrom-KeyMaterial $keyMaterialJson
        Write-OK "Retrieved existing key (KeyId=$($km.keyId), Created=$($km.created))"
    } else {
        Write-Fail "Secret '$SecretName' not found in '$VaultName'. Use -GenerateKeys to create a new key."
        exit 1
    }
}

# Decode the active key bytes.
$km       = ConvertFrom-KeyMaterial $keyMaterialJson
$keyBytes = [Convert]::FromBase64String($km.key)

if ($keyBytes.Length -ne $AES_KEY_BYTES) {
    Write-Fail "Invalid key length: expected $AES_KEY_BYTES bytes, got $($keyBytes.Length). Use -GenerateKeys to create a new key."
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
# Shared
$skippedCount          = 0
$wfaesSkippedCount     = 0
# Encryption-mode counters
$dpapiFileCount        = 0
$dpapiDecryptOkCount   = 0
$dpapiDecryptFailCount = 0
$dpapiDecryptFailFiles = [System.Collections.Generic.List[string]]::new()
$plainTextFileCount    = 0
$encryptOkCount        = 0
$encryptFailCount      = 0
$encryptFailFiles      = [System.Collections.Generic.List[string]]::new()
# Decryption-mode counters
$filesToDecryptCount   = 0
$decryptOkCount        = 0
$decryptFailCount      = 0
$decryptFailFiles      = [System.Collections.Generic.List[string]]::new()
$decryptOutputRoot     = $null

$stamp      = Get-Date -Format 'yyyy-MM-dd-HH-mm-ss-ff'
$sourceBase = if (Test-Path -LiteralPath $FilePath -PathType Container) { $FilePath } else { Split-Path $FilePath -Parent }

# ── Backup prompt (encrypt mode only) ─────────────────────────────────────────
if (-not $Decrypt -and $files.Count -gt 0) {
    $yn = Read-Host 'Create a backup of .bite files before encrypting? [Y/n]'
    if ([string]::IsNullOrWhiteSpace($yn) -or $yn -imatch '^y') {
        $defaultBackupRoot = "$($FilePath.TrimEnd('\', '/'))_$stamp"
        $inputPath         = Read-Host "  Backup directory [$defaultBackupRoot]"
        $backupRoot        = if ([string]::IsNullOrWhiteSpace($inputPath)) { $defaultBackupRoot } else { $inputPath.Trim() }

        Write-Step "Backing up .bite files to '$backupRoot'..."
        foreach ($bf in $files) {
            $rel     = [System.IO.Path]::GetRelativePath($sourceBase, $bf)
            $dest    = Join-Path $backupRoot $rel
            $destDir = Split-Path $dest -Parent
            if (-not (Test-Path -LiteralPath $destDir)) { New-Item -ItemType Directory -Path $destDir -Force | Out-Null }
            Copy-Item -LiteralPath $bf -Destination $dest -Force
        }
        Write-OK "Backup complete — $($files.Count) file(s) copied to '$backupRoot'"
    } else {
        Write-Skip 'Backup skipped.'
    }
}

# ── Decrypt output directory ───────────────────────────────────────────────────
if ($Decrypt) {
    $defaultDecryptRoot = "$($FilePath.TrimEnd('\', '/'))_decrypted_$stamp"
    $inputPath          = Read-Host "Output directory for decrypted files [$defaultDecryptRoot]"
    $decryptOutputRoot  = if ([string]::IsNullOrWhiteSpace($inputPath)) { $defaultDecryptRoot } else { $inputPath.Trim() }
    Write-Step "Decrypted files will be written to '$decryptOutputRoot'"
}

foreach ($file in $files) {
    $leafName             = Split-Path $file -Leaf
    $fileHasWfaesSkip     = $false
    $fileDpapiAttempted   = $false
    $fileDpapiDecryptFail = $false
    $filePlainAttempted   = $false
    $fileDecryptFail      = $false
    $fileHasEncryptedSrc  = $false
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
                    $fileHasEncryptedSrc = $true
                    Write-Host "    [WFAES] Decrypting: $leafName" -ForegroundColor DarkCyan
                    try {
                        $plainValue = Invoke-WfAesDecrypt $keyBytes $rawValue
                    } catch {
                        Write-Fail "    [WFAES] Decryption failed for '$leafName': $_"
                        $fileDecryptFail = $true
                        continue
                    }
                } elseif (Test-IsDpapiEncrypted $rawValue) {
                    $fileHasEncryptedSrc = $true
                    Write-Host "    [DPAPI] Decrypting: $leafName" -ForegroundColor DarkCyan
                    try {
                        $plainValue = Invoke-DpapiDecrypt $rawValue
                    } catch {
                        Write-Fail "    [DPAPI] Decryption failed for '$leafName': $_"
                        $fileDecryptFail = $true
                        continue
                    }
                    if ($plainValue -eq $rawValue) {
                        Write-Fail "    [DPAPI] Decryption returned same value for '$leafName' — possibly encrypted on another machine."
                        $fileDecryptFail = $true
                        continue
                    }
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
                    $fileDpapiAttempted = $true
                    Write-Host "    [DPAPI] Decrypting: $leafName" -ForegroundColor DarkCyan
                    try {
                        $plainValue = Invoke-DpapiDecrypt $rawValue
                    } catch {
                        Write-Fail "    [DPAPI] Decryption failed for '$leafName' (possibly encrypted on another machine): $_"
                        $fileDpapiDecryptFail = $true
                        continue
                    }
                    if ($plainValue -eq $rawValue) {
                        Write-Fail "    [DPAPI] Decryption returned same value for '$leafName' — possibly encrypted on another machine."
                        $fileDpapiDecryptFail = $true
                        continue
                    }

                } elseif (Test-IsWfAesEncrypted $rawValue) {
                    # ── Existing WFAES:: value ─────────────────────────────────
                    if (-not $GenerateKeys) {
                        Write-Skip "    [SKIP] Already WFAES-encrypted (use -GenerateKeys to rotate): $leafName"
                        $fileHasWfaesSkip = $true
                        continue
                    }
                    if ($oldKeyBytes) {
                        # GenerateKeys: decrypt with old key, then re-encrypt with new key.
                        Write-Host "    [ROTATE] Re-encrypting: $leafName" -ForegroundColor DarkMagenta
                        $plainValue = Invoke-WfAesDecrypt $oldKeyBytes $rawValue
                    } else {
                        Write-Skip "    [SKIP] WFAES-encrypted but no old key available for rotation: $leafName"
                        continue
                    }
                } else {
                    # plain-text connection string — encrypt directly.
                    $filePlainAttempted = $true
                }

                $encryptedValue = Invoke-WfAesEncrypt -keyBytes $keyBytes -plainText $plainValue
                $src.SetAttribute('ConnectionString', $encryptedValue)
                $modified = $true
            }
        }

        # ── Classify file and handle per-file failures ────────────────────────
        if ($Decrypt) {
            if ($fileHasEncryptedSrc) {
                $filesToDecryptCount++
                if ($fileDecryptFail) {
                    $decryptFailCount++
                    $decryptFailFiles.Add($leafName)
                    continue
                }
            }
        } else {
            if ($fileDpapiAttempted -and $fileDpapiDecryptFail) {
                $dpapiFileCount++
                $dpapiDecryptFailCount++
                $dpapiDecryptFailFiles.Add($leafName)
                $encryptFailCount++
                $encryptFailFiles.Add($leafName)
                continue
            }
            if ($fileDpapiAttempted)     { $dpapiFileCount++ }
            elseif ($filePlainAttempted) { $plainTextFileCount++ }
        }

        if (-not $modified) {
            if ($fileHasWfaesSkip) { $wfaesSkippedCount++ } else { $skippedCount++ }
            continue
        }

        # ── Write modified XML ─────────────────────────────────────────────
        $settings = [System.Xml.XmlWriterSettings]::new()
        $settings.Indent             = $true
        $settings.IndentChars        = '  '
        $settings.Encoding           = [System.Text.Encoding]::UTF8
        $settings.OmitXmlDeclaration = $true
        $settings.NewLineHandling    = [System.Xml.NewLineHandling]::Replace

        if ($Decrypt) {
            # Write decrypted content to the output directory, preserving relative structure.
            $rel        = [System.IO.Path]::GetRelativePath($sourceBase, $file)
            $outputPath = Join-Path $decryptOutputRoot $rel
            $outputDir  = Split-Path $outputPath -Parent
            if (-not (Test-Path -LiteralPath $outputDir)) { New-Item -ItemType Directory -Path $outputDir -Force | Out-Null }
            $writer = [System.Xml.XmlWriter]::Create($outputPath, $settings)
            try   { $xml.Save($writer) }
            finally { $writer.Dispose() }
            Write-OK "  Decrypted: $leafName  →  $outputPath"
        } else {
            # Overwrite in-place (backup was taken above if the user confirmed).
            $writer = [System.Xml.XmlWriter]::Create($file, $settings)
            try   { $xml.Save($writer) }
            finally { $writer.Dispose() }
            Write-OK "  Encrypted: $leafName"
        }
        if ($Decrypt) {
            $decryptOkCount++
        } else {
            if ($fileDpapiAttempted) { $dpapiDecryptOkCount++ }
            $encryptOkCount++
        }

    } catch {
        Write-Fail "  ERROR processing '$leafName': $_"
        if ($Decrypt) {
            $decryptFailCount++
            $decryptFailFiles.Add($leafName)
        } else {
            $encryptFailCount++
            $encryptFailFiles.Add($leafName)
        }
    }
}

# ── Wipe key bytes from memory ─────────────────────────────────────────────────
[Array]::Clear($keyBytes, 0, $keyBytes.Length)
if ($oldKeyBytes) { [Array]::Clear($oldKeyBytes, 0, $oldKeyBytes.Length) }

# ── Step 4: Summary ────────────────────────────────────────────────────────────
Write-Host ''
Write-Host ('─' * 55) -ForegroundColor DarkGray

if ($Decrypt) {
    Write-Host '  Decryption Summary' -ForegroundColor Cyan
    Write-Host ('─' * 55) -ForegroundColor DarkGray
    Write-Host "  Total Files             : $($files.Count)"
    Write-Host "  Total Files to decrypt  : $filesToDecryptCount"
    Write-Host ''
    Write-Host "  Decryption succeeded    : $decryptOkCount" -ForegroundColor ($decryptOkCount -gt 0 ? 'Green' : 'DarkGray')
    if ($decryptFailCount -gt 0) {
        Write-Host "  Decryption failed       : $decryptFailCount" -ForegroundColor Red
        foreach ($f in $decryptFailFiles) { Write-Host "      - $f" -ForegroundColor Red }
    } else {
        Write-Host "  Decryption failed       : 0" -ForegroundColor DarkGray
    }
} else {
    Write-Host '  Encryption Summary' -ForegroundColor Cyan
    Write-Host ('─' * 55) -ForegroundColor DarkGray
    $totalFilesToEncrypt = $encryptOkCount + $encryptFailCount
    Write-Host "  Total Files             : $($files.Count)"
    Write-Host "  Total Files to encrypt  : $totalFilesToEncrypt"
    Write-Host ''
    Write-Host "  DPAPI Encrypted Files   : $dpapiFileCount"
    Write-Host "    Decryption succeeded  : $dpapiDecryptOkCount" -ForegroundColor ($dpapiDecryptOkCount -gt 0 ? 'Green' : 'DarkGray')
    if ($dpapiDecryptFailCount -gt 0) {
        Write-Host "    Decryption failed     : $dpapiDecryptFailCount" -ForegroundColor Red
        foreach ($f in $dpapiDecryptFailFiles) { Write-Host "        - $f" -ForegroundColor Red }
    } else {
        Write-Host "    Decryption failed     : 0" -ForegroundColor DarkGray
    }
    Write-Host "  Plain Text Files        : $plainTextFileCount"
    Write-Host ''
    Write-Host "  Total Encryption succeeded : $encryptOkCount" -ForegroundColor ($encryptOkCount -gt 0 ? 'Green' : 'DarkGray')
    if ($encryptFailCount -gt 0) {
        Write-Host "  Total Encryption failed    : $encryptFailCount" -ForegroundColor Red
        foreach ($f in $encryptFailFiles) { Write-Host "      - $f" -ForegroundColor Red }
    } else {
        Write-Host "  Total Encryption failed    : 0" -ForegroundColor DarkGray
    }
    if ($wfaesSkippedCount -gt 0) {
        Write-Host ''
        Write-Host "  WFAES-skipped (already encrypted, use -GenerateKeys to rotate): $wfaesSkippedCount" -ForegroundColor Yellow
    }
}

Write-Host ('─' * 55) -ForegroundColor DarkGray

$hasFailed = ($Decrypt ? $decryptFailCount : $encryptFailCount) -gt 0
if ($hasFailed) {
    Write-Fail 'One or more files failed. Review errors above before deploying.'
    exit 1
}

Write-Host ''
Write-OK ($Decrypt ? "Done. Review decrypted files in '$decryptOutputRoot' and delete that directory after use — it contains plain-text credentials."
    : 'Done.')
