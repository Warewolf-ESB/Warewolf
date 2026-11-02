#Requires -Version 7.0
<#
.SYNOPSIS
    WFAES-encrypts the ConnectionString of every Warewolf source (.bite) under -Path with the key in -KeyMaterialPath,
    so the Warewolf Lightweight Execution Engine can decrypt them with the same Key Vault secret.

.DESCRIPTION
    Replaces Warewolf's Encrypt-Config.ps1 in the CI build: no az, no Key Vault call, no prompts. The key material is
    the Key Vault secret's value, read from a file (Engine/EngineEncryptionKey.txt after token replacement):
        {"version":1,"keyId":"...","key":"<base64 32 bytes>","created":"..."}

    The format is the one the engine's FileDecryptionHelper reads:
        WFAES::{Base64( [12-byte nonce][ciphertext][16-byte GCM tag] )}   (AES-256-GCM, UTF-8 plaintext)

    Like Encrypt-Config.ps1, it rewrites every <Source ConnectionString="..."> in a .bite file that has one, leaves
    WFAES:: values as they are, and saves the file with the same XmlWriter settings. Each value is decrypted again in
    memory and compared with the plaintext before the file is written. Plaintext is never written anywhere else.

    Unlike Encrypt-Config.ps1 it fails instead of skipping: a .bite with a <Source ConnectionString=...> that is not
    valid XML, or a value that does not round-trip, throws.

.OUTPUTS
    [pscustomobject] Files (.bite files found), Encrypted (values encrypted), AlreadyEncrypted (WFAES:: values left).
#>
[CmdletBinding()]
param(
    # A .bite file, or a folder searched recursively for .bite files.
    [Parameter(Mandatory)][string] $Path,
    # The WFAES key material: the Key Vault secret's value.
    [Parameter(Mandatory)][string] $KeyMaterialPath
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$prefix = 'WFAES::'

# AesGcm takes spans, which PowerShell cannot pass, so the calls are compiled.
if (-not ('ProfilerWfAes' -as [type])) {
    Add-Type -Language CSharp -TypeDefinition @'
using System;
using System.Security.Cryptography;
using System.Text;

public static class ProfilerWfAes
{
    const int NonceSize = 12;
    const int TagSize   = 16;

    // [12 nonce][ciphertext][16 tag], as FileDecryptionHelper reads it.
    public static byte[] Encrypt(byte[] key, string plainText)
    {
        var plain  = Encoding.UTF8.GetBytes(plainText);
        var result = new byte[NonceSize + plain.Length + TagSize];
        var nonce  = new byte[NonceSize];
        var cipher = new byte[plain.Length];
        var tag    = new byte[TagSize];
        RandomNumberGenerator.Fill(nonce);
        using (var aes = new AesGcm(key, TagSize)) aes.Encrypt(nonce, plain, cipher, tag);
        Buffer.BlockCopy(nonce,  0, result, 0,                        NonceSize);
        Buffer.BlockCopy(cipher, 0, result, NonceSize,                cipher.Length);
        Buffer.BlockCopy(tag,    0, result, NonceSize + cipher.Length, TagSize);
        return result;
    }

    public static string Decrypt(byte[] key, byte[] data)
    {
        if (data.Length < NonceSize + TagSize) throw new CryptographicException("WFAES payload too short.");
        var cipher = new byte[data.Length - NonceSize - TagSize];
        var nonce  = new byte[NonceSize];
        var tag    = new byte[TagSize];
        var plain  = new byte[cipher.Length];
        Buffer.BlockCopy(data, 0,                        nonce,  0, NonceSize);
        Buffer.BlockCopy(data, NonceSize,                cipher, 0, cipher.Length);
        Buffer.BlockCopy(data, NonceSize + cipher.Length, tag,   0, TagSize);
        using (var aes = new AesGcm(key, TagSize)) aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }
}
'@
}

# ── Key ────────────────────────────────────────────────────────────────────────────────────────────
if (-not (Test-Path -LiteralPath $KeyMaterialPath -PathType Leaf)) { throw "Key material not found: $KeyMaterialPath" }
try {
    $km = (Get-Content -LiteralPath $KeyMaterialPath -Raw).Trim() | ConvertFrom-Json
    $key = [Convert]::FromBase64String($km.key)
} catch {
    throw "$KeyMaterialPath is not WFAES key material (JSON with a base64 'key')."
}
if ($key.Length -ne 32) { throw "$KeyMaterialPath holds a $($key.Length)-byte key; WFAES needs 32 bytes." }
$keyId = $km.keyId
$km = $null

# ── Files ──────────────────────────────────────────────────────────────────────────────────────────
if (Test-Path -LiteralPath $Path -PathType Leaf) { $files = @(Get-Item -LiteralPath $Path) }
elseif (Test-Path -LiteralPath $Path -PathType Container) { $files = @(Get-ChildItem -LiteralPath $Path -Recurse -File -Filter '*.bite') }
else { throw "Path not found: $Path" }

# The settings Encrypt-Config.ps1 saves with, so the engine reads the same layout it always has.
$writerSettings = [System.Xml.XmlWriterSettings]::new()
$writerSettings.Indent = $true
$writerSettings.IndentChars = '  '
$writerSettings.Encoding = [System.Text.Encoding]::UTF8
$writerSettings.OmitXmlDeclaration = $true
$writerSettings.NewLineHandling = [System.Xml.NewLineHandling]::Replace

$encrypted = 0
$alreadyEncrypted = 0
try {
    foreach ($file in $files) {
        $content = Get-Content -LiteralPath $file.FullName -Raw -Encoding UTF8
        if ($content -notmatch '<Source\b' -or $content -notmatch '\bConnectionString\s*=') { continue }
        try { [xml] $xml = $content } catch { throw "$($file.FullName) has a <Source ConnectionString=...> but is not valid XML: $($_.Exception.Message)" }

        $changed = 0
        foreach ($src in $xml.SelectNodes('//Source[@ConnectionString]')) {
            $plain = $src.GetAttribute('ConnectionString')
            if ([string]::IsNullOrWhiteSpace($plain)) { continue }
            if ($plain.StartsWith($prefix, [StringComparison]::Ordinal)) { $alreadyEncrypted++; continue }

            $value = $prefix + [Convert]::ToBase64String([ProfilerWfAes]::Encrypt($key, $plain))
            if ([ProfilerWfAes]::Decrypt($key, [Convert]::FromBase64String($value.Substring($prefix.Length))) -cne $plain) {
                throw "$($file.Name): the encrypted ConnectionString does not decrypt back to the original."
            }
            $src.SetAttribute('ConnectionString', $value)
            $changed++
        }
        if (-not $changed) { continue }

        $writer = [System.Xml.XmlWriter]::Create($file.FullName, $writerSettings)
        try { $xml.Save($writer) } finally { $writer.Dispose() }
        $encrypted += $changed
        Write-Host "  [+] $($file.Name): $changed ConnectionString(s) encrypted (KeyId=$keyId)."
    }
} finally {
    [Array]::Clear($key, 0, $key.Length)
}

[pscustomobject]@{ Files = $files.Count; Encrypted = $encrypted; AlreadyEncrypted = $alreadyEncrypted }
