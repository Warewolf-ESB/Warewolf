# Warewolf Version: 3.0.2.79  |  Stamped: 2026-06-22
#Requires -Version 5.1
<#
.SYNOPSIS
    Encrypts or decrypts the values in a 'Warewolf License.secureconfig' file.

.DESCRIPTION
    Each <add value="..."/> attribute is individually transformed using the same
    AES-256-CBC algorithm as Dev2.Infrastructure SecurityEncryption.cs.

    Key material (fixed — mirrors SecurityEncryption.cs exactly):
      PassPhrase         : Pas5pr@se
      SaltValue          : s@1tValue
      HashAlgorithm      : SHA1
      PasswordIterations : 2
      KeySize            : 256 bit
      IV                 : @1B2c3D4e5F6g7H8  (ASCII, 16 bytes)
      Padding            : Zeros
      Output encoding    : Base64

    Empty values (e.g. an unset CustomerId) are left unchanged.

    Running -Encrypt on a file that already holds encrypted (Base64) values will
    double-encrypt them.  Run -Decrypt first if you are unsure of the current state.

.PARAMETER Path
    Path to the 'Warewolf License.secureconfig' file.
    Default: "Warewolf License.secureconfig" in the current directory.

.PARAMETER Encrypt
    Encrypt every non-empty value.

.PARAMETER Decrypt
    Decrypt every non-empty value back to plain text.

.PARAMETER OutPath
    Where to write the resulting XML.
    Default: overwrite the input file.
    Pass '-OutPath -' to print to stdout without writing to disk.

.EXAMPLE
    # Encrypt the default license config in the current directory
    .\Protect-LicenseConfig.ps1 -Encrypt

.EXAMPLE
    # Decrypt to inspect values (stdout only, file unchanged)
    .\Protect-LicenseConfig.ps1 -Decrypt -OutPath -

.EXAMPLE
    # Encrypt a specific file and write to a different location
    .\Protect-LicenseConfig.ps1 -Path "C:\build\Warewolf License.secureconfig" `
                                 -Encrypt `
                                 -OutPath "C:\deploy\Warewolf License.secureconfig"
#>

[CmdletBinding(DefaultParameterSetName = 'Encrypt')]
param(
    [string] $Path = "Warewolf License.secureconfig",

    [Parameter(ParameterSetName = 'Encrypt', Mandatory)]
    [switch] $Encrypt,

    [Parameter(ParameterSetName = 'Decrypt', Mandatory)]
    [switch] $Decrypt,

    [string] $OutPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── AES-CBC helpers (mirrors SecurityEncryption.cs exactly) ──────────────────

$_IV   = [System.Text.Encoding]::ASCII.GetBytes('@1B2c3D4e5F6g7H8')
$_Salt = [System.Text.Encoding]::ASCII.GetBytes('s@1tValue')

function Get-DerivedKeyBytes {
    # PasswordDeriveBytes with SHA1 / 2 iterations — identical to SecurityEncryption.cs.
    $pdb = New-Object System.Security.Cryptography.PasswordDeriveBytes(
        'Pas5pr@se', $_Salt, 'SHA1', 2)
    return $pdb.GetBytes(32)   # 256-bit key
}

function Invoke-SecurityEncrypt ([string]$PlainText) {
    $plain    = [System.Text.Encoding]::UTF8.GetBytes($PlainText)
    $keyBytes = Get-DerivedKeyBytes

    $aes         = [System.Security.Cryptography.Aes]::Create()
    $aes.Mode    = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::Zeros

    $enc = $aes.CreateEncryptor($keyBytes, $_IV)
    $ms  = [System.IO.MemoryStream]::new()
    $cs  = [System.Security.Cryptography.CryptoStream]::new(
               $ms, $enc, [System.Security.Cryptography.CryptoStreamMode]::Write)

    $cs.Write($plain, 0, $plain.Length)
    $cs.FlushFinalBlock()
    $cipher = $ms.ToArray()
    $cs.Dispose(); $ms.Dispose(); $aes.Dispose()

    return [Convert]::ToBase64String($cipher)
}

function Invoke-SecurityDecrypt ([string]$CipherText) {
    if ([string]::IsNullOrEmpty($CipherText)) { return $CipherText }

    try   { $cipher = [Convert]::FromBase64String($CipherText) }
    catch { return $CipherText }   # not valid Base64 → already plain text

    $keyBytes = Get-DerivedKeyBytes

    $aes         = [System.Security.Cryptography.Aes]::Create()
    $aes.Mode    = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::Zeros

    # Mirrors SecurityEncryption.Decrypt exactly: MemoryStream is initialised with
    # the cipher bytes (writable, position 0); the CryptoStream overwrites them
    # in place with the decrypted output.  ToArray() then returns the plain bytes.
    $dec = $aes.CreateDecryptor($keyBytes, $_IV)
    $ms  = [System.IO.MemoryStream]::new($cipher)
    $cs  = [System.Security.Cryptography.CryptoStream]::new(
               $ms, $dec, [System.Security.Cryptography.CryptoStreamMode]::Write)

    $cs.Write($cipher, 0, $cipher.Length)
    $cs.FlushFinalBlock()
    $plain = $ms.ToArray()
    $cs.Dispose(); $ms.Dispose(); $aes.Dispose()

    # Zero-padding removal matches DecryptKey() → TrimEnd('\0') in SubscriptionConfig.
    return [System.Text.Encoding]::UTF8.GetString($plain).TrimEnd([char]0)
}

# ── Load XML ──────────────────────────────────────────────────────────────────

if (-not (Test-Path -LiteralPath $Path)) {
    Write-Error "File not found: $Path"
    exit 1
}

[xml] $xml = Get-Content -LiteralPath $Path -Raw -Encoding UTF8

# ── Transform each <add> value ────────────────────────────────────────────────

$count = 0
foreach ($node in $xml.subscriptionSettings.add) {
    if ([string]::IsNullOrEmpty($node.value)) { continue }

    $node.value = if ($Encrypt) {
        Invoke-SecurityEncrypt $node.value
    } else {
        Invoke-SecurityDecrypt $node.value
    }
    $count++
}

$verb = if ($Encrypt) { 'encrypted' } else { 'decrypted' }
Write-Host "$count value(s) $verb."

# ── Write result ──────────────────────────────────────────────────────────────

$target = if ($OutPath) { $OutPath } else { $Path }

if ($target -eq '-') {
    # Print to stdout — useful for inspection without touching the file.
    $sw  = [System.IO.StringWriter]::new()
    $xws = [System.Xml.XmlWriterSettings]::new()
    $xws.Indent      = $true
    $xws.IndentChars = '  '
    $xws.OmitXmlDeclaration = $false
    $writer = [System.Xml.XmlWriter]::Create($sw, $xws)
    $xml.Save($writer)
    $writer.Flush(); $writer.Dispose()
    $sw.ToString()
} else {
    $xws              = [System.Xml.XmlWriterSettings]::new()
    $xws.Indent       = $true
    $xws.IndentChars  = '  '
    $xws.Encoding     = [System.Text.UTF8Encoding]::new($false)  # UTF-8, no BOM
    $xws.OmitXmlDeclaration = $false
    $writer = [System.Xml.XmlWriter]::Create($target, $xws)
    $xml.Save($writer)
    $writer.Flush(); $writer.Dispose()
    Write-Host "Written to: $(Resolve-Path -LiteralPath $target)"
}
