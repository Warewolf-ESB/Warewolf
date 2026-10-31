<#
.SYNOPSIS
    Works out which release zips in the release storage container are no longer mentioned in
    release-notes.php, and returns their names. It does NOT delete anything itself - the Release
    stage of pipeline-CLOUD.yml does the `az storage blob delete` (or only logs, in dry-run mode).

.DESCRIPTION
    A blob is returned for deletion only when ALL of these hold:
      * its name matches one of the release naming patterns (-ReleaseNamePrefix, each followed by
        anything and ending in .zip), so an unrelated file in the container is never touched;
      * no URL under -BaseUrl in the page names it - both the latest block and every archived block
        count as "mentioned", as does a pre-split release that only has a single engine link.

    Blob names are compared exactly (ordinal, case-sensitive), so 'Foo-3.0.2.9.zip' does not
    protect 'Foo-3.0.2.90.zip'.

    Refuses to return anything (throws) when the page cannot be trusted, so a truncated or wrong
    download can never turn into a wipe of the container:
      * the page mentions no blob URLs at all;
      * the page does not mention every blob in -ExpectedBlobName (the zips this very run
        uploaded and has just published).

.EXAMPLE
    ./Remove-UnreferencedReleases.ps1 -ReleaseNotesPath ./release-notes.php `
        -BaseUrl 'https://stwwreleases.blob.core.windows.net/releases/' `
        -BlobName $allBlobNames `
        -ExpectedBlobName 'AzureFunctionsPackage-linux-x64-3.0.2.90.zip'
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string]$ReleaseNotesPath,
    [Parameter(Mandatory)] [string]$BaseUrl,
    [Parameter(Mandatory)] [AllowEmptyCollection()] [string[]]$BlobName,
    [Parameter(Mandatory)] [string[]]$ExpectedBlobName,
    [string[]]$ReleaseNamePrefix = @(
        'AzureFunctionsPackage-',
        'Warewolf-ExecutionEngine-Deployment-Scripts-',
        'Warewolf-QueueProcessor-')
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path -LiteralPath $ReleaseNotesPath)) { throw "Release notes file not found: $ReleaseNotesPath" }
$content = Get-Content -LiteralPath $ReleaseNotesPath -Raw

if (-not $BaseUrl.EndsWith('/')) { $BaseUrl += '/' }
$referenced = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($m in [regex]::Matches($content, [regex]::Escape($BaseUrl) + '([^"''\s<>?#]+)')) {
    [void]$referenced.Add([uri]::UnescapeDataString($m.Groups[1].Value))
}

if ($referenced.Count -eq 0) {
    throw "'$ReleaseNotesPath' mentions no blobs under $BaseUrl - refusing to prune against a page that may be truncated or wrong."
}

$missing = @($ExpectedBlobName | Where-Object { -not $referenced.Contains($_) })
if ($missing) {
    throw "'$ReleaseNotesPath' does not mention the just-published blob(s): $($missing -join ', ') - refusing to prune."
}

$isReleaseBlob = {
    param([string]$Name)
    foreach ($prefix in $ReleaseNamePrefix) {
        if ($Name.StartsWith($prefix, [System.StringComparison]::Ordinal) -and
            $Name.EndsWith('.zip', [System.StringComparison]::Ordinal)) { return $true }
    }
    return $false
}

$BlobName | Where-Object { (& $isReleaseBlob $_) -and -not $referenced.Contains($_) }
