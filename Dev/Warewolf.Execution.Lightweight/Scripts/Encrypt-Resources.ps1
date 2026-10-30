<#
.SYNOPSIS
    WFAES-encrypts the source .bite files of a resources folder with a key supplied by the
    caller, writing the result to an output folder (or back in place). No az, no Key Vault.

.DESCRIPTION
    For CI/CD steps that cannot run az. The key material arrives as a pipeline secret
    variable (for example from AzureKeyVault@2) instead of being read from Key Vault.

    Every file under -SourceDirectory is considered:
      • .bite files go through Encrypt-Config.ps1 -KVSecret, which encrypts the
        ConnectionString attribute of each <Source> element. Other .bite files (workflows,
        Globals, JSON queue triggers) have no <Source ConnectionString> and pass through
        unchanged; values that are already WFAES:: are left as they are.
      • every other file (JSON, .txt, ...) is copied unchanged.

    SEPARATE OUTPUT FOLDER (-OutputDirectory differs from -SourceDirectory)
      The output receives every file and folder of the source, so the two trees match, with
      the <Source> connection strings encrypted. Files already in the output are overwritten;
      files that exist only in the output are kept. The source is never modified.

    IN PLACE (-OutputDirectory is the same folder as -SourceDirectory)
      Only the .bite files whose content changed are written back over the originals.

    ALL OR NOTHING
      The .bite files are first encrypted in a temporary staging folder, then proved:
        1. Encrypt-Config.ps1 -VerifyOnly decrypts every value in memory with the same key
           (a value already encrypted under a different key fails here);
        2. every non-empty <Source ConnectionString> must start with WFAES::.
      Only then is anything written to the output. Each file written is recorded first, with
      a backup of any file it replaces; on any failure the backups are restored, new files
      and folders are removed, and the script exits 1. The staging folder holds plaintext
      copies, so it is deleted whether the run succeeds or fails.

    The secret is never written to the console, a log or a file.

.PARAMETER KVSecret
    The key material: the full JSON value of the Key Vault secret
    ({"version":1,"keyId":"<guid>","key":"<base64 32-byte key>","created":"<date>"}).
    When omitted it is read from the WW_KV_SECRET environment variable; when neither is
    set the script fails. There is no default — pass it from a secret variable, never a literal.

.PARAMETER SourceDirectory
    One folder; it is searched recursively. To process several folders, call the script
    once per folder.

.PARAMETER OutputDirectory
    One folder; created when missing. The same folder as -SourceDirectory means in place.
    It cannot be inside -SourceDirectory, nor contain it, unless the two are the same.

.OUTPUTS
    A summary object: Mode (Copy | InPlace), SourceDirectory, OutputDirectory,
    FilesCopied (files written to the output), BiteFiles, SourcesEncrypted (.bite files
    this run encrypted), AlreadyEncrypted (.bite files whose sources were all WFAES:: already)
    and Unchanged (.bite files written as they were).

.EXAMPLE
    # Azure DevOps (pwsh): the secret variable reaches the script through env:
    #   - pwsh: ./Encrypt-Resources.ps1 -SourceDirectory '$(Build.SourcesDirectory)/Resources' -OutputDirectory '$(Build.ArtifactStagingDirectory)/Resources'
    #     env:
    #       WW_KV_SECRET: $(ProfilerEngineEncryptionKey)
    .\Encrypt-Resources.ps1 -SourceDirectory .\Resources -OutputDirectory .\Resources-encrypted

.EXAMPLE
    # In place
    .\Encrypt-Resources.ps1 -KVSecret $env:MY_KEY_MATERIAL -SourceDirectory .\Resources -OutputDirectory .\Resources

.NOTES
    Requires: PowerShell 7+ (AES-GCM) and Encrypt-Config.ps1 beside this script.
    DPAPI-encrypted values can only be converted on Windows, on the machine that created them.
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [string] $KVSecret,

    [Parameter(Mandatory)]
    [string] $SourceDirectory,

    [Parameter(Mandatory)]
    [string] $OutputDirectory
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Console helpers ────────────────────────────────────────────────────────────
function Write-Step  ([string]$msg) { Write-Host "[*] $msg" -ForegroundColor Cyan    }
function Write-OK    ([string]$msg) { Write-Host "[+] $msg" -ForegroundColor Green   }
function Write-Skip  ([string]$msg) { Write-Host "[-] $msg" -ForegroundColor Yellow  }
function Write-Fail  ([string]$msg) { Write-Host "[!] $msg" -ForegroundColor Red     }

# Full path without a trailing separator (a drive or filesystem root keeps its own).
function Get-NormalizedPath ([string]$path) {
    $full = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($path)
    return [System.IO.Path]::TrimEndingDirectorySeparator([System.IO.Path]::GetFullPath($full))
}

function Test-IsUnder ([string]$child, [string]$parent, [System.StringComparison]$comparison) {
    $sep    = [System.IO.Path]::DirectorySeparatorChar
    $prefix = $parent.EndsWith($sep) ? $parent : $parent + $sep
    return $child.StartsWith($prefix, $comparison)
}

# Same guard as Encrypt-Config.ps1 uses to pick the files it rewrites.
function Get-SourceConnectionStrings ([string]$file) {
    $content = Get-Content -LiteralPath $file -Raw -Encoding UTF8
    if ($null -eq $content -or $content -notmatch '<Source\b' -or $content -notmatch '\bConnectionString\s*=') { return @() }
    [xml]$xml = $content
    return @($xml.SelectNodes('//Source[@ConnectionString]') |
             ForEach-Object { $_.GetAttribute('ConnectionString') } |
             Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

function Test-SameContent ([string]$a, [string]$b) {
    return (Get-FileHash -LiteralPath $a -Algorithm SHA256).Hash -eq (Get-FileHash -LiteralPath $b -Algorithm SHA256).Hash
}

# ── Inputs: checked before anything is staged or written ──────────────────────
$secret = $KVSecret ? $KVSecret : $env:WW_KV_SECRET
if ([string]::IsNullOrWhiteSpace($secret)) {
    Write-Fail 'No key material: pass -KVSecret or set the WW_KV_SECRET environment variable (from a secret variable, never a literal).'
    exit 1
}

if (-not (Test-Path -LiteralPath $SourceDirectory -PathType Container)) {
    Write-Fail "Source directory '$SourceDirectory' not found."
    exit 1
}
$source = Get-NormalizedPath $SourceDirectory
$output = Get-NormalizedPath $OutputDirectory

if (Test-Path -LiteralPath $output -PathType Leaf) {
    Write-Fail "Output directory '$output' is a file."
    exit 1
}

$comparison = $IsWindows ? [System.StringComparison]::OrdinalIgnoreCase : [System.StringComparison]::Ordinal
$inPlace    = [string]::Equals($source, $output, $comparison)
if (-not $inPlace -and ((Test-IsUnder $output $source $comparison) -or (Test-IsUnder $source $output $comparison))) {
    Write-Fail "The output directory '$output' and the source directory '$source' are nested. Use separate folders, or the same folder to encrypt in place."
    exit 1
}

$encryptConfig = Join-Path $PSScriptRoot 'Encrypt-Config.ps1'
if (-not (Test-Path -LiteralPath $encryptConfig -PathType Leaf)) {
    Write-Fail "Encrypt-Config.ps1 not found beside this script ('$encryptConfig')."
    exit 1
}

$mode = $inPlace ? 'InPlace' : 'Copy'
Write-Step "Encrypting resources ($mode): '$source' -> '$output'"

# Hidden files included: the output mirrors everything in the source.
$allFiles  = @(Get-ChildItem -LiteralPath $source -Recurse -File -Force)
$biteFiles = @($allFiles | Where-Object { $_.Extension -ieq '.bite' })
$otherFiles = @($allFiles | Where-Object { $_.Extension -ine '.bite' })
$sourceDirs = @(Get-ChildItem -LiteralPath $source -Recurse -Directory -Force)
Write-OK "Found $($allFiles.Count) file(s), $($biteFiles.Count) of them .bite"

$stageRoot   = Join-Path ([System.IO.Path]::GetTempPath()) ('ww-encrypt-resources-' + [Guid]::NewGuid().ToString('N'))
$stageBite   = Join-Path $stageRoot 'bite'
$stageBackup = Join-Path $stageRoot 'backup'

# Rollback record: every file written to the output (Backup = $null when it was new) and
# every folder created, in the order they happened.
$journal     = [System.Collections.Generic.List[object]]::new()
$createdDirs = [System.Collections.Generic.List[string]]::new()

function New-TrackedDirectory ([string]$dir) {
    $missing = [System.Collections.Generic.List[string]]::new()
    $d = $dir
    while ($d -and -not (Test-Path -LiteralPath $d -PathType Container)) {
        $missing.Insert(0, $d)
        $d = Split-Path $d -Parent
    }
    foreach ($m in $missing) {
        $null = New-Item -ItemType Directory -Path $m
        $createdDirs.Add($m)
    }
}

function Write-TrackedFile ([string]$from, [string]$dest) {
    if (Test-Path -LiteralPath $dest -PathType Leaf) {
        $backup = Join-Path $stageBackup $journal.Count
        Copy-Item -LiteralPath $dest -Destination $backup -Force
        $journal.Add([pscustomobject]@{ Path = $dest; Backup = $backup })
    } else {
        New-TrackedDirectory (Split-Path $dest -Parent)
        $journal.Add([pscustomobject]@{ Path = $dest; Backup = $null })
    }
    Copy-Item -LiteralPath $from -Destination $dest -Force
}

function Undo-Writes {
    $failedPaths = [System.Collections.Generic.List[string]]::new()
    for ($i = $journal.Count - 1; $i -ge 0; $i--) {
        $entry = $journal[$i]
        try {
            if ($entry.Backup) { Copy-Item -LiteralPath $entry.Backup -Destination $entry.Path -Force }
            elseif (Test-Path -LiteralPath $entry.Path) { Remove-Item -LiteralPath $entry.Path -Force }
        } catch { $failedPaths.Add($entry.Path) }
    }
    for ($i = $createdDirs.Count - 1; $i -ge 0; $i--) {
        $dir = $createdDirs[$i]
        try {
            if ((Test-Path -LiteralPath $dir) -and -not (Get-ChildItem -LiteralPath $dir -Force | Select-Object -First 1)) {
                Remove-Item -LiteralPath $dir -Force
            }
        } catch { $failedPaths.Add($dir) }
    }
    return , $failedPaths
}

$failed  = $false
$summary = $null
try {
    # ── 1. Stage the .bite files ──────────────────────────────────────────────
    $null = New-Item -ItemType Directory -Path $stageBite   -Force
    $null = New-Item -ItemType Directory -Path $stageBackup -Force
    foreach ($f in $biteFiles) {
        $staged = Join-Path $stageBite ([System.IO.Path]::GetRelativePath($source, $f.FullName))
        $null = New-Item -ItemType Directory -Path (Split-Path $staged -Parent) -Force
        Copy-Item -LiteralPath $f.FullName -Destination $staged
    }

    # ── 2. Encrypt, then prove the key decrypts every value ───────────────────
    # Same process (&), so the secret never appears on a command line. A script that ends
    # without `exit` leaves $LASTEXITCODE alone, hence the reset before each call.
    $global:LASTEXITCODE = 0
    $null = & $encryptConfig -FilePath $stageBite -KVSecret $secret -NonInteractive -NoBackup
    if ($LASTEXITCODE -ne 0) { throw "Encrypt-Config.ps1 failed to encrypt (exit $LASTEXITCODE)." }

    $global:LASTEXITCODE = 0
    $null = & $encryptConfig -FilePath $stageBite -KVSecret $secret -VerifyOnly -NonInteractive
    if ($LASTEXITCODE -ne 0) { throw "Encrypt-Config.ps1 -VerifyOnly failed (exit $LASTEXITCODE): a value does not decrypt with this key." }

    # ── 3. No <Source> connection string may be left in plain text ────────────
    $encryptedNow = 0; $alreadyEncrypted = 0
    $plainFiles   = [System.Collections.Generic.List[string]]::new()
    $changed      = [System.Collections.Generic.HashSet[string]]::new()
    foreach ($f in $biteFiles) {
        $rel    = [System.IO.Path]::GetRelativePath($source, $f.FullName)
        $staged = Join-Path $stageBite $rel
        $values = @(Get-SourceConnectionStrings $staged)
        if ($values | Where-Object { -not $_.StartsWith('WFAES::') }) { $plainFiles.Add($rel); continue }
        if (-not (Test-SameContent $f.FullName $staged)) { $null = $changed.Add($rel); $encryptedNow++ }
        elseif ($values.Count -gt 0) { $alreadyEncrypted++ }
    }
    if ($plainFiles.Count -gt 0) {
        throw "Plain-text <Source> ConnectionString left in: $($plainFiles -join ', ')"
    }
    Write-OK "Staged and verified: $encryptedNow encrypted now, $alreadyEncrypted already encrypted."

    # ── 4. Write the output ───────────────────────────────────────────────────
    if ($inPlace) {
        foreach ($rel in ($changed | Sort-Object)) {
            Write-TrackedFile (Join-Path $stageBite $rel) (Join-Path $output $rel)
        }
    } else {
        New-TrackedDirectory $output
        foreach ($d in $sourceDirs) {
            New-TrackedDirectory (Join-Path $output ([System.IO.Path]::GetRelativePath($source, $d.FullName)))
        }
        # Non-.bite files first, then the staged .bite files: the output never holds a
        # plain-text copy of a file that was meant to be encrypted.
        foreach ($f in ($otherFiles | Sort-Object FullName)) {
            Write-TrackedFile $f.FullName (Join-Path $output ([System.IO.Path]::GetRelativePath($source, $f.FullName)))
        }
        foreach ($f in ($biteFiles | Sort-Object FullName)) {
            $rel = [System.IO.Path]::GetRelativePath($source, $f.FullName)
            Write-TrackedFile (Join-Path $stageBite $rel) (Join-Path $output $rel)
        }
    }

    $summary = [pscustomobject]@{
        Mode             = $mode
        SourceDirectory  = $source
        OutputDirectory  = $output
        FilesCopied      = $journal.Count
        BiteFiles        = $biteFiles.Count
        SourcesEncrypted = $encryptedNow
        AlreadyEncrypted = $alreadyEncrypted
        Unchanged        = $biteFiles.Count - $encryptedNow
    }
} catch {
    $failed = $true
    Write-Fail "Encryption failed: $($_.Exception.Message)"
    if ($journal.Count -gt 0 -or $createdDirs.Count -gt 0) {
        Write-Step 'Rolling back the output...'
        $notRestored = Undo-Writes
        if ($notRestored.Count -gt 0) {
            Write-Fail 'Rollback could not restore these paths; check them by hand:'
            foreach ($p in $notRestored) { Write-Fail "    $p" }
        } else {
            Write-OK 'Output restored to its state before the run.'
        }
    } else {
        Write-Skip 'Nothing was written to the output.'
    }
} finally {
    # The staging folder holds plaintext copies (and the replaced originals): always remove it.
    if (Test-Path -LiteralPath $stageRoot) {
        try   { Remove-Item -LiteralPath $stageRoot -Recurse -Force }
        catch { Write-Fail "Could not delete the staging folder '$stageRoot'; it may hold plain-text connection strings. Delete it by hand." }
    }
}

if ($failed) { exit 1 }

Write-OK "Done: $($summary.FilesCopied) file(s) written to '$output'."
$summary
exit 0
