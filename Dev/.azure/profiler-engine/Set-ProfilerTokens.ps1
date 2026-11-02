#Requires -Version 7.0
<#
.SYNOPSIS
    Replaces every #{Name} release token in the files under -Path with the environment variable of the same name.

.DESCRIPTION
    The DEV2 counterpart of Unlimited.ProfilerServices' replacetokens@5 steps (tokenPattern octopus,
    actionOnMissing fail), with no dependency on that marketplace extension. The pipeline puts the values in the
    step's environment: Key Vault secrets mapped explicitly (Azure DevOps never exports a secret on its own), plain
    pipeline variables as Azure DevOps exports them.

    Fails, before writing anything, when:
      - a token has no value, or its value is an unexpanded '$(Name)' macro (an undefined pipeline variable);
      - a value holds a character that would need escaping where it lands. A .bite token can sit in an XML
        attribute or inside the XML-escaped XAML of a workflow, and a .json token inside a JSON string, so a
        value is inserted verbatim and must not contain & < > " ' or \.
    Values are never written to the output; the report names tokens only.

.OUTPUTS
    [pscustomobject] Files (files that held tokens), Tokens (distinct token names), Replaced (occurrences).
#>
[CmdletBinding()]
param(
    # A folder searched recursively.
    [Parameter(Mandatory)][string] $Path,
    # File patterns to process.
    [string[]] $Include = @('*.bite', '*.json')
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$tokenRx = [regex] '#\{([A-Za-z0-9_.-]+)\}'
$unsafe  = [regex] '[&<>"''\\]'

if (-not (Test-Path -LiteralPath $Path -PathType Container)) { throw "Path not found: $Path" }
$files = @(Get-ChildItem -LiteralPath $Path -Recurse -File -Include $Include)

# Read everything first, so a missing value fails the step with every file untouched.
$pending = foreach ($file in $files) {
    $bytes = [System.IO.File]::ReadAllBytes($file.FullName)
    $bom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF
    $text = [System.Text.UTF8Encoding]::new($false).GetString($bytes, ($bom ? 3 : 0), $bytes.Length - ($bom ? 3 : 0))
    $found = $tokenRx.Matches($text)
    if ($found.Count) { [pscustomobject]@{ File = $file; Bom = $bom; Text = $text; Count = $found.Count; Names = @($found | ForEach-Object { $_.Groups[1].Value }) } }
}
$pending = @($pending)

$names = @($pending | ForEach-Object Names | Sort-Object -Unique)
$values = @{}
$problems = [System.Collections.Generic.List[string]]::new()
foreach ($name in $names) {
    $value = [Environment]::GetEnvironmentVariable($name)
    if ([string]::IsNullOrWhiteSpace($value) -or $value -match '^\$\(') {
        $problems.Add("#{$name} has no value: add a '$name' secret to the Key Vault (and to the step's env mapping), or a '$name' pipeline variable.")
    } elseif ($unsafe.IsMatch($value)) {
        $problems.Add("#{$name} has a value with a character that would need escaping (& < > `" ' \); it is inserted verbatim, so change the value.")
    } else {
        $values[$name] = $value
    }
}
if ($problems.Count) {
    foreach ($p in $problems) { Write-Host "##[error]$p" }
    throw "$($problems.Count) token(s) cannot be replaced under $Path; no file was changed."
}

$replaced = 0
foreach ($p in $pending) {
    $text = $tokenRx.Replace($p.Text, { param($m) $values[$m.Groups[1].Value] })
    [System.IO.File]::WriteAllText($p.File.FullName, $text, [System.Text.UTF8Encoding]::new($p.Bom))
    $replaced += $p.Count
    Write-Host "  [+] $($p.File.Name): $($p.Count) token(s) replaced ($((@($p.Names | Sort-Object -Unique)) -join ', '))."
}

[pscustomobject]@{ Files = $pending.Count; Tokens = $names.Count; Replaced = $replaced }
